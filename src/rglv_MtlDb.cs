using rqdq.rglv;
using rqdq.rmlv;
using rqdq.rcls;

namespace rqdq {
namespace rglv {

struct Mtl {
  public string? name;
  public Float3 ka;
  public Float3 kd;
  public Float3 ks;
  public float ns;
  public float d;
  public string? mapKd;
  public int mode;

  public void Reset() {
    this.name = null;
    this.ka = new Float3(0.2F);
    this.kd = new Float3(0.8F);
    this.ks = new Float3(1.0F);
    this.ns = 1.0F;
    this.d = 1.0F;
    this.mapKd = null;
    this.mode = 1; }}


class MtlDb : IMtlParserProgram {

  private DirContext _dc;
  // XXX private int _seq;
  private List<Mtl> _db = new();
  private Dictionary<string, int> _byName = new();

  private Mtl _cur;

  public
  MtlDb(DirContext dc) {
    _dc = dc;
    // XXX _seq = 0;
    _cur = new Mtl();
    _cur.Reset(); } 

  public
  string Resolve(string fn) {
    (_, var resolved) = _dc.Resolve(fn);
    return resolved; }

  public
  Mtl Find(string name) {
    return _db[_byName[name]]; }

  private
  void MaybePushAndReset() {
    if (_cur.name != null) {
      var id = _db.Count;
      _db.Add(_cur);
      _byName[_cur.name] = id;
      Console.WriteLine($"added material [{_cur.name}] -> {id}");
      _cur.Reset(); }}

  public
  void NewMtl(string data) {
    MaybePushAndReset();
    _cur.name = data; }

  public void MapKd(string data) { _cur.mapKd = data; } 
  public void Kd(float r, float g, float b) { _cur.kd = new Float3(r, g, b); }
  public void Ka(float r, float g, float b) { _cur.ka = new Float3(r, g, b); }
  public void Ks(float r, float g, float b) { _cur.ks = new Float3(r, g, b); }
  public void Ns(float e) { _cur.ns = e; }
  public void D(float d) { _cur.d = d; }
  public void Illum(int mode) { _cur.mode = mode; }
  public void Error(int lineNum, string msg) {
    Console.WriteLine($"mtl error in line {lineNum}: {msg}"); }
  public void End() {
    Console.WriteLine("MTL EOF!");
    MaybePushAndReset(); }}
  

}  // close package namespace
}  // close enterprise namespace
