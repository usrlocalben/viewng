using System.Numerics;

namespace rqdq {
namespace rmlv {

struct IVec2 {
  public long x, y;

  public IVec2(long x) { this.x = this.y = x; }
  public IVec2(long x, long y) { this.x = x; this.y = y; }
  public IVec2(IVec2 other) : this(other.x, other.y) { }

  static public IVec2 operator -(IVec2 a)              { IVec2 tmp; tmp.x = -a.x;          tmp.y = -a.y;          return tmp; }
  public static IVec2 operator +(IVec2 lhs, IVec2 rhs) { IVec2 tmp; tmp.x = lhs.x + rhs.x; tmp.y = lhs.y + rhs.y; return tmp; }
  public static IVec2 operator -(IVec2 lhs, IVec2 rhs) { IVec2 tmp; tmp.x = lhs.x - rhs.x; tmp.y = lhs.y - rhs.y; return tmp; }
  public static IVec2 operator *(IVec2 lhs, IVec2 rhs) { IVec2 tmp; tmp.x = lhs.x * rhs.x; tmp.y = lhs.y * rhs.y; return tmp; }
  public static IVec2 operator /(IVec2 lhs, IVec2 rhs) { IVec2 tmp; tmp.x = lhs.x / rhs.x; tmp.y = lhs.y / rhs.y; return tmp; } }


struct IVec3 {
  public long x, y, z;

  public IVec3(long x) { this.x = this.y = this.z = x; }
  public IVec3(long x, long y, long z) { this.x = x; this.y = y; this.z = z; }
  public IVec3(IVec2 other, long z) : this(other.x, other.y, z) { }
  public IVec3(IVec3 other) : this(other.x, other.y, other.z) { }

  public static IVec3 operator-(IVec3 rhs) { return new IVec3(-rhs.x, -rhs.y, -rhs.z); }
  public static IVec3 operator-(IVec3 lhs, IVec3 rhs) { return new IVec3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z); }
  public static IVec3 operator+(IVec3 lhs, IVec3 rhs) { return new IVec3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z); }
  public static IVec3 operator*(IVec3 lhs, IVec3 rhs) { return new IVec3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z); }
  public static IVec3 operator/(IVec3 lhs, IVec3 rhs) { return new IVec3(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z); }

  public IVec2 XY() { return new IVec2(this.x, this.y); } }


struct Float2 {
  public float x, y;

  public Float2(float x) { this.x = this.y = x; }
  public Float2(float x, float y) { this.x = x; this.y = y; }
  public Float2(Float2 other) : this(other.x, other.y) { }

  public static Float2 operator-(Float2 rhs) { return new Float2(-rhs.x, -rhs.y); }
  public static Float2 operator-(Float2 lhs, Float2 rhs) { return new Float2(lhs.x - rhs.x, lhs.y - rhs.y); }
  public static Float2 operator+(Float2 lhs, Float2 rhs) { return new Float2(lhs.x + rhs.x, lhs.y + rhs.y); }
  public static Float2 operator*(Float2 lhs, Float2 rhs) { return new Float2(lhs.x * rhs.x, lhs.y * rhs.y); }
  public static Float2 operator/(Float2 lhs, Float2 rhs) { return new Float2(lhs.x / rhs.x, lhs.y / rhs.y); } }


struct Float3 {
  public float x, y, z;

  public Float3(float x) { this.x = this.y = this.z = x; }
  public Float3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
  public Float3(IVec2 other, float z) : this(other.x, other.y, z) { }
  public Float3(Float3 other) : this(other.x, other.y, other.z) { }

  public static Float3 operator-(Float3 rhs) { return new Float3(-rhs.x, -rhs.y, -rhs.z); }
  public static Float3 operator-(Float3 lhs, Float3 rhs) { return new Float3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z); }
  public static Float3 operator+(Float3 lhs, Float3 rhs) { return new Float3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z); }
  public static Float3 operator*(Float3 lhs, Float3 rhs) { return new Float3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z); }
  public static Float3 operator/(Float3 lhs, Float3 rhs) { return new Float3(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z); }

  public Vector3 ToVector3() { return new Vector3(x, y, z); }
  public Float2 XY() { return new Float2(this.x, this.y); } }


}  // close package namespace
}  // close enterprise namespace