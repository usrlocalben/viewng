using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace rqdq {
namespace app {

public enum TokenKind {
  Space,
  Name, Function, Variable,
  Literal,
  Operator,
  Separator,
  BeginParen,
  EndParen }


public
class ASTNode {
  public Token _t;
  public ASTNode? _left;
  public ASTNode? _right;
  public List<ASTNode>? _args;

  public
  ASTNode(Token t, ASTNode? l = null, ASTNode? r = null, List<ASTNode>? a = null) {
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


public struct Token {
  public TokenKind kind;
  public string data;
  public double num;

  public int Prec() {
    if (kind == TokenKind.Operator) {
      if (data == "-" || data == "+") return 1;
      if (data == "*" || data == "/") return 2; }
    return -1; }

  public Token(TokenKind k, string d) { kind = k; data = d; num=0; }
  public Token(TokenKind k, double n) { kind = k; data = ""; num = n; }
  }


public
class Tokenizer {

  public static
  char Peek(ReadOnlySpan<char> text, int i) {
    if (0 < i && i < text.Length) {
      return text[i]; }
    return '$'; }

  public static
  bool IsWordChar(char c) {
    return c=='_' || ('a'<=c&&c<='z') || ('A'<=c&&c<='Z') || ('0'<=c&&c<='9'); }

  public static
  bool IsDigit(char c) {
    return '0'<=c && c<='9'; }

  public static
  bool IsWordStart(char c) {
    return IsWordChar(c) && !IsDigit(c); }

  public static
  bool IsSpace(char a) {
    return a == ' ' || a == '\t' || a == '\r' || a == '\n'; }

  public static
  int FindFirstNotOf(ReadOnlySpan<char> text, Func<char, bool> pred, int s=0) {
    for (int j=s; j<text.Length; ++j) {
      if (!pred(text[j])) return j; }
    return -1; }

  public static
  Token PopOne(ref ReadOnlySpan<char> text) {
    if (text.Length == 0) {
      throw new Exception("attempt to pop token from empty text"); }
    var a = text[0];
    var b = Peek(text, 1);
    if (IsSpace(a)) {
      var pos = FindFirstNotOf(text, IsSpace, 1);
      ReadOnlySpan<char> tmp;
      if (pos == -1) {
        tmp = text;
        text = ReadOnlySpan<char>.Empty; }
      else {
        tmp = text[0..pos];
        text = text[pos..]; }
      return new Token{kind=TokenKind.Space, data=tmp.ToString()}; }
    else if (IsWordStart(a)) {
      var pos = FindFirstNotOf(text, IsWordChar, 1);
      ReadOnlySpan<char> tmp;
      if (pos == -1) {
        tmp = text;
        text = ReadOnlySpan<char>.Empty; }
      else {
        tmp = text[0..pos];
        text = text[pos..]; }
      return new Token{kind=TokenKind.Name, data=tmp.ToString()}; }
    else if (IsDigit(a) || a=='.') {
      var pos = FindFirstNotOf(text, ch => IsDigit(ch) || ch=='.', 1);
      ReadOnlySpan<char> tmp;
      if (pos == -1) {
        // number until end of text
        tmp = text;
        text = ReadOnlySpan<char>.Empty; }
      else {
        tmp = text[0..pos];
        text = text[pos..]; }
      if (double.TryParse(tmp, out var result)) {
        return new Token{kind=TokenKind.Literal, num=result}; }
      throw new Exception($"bad number \"{tmp}\""); }

    else if (a == ',') {
      text = text[1..];
      return new Token{kind=TokenKind.Separator, data=","}; }
    else if (a == '(') {
      text = text[1..];
      return new Token{kind=TokenKind.BeginParen, data="("}; }
    else if (a == ')') {
      text = text[1..];
      return new Token{kind=TokenKind.EndParen, data=")"}; }
    else if (a=='/' || a=='*' || a=='-' || a=='+') {
      var tmp = "" + a;
      text = text[1..];
      return new Token{kind=TokenKind.Operator, data=tmp}; }
    else {
      throw new Exception($"unhandled input \"{text[0..5]}\"");} }}




public static
class ExprTester {

  public static
  ASTNode Foo() {
    Stack<ASTNode> outStack = new();
    Stack<Token> opStack = new();
    Stack<int> argsLenStack = new();

    List<string> varNames = new();
    varNames.Add("baz");
    varNames.Add("T");

    string s = "456+789";
    var ss = s.AsSpan();
    while (!ss.IsEmpty) {
      var t = Tokenizer.PopOne(ref ss);
      Console.WriteLine($"<token kind=\"{t.kind.ToString()}\" data=\"{t.data}\" num=\"{t.num}\">");
      if (t.kind == TokenKind.Space) continue;
      else if (t.kind == TokenKind.Name) {
        if (varNames.Contains(t.data)) {
          // var name
          t.kind = TokenKind.Variable;
          outStack.Push(new ASTNode(t, null, null)); }
        else {
          // function
          t.kind = TokenKind.Function;
          argsLenStack.Push(1);
          opStack.Push(t); }}
      else if (t.kind == TokenKind.Literal) {
        outStack.Push(new ASTNode(t, null, null)); }
      else if (t.kind == TokenKind.Separator) {
        argsLenStack.Push(argsLenStack.Pop() + 1);  // XXX modify top of stack?
        while (opStack.Count > 0 && opStack.Peek().kind != TokenKind.BeginParen) {
          var op = opStack.Pop();
          var r = outStack.Pop();
          var l = outStack.Pop();
          outStack.Push(new ASTNode(op, l, r));  }}
      else if (t.kind == TokenKind.Operator) {
        while (opStack.Count > 0 && opStack.Peek().kind == TokenKind.Operator
               && t.Prec() <= opStack.Peek().Prec()) {
          var op = opStack.Pop();
          var r = outStack.Pop();
          var l = outStack.Pop();
          outStack.Push(new ASTNode(op, l, r));  }
        opStack.Push(t); }
      else if (t.kind == TokenKind.BeginParen) {
        opStack.Push(t); }
      else if (t.kind == TokenKind.EndParen) {
        while (opStack.Count > 0 && opStack.Peek().kind != TokenKind.BeginParen) {
          var op = opStack.Pop();
          var r = outStack.Pop();
          var l = outStack.Pop();
          outStack.Push(new ASTNode(op, l, r));  }
        opStack.Pop();  // XXX assert this should be left-paren
        if (opStack.Count > 0 && opStack.Peek().kind == TokenKind.Function) {
          var op = opStack.Pop();
          var many = argsLenStack.Pop();
          List<ASTNode> args = new();
          for (int i=0; i<many; ++i) {
            args.Add(outStack.Pop()); }
          outStack.Push(new ASTNode(op, null, null, args));  }}}
    if (opStack.Count > 1) {
      throw new Exception($"opstack sizes is {opStack.Count} after parse"); }
    while (opStack.Count > 0) {
      var op = opStack.Pop();
      var l = outStack.Pop();
      var r = outStack.Pop();
      outStack.Push(new ASTNode(op, l, r));  }

    var root = outStack.Pop();
    Console.WriteLine(root.Eval());
      return root; }}
      

}  // close package namespace
}  // close enterprise namespace
