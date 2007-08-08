
/*
 * gui.cs - MplayerBuddy
 * author: Sumit Khanna
 * penguindreams.org (see site for licence)
 *
 * GUI Objects for Mplayerbuddy:
 *  Gui - Primary window with file listing
 *  FilmPopup - popup window for right clicks
 *    
 */
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Gtk;
using System.Web;


namespace org.penguindreams.MPlayerBuddy
{

    public class Gui : Window
    {

        private TreeView tree;
        private ListStore store;

        private ScrolledWindow scroller;

        private MainMenu menu;

        private VBox mainbox;

        //drag'n drop
        enum drop_types
        {
            TARGET_STRING,
            TARGET_URL
        };
        static Gtk.TargetEntry[] target_table =
        {
           new TargetEntry ("STRING",        0, (uint) drop_types.TARGET_STRING ),
           new TargetEntry ("text/plain",    0, (uint) drop_types.TARGET_STRING ),
           new TargetEntry ("text/uri-list", 0, (uint) drop_types.TARGET_URL ),             
        };

        public Gui(Playlist p) : base("MplayerBuddy")
        {
            //setup layout
            mainbox = new VBox();

            //setup tree
            store = p;
            tree = new PlayerTree(p);

            //event handlers
            this.DeleteEvent += windowClosed;

            //window scrolling support
            scroller = new ScrolledWindow();
            scroller.Add(tree);

            //add menu
            menu = new MainMenu();
            
            //put it together
            mainbox.PackStart(menu,false,false,0);
            mainbox.PackStart(scroller);

            Add(mainbox);
            Resize(200, 200);
            ShowAll();
        }


        public void windowClosed(object o,DeleteEventArgs a)
        {
            ((Playlist)store).killPlayers();
            Application.Quit();
        }

    }


    //Main top menu bar
    class MainMenu : MenuBar
    {
        private Menu mFile, mHelp, mConfig;
        private MenuItem miFile, miHelp, miConfig;

        //File Menu
        private MenuItem miExit;

        //config menu
        private MenuItem miPreferences;

        //help menu
        private MenuItem miAbout;

        public MainMenu() : base()
        {
            //menu containers
            mFile = new Menu();
            mHelp = new Menu();
            mConfig = new Menu();

            //Items to hold containers
            miFile = new MenuItem("File");
            miFile.Submenu = mFile;
            miHelp = new MenuItem("Help");
            miHelp.Submenu = mHelp;
            miConfig = new MenuItem("Config");
            miConfig.Submenu = mConfig;

            //build file menu
            miExit = new MenuItem("Exit");
            mFile.Add(miExit);

            //build config menu
            miPreferences = new MenuItem("Preferences");
            mConfig.Add(miPreferences);

            //build help menu
            miAbout = new MenuItem("About");
            mHelp.Add(miAbout);

            //events
            miExit.Activated += menuItemClicked;
            miPreferences.Activated += menuItemClicked;
            miAbout.Activated += menuItemClicked;

            //put it all together
            Append(miFile);
            Append(miConfig);
            Append(miHelp);
        }

        public void menuItemClicked(object o, EventArgs a)
        {
            if (o == miExit) {
                Application.Quit();
            }
            else if (o == miAbout) {
                AboutDialog about = new AboutDialog();
                about.Authors = new string[] {"Sumit Khanna"};
                about.Copyright = "Open Source. Some rights reserved. (See website)";
                about.Name = "MplayerBuddy";
                about.Website = "http://penguindreams.org";
                about.Show();
            }
            else if (o == miHelp) {
                
            }
        }
    }

    class FilmPopup : Menu
    {
        private MenuItem mPlay, mStop, mRewind, mFinish, mMove, mRemove;

        private ListStore list;

        private Player player;

        private TreeIter iter;

        public FilmPopup(Player p, ListStore store, TreeIter i) : base()
        {
            player = p;
            list = store;
            iter = i;
            
            mPlay = new MenuItem("Play");
            mStop = new MenuItem("Stop");
            mRewind = new MenuItem("Rewind");
            mFinish = new MenuItem("Finish");
            mMove = new MenuItem("Move");
            mRemove = new MenuItem("Remove");

            switch (p.getState())
            {
                case Player.player_state.FINISHED:
                	mPlay.Sensitive = false;
                	mStop.Sensitive = false;
                	mFinish.Sensitive = false;
                    break;
                case Player.player_state.STOPPED:
                    mStop.Sensitive = false;
                    break;
                case Player.player_state.PAUSED:
                case Player.player_state.PLAYING:
                    mMove.Sensitive = false;
                    mRemove.Sensitive = false;
                    mPlay.Sensitive = false;
                    break;
            }
            if (p.getTime() == 0)
            {
                mRewind.Sensitive = false;
            }

            Add(mPlay);
            Add(mStop);
            Add(mRewind);
            Add(mFinish);
            Add(mMove);
            Add(mRemove);

            mPlay.Activated += menuItemClicked;
            mStop.Activated += menuItemClicked;
            mRewind.Activated += menuItemClicked;
            mFinish.Activated += menuItemClicked;
            mMove.Activated += menuItemClicked;
            mRemove.Activated += menuItemClicked;

            Popup(null, null, null, 3, Gtk.Global.CurrentEventTime);
            ShowAll();
            
        }

        public void menuItemClicked(object o, EventArgs a)
        {
            if (o == mPlay)
            {
                player.startPlayer();
            }
            else if (o == mStop)
            {
            	player.killPlayer();
            }
            else if (o == mRewind)
            {
                player.rewindPlayer();
            }
            else if (o == mFinish)
            {
            }
            else if (o == mMove) 
            {
                FileChooserDialog fcMove = new FileChooserDialog("Choose Destination", new Window("Move File..."), FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Move", ResponseType.Accept);
                switch (fcMove.Run())
                {
                    case (int) ResponseType.Accept:
                        try
                        {
                            //File.Move does not support URIs. Normalize with UrlDecore to strip %20
                            // and Uri to strip file:///
                            String s = new Uri(HttpUtility.UrlDecode(player.getFile())).AbsolutePath;
                            String filename = System.IO.Path.GetFileName(HttpUtility.UrlDecode(player.getFile()));
                            File.Move(s, System.IO.Path.Combine(fcMove.Filename,filename) );
                            list.Remove(ref iter);
                        }
                        catch (Exception e)
                        {
                            MessageDialog md = new MessageDialog(new Window("Error Moving File"), DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Ok, e.ToString());
                            md.Run();
                            md.Destroy();
                        }
                        break;
                    case (int) ResponseType.Cancel:
                        break;
                }
                fcMove.Destroy();
            }
            else if (o == mRemove) 
            {
                list.Remove(ref iter);
            }
        }
    }
}
