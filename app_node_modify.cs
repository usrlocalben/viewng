using System.Numerics;
using SharpDX.Direct3D11;

namespace rqdq {
namespace app {

class GlModify : Node, IGl {
  private List<IGl> _glNode = new();
  private IValueNode? _rotateNode; private string? _rotateSlot;
  private IValueNode? _translateNode; private string? _translateSlot;
  private IValueNode? _scaleNode; private string? _scaleSlot;

  public
  GlModify(string id) : base(id) {}

  public override
  void Connect(string attr, Node target, string slot) {
    if (attr == "gl") {
      if (target is IGl node) {
        _glNode.Add(node); }
      else {
        throw new Exception("bad link"); }}
    else if (attr == "rotate") {
      if (target is IValueNode node) {
        _rotateNode = node;
        _rotateSlot = slot; }
      else {
        throw new Exception("bad link"); }}
    else if (attr == "translate") {
      if (target is IValueNode node) {
        _translateNode = node;
        _translateSlot = slot; }
      else {
        throw new Exception("bad link"); }}
    else if (attr == "scale") {
      if (target is IValueNode node) {
        _scaleNode = node;
        _scaleSlot = slot; }
      else {
        throw new Exception("bad link"); }} }
        
  public
  void Draw(DeviceContext dc, Matrix4x4 vmat, Matrix4x4 pmat) {
      var m = vmat;
      if (_rotateNode is not null) {
        Vector3 amt = _rotateNode.Eval(_rotateSlot)?.AsFloat3() ?? new(0);
        m = Matrix4x4.CreateRotationX(amt.X) *
            Matrix4x4.CreateRotationY(amt.Y) *
            Matrix4x4.CreateRotationZ(amt.Z) * m; }
      if (_translateNode is not null) {
        Vector3 amt = _translateNode.Eval(_translateSlot)?.AsFloat3() ?? new(0);
        m = Matrix4x4.CreateTranslation(amt) * m; }
      if (_scaleNode is not null) {
        Vector3 amt = _scaleNode.Eval(_scaleSlot)?.AsFloat3() ?? new(0);
        m = Matrix4x4.CreateScale(amt) * m; }
      foreach (var gl in _glNode) {
        gl.Draw(dc, m, pmat); } }}


class GlModifyCompiler : NodeCompilerBase {
  public static void Install() {
    AnyCompiler.Register("modify", (INodeCompiler)new GlModifyCompiler()); }
  public override void CompileImpl() {
    Input("rotate", false);
    Input("translate", false);
    Input("scale", false);
    InputMany("gl", true);
    _node = new GlModify(_id); }}


}  // close package namespace
}  // close enterprise namespace
