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
        
    public enum player_state { STOPPED, PAUSED, PLAYING, FINISHED, ERROR };

    private player_state state;

    private float time;

    private String file;
        
    private Process proc;
        
    private StreamReader procout;
        
    private StreamWriter procin;

    public Player(String file) {
      this.file = file;
      state = player_state.STOPPED;
      time = 0;
      proc = null;
      procout = null;
      procin = null;
    }

    public Player(String file, float time) {
      this.file = file;
      this.time = time;
      state = player_state.STOPPED;
      proc = null;
      procout = null;
      procin = null;
    }

    public Player(String file, player_state state) {
      this.state = state;
      this.file = file;
      time = (state == player_state.FINISHED) ? -1 : 0;
      proc = null;
      procout = null;
      procin = null;
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

    public float getTime() {
      return time;
    }

    public player_state getState() {
      return state;
    }

    public void finishPlayer() {
      time = -1;
      killPlayer();
      state = player_state.FINISHED;
    }

    public void killPlayer() {
      if(state == player_state.PLAYING || state == player_state.PAUSED) {
        procin.WriteLine("q");
        procin.Flush();
        proc.Kill();
        proc.WaitForExit();
        state = player_state.STOPPED;
      }       	
    }

    public void startPlayer() {
      //check if the file exists
      if(!File.Exists(this.getNormlaizedFile())) {
        state = player_state.ERROR;
        throw new FileNotFoundException();
      }
      else if(state == player_state.STOPPED) {
        Thread t = new Thread(new ThreadStart(spawnMPlayer));
        t.Start();
        state = player_state.PLAYING;
      }
                
    }

    public void rewindPlayer() {
      time = 0;
      if(state == player_state.PLAYING || state == player_state.PAUSED) {
        killPlayer();
        startPlayer();
      }
      else {
        state = player_state.STOPPED;
      }
    }
        
    //mplayer sends only \r, not \n to display data
    // values on the same line. o.ReadLine() stalls because of this
    private static string readMplayerOut(StreamReader o) {
      String r = "";
      while(true) {
        int c = o.Read();
        if(c == -1) {
          return r;
        }
        if(((char)c) == '\r') {
          break;
        }
        if(((char)c) == '\n') {
          break;
        }
        r += (char)c;		
      }
      return r;
    }

    private void processMplayerOutput(string line) {
      if(line.Trim().Equals("=====  PAUSE  =====")) {
        state = player_state.PAUSED;
      }
      else if(line.Contains("Quit")) {
        state = player_state.STOPPED;
        return;
      }
      else if(line.Contains("End of file")) {
        state = player_state.FINISHED;
        time = -1;
        return;
      }
      else if(line.StartsWith("A:")) {
        String[] parts = line.Split(':');
        try {
          time = (float)Convert.ToDouble(parts[1].Trim('V'));
          state = player_state.PLAYING;
        }
        catch(System.FormatException) {
          /* don't care / discard */
        }

      }			
    }

    enum OutputType { STDOUT, STDERR }

    class DataThreadInfo {
			
      public DataThreadInfo(Process process, OutputType type) {
        this.process = process;
        this.type = type;
      }

      public Process process;
      public OutputType type;
    }

    private void readDataThread(object obj) {
			
      DataThreadInfo info = (DataThreadInfo)obj;
			
      while(!info.process.HasExited) {     
        if(info.type == OutputType.STDOUT) {	
          processMplayerOutput(readMplayerOut(info.process.StandardOutput));
        }
        else if(info.type == OutputType.STDERR) {
          processMplayerOutput(readMplayerOut(info.process.StandardError)); //mplayer2 uses stderr
        }
      }//end while			
    }

    private void spawnMPlayer() {
      String args = " " + MplayerBuddy.conf.mplayerArgs + " ";
      if(time != 0) {
        args += " -ss " + time + " ";
      }
        
      proc = new Process();
      proc.StartInfo.FileName = (MplayerBuddy.conf.useCustomPath) ? MplayerBuddy.conf.mplayerCommand : "mplayer";
      proc.StartInfo.Arguments = "\"" + getNormlaizedFile() + "\"" + args;
      proc.StartInfo.UseShellExecute = false;
      proc.StartInfo.RedirectStandardOutput = true;
      proc.StartInfo.RedirectStandardInput = true;
      proc.StartInfo.RedirectStandardError = true;
			
      try {
        //throws Win32Exception on Windows or FileNotFound on Linux
        proc.Start(); 
        Thread oo = new Thread(new ParameterizedThreadStart(readDataThread));
        oo.Start(new DataThreadInfo(proc, OutputType.STDOUT));
        Thread ee = new Thread(new ParameterizedThreadStart(readDataThread));
        ee.Start(new DataThreadInfo(proc, OutputType.STDERR));
      }
      catch(Exception) {
        //TODO: handle this
        state = player_state.ERROR;
      }
			
      proc.WaitForExit();
      int exit = proc.ExitCode;
      if(exit != 0) {
        //ok...this is tricky. Mplayer will NEVER exit with a code other than 0
        // except upon a segment fault. 
        //TODO: figure out how to handle this?
        state = player_state.ERROR;
        Console.WriteLine("mplayer exited with an exit code of : " + exit);
      }

    }
//end spawnMplayer()

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
            AppendValues(new Player(parts[0], Player.player_state.FINISHED));
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
            String s = (p.getState() == Player.player_state.FINISHED) ? "F" : Convert.ToString(p.getTime());
            outp.WriteLine(p.getFile() + "|" + s);
          }
          outp.Flush();
          outp.Close();
        }
      }
    }
       
    /* kills all running mplayer processes for application exit */
    public void killPlayers() {
      foreach(object[] players in this) {
        Player p = (Player)players[0];
        p.killPlayer();
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
