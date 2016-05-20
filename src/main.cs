/*
 * main.cs - MplayerBuddy
 * author: Sumit Khanna
 * penguindreams.org (see site for licence)
 *
 * Entry point for appliccation
 *   -contains a timer for writing bookmark file every 10 sec
 *    
 */
using System;
using System.Text;
using Gtk;

namespace org.penguindreams.MplayerBuddy {
  
  class MplayerBuddy {
    
    static Playlist play;

    static Gui gui;
		
    public static MPVWindow mpv;
        
    public static Config conf;
        
    //Used for fatal errors upon loading config files
    // ends program execution with exit code 1
    static void errorOnLoad(String msg) {
      MessageDialog m = new MessageDialog(new Window("Error"), DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, msg);
      m.Run();
      m.Destroy();
      System.Environment.Exit(1);
    }

    static void Main(string[] args) {
      Application.Init();

      //home dir to store conf and playlist files
      String home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            
      //load playlist
      try {
        play = new Playlist(home + "/.mpvbuddy.list");
      } 
      catch(Exception e) {
        Console.WriteLine(e);
        errorOnLoad("Could not create/open playlist file.");
      }
            
      //load config
      try {
        conf = new Config(home + "/.mpv.config");
      } 
      catch(Exception) {
        errorOnLoad("Could not create/open config file.");
      }

      mpv = new MPVWindow();
      mpv.ShowAll();
      mpv.MpvCommand = MplayerBuddy.conf.mplayerCommand;

      //create main window
      gui = new Gui(play);
      gui.ShowAll();
			
      //save playlist ever 10 seconds
      GLib.Timeout.Add(10000, new GLib.TimeoutHandler(savePlaylist));

      Application.Run();

    }
 
    //called every 10 seconds to save playlist
    private static bool savePlaylist() {
      play.writeFile();
      return true;
    }
		
  }
}
