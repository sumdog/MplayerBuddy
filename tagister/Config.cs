using System;
using System.IO;

namespace tagster {
  
  public class Config {

    public string RepositoryPath { get; set; }

    public string LinksPath { get; set; }

    public string ConfigFile { get; set; }

    public Config(String file)
    {
      ConfigFile = file;
      var fs = File.Open(file, FileMode.OpenOrCreate, FileAccess.Read);

      //parse list file
      using (StreamReader inp = new StreamReader(fs))
      {
        String line;
        while ((line = inp.ReadLine()) != null)
        {
          String[] parts = line.Split(':');

          if (parts[0].Equals("RepositoryPath"))
          {
            RepositoryPath = parts[1];
          }
          else if (parts[0].Equals("LinksPath"))
          {
            LinksPath = parts[1];
          }
        }
        inp.Close();
      }
      fs.Close();
    }


    //save configuration back to file
    public bool SaveConfig()
    {
      lock (ConfigFile)
      {

        using (StreamWriter outp = new StreamWriter(ConfigFile))
        {
          outp.WriteLine("RepositoryPath:{0}", RepositoryPath);
          outp.WriteLine("LinksPath:{0}", LinksPath);
          outp.Flush();
          outp.Close();
        }
      }
      return true;
    }
  }
}

