using System.Text.Json;
using System.Windows.Forms.VisualStyles;

namespace rqdq {
namespace app {

interface INodeCompiler {
  CompileResult Compile(ReadOnlySpan<char> id, JsonElement data); }


public
class CompileResult {
  public Node? root = null;
  public readonly List<Node> nodes = new();
  public readonly List<NodeLink> links = new(); }


public abstract
class NodeCompilerBase : INodeCompiler {

  protected JsonElement _data;
  protected string _id;
  protected readonly List<NodeLink> _links = new();
  protected Node? _node = null;
  protected readonly CompileResult _out = new();

  public
  CompileResult Compile(ReadOnlySpan<char> id, JsonElement data) {
    _id = id.ToString();
    _data = data;
    _links.Clear();
    _node = null;
    _out.root = null;
    _out.links.Clear();
    _out.nodes.Clear();

    CompileImpl();
    if (_node is null) {
      throw new Exception("badness"); }
    _out.root = _node;
    _out.nodes.Add(_node);
    _out.links.AddRange(_links);
    return _out; }

  protected
  void Input(string attr, bool required) {
    if (_data.TryGetProperty(attr, out var attrData)) {
      if (attrData.ValueKind == JsonValueKind.String) {
        _links.Add(new NodeLink(_id, attr, attrData.GetString() ?? ""));  // XXX better way to convert nullable to non-nullable?
        }
      else if (attrData.ValueKind == JsonValueKind.Object) {
        CompileResult result = AnyCompiler.Compile(attrData);
        if (result.root is not null) {
          var subId = result.root.Id;
          _links.Add(new NodeLink(_id, attr, subId));
          _out.nodes.AddRange(result.nodes);
          _out.links.AddRange(result.links); }
        else {
          throw new Exception($"unknown or malformed object at attr=\"{attr}\""); }}
      else {
        throw new Exception("node must be object or string"); }}
    else {
      if (required) {
        throw new Exception("required!"); }}}

  public abstract
  void CompileImpl(); }


internal static
class ParseUtil {

  private static int _idSeq = 0;

  public static
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


internal static
class AnyCompiler {
  private static
  Dictionary<string, INodeCompiler> _db = new();

  public static
  void Register(string name, INodeCompiler nc) {
    Console.WriteLine($"registered [{name}]");
    _db[name] = nc; }

  public static
  CompileResult Compile(JsonElement data) {
    if (ParseUtil.TryExtractCommand(data, out string name, out string id, out JsonElement payload)) {
      if (_db.TryGetValue(name, out var nc)) {
        return nc.Compile(id, payload); }}
    return new(); } }


public static
class GraphLinker {

  public static
  void Link(List<Node> pgm, List<NodeLink> links) {
    Dictionary<string, Node> byId = new();
    for (int i=0; i<pgm.Count; ++i) {
      if (byId.TryGetValue(pgm[i].Id, out _)) {
        throw new Exception($"node id \"{pgm[i].Id}\" not unique"); }
      byId[pgm[i].Id] = pgm[i]; }
    foreach (var link in links) {
      var fromNode = byId[link.Id];
      var (depId, depSlot) = link.Slot();
      if (byId.TryGetValue(depId, out Node toNode)) {
        Console.WriteLine($"link from={fromNode.Id}:{link.Attr} to={depId}:{depSlot}");
        fromNode.Connect(link.Attr, toNode, depSlot); }
      else {
        throw new Exception($"unresolved link from={fromNode.Id}:{link.Attr} to={depId}:{depSlot}"); }}}}


}  // close package namespace
}  // close enterprise namespace
