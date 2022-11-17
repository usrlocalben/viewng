using System.IO;

namespace rqdq.rcls {

public
class DirContext {
  private string _dir;

  public DirContext(string d) {
    _dir = d; }

  public static
  DirContext FromFile(string f) {
    var dir = Path.GetDirectoryName(Path.GetFullPath(f));
    if (dir == null) {
      throw new Exception($"failed to create DirContext from \"{f}\""); }
    return new DirContext(dir); }

  public
  (DirContext, string) Resolve(string fn) {
    if (Path.IsPathRooted(fn)) {
      return (DirContext.FromFile(fn), fn); }
    else {
      var tmp = Path.Join(_dir, fn);
      return (DirContext.FromFile(tmp), tmp); }}}
      

}  // close package namespace
