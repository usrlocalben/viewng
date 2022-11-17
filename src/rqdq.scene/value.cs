using System.Numerics;

namespace rqdq.scene {

class FlexFloat : IFlexValue {
  private readonly float _a;
  public FlexFloat(float a) { _a = a; }
  public string AsString () => _a.ToString();
  public float AsFloat () => _a;
  public Vector2 AsFloat2 () => new(_a);
  public Vector3 AsFloat3 () => new(_a);
  public Vector4 AsFloat4 () => new(_a);}

class FlexFloat2 : IFlexValue {
  private readonly Vector2 _a;
  public FlexFloat2(Vector2 a) { _a = a; }
  public string AsString () => _a.ToString();
  public float AsFloat () => _a.X;
  public Vector2 AsFloat2 () => _a;
  public Vector3 AsFloat3 () => new(_a, 0);
  public Vector4 AsFloat4 () => new(_a, 0, 0);}

class FlexFloat3 : IFlexValue {
  private readonly Vector3 _a;
  public FlexFloat3(Vector3 a) { _a = a; }
  public string AsString () => _a.ToString();
  public float AsFloat () => _a.X;
  public Vector2 AsFloat2 () => new(_a.X, _a.Y);
  public Vector3 AsFloat3 () => _a;
  public Vector4 AsFloat4 () => new(_a, 0);}

class FlexFloat4 : IFlexValue {
  private readonly Vector4 _a;
  public FlexFloat4(Vector4 a) { _a = a; }
  public string AsString () => _a.ToString();
  public float AsFloat () => _a.X;
  public Vector2 AsFloat2 () => new(_a.X, _a.Y);
  public Vector3 AsFloat3 () => new(_a.X, _a.Y, _a.Z);
  public Vector4 AsFloat4 () => _a;}

class FlexString : IFlexValue {
  private readonly string _a;
  public FlexString(string a) { _a = a; }
  public string AsString () => _a;
  public float AsFloat () => _a.Length > 0 ? 1.0F : 0.0F;
  public Vector2 AsFloat2 () => new(_a.Length > 0 ? 1.0F : 0.0F);
  public Vector3 AsFloat3 () => new(_a.Length > 0 ? 1.0F : 0.0F);
  public Vector4 AsFloat4 () => new(_a.Length > 0 ? 1.0F : 0.0F); }


class Float3Node : Node, IValueNode {
  private readonly Vector3 _value;
  public Float3Node(string id, Vector3 value) : base(id) { _value = value; }
  public IFlexValue Eval(string slot) { return new FlexFloat3(_value); }}


class Float3NodeCompiler : NodeCompilerBase {
  public static void Install() {
    AnyCompiler.Register("float3", (id, elem) => new Float3NodeCompiler().Compile(id, elem)); }
  public override void CompileImpl() {
    var x = (float)_data.GetProperty("x").GetDouble();
    var y = (float)_data.GetProperty("y").GetDouble();
    var z = (float)_data.GetProperty("z").GetDouble();
    _node = new Float3Node(_id, new Vector3(x, y, z)); }}


public 
class SystemValues : Node, IValueNode {

  static private readonly FlexFloat notFoundValue = new(0);
  private readonly Dictionary<string, IFlexValue> _db = new();

  public SystemValues(string id) : base(id) {}

  public override void Connect(string attr, Node target, string slot) {}

  public void Upsert(string n, float x) { _db[n] = new FlexFloat(x); }
  public void Upsert(string n, Vector2 x) { _db[n] = new FlexFloat2(x); }
  public void Upsert(string n, Vector3 x) { _db[n] = new FlexFloat3(x); }
  public void Upsert(string n, Vector4 x) { _db[n] = new FlexFloat4(x); }

  public
  IFlexValue Eval(string slot) {
    if (_db.TryGetValue(slot, out var value)) {
      return value; }
    return notFoundValue; }}


}  // close package namespace
