using System.CodeDom.Compiler;
using System.Collections.Generic;
using rqdq.rclt;

namespace rqdq {
namespace app {

public
class ExprNode {
  public Token _t;
  public ExprNode? _left;
  public ExprNode? _right;
  public List<ExprNode>? _args;

  public
  ExprNode(Token t, ExprNode? l = null, ExprNode? r = null, List<ExprNode>? a = null) {
    _t = t; _left = l; _right = r; _args = a; }

  public static void Indent(int n) { for (int i=0; i<n; ++i) Console.Write('.'); }

  public
  double Eval(int depth=0) {
    if (_t.kind == TokenKind.Literal) {
      Indent(depth);
      Console.WriteLine(_t.num);
      return _t.num; }
    else if (_t.kind == TokenKind.Variable) {
      Indent(depth); Console.WriteLine($"var {_t.data}");
      return 666.0; }
    else if (_t.kind == TokenKind.Operator) {
      var lv = _left.Eval(depth+1);
      Indent(depth);
      Console.WriteLine(_t.data);
      var rv = _right.Eval(depth+1);
      return lv + rv; }
    else if (_t.kind == TokenKind.Function) {
      Indent(depth); Console.WriteLine($"func {_t.data}");
      foreach (var arg in _args) {
        var av = arg.Eval(depth + 5); }
      Indent(depth); Console.WriteLine("endfunc");
      return 0.0; }
    else {
      throw new Exception($"unhandled eval kind \"{_t.kind.ToString()}\"");}}
    }


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
          outStack.Push(new ExprNode(tok, null, null)); }}
      else if (tok.kind == TokenKind.Literal) {
        outStack.Push(new ExprNode(tok, null, null)); }
      else if (tok.kind == TokenKind.Separator) {
        argsLenStack.Push(argsLenStack.Pop() + 1);  // XXX modify top of stack?
        while (opStack.Count > 0 && opStack.Peek().kind != TokenKind.BeginParen) {
          var op = opStack.Pop();
          var r = outStack.Pop();
          var l = outStack.Pop();
          outStack.Push(new ExprNode(op, l, r));  }}
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
          List<ExprNode> args = new();
          for (int i=0; i<many; ++i) {
            args.Add(outStack.Pop()); }
          outStack.Push(new ExprNode(op, null, null, args));  }}}
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
