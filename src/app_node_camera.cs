using System.Numerics;

namespace rqdq {
namespace app {

class LookAt : Node, ICamera {
  private IValueNode? _positionNode;
  private string? _positionSlot;
  private IValueNode? _targetNode;
  private string? _targetSlot;
  private IValueNode? _aspectNode;
  private string? _aspectSlot;

  public LookAt(string id) : base(id) {}

  public override
  void Connect(string attr, Node target, string slot) {
    if (attr == "position") {
      if (target is IValueNode node) {
        _positionSlot = slot;
        _positionNode = node; }
      else {
        throw new Exception("bad link"); }}
    else if (attr == "target") {
      if (target is IValueNode node) {
        _targetSlot = slot;
        _targetNode = node; }
      else {
        throw new Exception("bad link"); }}
    else if (attr == "aspect") {
      if (target is IValueNode node) {
        _aspectSlot = slot;
        _aspectNode = node; }
      else {
        throw new Exception("bad link"); }}}
      
  public
  Matrix4x4 GetViewMatrix() {  
    var pos = _positionNode?.Eval(_positionSlot)?.AsFloat3() ?? new Vector3(0, 0, -5);
    var target = _targetNode?.Eval(_targetSlot)?.AsFloat3() ?? new Vector3(0, 0, 0);
    return Matrix4x4.CreateLookAt(pos, target, Vector3.UnitY); }

  public
  Matrix4x4 GetProjMatrix() {
    var ax = _aspectNode?.Eval(_aspectSlot)?.AsFloat2() ?? new Vector2(1, 1);
    var aspect = ax.X / ax.Y;
    return Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4.0F, aspect, 0.1F, 100.0F); } }


class LookAtCompiler : NodeCompilerBase {
  public static void Install() {
    AnyCompiler.Register("look", (id, elem) => new LookAtCompiler().Compile(id, elem)); }
  public override void CompileImpl() {
    Input("position", required: false);
    Input("target", required: false);
    Input("aspect", required: false);
    _node = new LookAt(_id); }}


}  // close package namespace
}  // close enterprise namespace
