using System.Buffers.Text;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using BTU = rqdq.rclt.ByteTextUtil;

namespace rqdq {
namespace rglv {

abstract internal class ParserBase {
  private static readonly byte[] CR = Encoding.UTF8.GetBytes("\r");
  private static readonly byte HASH = Encoding.UTF8.GetBytes("#")[0];

  unsafe public static
  string Decode(in ReadOnlySpan<byte> text) {
    fixed (byte* begin = &MemoryMarshal.GetReference(text)) {
      var lengthInCodepoints = Encoding.UTF8.GetCharCount(begin, text.Length);
      fixed (char* buf = stackalloc char[lengthInCodepoints]) {
        var lengthInChars = Encoding.UTF8.GetChars(begin, text.Length, buf, lengthInCodepoints);
        return new string(buf, 0, lengthInChars); } } }

  public abstract
  void Parse(ref ReadOnlySpan<byte> data);

  public
  void Parse(MemoryMappedFile mm) {
    using (var vs = mm.CreateViewStream()) {
      using (var mmv = vs.SafeMemoryMappedViewHandle) {
        ReadOnlySpan<byte> bytes;
        unsafe {
          byte* ptrMemMap = (byte*)0;
          mmv.AcquirePointer(ref ptrMemMap);
          bytes = new ReadOnlySpan<byte>(ptrMemMap, (int)mmv.ByteLength);
          Parse(ref bytes);
          mmv.ReleasePointer(); } } } }

  internal static
  bool ConsumeFloat3(ref ReadOnlySpan<byte> text, out float x, out float y, out float z) {
    if (BTU.ConsumeValue(ref text, out x)) {
      BTU.LTrim(ref text);
      if (BTU.ConsumeValue(ref text, out y)) {
        BTU.LTrim(ref text);
        if (BTU.ConsumeValue(ref text, out z)) {
          return true; }}}
    x = y = z = 0.0F;
    return false; }

  internal static
  bool ConsumeFloat2(ref ReadOnlySpan<byte> text, out float x, out float y) {
    if (BTU.ConsumeValue(ref text, out x)) {
      BTU.LTrim(ref text);
      if (BTU.ConsumeValue(ref text, out y)) {
        return true; }}
    x = y = 0.0F;
    return false; }

  internal static
  void RemoveComment(ref ReadOnlySpan<byte> text) {
    var pos = text.IndexOf(HASH);
    if (pos != -1) {
      text = text[0..pos]; } } }

class ObjParser : ParserBase {

  private readonly IObjParserProgram _pgm;
  private readonly byte[] prefixMtllib = Encoding.UTF8.GetBytes("mtllib ");
  private readonly byte[] prefixUsemtl = Encoding.UTF8.GetBytes("usemtl ");
  private readonly byte[] prefixG = Encoding.UTF8.GetBytes("g ");
  private readonly byte[] prefixV = Encoding.UTF8.GetBytes("v ");
  private readonly byte[] prefixVT = Encoding.UTF8.GetBytes("vt ");
  private readonly byte[] prefixVN = Encoding.UTF8.GetBytes("vn ");
  private readonly byte[] prefixF = Encoding.UTF8.GetBytes("f ");

  public ObjParser(IObjParserProgram p) {
    _pgm = p; }

  override public
  void Parse(ref ReadOnlySpan<byte> text) {
    int lineNum = -1;
    while (!text.IsEmpty) {
      ++lineNum;
      var data = BTU.PopLine(ref text);
      if (!data.IsEmpty && data[0] != 0) {
        ParseLine(lineNum, data); }}
    _pgm.End(); }

  void ParseLine(int lineNum, ReadOnlySpan<byte> data) {
    RemoveComment(ref data);
    BTU.LTrim(ref data);
    if (data.IsEmpty) return;
    if (BTU.ConsumePrefix(ref data, prefixF)) {
      // "f n n n"
      // "f n/n n/n n/n"
      // "f n//n n//n n//n"
      // "f n/n/n n/n/n n/n/n"
      BTU.LTrim(ref data);
      _pgm.BeginFace();
      while (!data.IsEmpty) {
        var word = BTU.PopWord(ref data);
        int colNum = 0;
        while (!word.IsEmpty) {
          if (Utf8Parser.TryParse(word, out int idx, out int taken)) {
            word = word[taken..];
            switch (colNum) {
            case 0: 
              _pgm.BeginVertex();
              _pgm.IndexP(idx); break;
            case 1: _pgm.IndexT(idx); break;
            case 2: _pgm.IndexN(idx); break; } }
          BTU.ConsumeChar(ref word);
          // XXX if (!word.IsEmpty) word = word[1..];
          ++colNum; }
        _pgm.EndVertex(); }
      _pgm.EndFace(); }
    else if (BTU.ConsumePrefix(ref data, prefixV)) {
      BTU.LTrim(ref data);
      if (ConsumeFloat3(ref data, out float x, out float y, out float z)) {
        _pgm.P(x, y, z); }
      else {
        _pgm.Error(lineNum, "bad float3 in position");
      return; } }
    else if (BTU.ConsumePrefix(ref data, prefixVN)) {
      BTU.LTrim(ref data);
      if (ConsumeFloat3(ref data, out float x, out float y, out float z)) {
        _pgm.N(x, y, z); }
      else {
        _pgm.Error(lineNum, "bad float3 in normal");
        return; } }
    else if (BTU.ConsumePrefix(ref data, prefixVT)) {
      BTU.LTrim(ref data);
      if (ConsumeFloat2(ref data, out float x, out float y)) {
        _pgm.T(x, y); }
      else {
        _pgm.Error(lineNum, "bad float2 in texture");
        return; } }
    else if (BTU.ConsumePrefix(ref data, prefixMtllib)) {
      BTU.LTrim(ref data);  // XXX need rtrim
      _pgm.MtlLib(Decode(in data)); }
    else if (BTU.ConsumePrefix(ref data, prefixUsemtl)) {
      BTU.LTrim(ref data);  // XXX need rtrim
      _pgm.UseMtl(Decode(in data)); }
    else if (BTU.ConsumePrefix(ref data, prefixG)) {
      BTU.LTrim(ref data);  // XXX need rtrim
      _pgm.G(Decode(in data)); }
    else {
      _pgm.Error(lineNum, $"unknown command {Decode(in data)}"); } }}


class MtlParser : ParserBase {

