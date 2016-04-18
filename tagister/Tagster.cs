using System;
using Gtk;

namespace tagister {
  class Tagster {
    public static void Main(string[] args) {
      Application.Init();
      TagBrowser win = new TagBrowser();
      win.Show();
      Application.Run();
    }
  }
}
