using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace org.penguindreams.MplayerBuddy
{
    public class Config
    {

        private int xpos, ypos, xsize, ysize;

        private string configfile, mplayerpath, mplayerargs;

        //read in configuration settings from a file
        // if the file doesn't exist, create it
        public Config(String file)
        {
            //set our defaults
            xpos = 100;
            ypos = 100;
            xsize = 500;
            ysize = 200;
            mplayerpath = null;
            mplayerargs = "";

            configfile = file;
            FileStream fs = null;

            fs = File.Open(configfile, FileMode.OpenOrCreate, FileAccess.Read);

            //parse list file
            using (StreamReader inp = new StreamReader(fs))
            {
                String line;
                while ((line = inp.ReadLine()) != null)
                {
                    String[] parts = line.Split(':');

                    if (parts[0].Equals("XPOS"))
                    {
                        xpos = Convert.ToInt32(parts[1]);
                    }
                    else if (parts[0].Equals("YPOS"))
                    {
                        ypos = Convert.ToInt32(parts[1]);
                    }
                    else if (parts[0].Equals("XSIZE"))
                    {
                        xsize = Convert.ToInt32(parts[1]);
                    }
                    else if (parts[0].Equals("YSIZE"))
                    {
                        ysize = Convert.ToInt32(parts[1]);
                    }
                    else if (parts[0].Equals("MPLAYER"))                    
                    {
                        mplayerpath = parts[1];
                    }
                    else if (parts[0].Equals("MARGS"))
                    {
                        mplayerargs = parts[1];
                    }
                    
                }
                inp.Close();
            }
            fs.Close();
        }


        //save configuration back to file
        public bool saveConfig()
        {
            lock (configfile)
            {

                using (StreamWriter outp = new StreamWriter(configfile))
                {
                    outp.WriteLine("XPOS:{0}",xpos);
                    outp.WriteLine("YPOS:{0}",ypos);
                    outp.WriteLine("XSIZE:{0}",xsize);
                    outp.WriteLine("YSIZE:{0}",ysize);
                    outp.WriteLine("MARGS:{0}", mplayerargs);
                    if (mplayerpath != null)
                    {
                        outp.WriteLine("MPLAYER:{0}", mplayerpath);
                    }
                    outp.Flush();
                    outp.Close();
                }
            }
            return true;
        }

        //------------------
        // Begin Properities
        //------------------


        public int xPosition
        {
            get { return xpos; }
            set { xpos = value; }
        }

        public int yPosition
        {
            get { return ypos; }
            set { ypos = value; }
        }

        public int xSize
        {
            get { return xsize; }
            set { xsize = value; }
        }

        public int ySize
        {
            get { return ysize; }
            set { ysize = value; }
        }

        public string mplayerCommand
        {
            get { return mplayerpath; }
            set { mplayerpath = value; }
        }

        public string mplayerArgs
        {
            get { return mplayerargs; }
            set { mplayerargs = value; }
        }

        //--End Properities--
        

    }
}
