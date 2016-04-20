using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;

namespace tagister {
  
  public class TagDB {
  
    const string dbFile = "/home/skhanna/.tagster/tagster.sqlite3";

    private IDbConnection conn;

    public TagDB() {
      if(!File.Exists(dbFile)) {
        SqliteConnection.CreateFile(dbFile);
      }
      conn = new SqliteConnection(String.Format("URI=file:{0}", dbFile));
      conn.Open();
      var cmd = conn.CreateCommand();
      cmd.CommandText = "CREATE TABLE IF NOT EXISTS tags(id INT PRIMARY KEY NOT NULL, tag TEXT)";
      cmd.ExecuteReader();
    }

  }

}

