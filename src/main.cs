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
using System.Diagnostics;
using System.Text;
using Gtk;
using System.Runtime.InteropServices;

namespace org.penguindreams.MplayerBuddy
{
    class MplayerBuddy
    {
        static Playlist play;

        static Gui gui;
		
		static MPVWindow mpv;
        
        public static Config conf;
        
        //Used for fatal errors upon loading config files
        // ends program execution with exit code 1
        static void errorOnLoad(String msg) {
            MessageDialog m = new MessageDialog(new Window("Error"), DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, msg);
            m.Run();
            m.Destroy();
            System.Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            Application.Init();

            //home dir to store conf and playlist files
            String home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            
            //load playlist
            try
            {
                play = new Playlist(home + "/.mplayerbuddy.list");
            }
            catch (Exception e)
            {
				Console.WriteLine(e);
                errorOnLoad("Could not create/open playlist file.");
            }
            
            //load config
            try {
                conf = new Config(home + "/.mplayerbuddy.config");
            }
            catch(Exception) {
                errorOnLoad("Could not create/open config file.");
            }

            //create main window
            gui = new Gui(play);
            gui.ShowAll();
			
			mpv = new MPVWindow();
			mpv.ShowAll ();

            //save playlist ever 10 seconds
            GLib.Timeout.Add(10000, new GLib.TimeoutHandler(savePlaylist));
			
			GLib.Timeout.Add(2000, new GLib.TimeoutHandler(testMpvPlayerAbility));

            Application.Run();

        }

        //called every 10 seconds to save playlist
        private static bool savePlaylist() 
        {
                play.writeFile();
                return true;
        }
		
		[DllImport("gdk-x11-2.0")]
        private static extern int gdk_x11_drawable_get_xid(IntPtr drawable);
		
		private static bool testMpvPlayerAbility() {
			Console.WriteLine ("mpv test");
			Process proc;
			proc = new Process();
			proc.StartInfo.FileName = "/usr/bin/mpv";
			//proc.StartInfo.Arguments = "";
			int xwinid = gdk_x11_drawable_get_xid(mpv.GdkWindow.Handle);
			Console.WriteLine(string.Format ("WID {0}",xwinid));
			proc.StartInfo.Arguments = string.Format("/media/holly/webop/movies-watched-nz/wiwo_air_wifey.wmv --wid {0}", xwinid);
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardInput = true;
			proc.StartInfo.RedirectStandardError = true;
			
            try {
            	//throws Win32Exception on Windows or FileNotFound on Linux
            	proc.Start(); 
				//Thread oo = new Thread(new ParameterizedThreadStart(readDataThread));
			    //oo.Start(new DataThreadInfo(proc,OutputType.STDOUT));
				//Thread ee = new Thread(new ParameterizedThreadStart(readDataThread));
				//ee.Start(new DataThreadInfo(proc,OutputType.STDERR));
            }
            catch(Exception e) {
            	//TODO: handle this
            	//state = player_state.ERROR;
				Console.WriteLine("Error spawning: " + e);
            }
			
            proc.WaitForExit();
			Console.WriteLine("I have exited?");
			Console.WriteLine ("Has Exited " + proc.HasExited);
			Console.WriteLine ("Code " + proc.ExitCode);
			
			return false;
		}
    }
}
