﻿using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using System.Linq;
using System.Collections.Generic;

namespace tagister {

  public class Tag {
    public string Name { get; set; }
    public bool Set { get; set; }
  }

  public class FileTags {
    public string File { get; set; }
    public List<Tag> Tags { get; set; }
  }
  
  public class TagDB {

    string Home = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

    private IDbConnection conn;

    private List<Dictionary<string,object>> RunSQL(string sql, List<object> args = null) {
      using(var cmd = conn.CreateCommand()) {
        cmd.CommandText = sql;

        if(args != null) {
          //args.ForEach(a => Console.WriteLine(a));
          //args.ForEach(a => cmd.Parameters.Add(a.ToString()));
          for(int x=0; x < args.Count(); x++) {
            cmd.Parameters.Add(args[x]);
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

    public List<String> Tags {
      get {
        return RunSQL("SELECT * FROM tags").Select( row => row["tag"].ToString() ).ToList(); 
      }
    }

    public void AddTag(string tag) {
      RunSQL("INSERT INTO tags(tag) VALUES(:1)", new List<object>{tag});  
    }

    public FileTags FileTags(string file) {
      RunSQL("SELECT * FROM tags t LEFT OUTER JOIN filetags ft ON ft.tag_id=t.id LEFT OUTER JOIN files f ON f.id=ft.file_id WHERE f.name=:1 OR f.name IS NULL",
        new List<object> { file }).ForEach(a => Console.WriteLine(a));
      return null;
    }

  }

}
/**
 * pastebin to implement:
 * 
 * 
 * CREATE TABLE IF NOT EXISTS tags(id INTEGER PRIMARY KEY, tag TEXT NOT NULL, UNIQUE(tag));
CREATE TABLE IF NOT EXISTS files(id INTEGER PRIMARY KEY, name TEXT NOT NULL, UNIQUE(name));
CREATE TABLE IF NOT EXISTS filetags(file_id INT NOT NULL, tag_id INT NOT NULL, 
      UNIQUE(file_id, tag_id), FOREIGN KEY(file_id) REFERENCES files(id), FOREIGN KEY(tag_id) REFERENCES tags(id));

INSERT INTO files(name) VALUES('somefile.mp4');

INSERT INTO tags(tag) VALUES('car');
INSERT INTO tags(tag) VALUES('boat');

INSERT INTO filetags VALUES(1,2);


-- How do I get a selection of all files with all tags?
-- (with nulls if a file has no tags)
-- The following gets close to what I want:

SELECT * FROM tags t LEFT OUTER JOIN filetags ft ON ft.tag_id=t.id;

-- but if I try to join the file name, I lose the nulls:

SELECT * FROM tags t LEFT OUTER JOIN filetags ft ON ft.tag_id=t.id LEFT OUTER JOIN files f ON f.id=ft.file_id WHERE f.name='somefile.mp4';


and PRAGMA foreign_keys=1
*/

