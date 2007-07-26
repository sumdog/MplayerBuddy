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

namespace org.penguindreams.MPlayerBuddy
{
    class MPLayerBuddy
    {
        static Playlist play;

        static Gui gui;

        static void Main(string[] args)
        {
            Application.Init();

            String home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            
            try
            {
                play = new Playlist(home + "/.mplayerbuddy.list");
            }
            catch (System.Exception e)
            {
                MessageDialog m = new MessageDialog(new Window("Error"), DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, "Could not create/open playlist file.");
                m.Run();
                m.Destroy();
                System.Environment.Exit(1);
            }

            //create main window
            gui = new Gui(play);

            //save playlist ever 10 seconds
            GLib.Timeout.Add(10000, new GLib.TimeoutHandler(savePlaylist));

            Application.Run();

        }


        private static bool savePlaylist() 
        {
                play.writeFile();
                return true;
        }

        static void errorWindowClose(object o,DeleteEventArgs a) 
        {
            Application.Quit();
        }
    }
}
