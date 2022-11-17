namespace rqdq.rclt {

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

  public static
  IEnumerable<Tuple<int, int>> Split(string text) {
    int level=0, j=0, k=0;
    while (true) {
      bool found = false;
      for (k=j; k<text.Length; ++k) {
             if (text[k] == '(') { ++j; }
        else if (text[k] == ')') { --j; }
        else if (text[k] == ',' && level == 0) {
          yield return new Tuple<int, int>(j, k);
          j = k + 1;
          found = true;
          break; }}
      if (!found) {
        break; }}
    yield return new Tuple<int, int>(j, k); } }


}  // close package namespace
