using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using rqdq.rcls;

namespace rqdq {
namespace rglv {

class ObjLoader {

  static public
  (ObjMesh mesh, long took, long sizeInBytes) Load(string path) {

    var dirContext = DirContext.FromFile(path);
    var mesh = new ObjMesh(dirContext);
    var timer = Stopwatch.StartNew();

    long inputSizeInBytes;
    using (var mm = MemoryMappedFile.CreateFromFile(path, FileMode.Open)) {
      using (var vs = mm.CreateViewStream()) {
        inputSizeInBytes = vs.Length; }
      new ObjParser(mesh).Parse(mm); }

    return (mesh, timer.ElapsedMilliseconds, inputSizeInBytes); }}


}  // close package namespace
}  // close enterprise namespace
