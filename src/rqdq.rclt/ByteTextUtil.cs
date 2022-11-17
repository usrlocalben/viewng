using System.Buffers.Text;

namespace rqdq.rclt {

/// <summary>
/// utility methods for text-parsing over a span in-place (i.e.
/// mutating the passed span). facilitates writing parsers that
/// avoid copies or allocation.
/// </summary>
public static
class ByteTextUtil {

  const byte NL = (byte)'\n';  // new-line char as byte
  const byte CR = (byte)'\r';  // carrage-return as byte (DOS format)
  const byte SP = (byte)' ';   // space char as byte

  /// <summary>
  /// split a span at the first newline, and "pop" it by advancing
  /// the begin pos of the input span, and returning the line. if
  /// a carrage-return preceeds the newline, it is removed. if no
  /// new-line is found, then the entire input is returned and the
  /// input span is cleared (i.e. it is the last line)
  /// </summary>
  /// <param name="text">ascii text span to extract from</param>
  /// <returns>first line of text</returns>
  static public
  ReadOnlySpan<byte> PopLine(ref ReadOnlySpan<byte> text) {
    var pos = text.IndexOf(NL);
    ReadOnlySpan<byte> extract;
    if (pos == -1) {
      // not found, this is the last line
      extract = text;
      text = ReadOnlySpan<byte>.Empty; }
    else {
      var endPos = pos;
      if (pos > 0 && text[pos-1] == CR) {
        --endPos; /* DOS-format detected */ }
      extract = text[..endPos];
      text = text[(pos+1)..]; }
    return extract; } 

  /// <summary>
  /// extract the first space-delimited word/token from text
  /// </summary>
  /// <param name="text">text span to extract from</param>
  /// <returns>first word</returns>
  static public
  ReadOnlySpan<byte> PopWord(ref ReadOnlySpan<byte> text) {
    var pos = text.IndexOf(SP);
    ReadOnlySpan<byte> extract;
    if (pos == -1) {
      extract = text;
      text = ReadOnlySpan<byte>.Empty; }
    else {
      extract = text[..pos];
      text = text[(pos+1)..].TrimStart(SP); }
    return extract; }

  /// <summary>
  /// remove leading spaces from a span in-place
  /// </summary>
  /// <param name="text">span to remove from</param>
  static public
  void LTrim(ref ReadOnlySpan<byte> text) {
    text = text.TrimStart(SP); }

  /// <summary>
  /// try to remove a prefix from a span
  /// </summary>
  /// <param name="text">span to compare and remove from</param>
  /// <param name="prefix">prefix as ascii bytes</param>
  /// <returns>false unless the prefix was matched and removed</returns>
  static public
  bool ConsumePrefix(ref ReadOnlySpan<byte> text, byte[] prefix) {
    if (text.StartsWith(prefix)) {
      text = text[prefix.Length..];
      return true; }
    return false; }

  static public
  bool ConsumeValue(ref ReadOnlySpan<byte> text, out float value) {
    bool good = Utf8Parser.TryParse(text, out value, out int taken);
    if (good) {
      text = text[taken..]; }
    return good; }

  static public
  bool ConsumeValue(ref ReadOnlySpan<byte> text, out int value) {
    bool good = Utf8Parser.TryParse(text, out value, out int taken);
    if (good) {
      text = text[taken..]; }
    return good; }

  /// <summary>
  /// consume a single character from a span if possible
  /// </summary>
  /// <param name="text"></param>
  /// <returns>false unless a character was removed</returns>
  static public
  bool ConsumeChar(ref ReadOnlySpan<byte> text) {
    if (text.IsEmpty) return false;
    text = text[1..];
    return true; }}


}  // close package namespace
