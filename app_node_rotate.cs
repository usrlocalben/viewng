using System.Numerics;
using SharpDX.Direct3D11;

namespace rqdq {
namespace app {

class GlRotate : Node, IGl {
  private IGl? _glNode;
  private IValueNode? _valueNode;
  private string? _valueSlot;

  public
  GlRotate(string id, NodeRef[] refs) : base(id, refs) {}

  public override
  void Connect(string attr, Node target, string slot) {
    if (attr == "gl") {
      if (target is IGl node) {
        _glNode = node; }
      else {
        throw new Exception("bad link"); }}
    else if (attr == "amount") {
      if (target is IValueNode node) {
        _valueNode = node;
        _valueSlot = slot; }
      else {
        throw new Exception("bad link"); }}}
        
  public
  void Draw(DeviceContext dc, Matrix4x4 vmat, Matrix4x4 pmat) {
      Vector3 rot = _valueNode?.Eval(_valueSlot)?.AsFloat3() ?? new(0);
      var m = Matrix4x4.CreateRotationX(rot.X) *
              Matrix4x4.CreateRotationY(rot.Y) *
              Matrix4x4.CreateRotationZ(rot.Z);
      _glNode?.Draw(dc, m*vmat, pmat); } }


class GlRotateCompiler : NodeCompilerBase {
  static public void Install() {
    AnyCompiler.Register("rotate", new CompileFunc(new GlRotateCompiler().Compile)); }
  public override void CompileImpl() {
    Input("amount", false);
    Input("gl", true);
    _out.node = new GlRotate(_id, _links.ToArray()); }}


}  // close package namespace
}  // close enterprise namespace
