using System;
using System.IO;
using Gtk;

namespace tagister {
  class Tagster {
    public static void Main(string[] args) {
      Application.Init();

      String[] tags = File.ReadAllLines("tags.txt");

      var db = new TagDB();

      TagBrowser win = new TagBrowser(db.Tags.ToArray());
      win.ShowAll();
      Application.Run();
    }
  }
}
