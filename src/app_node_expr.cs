using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using rqdq.rclt;

namespace rqdq {
namespace app {

public abstract
class ExprNode {
  protected ExprNode() {}
  // public static void Indent(int n) { for (int i=0; i<n; ++i) Console.Write('.'); }
  public abstract double Eval(Dictionary<string, double> varDb); }

public class ExprLiteral : ExprNode {
  private readonly double _value;
  public ExprLiteral(double value) : base() { _value = value; }
  public override double Eval(Dictionary<string, double> varDb) { return _value; }}

public class ExprVariable : ExprNode {
  private readonly string _name;
  public ExprVariable(string name) : base() { _name = name; }
  public override double Eval(Dictionary<string, double> varDb) { return varDb[_name]; }}


abstract
class ExprFn1 : ExprNode {
  public ExprNode? _a;
  public ExprFn1(ExprNode? a) : base() { _a = a; }
  public override double Eval(Dictionary<string, double> varDb) { var a = _a.Eval(varDb); return Fn(a); }
  public abstract double Fn(double x); }

abstract
class ExprFn2 : ExprNode {
  public ExprNode? _a;
  public ExprNode? _b;
  public ExprFn2(ExprNode? a, ExprNode? b) : base() { _a = a; _b = b; }
  public override double Eval(Dictionary<string, double> varDb) { var a = _a.Eval(varDb); var b = _b.Eval(varDb); return Fn(a, b); }
  public abstract double Fn(double x, double b); }

abstract
class ExprFn3 : ExprNode {
  public ExprNode? _a;
  public ExprNode? _b;
  public ExprNode? _c;
  public ExprFn3(ExprNode? a, ExprNode? b, ExprNode? c) : base() { _a = a; _b = b; _c = c; }
  public override double Eval(Dictionary<string, double> varDb) { var a = _a.Eval(varDb); var b = _b.Eval(varDb); var c = _c.Eval(varDb); return Fn(a, b, c); }
  public abstract double Fn(double x, double b, double c); }


class ExprFnSin : ExprFn1 { public ExprFnSin(ExprNode? a) : base(a) {} public override double Fn(double x) { return Math.Sin(x); }}
class ExprFnCos : ExprFn1 { public ExprFnCos(ExprNode? a) : base(a) {} public override double Fn(double x) { return Math.Cos(x); }}
class ExprFnTan : ExprFn1 { public ExprFnTan(ExprNode? a) : base(a) {} public override double Fn(double x) { return Math.Tan(x); }}
class ExprFnSqrt : ExprFn1 { public ExprFnSqrt(ExprNode? a) : base(a) {} public override double Fn(double x) { return Math.Sqrt(x); }}
class ExprFnFloor : ExprFn1 { public ExprFnFloor(ExprNode? a) : base(a) {} public override double Fn(double x) { return Math.Floor(x); }}
class ExprFnCeil : ExprFn1 { public ExprFnCeil(ExprNode? a) : base(a) {} public override double Fn(double x) { return Math.Ceiling(x); }}
class ExprFnAbs : ExprFn1 { public ExprFnAbs(ExprNode? a) : base(a) {} public override double Fn(double x) { return Math.Abs(x); }}
class ExprFnSign : ExprFn1 { public ExprFnSign(ExprNode? a) : base(a) {} public override double Fn(double x) { return Math.Sign(x); }}
class ExprFnExp : ExprFn1 { public ExprFnExp(ExprNode? a) : base(a) {} public override double Fn(double x) { return Math.Exp(x); }}
class ExprFnFract : ExprFn1 { public ExprFnFract(ExprNode? a) : base(a) {} public override double Fn(double x) { return x - Math.Floor(x); }}

class ExprFnMin : ExprFn2 { public ExprFnMin(ExprNode? a, ExprNode? b) : base(a, b) {} public override double Fn(double x, double y) { return Math.Min(x, y); }}
class ExprFnMax : ExprFn2 { public ExprFnMax(ExprNode? a, ExprNode? b) : base(a, b) {} public override double Fn(double x, double y) { return Math.Max(x, y); }}
class ExprFnPow : ExprFn2 { public ExprFnPow(ExprNode? a, ExprNode? b) : base(a, b) {} public override double Fn(double x, double y) { return Math.Pow(x, y); }}
class ExprFnAdd : ExprFn2 { public ExprFnAdd(ExprNode? a, ExprNode? b) : base(a, b) {} public override double Fn(double x, double y) { return x + y; }}
class ExprFnSub : ExprFn2 { public ExprFnSub(ExprNode? a, ExprNode? b) : base(a, b) {} public override double Fn(double x, double y) { return x - y; }}
class ExprFnMul : ExprFn2 { public ExprFnMul(ExprNode? a, ExprNode? b) : base(a, b) {} public override double Fn(double x, double y) { return x * y; }}
class ExprFnDiv : ExprFn2 { public ExprFnDiv(ExprNode? a, ExprNode? b) : base(a, b) {} public override double Fn(double x, double y) { return x / y; }}
class ExprFnMod : ExprFn2 { public ExprFnMod(ExprNode? a, ExprNode? b) : base(a, b) {} public override double Fn(double x, double y) { return x % y; }}

class ExprFnClamp : ExprFn3 { public ExprFnClamp(ExprNode? a, ExprNode? b, ExprNode? c) : base(a, b, c) {} public override double Fn(double x, double a, double b) { return Math.Min(Math.Max(x, a), b); }}
class ExprFnLerp : ExprFn3 { public ExprFnLerp(ExprNode? a, ExprNode? b, ExprNode? c) : base(a, b, c) {} public override double Fn(double a, double b, double t) { return a*(1.0-t) + b*t; }}

static
class ExprFnFactory {
  public static
  ExprNode Make(string n, ExprNode? a = null, ExprNode? b = null, ExprNode? c = null) {
    if (n == "sin") { return new ExprFnSin(a); }
    if (n == "cos") { return new ExprFnCos(a); }
    if (n == "tan") { return new ExprFnTan(a); }
    if (n == "sqrt") { return new ExprFnSqrt(a); }
    if (n == "floor") { return new ExprFnFloor(a); }
    if (n == "ceil") { return new ExprFnCeil(a); }
    if (n == "abs") { return new ExprFnAbs(a); }
    if (n == "sign") { return new ExprFnSign(a); }
    if (n == "exp") { return new ExprFnExp(a); }
    if (n=="fract" || n=="frac") { return new ExprFnFract(a); }

    if (n=="min") { return new ExprFnMin(a, b); }
    if (n=="max") { return new ExprFnMax(a, b); }
    if (n=="pow") { return new ExprFnPow(a, b); }
    if (n=="+") { return new ExprFnAdd(a, b); }
    if (n=="-") { return new ExprFnSub(a, b); }
    if (n=="*") { return new ExprFnMul(a, b); }
    if (n=="/") { return new ExprFnDiv(a, b); }
    if (n=="%") { return new ExprFnMod(a, b); }

    if (n=="clamp") { return new ExprFnClamp(a, b, c); }
    if (n=="lerp" || n=="mix") { return new ExprFnLerp(a, b, c); }
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
  ExprNode Compile(ReadOnlySpan<char> text) {
    Stack<ExprNode> outStack = new();
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
          outStack.Push(ExprFnFactory(op.data, l, r)); }}
      else if (tok.kind == TokenKind.Operator) {
        while (opStack.Count > 0 && opStack.Peek().kind == TokenKind.Operator
               && tok.Prec() <= opStack.Peek().Prec()) {
          var op = opStack.Pop();
          var r = outStack.Pop();
          var l = outStack.Pop();
          outStack.Push(new ExprNode(op, l, r));  }
        opStack.Push(tok); }
      else if (tok.kind == TokenKind.BeginParen) {
        opStack.Push(tok); }
      else if (tok.kind == TokenKind.EndParen) {
        while (opStack.Count > 0 && opStack.Peek().kind != TokenKind.BeginParen) {
          var op = opStack.Pop();
          var r = outStack.Pop();
          var l = outStack.Pop();
          outStack.Push(new ExprNode(op, l, r));  }
        opStack.Pop();  // XXX assert this should be left-paren
        if (opStack.Count > 0 && opStack.Peek().kind == TokenKind.Function) {
          var op = opStack.Pop();
          var many = argsLenStack.Pop();
          if (many > 3) {
            throw new Exception("too many args in exprnode function call"); }
          ExprNode? a1 = null;
          ExprNode? a2 = null;
          ExprNode? a3 = null;
          if (many >= 1) a1 = outStack.Pop();
          if (many >= 2) a2 = outStack.Pop();
          if (many >= 3) a3 = outStack.Pop();
          outStack.Push(ExprFnFactory.Make(op, a1, a2, a3)); }}}
    if (opStack.Count > 1) {
      throw new Exception($"opstack sizes is {opStack.Count} after parse"); }
    while (opStack.Count > 0) {
      var op = opStack.Pop();
      var l = outStack.Pop();
      var r = outStack.Pop();
      outStack.Push(new ExprNode(op, l, r));  }

    var root = outStack.Pop();
    Console.WriteLine(root.Eval());
      return root; }}
      

}  // close package namespace
}  // close enterprise namespace
