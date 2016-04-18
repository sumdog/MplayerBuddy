/*
 * lib.cs - MplayerBuddy
 * author: Sumit Khanna
 * penguindreams.org (see site for licence)
 *
 * Library classes for Mplayerbuddy:
 *    Player - controls actual mplayer process, current time, etc.
 *    Playlist - extention of Gtk:ListStore that holds Players
 *    
 */
using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace org.penguindreams.MplayerBuddy {
  
  public class Player : MPVPlayer {

    public Player(String file) : base(file){}

    public Player(String file, float time) : base(file,time) {}

    public Player(String file, PlayerState state) : base(file,state) {}
        
    public void finishPlayer() {
      if(State == PlayerState.PLAYING) {
        MplayerBuddy.mpv.UnloadPlayer();
      }
      Time = -1;
      State = PlayerState.FINISHED;
    }

    public void startPlayer() {
      //check if the file exists
      if(!File.Exists(this.getNormlaizedFile())) {
        State = PlayerState.ERROR;
        throw new FileNotFoundException();
      }
      else if (State != PlayerState.FINISHED && State != PlayerState.ERROR){
        MplayerBuddy.mpv.LoadPlayer(this);
        State = PlayerState.PLAYING;
      }              
    }

    public void rewindPlayer() {
      Time = 0;
      if(State == PlayerState.PLAYING) {
        MplayerBuddy.mpv.Rewind();
      }
      else {
        State = PlayerState.STOPPED;
      }
    }

  }

  public class Playlist : Gtk.ListStore {

    private String playlist;


    /*
     * Constructs a playlist Gtk.ListStore from a file path 
     * Be sure to catch IO exceptions when calling this
     */
    public Playlist(String listfile) : base(typeof(Player)) {
      playlist = listfile;
      FileStream fs = null;

      fs = File.Open(playlist, FileMode.OpenOrCreate, FileAccess.Read);

      //parse list file
      using(StreamReader inp = new StreamReader(fs)) {
        String line;
        while((line = inp.ReadLine()) != null) {
          String[] parts = line.Split('|');

          if(parts.Length != 2) {
            AppendValues(new Player(parts[0], (float)0.0));
          }
          else if(parts[1].Equals("F")) {
            AppendValues(new Player(parts[0], Player.PlayerState.FINISHED));
          }
          else {
            AppendValues(new Player(parts[0], (float)Convert.ToDouble(parts[1])));
          }
        }
        inp.Close();
      }
      fs.Close();
    }

    public string iosync = "";

    public void writeFile() {
      lock(iosync) {
        using(StreamWriter outp = new StreamWriter(playlist)) {
          foreach(object[] row in this) {
            Player p = (Player)row[0];
            String s = (p.State == Player.PlayerState.FINISHED) ? "F" : Convert.ToString(p.Time);
            outp.WriteLine(p.getFile() + "|" + s);
          }
          outp.Flush();
          outp.Close();
        }
      }
    }
        
    /* adds player and returns false on duplicates */
    public bool addPlayer(string file, float time) {
      foreach(object[] players in this) {
        //check for duplicates
        if(((Player)players[0]).getFile().Equals(file)) {
          return false;
        }
      }
      //no duplices, so add to list
      this.AppendValues(new Player(file, time));
      return true; 
    }
  }

}
