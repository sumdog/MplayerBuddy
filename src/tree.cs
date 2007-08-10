/*
 * tree.cs - MplayerBuddy
 * author: Sumit Khanna
 * penguindreams.org (see site for licence)
 *
 * Contains the TreeView object used by MplayerBuddy
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
        	//we need to do this first so we get the current
        	//item and not the previous item in the tree
        	bool ret = base.OnButtonPressEvent(evnt);
        	
            TreeIter i;
            TreeSelection s = this.Selection;
            s.GetSelected(out i);
            Player p = (Player)this.Model.GetValue(i, 0);

			//double click main button (Play)
            if (evnt.Type == Gdk.EventType.TwoButtonPress && evnt.Button == 1)
            {
                try
                {
                    p.startPlayer();
                }
                catch (System.Exception)
                {
                    /* no item was selected. do nothing. */
                }
            }
            //secondary button (popup menu)
            else if (evnt.Type == Gdk.EventType.ButtonPress && evnt.Button == 3)
            {
                try
                {
                    FilmPopup pop = new FilmPopup(p, playlist, i);
                }
                catch (System.Exception)
                {
                    /* no item was selected. do nothing */
                }
            }

            return ret;
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
	

}
