using System.Numerics;
using rqdq.rclt;

namespace rqdq {
namespace app {

public interface IExprNode {
  double Eval(Dictionary<string, double> db); }


class ExprLiteral : IExprNode {
  private readonly double _value;
  public ExprLiteral(double value) { _value = value; }
  public double Eval(Dictionary<string, double> db) { return _value; }}


class ExprVariable : IExprNode {
  private readonly string _name;
  public ExprVariable(string name) { _name = name; }
  public double Eval(Dictionary<string, double> db) { return db[_name]; }}


class ExprFn1 : IExprNode {
  private readonly IExprNode _a;
  private readonly Func<double, double> _op;
  public ExprFn1(Func<double, double> op, IExprNode a) {
    _op = op; _a = a; }
  public double Eval(Dictionary<string, double> db) {
    return _op(_a.Eval(db)); }}


class ExprFn2 : IExprNode {
  private readonly IExprNode _a, _b;
  private readonly Func<double, double, double> _op;
  public ExprFn2(Func<double, double, double> op,
                 IExprNode a, IExprNode b) {
    _op = op; _a = a; _b = b; }
  public double Eval(Dictionary<string, double> db) {
    return _op(_a.Eval(db), _b.Eval(db)); }}


class ExprFn3 : IExprNode {
  private readonly IExprNode _a, _b, _c;
  private readonly Func<double, double, double, double> _op;
  public ExprFn3(Func<double, double, double, double> op,
                 IExprNode a, IExprNode b, IExprNode c) {
    _op = op; _a = a; _b = b; _c = c; }
  public double Eval(Dictionary<string, double> db) {
    return _op(_a.Eval(db), _b.Eval(db), _c.Eval(db)); }}


static
class ExprFnFactory {

  private static Dictionary<string, Func<double, double>> _fns1 = new() {
    { "sin", Math.Sin },
    { "cos", Math.Cos },
    { "tan", Math.Tan },
    { "sqrt", Math.Sqrt },
    { "exp", Math.Exp },
    { "floor", Math.Floor },
    { "ceil", Math.Ceiling },
    { "frac", a => a - Math.Floor(a) },
    { "abs", Math.Abs },
    { "sign", a => (double)Math.Sign(a) } };

  private static Dictionary<string, Func<double, double, double>> _fns2 = new() {
    { "min", Math.Min },
    { "max", Math.Max },
    { "pow", Math.Pow },
    { "+", (a, b) => a + b },
    { "-", (a, b) => a - b },
    { "*", (a, b) => a * b },
    { "/", (a, b) => a / b },
    { "%", (a, b) => a % b } };

  private static Dictionary<string, Func<double, double, double, double>> _fns3 = new() {
    { "clamp", (a, l, h) => Math.Min(Math.Max(a, l), h) },
    { "lerp", (a, b, t) => a*(1.0-t) + b*t } };

  public static
  IExprNode Make(string n, IExprNode? a = null, IExprNode? b = null, IExprNode? c = null) {
    if (_fns1.TryGetValue(n, out var fn1)) {
      return new ExprFn1(fn1, a); }
    if (_fns2.TryGetValue(n, out var fn2)) {
      return new ExprFn2(fn2, a, b); }
    if (_fns3.TryGetValue(n, out var fn3)) {
      return new ExprFn3(fn3, a, b, c); }
    throw new Exception($"unknown exprfn function \"{n}\""); }}


public static
class ExprCompiler {

  private static
  HashSet<string> _funcNames = new() {
    "sin", "cos",
    "pow", "exp", "sqrt",
    "abs", "sign", "floor", "ceil", "fract",
    "mod",
    "min", "max", "clamp", "mix" };

