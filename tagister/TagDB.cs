using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;

namespace tagister {
  
  public class TagDB {

    string Home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

    private IDbConnection conn;

    private void RunSQL(string sql) {
      var cmd = conn.CreateCommand();
      cmd.CommandText = sql;
      var r = cmd.ExecuteReader();
      cmd.Dispose();
      //return r;
    }

    public TagDB() {

      var dbFile = Path.Combine(Home, ".tagster/tagster.sqlite3");

      if(!File.Exists(dbFile)) {
        SqliteConnection.CreateFile(dbFile);
      }
      using(conn = new SqliteConnection(String.Format("URI=file:{0}", dbFile))){;
        conn.Open();
        RunSQL("CREATE TABLE IF NOT EXISTS tags(id INT PRIMARY KEY NOT NULL, tag TEXT)");
        RunSQL("CREATE TABLE IF NOT EXISTS files(id INT PRIMARY KEY NOT NULL, name TEXT)");
        RunSQL("CREATE TABLE IF NOT EXISTS filetags(file_id INT, tag_id INT)");
        conn.Close();
      }
    }

    public String[] Tags {
      get {
        RunSQL("SELECT * FROM tags"); 
        return new String[] { "", "" };
      }
    }

  }

}

