using System.Numerics;
using SharpDX.Direct3D11;
using DXDevice = SharpDX.Direct3D11.Device;

namespace rqdq {
namespace app {

public
struct NodeRef {
  public string attr;
  public string target; };


public
class Node {
  public readonly string _id;
  public readonly NodeRef[] _refs;

  public virtual
  void Receive(string n, float a) {}

  public virtual
  void Connect(string attr, Node target, string slot) {}

  public virtual
  void Verify() {}

  public virtual
  void Init(DXDevice device) {}

  public
  Node(string id, NodeRef[] refs) {
    _id = id;
    _refs = refs;
  }}


interface ICamera {
  Matrix4x4 GetViewMatrix();
  Matrix4x4 GetProjMatrix(); }


interface IFlexValue {
  string AsString();
  float AsFloat();
  Vector2 AsFloat2();
  Vector3 AsFloat3();
  Vector4 AsFloat4(); };


interface IValueNode {
  IFlexValue Eval(string slot); };


interface IGl {
  void Draw(DeviceContext dc, Matrix4x4 vmat, Matrix4x4 pmat); }


interface ILayer {
  Vector4 GetColor();
  void Draw(DeviceContext dc); }


}  // close package namespace
}  // close enterprise namespace
