namespace rqdq {
namespace rclt {

public static
class NestedSplitter {

  /*
  public static
  Tuple<char, bool> Decomp(char ch) {
    bool close = false;
         if (ch == ')') { ch = '('; close = true; }
    else if (ch == ']') { ch = '['; close = true; }
    else if (ch == '}') { ch = '{'; close = true; }
    else if (ch == '>') { ch = '<'; close = true; }
    return new Tuple<char, bool>(ch, close); }
  */
      
  /*public static
  List<string> Split(string text) {
    List<string> ax = new();
    int level = 0;
    int j = 0;
    while (true) {
      bool found = false;
      for (int k=j; k<text.Length; ++k) {
        if (text[k] == '(') {
          ++j; }
        else if (text[k] == ')') {
          --j; }
        else if (text[k] == ',' && level == 0) {
          ax.Add(text[j..k]);
          j = k + 1;
          found = true;
          break; }}
      if (!found) {
        break; }}
    ax.Add(text[j..]);
    return ax; }*/

  public static
  IEnumerable<Tuple<int, int>> Split(string text) {
  Console.WriteLine($"smartsplit [{text}]");
    // XXX List<Tuple<int, int>> ax = new();
    int level=0, j=0, k=0;
    while (true) {
      bool found = false;
      for (k=j; k<text.Length; ++k) {
             if (text[k] == '(') { ++j; }
        else if (text[k] == ')') { --j; }
        else if (text[k] == ',' && level == 0) {
          // ax.Add(new Tuple<int, int>(j, k));
          yield return new Tuple<int, int>(j, k);
          // Console.WriteLine($"[{j}, {k})");
          j = k + 1;
          found = true;
          break; }}
      if (!found) {
        break; }}
    // ax.Add(new Tuple<int, int>(j, k));
    yield return new Tuple<int, int>(j, k);
    // Console.WriteLine($"[{j}, {k})");
    // return ax;
    }

}

}  // close package namespace
}  // close enterprise namespace
