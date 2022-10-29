using System.Numerics;
using rqdq.rglv;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using DXBuffer = SharpDX.Direct3D11.Buffer;
using DXDevice = SharpDX.Direct3D11.Device;

namespace rqdq {
namespace app {

class Node {
  public virtual
  void Receive(string n, float a) {}
  }


interface ICamera {
  Matrix4x4 GetViewMatrix();
  Matrix4x4 GetProjMatrix(); }


interface INamedValue {
  void GetValue(ReadOnlySpan<char> name, out float a);
  void GetValue(ReadOnlySpan<char> name, out Vector2 a);
  void GetValue(ReadOnlySpan<char> name, out Vector3 a);
  void GetValue(ReadOnlySpan<char> name, out Vector4 a); };

  /*
class NamedFloat : Node, INamedValue {
  private float _a;
  public NamedFloat(float a) {
    _a = a; }
  public void GetValue(ReadOnlySpan<char> name, out float a) { a = _a; }
  public void GetValue(ReadOnlySpan<char> name, out Vector2 a) { a = _a; }
  public void GetValue(ReadOnlySpan<char> name, out Vector3 a) { a = _a; }
  public void GetValue(ReadOnlySpan<char> name, out Vector4 a) { a = _a; }
  }
*/

class NamedMul : Node, INamedValue {
  private readonly INamedValue _a, _b;

  public
  NamedMul(INamedValue a, INamedValue b) {
    _a = a;
    _b = b; }

  public void GetValue(ReadOnlySpan<char> name, out float a) { _a.GetValue(name, out float va); _b.GetValue(name, out float vb); a = va * vb; }
  public void GetValue(ReadOnlySpan<char> name, out Vector2 a) { _a.GetValue(name, out Vector2 va); _b.GetValue(name, out Vector2 vb); a = va * vb; }
  public void GetValue(ReadOnlySpan<char> name, out Vector3 a) { _a.GetValue(name, out Vector3 va); _b.GetValue(name, out Vector3 vb); a = va * vb; }
  public void GetValue(ReadOnlySpan<char> name, out Vector4 a) { _a.GetValue(name, out Vector4 va); _b.GetValue(name, out Vector4 vb); a = va * vb; }
  }

class NamedAdd : Node, INamedValue {
  private readonly INamedValue _a, _b;

  public
  NamedAdd(INamedValue a, INamedValue b) {
    _a = a;
    _b = b; }

  public void GetValue(ReadOnlySpan<char> name, out float a) { _a.GetValue(name, out float va); _b.GetValue(name, out float vb); a = va + vb; }
  public void GetValue(ReadOnlySpan<char> name, out Vector2 a) { _a.GetValue(name, out Vector2 va); _b.GetValue(name, out Vector2 vb); a = va + vb; }
  public void GetValue(ReadOnlySpan<char> name, out Vector3 a) { _a.GetValue(name, out Vector3 va); _b.GetValue(name, out Vector3 vb); a = va + vb; }
  public void GetValue(ReadOnlySpan<char> name, out Vector4 a) { _a.GetValue(name, out Vector4 va); _b.GetValue(name, out Vector4 vb); a = va + vb; }
  }

interface IDrawable {
  void Draw(DeviceContext dc, Matrix4x4 vmat, Matrix4x4 pmat); }


interface ILayer {
  RawColor4 GetColor();
  void Draw(DeviceContext dc); }


class GlLayer : Node, ILayer {
  private readonly ICamera _camera;
  private readonly IDrawable _drawable;
  private readonly RawColor4 _bkg;
  
  public
  GlLayer(ICamera c, IDrawable d, RawColor4 bkg) {
    _camera = c;
    _drawable = d;
    _bkg = bkg; }

  public
  void Draw(DeviceContext dc) {
    var vmat = _camera.GetViewMatrix();
    var pmat = _camera.GetProjMatrix();
    _drawable.Draw(dc, vmat, pmat); }

  public
  RawColor4 GetColor() {
    return _bkg; }}


class LookAtCamera : Node, ICamera {
  private readonly Vector3 _position;
  private float _width;
  private float _height;

  public LookAtCamera(Vector3 p) {
    _position = p; }

  public override
  void Receive(string n, float a) {
    if (n == "canvas.width") {
      _width = a; }
    else if (n == "canvas.height") {
      _height = a; }}

