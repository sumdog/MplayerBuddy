using System;
using System.IO;
using System.Collections.Generic;
using Gtk;
using org.penguindreams.MplayerBuddy;
using System.Linq;
using Mono.Unix;

namespace tagster {
  class Tagster {

    private static TagDB db;

    private static TagBrowser gui;

    private static MPVWindow mpv;

    private static Config config;

    public readonly static String home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

    public static string RepositoryRoot;

    public static void Main(string[] args) {
      Application.Init();

      gui = new TagBrowser();
      mpv = new MPVWindow();
      db = new TagDB(Path.Combine(home, ".tagster/tagster.sqlite3"));
      config = new Config(Path.Combine(home,".tagster/config"));
      RepositoryRoot = config.RepositoryPath;

      if(args.Length > 0) {
        switch(args[0]) {
          case "tagcount":
            foreach(KeyValuePair<string,long> e in db.TagUsage()) {
              Console.WriteLine(String.Format("{0}:\t{1}", e.Key, e.Value));
            }
            break;
          case "createlinks":
            db.ListFiles().ForEach( file => {
              db.TagsForFile(file).ForEach( tag => {
                
                var destDir = Path.Combine(config.LinksPath, tag.Name);
                var destFile = Path.Combine(destDir, file.File);

                Directory.CreateDirectory(destDir);
                UnixFileInfo target = new UnixFileInfo(Path.Combine(config.RepositoryPath, file.File));
                if(tag.Set && !File.Exists(destFile)) {
                  Console.WriteLine(String.Format("Creating New Link {0} -> {1}", target.FullName, destFile));
                  target.CreateSymbolicLink(destFile);
                }

              });
            });
            break;
          case "import":
            var currentFiles = db.ListFiles().Select( f => f.File ).ToArray();

            System.IO.Directory.GetFiles(RepositoryRoot).ToList().ForEach( f => {
              var filename = new FileInfo(f).Name;
              if(!currentFiles.Contains(filename)) {
                db.AddFile(filename);
                Console.WriteLine(String.Format("Adding {0}", filename));
              }
              else {
                Console.WriteLine(String.Format("Skipping {0}", filename));
              }
            });

            break;
          case "restore":
            File.OpenText(String.Format("{0}/tags", RepositoryRoot)).ReadToEnd().Split('\n').ToList().ForEach( line => {
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
