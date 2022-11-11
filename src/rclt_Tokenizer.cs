namespace rqdq {
namespace rclt {

public
enum TokenKind {
  Space,
  Name, Function, Variable,
  Literal,
  Operator,
  Unary,
  Separator,
  BeginParen,
  EndParen }


public
struct Token {
  public TokenKind kind;
  public string data;
  public double num;

  public Token(TokenKind k, string d) { kind = k; data = d; num=0; }
  public Token(TokenKind k, double n) { kind = k; data = ""; num = n; }

  public
  int Prec() {
    if (kind == TokenKind.Operator) {
      if (data == "-" || data == "+") return 1;
      if (data == "*" || data == "/") return 2; }
    return -1; }}


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
  Token Pop(ref ReadOnlySpan<char> text) {
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


}  // close package namespace
}  // close enterprise namespace
