using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace org.penguindreams.MPlayerBuddy
{

    public class Player {
        
        public enum player_state  { STOPPED , PAUSED, PLAYING , FINISHED  };

        private player_state state;

        private float time;

        private String file;

        public Player(String file)
        {
            this.file = file;
            state = player_state.STOPPED;
            time = 0;
        }

        public Player(String file, float time)
        {
            this.file = file;
            this.time = time;
            state = player_state.STOPPED;
        }

        public Player(String file, player_state state)
        {
            this.state = state;
            this.file = file;
            time = 0;
        }

        public String getFile()
        {
            return file;
        }

        public float getTime()
        {
            return time;
        }

        public player_state getState()
        {
            return state;
        }
        
        public void killPlayer() 
        {
        	state = player_state.STOPPED;
        	
        }

        public void startPlayer()
        {
            //TODO: start player
            if (state == player_state.STOPPED)
            {
                Thread t = new Thread(new ThreadStart(spawnMPlayer));
                t.Start();
                state = player_state.PLAYING;
            }
                
        }

        public void rewindPlayer()
        {
            if (state == player_state.STOPPED)
            {
                time = 0;
            }
            else
            {
                throw new Exception("Illegal State Change");
            }
        }
        
        //mplayer sends only \r, not \n to display data
        // values on the same line. o.ReadLine() stalls because of this
        private static string readMplayerOut(StreamReader o) 
        {
          String r = "";
          while(true) 
          {
            int c = o.Read();
            if(c == -1) 
            { return r; }
            if( ((char)c) == '\r') 
            { break; }
            r += (char)c;
          }
          return r;
        }

        private void spawnMPlayer() 
        {
        	String args = "";
        	if(time != 0) {
        		args += " -ss " + time + " ";
        	}
        
            Process proc = new Process();
            proc.StartInfo.FileName = "mplayer";
            proc.StartInfo.Arguments = "\"" + HttpUtility.UrlDecode(file) + "\"" + args;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            StreamReader o = proc.StandardOutput;
            String l;
            while ( !proc.HasExited  )
            {
            	//if the GUI exits, it tells the Playlist 
            	//  to set this flag to let us know we should
            	//  exit too
            	if(state == player_state.STOPPED) {
            		proc.Kill();
            		break;
            	}
     
                l = readMplayerOut(o); //o.ReadLine() replacement
                if(l.Trim().Equals("===== PAUSE =====")) {
					state = player_state.PAUSED;
                }
                else if (l.Contains("Quit"))
                {
                    state = player_state.STOPPED;
                    break;
                }
                else if (l.Contains("End of file"))
                {
                    state = player_state.FINISHED;
                    break;
                }
                else if (l.StartsWith("A:"))
                {
                    String[] parts = l.Split(':');
                    try
                    {
                        time = (float)Convert.ToDouble(parts[1].Trim('V'));
                    }
                    catch (System.FormatException e)
                    {
                        /* don't care / discard */
                    }

                }
            }

        }


    }

    public class Playlist : Gtk.ListStore {

        private String playlist;


        /*
         * Constructs a playlist Gtk.ListStore from a file path 
         * Be sure to catch IO exceptions when calling this
         */
        public Playlist(String listfile) : base(typeof(Player))
        {
            playlist = listfile;
            FileStream fs = null;

            fs = File.Open(playlist, FileMode.OpenOrCreate, FileAccess.Read);

            //parse list file
            using (StreamReader inp = new StreamReader(fs))
            {
                String line;
                while ((line = inp.ReadLine()) != null)
                {
                    String[] parts = line.Split('|');
                    if (parts[1].Equals("F"))
                    {
                        AppendValues(new Player(parts[0],Player.player_state.FINISHED));
                    }
                    else
                    {
                        AppendValues(new Player(parts[0], (float)Convert.ToDouble(parts[1])));
                    }
                }
                inp.Close();
            }
            fs.Close();
        }

		public string iosync = "";
		
        public void writeFile()
        {
            lock (iosync)
            {

            	using(StreamWriter outp = new StreamWriter(playlist))
            	{
                	foreach (object[] row in this)
                	{
                    	Player p = (Player) row[0];
                    	String s = (p.getState() == Player.player_state.FINISHED) ? "F" : Convert.ToString(p.getTime());
                    	outp.WriteLine(p.getFile() + "|" + s);
                	}
                	outp.Flush();
                	outp.Close();
            	}
            }
        }
        
        /* kills all running mplayer processes for application exit */
        public void killPlayers() 
        {
        	foreach(object[] players in this) 
        	{
        		Player p = (Player) players[0];
        		p.killPlayer();
        	}
        }
    }


}
