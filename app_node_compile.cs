using System.Text.Json;

namespace rqdq {
namespace app {

delegate CompileResult CompileFunc(ReadOnlySpan<char> id, JsonElement data);


public
class CompileResult {
  public bool success = false;
  public Node? node = null;
  public readonly List<Node> deps = new(); }


abstract public class NodeCompilerBase {

  protected JsonElement _data;
  protected string _id;
  protected readonly List<NodeRef> _links = new();
  protected CompileResult _out = new();

  public
  CompileResult Compile(ReadOnlySpan<char> id, JsonElement data) {
    _id = id.ToString();
    _data = data;
    _links.Clear();
    _out.node = null;
    _out.deps.Clear();

    CompileImpl();
    _out.success = _out.node is not null;
    if (!_out.success) {
      throw new Exception("badness"); }
    return _out; }

  protected
  void Input(string attr, bool required) {
    if (_data.TryGetProperty(attr, out var attrData)) {
      if (attrData.ValueKind == JsonValueKind.String) {
        _links.Add(new NodeRef{ attr=attr, target=attrData.GetString() ?? ""});  // XXX better way to convert nullable to non-nullable?
        }
      else if (attrData.ValueKind == JsonValueKind.Object) {
        CompileResult result = AnyCompiler.Compile(attrData);
        if (result.success) {
          var subId = result.node._id;
          _links.Add(new NodeRef{ attr=attr, target=subId });
          _out.deps.Add(result.node);
          _out.deps.AddRange(result.deps); }
        else {
          throw new Exception($"unknown or malformed object at attr=\"{attr}\""); }}
      else {
        throw new Exception("node must be object or string"); }}
    else {
      if (required) {
        throw new Exception("required!"); }}}

  public abstract void CompileImpl(); }


static class ParseUtil {

  private static int _idSeq = 0;

  static public
  bool TryExtractCommand(JsonElement data, out string name, out string id, out JsonElement enclosed) {
    id = $"__auto{_idSeq++}__";
    name = "";
    enclosed = new JsonElement();
    foreach (var prop in data.EnumerateObject()) {
      if (prop.Name.StartsWith('$')) {
        name = prop.Name[1..];
        if (prop.Value.ValueKind == JsonValueKind.Object) {
          enclosed = prop.Value;
          if (prop.Value.TryGetProperty("id", out var idElem)) {
            if (idElem.ValueKind == JsonValueKind.String) {
              id = idElem.GetString(); }}
          return true; }}}
    return false; }}


static class AnyCompiler {
  private static
  Dictionary<string, CompileFunc> _db = new();

  public static
  void Register(string name, CompileFunc ff) {
    Console.WriteLine($"registered [{name}]");
    _db[name] = ff; }

  public static
  CompileResult Compile(JsonElement data) {
    if (ParseUtil.TryExtractCommand(data, out string name, out string id, out JsonElement payload)) {
      if (_db.TryGetValue(name, out var cf)) {
        var cr = cf(id, payload);
        return cr; }}
    return new(); } }


public static
class NodeLinker {

  static public
  void Link(List<Node> pgm) {
    Dictionary<string, int> byId = new();
    for (int i=0; i<pgm.Count; ++i) {
      if (byId.TryGetValue(pgm[i]._id, out _)) {
        throw new Exception($"node id \"{pgm[i]._id}\" not unique"); }
      byId[pgm[i]._id] = i; }
    foreach (var node in pgm) {
      foreach (var input in node._refs) {
        var parts = input.target.Split(':', 2);
        var depId = parts[0];
        var depSlot = parts.Length ==1 ? "default" : parts[1];
        if (byId.TryGetValue(depId, out int depIdx)) {
          node.Connect(input.attr, pgm[depIdx], depSlot); }
        else {
          throw new Exception($"node={node._id} attr={input.attr} dep={depId} not found"); }}}} }


}  // close package namespace
}  // close enterprise namespace
