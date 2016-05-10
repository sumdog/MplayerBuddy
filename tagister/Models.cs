using System.Collections.Generic;
using Gtk;
using System;
using org.penguindreams.MplayerBuddy;

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
    public long Id { get; set; }
  }

  public class TagCheckButton : CheckButton {
  
    public Tag Tag { get; set; }

    public TagCheckButton(Tag tag) : base(tag.Name) {
      Tag = tag;
    }
  }

  public class Viewer : MPVPlayer {
    public Viewer(String file) : base(new Uri(file).AbsoluteUri) {
      Console.WriteLine(file);
    }
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
