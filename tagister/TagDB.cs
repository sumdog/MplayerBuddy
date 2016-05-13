using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using System.Linq;
using System.Collections.Generic;

namespace tagster {
  
  public class TagDB {

    string Home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

    private SqliteConnection conn;

    private List<Dictionary<string,object>> RunSQL(string sql, List<object> args = null) {
      using(var cmd = new SqliteCommand(sql, conn)) {

        if(args != null) {
          for(int x=0; x < args.Count(); x++) {
            cmd.Parameters.AddWithValue(String.Format("@{0}",x), args[x]);
          }
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
      RunSQL("PRAGMA foreign_keys=1");
      RunSQL("CREATE TABLE IF NOT EXISTS tags(id INTEGER PRIMARY KEY, tag TEXT NOT NULL, UNIQUE(tag))");
      RunSQL("CREATE TABLE IF NOT EXISTS files(id INTEGER PRIMARY KEY, name TEXT NOT NULL, UNIQUE(name))");
      RunSQL("CREATE TABLE IF NOT EXISTS filetags(file_id INT NOT NULL, tag_id INT NOT NULL, UNIQUE(file_id, tag_id), FOREIGN KEY(file_id) REFERENCES files(id), FOREIGN KEY(tag_id) REFERENCES tags(id))");
    }

    public List<Tag> Tags {
      get {
        return RunSQL("SELECT * FROM tags").Select( row => new Tag() { Id = Convert.ToInt64(row["id"]), Name = row["tag"].ToString(), Set = false } ).ToList(); 
      }
    }

    public void AddTag(string tag) {
      RunSQL("INSERT INTO tags(tag) VALUES(@0)", new List<object>{tag});  
    }

    public void UpdateTag(TFile file, Tag tag) {
      RunSQL((tag.Set) ? 
        "INSERT INTO filetags(file_id, tag_id) VALUES(@0, @1)" : 
        "DELETE FROM filetags WHERE file_id=@0 AND tag_id=@1", new List<object> {file.Id, tag.Id} );
    }

    public List<Tag> TagsForFile(TFile file) {
      return RunSQL(@"SELECT f.id AS file_id, f.name AS name, t.tag AS tag, t.id AS tag_id, (ft.file_id IS NOT NULL) AS file_has_tag 
                      FROM files f CROSS JOIN tags t LEFT JOIN filetags ft ON ft.file_id=f.id AND ft.tag_id=t.id 
                      WHERE f.id=@0 ORDER BY t.tag",
        new List<object> { file.Id }).Select( a => 
          new Tag() { Name = a["tag"].ToString(), Set = (a["file_has_tag"].ToString() == "1"), Id = Convert.ToInt64(a["tag_id"]) } 
        ).ToList();
    }

    const string NUM_TAGS_SQL = "SELECT f.id AS file_id, f.name AS name, SUM(ft.file_id IS NOT NULL) AS file_has_tag FROM files f ";

    public List<TFile> ListFiles() {
      return RunSQL("SELECT * FROM files").Select( row => new TFile { Id = Convert.ToInt64(row["id"]), File = row["name"].ToString() } ).ToList();
    }

  }

}