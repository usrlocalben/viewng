using System.Diagnostics;
using System.IO.MemoryMappedFiles;

namespace rqdq.rglv {

class MtlLoader {

  static public
  (MtlDb db, long took, long sizeInBytes) Load(string path) {

    var dc = rcls.DirContext.FromFile(path);
    var db = new MtlDb(dc);
    var timer = Stopwatch.StartNew();

    long inputSizeInBytes;
    using (var mm = MemoryMappedFile.CreateFromFile(path, FileMode.Open)) {
      using (var vs = mm.CreateViewStream()) {
        inputSizeInBytes = vs.Length; }
      new MtlParser(db).Parse(mm); }

    return (db, timer.ElapsedMilliseconds, inputSizeInBytes); }}


}  // close package namespace
