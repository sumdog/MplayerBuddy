using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using System.Linq;
using System.Collections.Generic;

namespace tagister {
  
  public class TagDB {

    string Home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

    private IDbConnection conn;

    private List<Dictionary<string,object>> RunSQL(string sql, List<object> args = null) {
      using(var cmd = conn.CreateCommand()) {
        cmd.CommandText = sql;

        if(args != null) {
          args.ForEach(a => cmd.Parameters.Add(a));
        }

        using(IDataReader rdr = cmd.ExecuteReader()) {
          var columns = Enumerable.Range(0, rdr.FieldCount).Select(rdr.GetName).ToList();

          var retList = new List<Dictionary<string,object>>();

          while(rdr.Read()) {
            var fieldMap = new Dictionary<string,object>();
            for(int i = 0; i < rdr.FieldCount; i++) {
              fieldMap[columns[i]] = rdr.GetValue(i);
            }
            retList.Add(fieldMap);
          }

          return retList;
        }
      }
    }

    public TagDB() {

      var dbFile = Path.Combine(Home, ".tagster/tagster.sqlite3");

      if(!File.Exists(dbFile)) {
        SqliteConnection.CreateFile(dbFile);
      }
      conn = new SqliteConnection(String.Format("URI=file:{0}", dbFile));
      conn.Open();
      RunSQL("CREATE TABLE IF NOT EXISTS tags(id INT PRIMARY KEY NOT NULL, tag TEXT)");
      RunSQL("CREATE TABLE IF NOT EXISTS files(id INT PRIMARY KEY NOT NULL, name TEXT)");
      RunSQL("CREATE TABLE IF NOT EXISTS filetags(file_id INT, tag_id INT)");
    }

    public List<String> Tags {
      get {
        return RunSQL("SELECT * FROM tags").Select( row => row["tag"].ToString() ).ToList(); 
      }
    }

    public void AddTag(string tag) {
      RunSQL("INSERT INTO tags(tag) VALUES(:1)", new List<object>{tag});  
    }

  }

}

