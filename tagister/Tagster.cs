using System;
using System.IO;
using Gtk;
using org.penguindreams.MplayerBuddy;

namespace tagster {
  class Tagster {

    private readonly static TagDB db = new TagDB();

    private readonly static TagBrowser gui = new TagBrowser();

    private readonly static MPVWindow mpv = new MPVWindow();

    public static void Main(string[] args) {
      Application.Init();

      //Dependency Injection
      gui.Database = db;
      gui.MPV = mpv;

      gui.ShowAll();
      Application.Run();
    }
  }
}