  public static
  IExprNode Compile(ReadOnlySpan<char> text) {
    Stack<IExprNode> outStack = new();
    Stack<Token> opStack = new();
    Stack<int> argsLenStack = new();

    var remainder = text;
    while (!remainder.IsEmpty) {
      var tok = Tokenizer.Pop(ref remainder);
      if (tok.kind == TokenKind.Space) {
        continue; }
      else if (tok.kind == TokenKind.Name) {
        if (_funcNames.Contains(tok.data)) {
          // function
          tok.kind = TokenKind.Function;
          argsLenStack.Push(1);
          opStack.Push(tok); }
        else {
          // var name
          tok.kind = TokenKind.Variable;
          outStack.Push(new ExprVariable(tok.data)); }}
      else if (tok.kind == TokenKind.Literal) {
        outStack.Push(new ExprLiteral(tok.num)); }
      else if (tok.kind == TokenKind.Separator) {
        argsLenStack.Push(argsLenStack.Pop() + 1);  // XXX modify top of stack?
        while (opStack.Count > 0 && opStack.Peek().kind != TokenKind.BeginParen) {
          var op = opStack.Pop();
          var r = outStack.Pop();
          var l = outStack.Pop();
          outStack.Push(ExprFnFactory.Make(op.data, l, r)); }}
      else if (tok.kind == TokenKind.Operator) {
        while (opStack.Count > 0 && opStack.Peek().kind == TokenKind.Operator
               && tok.Prec() <= opStack.Peek().Prec()) {
          var op = opStack.Pop();
          var r = outStack.Pop();
          var l = outStack.Pop();
          outStack.Push(ExprFnFactory.Make(op.data, l, r));  }
        opStack.Push(tok); }
      else if (tok.kind == TokenKind.BeginParen) {
        opStack.Push(tok); }
      else if (tok.kind == TokenKind.EndParen) {
        while (opStack.Count > 0 && opStack.Peek().kind != TokenKind.BeginParen) {
          var op = opStack.Pop();
          var r = outStack.Pop();
          var l = outStack.Pop();
          outStack.Push(ExprFnFactory.Make(op.data, l, r));  }
        opStack.Pop();  // XXX assert this should be left-paren
        if (opStack.Count > 0 && opStack.Peek().kind == TokenKind.Function) {
          var op = opStack.Pop();
          var many = argsLenStack.Pop();
          if (many > 3) {
            throw new Exception("too many args in exprnode function call"); }
          IExprNode? a1 = null;
          IExprNode? a2 = null;
          IExprNode? a3 = null;
          if (many >= 1) a1 = outStack.Pop();
          if (many >= 2) a2 = outStack.Pop();
          if (many >= 3) a3 = outStack.Pop();
          outStack.Push(ExprFnFactory.Make(op.data, a1, a2, a3)); }}}
    /*if (opStack.Count > 1) {
      throw new Exception($"opstack sizes is {opStack.Count} after parse"); }*/
    while (opStack.Count > 0) {
      var op = opStack.Pop();
      var l = outStack.Pop();
      var r = outStack.Pop();
      outStack.Push(ExprFnFactory.Make(op.data, l, r));  }

    var root = outStack.Pop();
      return root; }}


class ComputedNode : Node, IValueNode {

  struct VarLink {
    public string name;
    public IValueNode node;
    public string slot; }

  private IExprNode[] _exprs;
  private List<VarLink> _vars = new();
  public ComputedNode(string id, IExprNode[] exprs) : base(id) { _exprs = exprs; }

  public override
  void Connect(string attr, Node target, string slot) {
    if (target is IValueNode node) {
      VarLink tmp;
      tmp.name = attr;
      tmp.node = node;
      tmp.slot = slot;
      _vars.Add(tmp); }
    else {
      throw new Exception("bad link"); }}

  public IFlexValue Eval(string slot) {

    Dictionary<string, double> varDb = new();
    foreach (var it in _vars) {
      varDb[it.name] = it.node.Eval(it.slot).AsFloat(); }

    float[] vals = new float[_exprs.Length];
    for (int i=0; i<_exprs.Length; ++i) {
      vals[i] = (float)_exprs[i].Eval(varDb); }

         if (vals.Length == 1) { return new FlexFloat(vals[0]); }
    else if (vals.Length == 2) { return new FlexFloat2(new Vector2(vals[0], vals[1])); }
    else if (vals.Length == 3) { return new FlexFloat3(new Vector3(vals[0], vals[1], vals[2])); }
    else if (vals.Length == 4) { return new FlexFloat4(new Vector4(vals[0], vals[1], vals[2], vals[3])); }
    else {
      throw new Exception("too many vals in computed"); }}}


class ComputedNodeCompiler : NodeCompilerBase {
  private const int _maxElems = 4;
  public static void Install() {
    AnyCompiler.Register("computed", (id, elem) => new ComputedNodeCompiler().Compile(id, elem)); }

  public override void CompileImpl() {
    var exprText = _data.GetProperty("expr").GetString();
    var inputs = _data.GetProperty("inputs");
    foreach (var elem in inputs.EnumerateObject()) {
      var name = elem.Name;
      var source = elem.Value.GetString();
      _links.Add(new NodeLink(_id, name, source)); }

    var cnt = 0;
    foreach (var part in rqdq.rclt.NestedSplitter.Split(exprText)) ++cnt;
    var exprs = new IExprNode[cnt];
    cnt = 0;
    foreach (var (j, k) in rqdq.rclt.NestedSplitter.Split(exprText)) {
      if (cnt >= _maxElems) break;
      exprs[cnt++] = ExprCompiler.Compile(exprText.AsSpan()[j..k]); }

    _node = new ComputedNode(_id, exprs); }}


}  // close package namespace
}  // close enterprise namespace
