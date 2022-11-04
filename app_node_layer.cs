using System.Numerics;
using SharpDX.Direct3D11;

namespace rqdq {
namespace app {

class GlLayer : Node, ILayer {
  public struct Options {
    public Vector4? color; }
    
  private ICamera? _cameraNode;

  private IGl? _glNode;

  private IValueNode? _colorNode;
  private string? _colorSlot;
  private readonly Vector4 _color;
  
  public
  GlLayer(string id, Options? opt=null) : base(id) {
    _color = opt?.color ?? new(0.0F);}

  public override
  void Verify() {
    if (_cameraNode is null) { throw new Exception("no camera node"); }
    if (_glNode is null) { throw new Exception("no gl node"); }}

  public override
  void Connect(string attr, Node target, string slot) {
    if (attr == "color") {
      if (target is IValueNode node) {
        _colorNode = node;
        _colorSlot = slot; }
      else {
        throw new Exception("bad link"); }}
    else if (attr == "camera") {
      if (target is ICamera node) {
        _cameraNode = node; }
      else {
        throw new Exception("bad link"); }}
    else if (attr == "gl") {
      if (target is IGl node) {
        _glNode = node; }
      else {
        throw new Exception("bad link"); }}
    }

  public
  void Draw(DeviceContext dc) {
    var vmat = _cameraNode.GetViewMatrix();
    var pmat = _cameraNode.GetProjMatrix();
    _glNode.Draw(dc, vmat, pmat); }

  public
  Vector4 GetColor() {
    return _colorNode?.Eval(_colorSlot)?.AsFloat4() ?? _color; }}


public class GlLayerCompiler : NodeCompilerBase {
  static public void Install() {
    AnyCompiler.Register("layer", (INodeCompiler)new GlLayerCompiler()); }
  public override void CompileImpl() {
    Input("camera", required: true);
    Input("gl", required: true);
    Input("color", required: false);
    _node = new GlLayer(_id); }} 


}  // close package namespace
}  // close enterprise namespace
