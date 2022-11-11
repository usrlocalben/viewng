using System.Numerics;
using rqdq.rglv;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using DXBuffer = SharpDX.Direct3D11.Buffer;
using DXDevice = SharpDX.Direct3D11.Device;

namespace rqdq {
namespace app {

class GlMesh : Node, IGl {

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
  // private readonly DXDevice _device;
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
  GlMesh(string id, ObjMesh mesh) : base(id) {
    _mesh = mesh; }

  public override
  void Init(DXDevice device) {
    _vertexShaderBytecode = ShaderBytecode.Compile(src, "VS", "vs_4_0");
    _pixelShaderBytecode = ShaderBytecode.Compile(src, "PS", "ps_4_0");
    _vertexShader = new VertexShader(device, _vertexShaderBytecode);
    _pixelShader = new PixelShader(device, _pixelShaderBytecode);
    _signature = ShaderSignature.GetInputSignature(_vertexShaderBytecode);
    _layout = new InputLayout(device, _signature, new[] {
      new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
      new InputElement("COLOR", 0, Format.R32G32B32_Float, 12, 0), });

    _theData = _mesh.MakeBuffer();
    _vertices = DXBuffer.Create(device, BindFlags.VertexBuffer, _theData);

    _constantBuffer = new DXBuffer(
      device,
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
    dc.Draw(_theData.Length / 2, 0); }

  public override
  void Dispose() {
    _signature.Dispose();
    _vertexShaderBytecode.Dispose();
    _vertexShader.Dispose();
    _pixelShaderBytecode.Dispose();
    _pixelShader.Dispose();
    _vertices.Dispose();
    _layout.Dispose();
    _constantBuffer.Dispose(); }}


public class GlMeshCompiler : NodeCompilerBase {
  public static void Install() {
    AnyCompiler.Register("mesh", (id, elem) => new GlMeshCompiler().Compile(id, elem)); }
  public override void CompileImpl() {
    Console.WriteLine("mesh compile");
    if (_data.TryGetProperty("src", out var src)) {
      var fn = src.GetString();
      fn = Path.Join(rqdq.app.MyApp.DataDir, fn);
      Console.WriteLine($"loading mesh [{fn}]");
      (var mesh, var took, var inputSizeInBytes) = ObjLoader.Load(fn);

      Console.WriteLine($"perf.Elapsed   {took} (ms)");
      Console.WriteLine($"    .Rate      {inputSizeInBytes / took / 1000.0} (MB/sec)");
      Console.WriteLine($"count.Position {mesh.cntPosition}");
      Console.WriteLine($"     .Normal   {mesh.cntNormal}");
      Console.WriteLine($"     .UV       {mesh.cntUV}");
      Console.WriteLine($"     .Prims    {mesh.cntPrim}");
      Console.WriteLine($"stat.maxDegree {mesh.maxDegree} (vertices)");

      /*foreach (var it in mesh.Materials) {
        Console.WriteLine($"material \"{it}\""); }
      foreach (var it in mesh.Groups) {
        Console.WriteLine($"group \"{it}\""); }*/
      _node = new GlMesh(_id, mesh); }
      else {
      throw new Exception("no mesh src");
      }}}


}  // close package namespace
}  // close enterprise namespace
