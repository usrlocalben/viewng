using System.Numerics;

namespace rqdq {
namespace app {

class MulNode : Node, IValueNode {

  public struct Options {
    public Vector4? a;
    public Vector4? b; }

  private IValueNode? _aNode;
  private IValueNode? _bNode;
  private string? _aSlot;
  private string? _bSlot;
  private readonly Vector4 _aValue;
  private readonly Vector4 _bValue;

  public
  MulNode(string id, NodeRef[] refs, Options? opt = null) : base(id, refs) {
    _aValue = opt?.a ?? new Vector4(1);
    _bValue = opt?.b ?? new Vector4(1); }

  public override
  void Connect(string attr, Node target, string slot) {
    if (attr == "a") {
      if (target is IValueNode node) {
        _aNode = node;
        _aSlot = slot; }
      else {
        throw new Exception("bad link"); }}
    else if (attr == "b") {
      if (target is IValueNode node) {
        _bNode = node;
        _bSlot = slot; }
      else {
        throw new Exception("bad link"); }}}

  public
  IFlexValue Eval(string slot) {
    Vector4 va = _aNode?.Eval(_aSlot)?.AsFloat4() ?? _aValue;
    Vector4 vb = _bNode?.Eval(_bSlot)?.AsFloat4() ?? _bValue;
    return new FlexFloat4(va * vb); } }


public class MulNodeCompiler : NodeCompilerBase {
  static public void Install() {
    AnyCompiler.Register("mul", new CompileFunc(new MulNodeCompiler().Compile)); }
  public override void CompileImpl() {
    Input("a", required: false);
    Input("b", required: false);
    _out.node = new MulNode(_id, _links.ToArray()); }}


class AddNode : Node, IValueNode {

  public struct Options {
    public Vector4? a;
    public Vector4? b; }

  private IValueNode? _aNode;
  private IValueNode? _bNode;
  private string? _aSlot;
  private string? _bSlot;
  private readonly Vector4 _aValue;
  private readonly Vector4 _bValue;

  public
  AddNode(string id, NodeRef[] refs, Options? opt = null) : base(id, refs) {
    _aValue = opt?.a ?? new Vector4(1);
    _bValue = opt?.b ?? new Vector4(1); }

  public override
  void Connect(string attr, Node target, string slot) {
    if (attr == "a") {
      if (target is IValueNode node) {
        _aNode = node;
        _aSlot = slot; }
      else {
        throw new Exception("bad link"); }}
    else if (attr == "b") {
      if (target is IValueNode node) {
        _bNode = node;
        _bSlot = slot; }
      else {
        throw new Exception("bad link"); }}}

  public
  IFlexValue Eval(string slot) {
    Vector4 va = _aNode?.Eval(_aSlot)?.AsFloat4() ?? _aValue;
    Vector4 vb = _bNode?.Eval(_bSlot)?.AsFloat4() ?? _bValue;
    return new FlexFloat4(va + vb); } }


public class AddNodeCompiler : NodeCompilerBase {
  static public void Install() {
    AnyCompiler.Register("add", new CompileFunc(new AddNodeCompiler().Compile)); }
  public override void CompileImpl() {
    Input("a", required: false);
    Input("b", required: false);
    _out.node = new AddNode(_id, _links.ToArray()); }}


}  // close package namespace
}  // close enterprise namespace