  private readonly IMtlParserProgram _pgm;
  private readonly byte[] prefixNewMtl = Encoding.UTF8.GetBytes("newmtl ");
  private readonly byte[] prefixMapKd = Encoding.UTF8.GetBytes("map_Kd ");
  private readonly byte[] prefixKd = Encoding.UTF8.GetBytes("Kd ");
  private readonly byte[] prefixKa = Encoding.UTF8.GetBytes("Ka ");
  private readonly byte[] prefixKs = Encoding.UTF8.GetBytes("Ks ");
  private readonly byte[] prefixNs = Encoding.UTF8.GetBytes("Ns ");
  private readonly byte[] prefixD = Encoding.UTF8.GetBytes("d ");
  private readonly byte[] prefixTr = Encoding.UTF8.GetBytes("Tr ");
  private readonly byte[] prefixIllum = Encoding.UTF8.GetBytes("illum ");

  public MtlParser(IMtlParserProgram p) {
    _pgm = p; }

  public override
  void Parse(ref ReadOnlySpan<byte> text) {
    int lineNum = -1;
    while (!text.IsEmpty) {
      ++lineNum;
      var data = BTU.PopLine(ref text);
      RemoveComment(ref data);
      BTU.LTrim(ref data);
      if (data.IsEmpty) continue;
      if (data[0] == 0) break;
      if (BTU.ConsumePrefix(ref data, prefixNewMtl)) {
        BTU.LTrim(ref data);
        _pgm.NewMtl(Decode(data)); }
      else if (BTU.ConsumePrefix(ref data, prefixMapKd)) {
        BTU.LTrim(ref data);
        _pgm.MapKd(Decode(data)); }
      else if (BTU.ConsumePrefix(ref data, prefixKa)) {
        BTU.LTrim(ref data);
        if (ConsumeFloat3(ref data, out float x, out float y, out float z)) {
          _pgm.Ka(x, y, z); }
        else {
          _pgm.Error(lineNum, "bad float3 in Ka");
          return; } }
      else if (BTU.ConsumePrefix(ref data, prefixKd)) {
        BTU.LTrim(ref data);
        if (ConsumeFloat3(ref data, out float x, out float y, out float z)) {
          _pgm.Kd(x, y, z); }
        else {
          _pgm.Error(lineNum, "bad float3 in Kd");
          return; } }
      else if (BTU.ConsumePrefix(ref data, prefixKs)) {
        BTU.LTrim(ref data);
        if (ConsumeFloat3(ref data, out float x, out float y, out float z)) {
          _pgm.Ks(x, y, z); }
        else {
          _pgm.Error(lineNum, "bad float3 in Ks");
          return; } }
      else if (BTU.ConsumePrefix(ref data, prefixNs)) {
        BTU.LTrim(ref data);
        bool good = BTU.ConsumeValue(ref data, out float value);
        if (good) {
          _pgm.Ns(value); }
        else {
          _pgm.Error(lineNum, "bad float in Ns");
          return; } }
      else if (BTU.ConsumePrefix(ref data, prefixD)) {
        BTU.LTrim(ref data);
        bool good = BTU.ConsumeValue(ref data, out float value);
        if (good) {
          _pgm.D(value); }
        else {
          _pgm.Error(lineNum, "bad float in D");
          return; } }
      else if (BTU.ConsumePrefix(ref data, prefixTr)) {
        BTU.LTrim(ref data);
        bool good = BTU.ConsumeValue(ref data, out float value);
        if (good) {
          _pgm.D(1.0F - value); }
        else {
          _pgm.Error(lineNum, "bad float in Tr");
          return; } }
      else if (BTU.ConsumePrefix(ref data, prefixIllum)) {
        BTU.LTrim(ref data);
        bool good = BTU.ConsumeValue(ref data, out int value);
        if (good) {
          _pgm.Illum(value); }
        else {
          _pgm.Error(lineNum, "bad int in illum");
          return; } }
      else {
        _pgm.Error(lineNum, $"unknown command {Decode(in data)}");
        return; } }
    _pgm.End(); } }


}  // close package namespace
}  // close enterprise namespace
