using System;
using System.IO;
using Gtk;
using org.penguindreams.MplayerBuddy;
using System.Linq;

namespace tagster {
  class Tagster {

    private readonly static TagDB db = new TagDB();

    private readonly static TagBrowser gui = new TagBrowser();

    private readonly static MPVWindow mpv = new MPVWindow();

    public static void Main(string[] args) {
      Application.Init();

      if(args.Length > 0) {
        switch(args[0]) {
          case "import":
            System.IO.Directory.GetFiles("/media/holly/webop/movies-watched").ToList().ForEach( f => db.AddFile(f) );
            break;
          case "restore":
            File.OpenText("/media/holly/webop/movies-watched/tags").ReadToEnd().Split('\n').ToList().ForEach( line => {
              var parts = line.Split( new string[]{"--"}, StringSplitOptions.RemoveEmptyEntries );
              if(parts.Length == 2) {

                var i = db.ListFiles().Find( f => f.File == parts[0].Trim() );
                if(i != null) {
                  Console.WriteLine("Found " + i.File);
                }
                else {
                  Console.WriteLine("Lost " + i);
                }

                parts[1].Split(new char[] {' '}).ToList().ForEach( tag => { 
                  if(tag != "") {
                    try {
                      db.AddTag(tag);
                    }
                    catch(Exception e) {
                      Console.WriteLine(String.Format("Tag Exists: {0}",tag));
                    }
                    Console.WriteLine("Updating");
                    var ta = db.Tags.ToList().Find(t => t.Name == tag);

                    db.UpdateTag( i, new Tag() { Name = ta.Name, Id = ta.Id, Set = true } );
                  }
                });

              }
            });
            break;
        }
      }
      else {
        //Dependency Injection
        gui.Database = db;
        mpv.ShowAll();
        mpv.MpvCommand = "/usr/bin/mpv";
        gui.MPV = mpv;

        gui.ShowAll();
        Application.Run();
      }
    }
  }
}
