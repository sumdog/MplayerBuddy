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
  
  public class Player {
        
    public enum PlayerState { STOPPED, PAUSED, PLAYING, FINISHED, ERROR };

    private PlayerState state;

    public PlayerState State {
      get { return state; }
      set { state = value; }
    }

    private float time;
    public float Time {
      get { return time; }
      set { time = value; }
    }

    private String file;

    public Player(String file) {
      this.file = file;
      state = PlayerState.STOPPED;
      time = 0;
    }

    public Player(String file, float time) {
      this.file = file;
      this.time = time;
      state = PlayerState.STOPPED;
    }

    public Player(String file, PlayerState state) {
      this.state = state;
      this.file = file;
      time = (state == PlayerState.FINISHED) ? -1 : 0;
    }

    /* returns full file URI */
    public String getFile() {
      return file;
    }
        
    /* removes annoying URL encoding from filename */
    public string getNormlaizedFile() {
      //Normalize with UrlDecore to strip %20
      // and Uri to strip file:///
      return HttpUtility.UrlDecode(new Uri(file).AbsolutePath);
    }
        
    /* returns just the file name */
    public string getFileName() {
      return System.IO.Path.GetFileName(HttpUtility.UrlDecode(file));
    }

    public void finishPlayer() {
      time = -1;
      state = PlayerState.FINISHED;
    }

    public void startPlayer() {
      //check if the file exists
      if(!File.Exists(this.getNormlaizedFile())) {
        state = PlayerState.ERROR;
        throw new FileNotFoundException();
      }
      else if (State != PlayerState.FINISHED && State != PlayerState.ERROR){
        MplayerBuddy.mpv.LoadPlayer(this);
        state = PlayerState.PLAYING;
      }              
    }

    public void rewindPlayer() {
      state = PlayerState.STOPPED;
      time = 0;
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
