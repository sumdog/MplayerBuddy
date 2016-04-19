using System;
using System.IO;
using Gtk;

namespace tagister {
  class Tagster {
    public static void Main(string[] args) {
      Application.Init();

      String[] tags = File.ReadAllLines("tags.txt");

      TagBrowser win = new TagBrowser(tags);
      win.ShowAll();
      Application.Run();
    }
  }
}
