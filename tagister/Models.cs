using System.Collections.Generic;
using Gtk;
using System;

namespace tagster {

  public class FileTaggerView : TreeView {

    public FileTaggerView() {
      AppendColumn(CreateColumn("File"));
    }

    private TreeViewColumn CreateColumn(string title) {
      var c = new TreeViewColumn() { Title = title };
      CellRenderer cr = new CellRendererText();
      c.PackStart(cr, true);
      c.SetCellDataFunc(cr, new TreeCellDataFunc( 
        (TreeViewColumn col, CellRenderer cell, TreeModel m, TreeIter iter) => {
          (cell as CellRendererText).Text = ((TFile)m.GetValue(iter, 0)).File;
        }));
      return(c);
    }
  }

  public class FileTaggerList : ListStore {
  
    //TODO: nullable type?
    public FileTaggerList(List<TFile> fileList = null) : base(typeof(TFile)) {
      if(fileList != null) {
        FileList = fileList;
      }
      else {
        FileList = new List<TFile>();
      }
    }

    private List<TFile> fileList;

    public List<TFile> FileList {
      get {
        return fileList;
      }
      set {
        this.Clear();
        foreach(TFile f in value) {
          AppendValues(f);
        }
        fileList = value;
      }
    }
  
  }

  public class Tag {
    public string Name { get; set; }
    public bool Set { get; set; }
  }

  public class FileTags {
    public TFile File { get; set; }
    public List<Tag> Tags { get; set; }
  }

  public class TFile {
    public long Id { get; set; }
    public string File { get; set; }
  }

}
