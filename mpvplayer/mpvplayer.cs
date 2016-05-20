
/*
 * gui.cs - MplayerBuddy
 * author: Sumit Khanna
 * penguindreams.org (see site for licence)
 *
 * Place holder for mpv window
 *    
 */
using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Gtk;
using System.Web;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using Mono.Unix;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace org.penguindreams.MplayerBuddy {

  public abstract class MPVPlayer {
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

    public MPVPlayer(String file) {
      this.file = file;
      state = PlayerState.STOPPED;
      time = 0;
    }

    public MPVPlayer(String file, float time) {
      this.file = file;
      this.time = time;
      state = PlayerState.STOPPED;
    }

    public MPVPlayer(String file, PlayerState state) {
      this.state = state;
      this.file = file;
      time = (state == PlayerState.FINISHED) ? -1 : 0;
    }

    /* removes annoying URL encoding from filename */
    public string getNormlaizedFile() {
      //Normalize with UrlDecore to strip %20
      // and Uri to strip file:///
      return HttpUtility.UrlDecode(new Uri(file).AbsolutePath);
    }

    /* returns full file URI */
    public String getFile() {
      return file;
    }

    /* returns just the file name */
    public string getFileName() {
      return System.IO.Path.GetFileName(HttpUtility.UrlDecode(file));
    }


  }

  public class MPVWindow : Window {
		
    public readonly string MPV_SOCKET = String.Format("/tmp/mpvplayer-{0}.sock", Guid.NewGuid().ToString());
		
    private string mpvCommand;

    public string MpvCommand {
      get { return mpvCommand; }
      set {
        mpvCommand = value;
				
        if(mpvProcess != null) {
          try {
            mpvProcess.Kill();
          }
          catch(Exception e) {
            Console.Error.WriteLine(string.Format("Error killing old mpv process {0}", e));
          }
          finally {
            mpvProcess = null;
          }
        }
				
        //TODO big try catch with error report
				
        mpvProcess = new Process();
        mpvProcess.StartInfo.FileName = mpvCommand;
        mpvProcess.StartInfo.Arguments = string.Format(
          "--wid {0} --input-ipc-server=\"{1}\" --idle  --input-cursor=no ",
          gdk_x11_drawable_get_xid(this.GdkWindow.Handle),
          MPV_SOCKET
        );
        mpvProcess.StartInfo.UseShellExecute = false;
        mpvProcess.Start();
				
        //TODO: kill off existing sockets?
        mpvSocket = new System.Net.Sockets.Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
        mpvSocket.Connect(new UnixEndPoint(MPV_SOCKET));
        socketStream = new NetworkStream(mpvSocket);

        Thread t = new Thread(new ThreadStart(SocketReader));
        t.Start();
      }
    }
      
    private MPVPlayer currentPlayer;
    private String playerLock = "";

    private Process mpvProcess;
    private System.Net.Sockets.Socket mpvSocket;
    private NetworkStream socketStream;

    public MPVWindow() : base("mpv-viewer") {
      currentPlayer = null;
      mpvProcess = null;
      mpvSocket = null;

      this.AddEvents((int)Gdk.EventMask.ButtonPressMask);
      this.AddEvents((int)Gdk.EventMask.ScrollMask);
      this.ButtonPressEvent += AddButtonPressed;
      this.ScrollEvent += AddScrollEvent;

      GLib.Timeout.Add(1000, new GLib.TimeoutHandler(PlaybackTimeTimer));
    }

    protected virtual void AddScrollEvent(object sender, ScrollEventArgs e) {
      switch(e.Event.Direction) {
        case Gdk.ScrollDirection.Up:
          WriteCommand("osd-msg-bar", new string[]{ "add" , "volume" , "5" });
          break;
        case Gdk.ScrollDirection.Down:
          WriteCommand("osd-msg-bar", new string[]{ "add" , "volume" , "-5" });
          break;
        case Gdk.ScrollDirection.Left:
          break;
        case Gdk.ScrollDirection.Right:
          break;
      }
    }

    public event EventHandler BackMouseButton;
    public event EventHandler FrontMouseButton;

    protected virtual void AddButtonPressed(object sender, ButtonPressEventArgs e) {
      if(e.Event.Type == Gdk.EventType.TwoButtonPress && e.Event.Button == 1) {
        if(this.GdkWindow.State == Gdk.WindowState.Fullscreen) {
          this.Unfullscreen();
        }
        else {
          this.Fullscreen();
        }
      }
      else if(e.Event.Type == Gdk.EventType.ButtonPress) {
        switch(e.Event.Button) {
          case 2:
            WriteCommand("osd-msg-bar", new string[]{ "show-progress" });
            break;
          case 3:
            WriteCommand("osd-msg-bar", new string[]{ "cycle" , "pause" });
            break;
          case 8:
            if(FrontMouseButton != null) {
              FrontMouseButton(this, e);
            }
            break;
          case 9:
            if(BackMouseButton != null) {
              BackMouseButton(this, e);  
            }
            break;
        }
      }
    }

    public void LoadPlayer(MPVPlayer play) {
      lock(playerLock) {
        if(!play.Equals(currentPlayer)) {

          if(currentPlayer != null) {
            if(currentPlayer.State == MPVPlayer.PlayerState.FINISHED ||
               currentPlayer.State == MPVPlayer.PlayerState.ERROR) {
            }
            else {
              currentPlayer.State = MPVPlayer.PlayerState.STOPPED;
            }
          }

          currentPlayer = play;
          WriteCommand("loadfile", new string[] { play.getNormlaizedFile() });
        }
      }
    }

    private bool PlaybackTimeTimer() {
      WriteCommand("get_property", new string[] {"playback-time"});
      return true;
    }

    private void WriteCommand(string command, string[] param) {
      if(socketStream != null && currentPlayer != null) {
        lock(playerLock) {
          JArray cmd = new JArray();
          cmd.Add(command);
          foreach(string p in param){
            cmd.Add(p);
          }
          JObject o = new JObject();
          o["command"] = cmd;
          o["request_id"] = currentPlayer.GetHashCode();
          Console.WriteLine(o);
          var buffer = Encoding.ASCII.GetBytes(o.ToString(Formatting.None) + "\n");
          socketStream.Write(buffer, 0, buffer.Length);   
        }
      }
    }

    public void Rewind() {
      WriteCommand("seek", new string[] { "0", "absolute+exact" });  
      currentPlayer.Time = 0;
    }

    public void UnloadPlayer() {
      WriteCommand("stop", new string[] { }); 
    }

    private void SocketReader() {
      var reader = new StreamReader(socketStream);
      while(true) {
        var x = reader.ReadLine();
        Console.WriteLine(x);
        JObject jIn = JObject.Parse(x);

        var data = jIn.SelectToken("data");
        var evnt = jIn.SelectToken("event");
        var requestId = jIn.SelectToken("request_id");

        Console.WriteLine(string.Format("Event:{0} - Data:{1} - RequestID:{2}", evnt, data, requestId));

        float time;

        if(evnt != null) {
          switch(evnt.ToString()) {
            case "file-loaded":
              WriteCommand("seek", new string[] { currentPlayer.Time.ToString(), "absolute+exact" });
              break;
            case "pause":
              currentPlayer.State = MPVPlayer.PlayerState.PAUSED;
              break;
            case "idle":
              currentPlayer.State = MPVPlayer.PlayerState.FINISHED;
              currentPlayer = null;
              break;
          }
        }

        if(data != null && float.TryParse(data.ToString(), out time))
        {
          if(requestId.ToString().Equals(currentPlayer.GetHashCode().ToString())) {
            currentPlayer.Time = time;  
          }
        }
      }
    }

    [DllImport("gdk-x11-2.0")]
    private static extern int gdk_x11_drawable_get_xid(IntPtr drawable);

  }
}