namespace rqdq.rglv {

interface IObjParserProgram {
  void MtlLib(string data);
  void UseMtl(string data);

  void G(string data);

  void P(float x, float y, float z);
  void N(float x, float y, float z);
  void T(float x, float y);

  void BeginFace();
  void BeginVertex();
  void IndexP(int idx);
  void IndexN(int idx);
  void IndexT(int idx);
  void EndVertex();
  void EndFace();

  void End();
  void Error(int lineNum, string msg); }


interface IMtlParserProgram {
  void NewMtl(string data);

  void MapKd(string data);
  void Ka(float x, float y, float z);
  void Kd(float x, float y, float z);
  void Ks(float x, float y, float z);
  void Ns(float e);
  void D(float d);
  void Illum(int mode);

  void End();
  void Error(int lineNum, string msg); }


}  // close package namespace
