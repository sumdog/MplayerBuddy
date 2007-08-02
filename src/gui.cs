
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
            store = p;
            tree = new PlayerTree(p);

            //event handlers
            this.DeleteEvent += windowClosed;

            //refresh treeview every 1/10th of a second
            //GLib.Timeout.Add(100, new GLib.TimeoutHandler(refreshTreeView));

            //window scrolling support
            scroller = new ScrolledWindow();
            scroller.Add(tree);

            Add(scroller);
            Resize(200, 200);
            ShowAll();
        }


        //check to see if the files we loaded still exist
        public void consistanceCheck()
        {
            /*
            foreach (object[] row in this)
            {
                Player p = (Player)row[0];
                if (!File.Exists(p.getFile))
                {
                    MessageDialog fileMsg = new MessageDialog(new Window("File Not Found"), DialogFlags.DestroyWithParent,
MessageType.Warning, ButtonsType.YesNo, "The file ? was not found");

                }
            }
             */
        }

 

        public void windowClosed(object o,DeleteEventArgs a)
        {
            ((Playlist)store).killPlayers();
            //MPLayerBuddy.savePlaylist();
            Application.Quit();
        }

    }


    public class PlayerTree : TreeView
    {

        private Playlist playlist;

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

        public PlayerTree(Playlist p)
        {
            playlist = p;

            Gtk.TreeViewColumn colFilm = new TreeViewColumn();
            colFilm.Title = "Film";
            Gtk.TreeViewColumn colTime = new TreeViewColumn();
            colTime.Title = "Time";

            this.AppendColumn(colFilm);
            this.AppendColumn(colTime);

            CellRenderer colFilmCel = new CellRendererText();
            colFilm.PackStart(colFilmCel, true);
            CellRenderer colTimeCel = new CellRendererText();
            colTime.PackStart(colTimeCel, true);

            colFilm.SetCellDataFunc(colFilmCel, new TreeCellDataFunc(renderFilmName));
            colTime.SetCellDataFunc(colTimeCel, new TreeCellDataFunc(renderTime));

            this.Model = playlist;


            //setup Drag'n Drop
            //target_table 
            Gtk.Drag.DestSet(this, Gtk.DestDefaults.All, target_table,
                Gdk.DragAction.Copy | Gdk.DragAction.Move | Gdk.DragAction.Link);
            this.DragDataReceived += new DragDataReceivedHandler(dataReceived);

        }

        protected override bool OnButtonPressEvent(Gdk.EventButton evnt)
        {
            TreeIter i;
            TreeSelection s = this.Selection;
            s.GetSelected(out i);

            if (evnt.Type == Gdk.EventType.TwoButtonPress && evnt.Button == 1)
            {
                try
                {
                    Player p = (Player)this.Model.GetValue(i, 0);
                    p.startPlayer();
                }
                catch (System.Exception)
                {
                    /* no item was selected. do nothing. */
                }
            }
            else if (evnt.Type == Gdk.EventType.ButtonPress && evnt.Button == 3)
            {
                try
                {
                    Player p = (Player)this.Model.GetValue(i, 0);
                    FilmPopup pop = new FilmPopup(p, playlist, i);
                }
                catch (System.Exception)
                {
                    /* no item was selected. do nothing */
                }
            }

            return base.OnButtonPressEvent(evnt);
        }


        private void renderFilmName(TreeViewColumn col, CellRenderer cell, TreeModel m, TreeIter iter)
        {
            Player p = (Player)m.GetValue(iter, 0);
            String f = HttpUtility.UrlDecode(p.getFile());
            (cell as CellRendererText).Text = System.IO.Path.GetFileName(f);
        }

        private void renderTime(TreeViewColumn col, CellRenderer cell, TreeModel m, TreeIter iter)
        {
            Player p = (Player)m.GetValue(iter, 0);

            if (p.getState() == Player.player_state.FINISHED)
            {
                (cell as CellRendererText).Text = "done";
            }
            else
            {

                float t = p.getTime();
                int hour = (int)(t / 3600);
                int min = (int)(t - hour * 3600) / 60;
                int sec = (int)(t - (hour * 3600) - (min * 60));

                String time = Convert.ToString(hour).PadLeft(2, '0') + ":" +
                    Convert.ToString(min).PadLeft(2, '0') + ":" +
                    Convert.ToString(sec).PadLeft(2, '0');

                (cell as CellRendererText).Text = time;
            }
        }

        public void dataReceived(object o, DragDataReceivedArgs a)
        {
            String data = System.Text.Encoding.UTF8.GetString(a.SelectionData.Data);

            switch (a.Info)
            {
                case (uint)drop_types.TARGET_STRING: //nautilus/gnome
                case (uint)drop_types.TARGET_URL:    //explorer/Win32
                    string[] uri_list = Regex.Split(data, "\r\n");
                    foreach (string u in uri_list)
                    {
                        if (u.Length > 1)
                        {
                            playlist.AppendValues(new Player(u, (float)0));
                        }
                    }
                    break;
            }
            Gtk.Drag.Finish(a.Context, true, false, a.Time);
        }
    }


    public class FilmPopup : Menu
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
                            //String s = HttpUtility.UrlDecode(player.getFile());
                            String s = new Uri(HttpUtility.UrlDecode(player.getFile())).AbsolutePath;
                            File.Move(s, fcMove.Filename);
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
