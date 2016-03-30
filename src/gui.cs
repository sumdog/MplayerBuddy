
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


namespace org.penguindreams.MplayerBuddy
{

    public class Gui : Window
    {

        private TreeView tree;
        private ListStore store;

        private ScrolledWindow scroller;

        private MainMenu menu;

        private VBox mainbox;

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
            Resize(MplayerBuddy.conf.xSize, MplayerBuddy.conf.ySize);
            Move(MplayerBuddy.conf.xPosition,MplayerBuddy.conf.yPosition);

        }


        public void windowClosed(object o,DeleteEventArgs a)
        {
            ((Playlist)store).killPlayers();
            if(MplayerBuddy.conf.saveSettingsOnExit) {
                int xsize, ysize, xpos, ypos;
                GetPosition(out xpos,out ypos);
                GetSize(out xsize, out ysize);
                MplayerBuddy.conf.xSize = xsize;
                MplayerBuddy.conf.ySize = ysize;
                MplayerBuddy.conf.xPosition = xpos;
                MplayerBuddy.conf.yPosition = ypos;
                MplayerBuddy.conf.saveConfig();
            }
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
            miExit = new ImageMenuItem(Stock.Quit, null);
            mFile.Add(miExit);

            //build config menu
            miPreferences = new ImageMenuItem(Stock.Preferences, null);
            mConfig.Add(miPreferences);

            //build help menu
            miAbout = new ImageMenuItem(Stock.About, null);
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
                about.ProgramName = "MplayerBuddy";
                about.Website = "http://penguindreams.org";
                about.Show();
            }
            else if (o == miPreferences) {
                Preferences prefs = new Preferences();
                prefs.ShowAll();
            }
        }
    }

    class FilmPopup : Menu
    {
        private MenuItem mPlay, mStop, mRewind, mFinish, mMove, mRemove, mDelete;

        private ListStore list;

        private Player player;

        private TreeIter iter;

        public FilmPopup(Player p, ListStore store, TreeIter i) : base()
        {
            player = p;
            list = store;
            iter = i;
            
            mPlay = new ImageMenuItem(Stock.MediaPlay,null);
            mStop = new ImageMenuItem(Stock.MediaStop,null);
            mRewind = new ImageMenuItem(Stock.MediaRewind, null);
            mDelete = new ImageMenuItem(Stock.Delete,null);
			
            mFinish = new MenuItem("Finish");
            mMove = new MenuItem("Move");
            mRemove = new MenuItem("Remove");			

            switch (p.getState())
            {
            	case Player.player_state.ERROR:
                	mStop.Sensitive = false;
                	mFinish.Sensitive = false;
                	mMove.Sensitive = false;
                	mRewind.Sensitive = false;
            		break;
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
			Add(mDelete);

            mPlay.Activated += menuItemClicked;
            mStop.Activated += menuItemClicked;
            mRewind.Activated += menuItemClicked;
            mFinish.Activated += menuItemClicked;
            mMove.Activated += menuItemClicked;
            mRemove.Activated += menuItemClicked;
			mDelete.Activated += menuItemClicked;

            Popup(null, null, null, 3, Gtk.Global.CurrentEventTime);
            
        }

        public void menuItemClicked(object o, EventArgs a)
        {
            if (o == mPlay)
            {
            	try {
                	player.startPlayer();
                }
                catch(FileNotFoundException) {
                	MessageDialog md = new MessageDialog(new Window("Could not start mplayer"),DialogFlags.DestroyWithParent,
                		MessageType.Error,ButtonsType.YesNo,"The file {0} does not exist. Would you like to remove it from the playlist?",
                		player.getFileName());
                	if(md.Run() == (int) ResponseType.Yes) {
						list.Remove(ref iter);
                	}
                	md.Destroy();
                }
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
            	player.finishPlayer();
            }
            else if (o == mMove) 
            {
                FileChooserDialog fcMove = new FileChooserDialog("Choose Destination", new Window("Move File..."), FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Move", ResponseType.Accept);
                switch (fcMove.Run())
                {
                    case (int) ResponseType.Accept:
                        try
                        {
                            //File.Move does not support URIs. Normalize
                            File.Move(player.getNormlaizedFile(), System.IO.Path.Combine(fcMove.Filename,player.getFileName()) );
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
			else if(o == mDelete) 
			{
				MessageDialog delConfirm = new MessageDialog(new Window("Confirm Delete"), DialogFlags.DestroyWithParent, MessageType.Question, 
				         ButtonsType.YesNo, "Are you sure you want to delete {0}?",player.getFileName());
				if(delConfirm.Run() == (int) ResponseType.Yes) {
					try {
						File.Delete(player.getNormlaizedFile());
						list.Remove(ref iter);
					}
					catch(Exception e) {
                            MessageDialog md = new MessageDialog(new Window("Error Deleting File"), DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Ok, e.ToString());
                            md.Run();
                            md.Destroy();
					}
				}
				delConfirm.Destroy();
			}
        }
    }
}
