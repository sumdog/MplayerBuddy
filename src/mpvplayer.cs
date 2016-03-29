
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


namespace org.penguindreams.MplayerBuddy
{

    public class MPVWindow : Window
    {
		
		public const string MPV_SOCKET = "/tmp/mpvbuddy.sock";
		
		private string mpvCommand;
		public string MpvCommand
		{
			get { return mpvCommand; }
			set
			{
				mpvCommand = value;
				
				if (mpvProcess != null)
				{
					try
					{
						mpvProcess.Kill();
					}
					catch (Exception e) {
						Console.Error.WriteLine (string.Format ("Error killing old mpv process {0}", e));
					} 
					finally {
						mpvProcess = null;
					}
				}
				
				initMPVProcess ();
			}
		}
		
		private String currentFile;
		public String CurrentFile {
			get { return currentFile; }
			set {
				currentFile = value;
				initMPVProcess();
			}
		}
		
		private Process mpvProcess;
		private System.Net.Sockets.Socket mpvSocket;
		
		public MPVWindow () : base("mpv-viewer")
		{
			MpvCommand = MplayerBuddy.conf.mplayerCommand;
			currentFile = null;
			mpvProcess = null;
			mpvSocket = null;
		}
		
		[DllImport("gdk-x11-2.0")]
        private static extern int gdk_x11_drawable_get_xid(IntPtr drawable);
		
		private void initMPVProcess ()
		{
			
			if (currentFile != null) {
				if (mpvProcess == null) {
					mpvProcess = new Process ();
					mpvProcess.StartInfo.FileName = mpvCommand;
					mpvProcess.StartInfo.Arguments = string.Format(
						"--wid {0} --input-unix-socket=\"{1}\" \"{2}\"",
						gdk_x11_drawable_get_xid(this.GdkWindow.Handle),
						MPV_SOCKET,
						currentFile
					);
					mpvProcess.StartInfo.UseShellExecute = false;
					mpvProcess.Start();
					
					mpvSocket = new System.Net.Sockets.Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
					mpvSocket.Connect(new UnixEndPoint(MPV_SOCKET));
				}
			}
		}
	}
}