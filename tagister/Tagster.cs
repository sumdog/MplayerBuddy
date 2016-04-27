﻿using System;
using System.IO;
using Gtk;

namespace tagister {
  class Tagster {

    public static TagDB db;

    public static void Main(string[] args) {
      Application.Init();

      db = new TagDB();

      TagBrowser win = new TagBrowser(db.Tags.ToArray());
      win.ShowAll();
      Application.Run();
    }
  }
}
