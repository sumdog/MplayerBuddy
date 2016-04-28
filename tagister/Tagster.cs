using System;
using System.IO;
using Gtk;

namespace tagster {
  class Tagster {

    private readonly static TagDB db = new TagDB();

    private readonly static TagBrowser gui = new TagBrowser();

    public static void Main(string[] args) {
      Application.Init();

      //Dependency Injection
      gui.Database = db;

      gui.ShowAll();
      Application.Run();
    }
  }
}