  public
  Matrix4x4 GetViewMatrix() {  
    return Matrix4x4.CreateLookAt(_position, new Vector3(0, 0, 0), Vector3.UnitY); }

  public
  Matrix4x4 GetProjMatrix() {
    var aspect = _width / _height;
    return Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4.0F, aspect, 0.1F, 100.0F); }
  }


class MyRotate : Node, IDrawable {
  private readonly IDrawable _child;
  private float _T;

  public
  MyRotate(IDrawable c) {
    _child = c; }

  public override
  void Receive(string n, float a) {
    if (n == "T") {
      _T = a; }}

  public
  void Draw(DeviceContext dc, Matrix4x4 vmat, Matrix4x4 pmat) {
      var m = Matrix4x4.CreateRotationX(_T * 0) *
              Matrix4x4.CreateRotationY(_T * 2) *
              Matrix4x4.CreateRotationZ(_T * 0.0F);
      _child.Draw(dc, m*vmat, pmat); }
  }
    

class MyScene : Node, IDrawable {

  static readonly string src = @"
struct VS_DATA {
  float3 pos : POSITION;
  float3 col : COLOR; };

struct PS_DATA {
  float4 pos : SV_POSITION;
  float3 col : COLOR; };

float4x4 worldViewProj;

PS_DATA VS(VS_DATA data) {
  PS_DATA output = (PS_DATA)0;
  output.pos = mul(float4(data.pos, 1), worldViewProj);
  output.col = data.col * (output.pos.z / output.pos.w);
  return output; }

float4 PS(PS_DATA data) : SV_Target {
  return float4(data.col, 1); }
";  
  private readonly DXDevice _device;
  private readonly ObjMesh _mesh;
  private CompilationResult? _vertexShaderBytecode;
  private CompilationResult? _pixelShaderBytecode;
  private VertexShader? _vertexShader;
  private PixelShader? _pixelShader;
  private ShaderSignature? _signature;
  private InputLayout? _layout;

  private Vector3[]? _theData;
  private DXBuffer? _vertices;
  private DXBuffer? _constantBuffer;

  public
  MyScene(DXDevice device, ObjMesh mesh) {
    _device = device;
    _mesh = mesh;
    Init(); }

  private void Init() {
    _vertexShaderBytecode = ShaderBytecode.Compile(src, "VS", "vs_4_0");
    _pixelShaderBytecode = ShaderBytecode.Compile(src, "PS", "ps_4_0");
    _vertexShader = new VertexShader(_device, _vertexShaderBytecode);
    _pixelShader = new PixelShader(_device, _pixelShaderBytecode);
    _signature = ShaderSignature.GetInputSignature(_vertexShaderBytecode);
    _layout = new InputLayout(_device, _signature, new[] {
      new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
      new InputElement("COLOR", 0, Format.R32G32B32_Float, 12, 0), });

    _theData = _mesh.MakeBuffer();
    _vertices = DXBuffer.Create(_device, BindFlags.VertexBuffer, _theData);

    _constantBuffer = new DXBuffer(
      _device,
      Utilities.SizeOf<Matrix4x4>(),
      ResourceUsage.Default,
      BindFlags.ConstantBuffer,
      CpuAccessFlags.None,
      ResourceOptionFlags.None,
      0);}

    public
    void Draw(DeviceContext dc, Matrix4x4 vmat, Matrix4x4 pmat) {
      dc.InputAssembler.InputLayout = _layout;
      dc.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
      dc.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertices, Utilities.SizeOf<Vector3>() * 2, 0));
      dc.VertexShader.SetConstantBuffer(0, _constantBuffer);
      dc.VertexShader.Set(_vertexShader);
      dc.PixelShader.Set(_pixelShader);

      var wvp = Matrix4x4.Multiply(vmat, pmat);
      wvp = Matrix4x4.Transpose(wvp);
      dc.UpdateSubresource(ref wvp, _constantBuffer);
      dc.Draw(380*3, 0); }

    public
    void Dispose() {
      _signature.Dispose();
      _vertexShaderBytecode.Dispose();
      _vertexShader.Dispose();
      _pixelShaderBytecode.Dispose();
      _pixelShader.Dispose();
      _vertices.Dispose();
      _layout.Dispose();
      _constantBuffer.Dispose(); }}


}  // close package namespace
}  // close enterprise namespace
