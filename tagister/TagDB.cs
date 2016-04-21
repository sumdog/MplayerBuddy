using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;

namespace tagister {
  
  public class TagDB {
  
    const string dbFile = "/home/skhanna/.tagster/tagster.sqlite3";

    private IDbConnection conn;

    private void RunSQL(string sql) {
      var cmd = conn.CreateCommand();
      cmd.CommandText = sql;
      cmd.ExecuteReader();
      cmd.Dispose();
    }

    public TagDB() {
      if(!File.Exists(dbFile)) {
        SqliteConnection.CreateFile(dbFile);
      }
      conn = new SqliteConnection(String.Format("URI=file:{0}", dbFile));
      conn.Open();
      RunSQL("CREATE TABLE IF NOT EXISTS tags(id INT PRIMARY KEY NOT NULL, tag TEXT)");
      RunSQL("CREATE TABLE IF NOT EXISTS files(id INT PRIMARY KEY NOT NULL, name TEXT)");
      RunSQL("CREATE TABLE IF NOT EXISTS filetags(file_id INT, tag_id INT)");
    }

  }

}

