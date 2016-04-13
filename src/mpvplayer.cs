
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

  public class MPVWindow : Window {
		
    public const string MPV_SOCKET = "/tmp/mpvbuddy.sock";
		
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
          "--wid {0} --input-unix-socket=\"{1}\" --idle  --input-cursor=no ",
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
      
    private Player currentPlayer;
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

      Console.WriteLine("==============" + e.Event.Type );
    }

    protected virtual void AddButtonPressed(object sender, ButtonPressEventArgs e) {
      if(e.Event.Type == Gdk.EventType.TwoButtonPress) {
        if(this.GdkWindow.State == Gdk.WindowState.Fullscreen) {
          this.Unfullscreen();
        }
        else {
          this.Fullscreen();
        }
      }
      else if(e.Event.Type == Gdk.EventType.ButtonPress) {
        Console.WriteLine("Button:::::" + e.Event.Button);
        switch(e.Event.Button) {
          case 1:
            break;
          case 2:
            break;
          case 3:
            break;
          case 4:
            break;
          case 5:
            break;
        }

        WriteCommand("osd-msg-bar", new string[]{ "show-progress" });
      }
    }

    public void LoadPlayer(Player play) {
      lock(playerLock) {
        if(!play.Equals(currentPlayer)) {

          if(currentPlayer != null) {
            if(currentPlayer.State == Player.PlayerState.FINISHED ||
               currentPlayer.State == Player.PlayerState.ERROR) {
            }
            else {
              currentPlayer.State = Player.PlayerState.STOPPED;
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
              currentPlayer.State = Player.PlayerState.PAUSED;
              break;
            case "idle":
              currentPlayer.State = Player.PlayerState.FINISHED;
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