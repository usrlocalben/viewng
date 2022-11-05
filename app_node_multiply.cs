using System.Numerics;
using SharpDX.Direct3D11;

namespace rqdq {
namespace app {

class GlMultiply : Node, IGl {
  private IGl? _glNode;
  private IValueNode? _manyNode;
  private string? _manySlot;
  private IValueNode? _translateNode;
  private string? _translateSlot;
  private IValueNode? _rotateNode;
  private string? _rotateSlot;

  public
  GlMultiply(string id) : base(id) {}

  public override
  void Connect(string attr, Node target, string slot) {
    if (attr == "gl") {
      if (target is IGl node) {
        _glNode = node; }
      else {
        throw new Exception("bad link"); }}
    else if (attr == "translate") { if (target is IValueNode node) { _translateNode = node; _translateSlot = slot; } else { throw new Exception("bad link"); }}
    else if (attr == "rotate") { if (target is IValueNode node) { _rotateNode = node; _rotateSlot = slot; } else { throw new Exception("bad link"); }}
    else if (attr == "many") { if (target is IValueNode node) { _manyNode = node; _manySlot = slot; } else { throw new Exception("bad link"); }}
    }
        
  public
  void Draw(DeviceContext dc, Matrix4x4 vmat, Matrix4x4 pmat) {
    int many = (int)(_manyNode?.Eval(_manySlot)?.AsFloat() ?? 1);
    Vector3 rotateAmt = _rotateNode?.Eval(_rotateSlot)?.AsFloat3() ?? new Vector3(0);
    Vector3 translateAmt = _translateNode?.Eval(_translateSlot)?.AsFloat3() ?? new Vector3(0);

    Vector3 rotate = new Vector3(0);
    Vector3 translate = new Vector3(0);
    for (int i=0; i<many; ++i, rotate+=rotateAmt, translate+=translateAmt) {
      var m = Matrix4x4.CreateRotationX(rotate.X) *
              Matrix4x4.CreateRotationY(rotate.Y) *
              Matrix4x4.CreateRotationZ(rotate.Z) *
              Matrix4x4.CreateTranslation(translate);
      _glNode?.Draw(dc, m*vmat, pmat); }}}


class GlMultiplyCompiler : NodeCompilerBase {
  public static void Install() {
    AnyCompiler.Register("multiply", (INodeCompiler)new GlMultiplyCompiler()); }
  public override void CompileImpl() {
    Input("many", false);
    Input("rotate", false);
    Input("translate", false);
    Input("gl", true);
    _node = new GlMultiply(_id); }}


}  // close package namespace
}  // close enterprise namespace
