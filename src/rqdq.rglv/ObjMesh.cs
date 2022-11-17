using System.Numerics;
using rqdq.rcls;
using rqdq.rmlv;

namespace rqdq.rglv {

public
class ObjMesh : IObjParserProgram {
  private DirContext _dir;

  private List<Float3> _vP = new();
  private List<Float3> _vN = new();
  private List<Float2> _vT = new();
  private List<int> _primP = new();
  private List<int> _primN = new();
  private List<int> _primT = new();
  private List<int> _primM = new();
  private List<int> _primG = new();

  private MtlDb? _mtl;

  private int _seqMat = 0;
  private Dictionary<string, int> _hashMat = new();
  private Dictionary<int, string> _unhashMat = new();
  private int _curMat = 0;

  private int _seqGroup = 0;
  private Dictionary<string, int> _hashGroup = new();
  private int _curGroup = 0;

  private List<int> _tmpP = new(); 
  private List<int> _tmpN = new(); 
  private List<int> _tmpT = new(); 

  public int cntPosition { get => _vP.Count(); }
  public int cntNormal   { get => _vN.Count(); }
  public int cntUV       { get => _vT.Count(); }
  public int cntPrim     { get => _primM.Count(); }
  public int maxDegree   { get => 3; }
  public IEnumerable<string> Materials { get => _hashMat.Keys; }
  public IEnumerable<string> Groups { get => _hashGroup.Keys; }

  public
  ObjMesh(DirContext d) {
    _dir = d; }

  public
  void UseMtl(string data) {
    var found = _hashMat.TryGetValue(data, out int id);
    if (!found) {
      _hashMat[data] = id = _seqMat++;
      _unhashMat[id] = data; }
    _curMat = id; }

  public void MtlLib(string fn) {
    (_, var path) = _dir.Resolve(fn);
    (_mtl, _, _) = MtlLoader.Load(path); }

  public void G(string data) {
    var found = _hashGroup.TryGetValue(data, out int id);
    if (!found) {
      _hashGroup[data] = id = _seqGroup++; }
    _curGroup = id; }

  public void P(float x, float y, float z) {
    _vP.Add(new Float3(x, y, z)); }
  public void N(float x, float y, float z) {
    _vN.Add(new Float3(x, y, z)); }
  public void T(float x, float y) {
    _vT.Add(new Float2(x, y)); }

  public void BeginFace() {
    _tmpP.Clear();
    _tmpN.Clear();
    _tmpT.Clear(); }

  public void BeginVertex() { }

  public void IndexP(int i) { 
    if (i < 0) i = _vP.Count() + i; else --i;
    _tmpP.Add(i); }
  public void IndexN(int i) {
    if (i < 0) i = _vN.Count() + i; else --i;
    _tmpN.Add(i); }
  public void IndexT(int i) {
    if (i < 0) i = _vT.Count() + i; else --i;
    _tmpT.Add(i); }

  public void EndVertex() { }

  public void EndFace() {
    var degree = _tmpP.Count();
    for (int j = 1; j < degree - 1; j++) {
      _primM.Add(_curMat);
      _primG.Add(_curGroup);
      _primP.Add(_tmpP[0]);
      if (_tmpN.Count() > 0) _primN.Add(_tmpN[0]);
      if (_tmpT.Count() > 0) _primT.Add(_tmpT[0]);
      for (int k = j; k < j + 2; k++) {
        _primP.Add(_tmpP[k]);
        if (_tmpN.Count() > 0) _primN.Add(_tmpN[k]);
        if (_tmpT.Count() > 0) _primT.Add(_tmpT[k]); } } }

  public void Error(int l, string msg) {
    Console.WriteLine($"parse error: line={l} msg={msg}"); }

  public void End() { }

  public Vector3[] MakeBuffer() {
    int nPrims = _primP.Count() / 3;
    Vector3[] arr = new Vector3[nPrims * 3 * 2];

    for (int pi=0; pi<nPrims; ++pi) {
      var mtl = _mtl.Find(_unhashMat[_primM[pi]]);
      for (int vi=0; vi<3; ++vi) {
        arr[pi*3*2 + vi*2 + 0] = _vP[_primP[pi*3 + (2-vi)]].ToVector3() * 0.025F;
        arr[pi*3*2 + vi*2 + 1] = mtl.kd.ToVector3(); }}
    return arr; } }


}  // close package namespace
