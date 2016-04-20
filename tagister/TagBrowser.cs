using System;
using Gtk;

public class FileBrowser : Gtk.Container {

  private HBox FileUIComponents;


  public enum Listings {
    All, Tagged, Untagged
  }

  public FileBrowser(string directory) : base() {
    
  }

}

public class BottomNav : HBox {

  private Button newTag;

  public BottomNav() : base() {
    newTag = new Button("New Tag");

    Add(newTag);
  }
}

public class TagBrowser: Gtk.Window {
  
  public TagBrowser(String[] tags) : base("Tagister.ninja") {
    
    var splitPanel = new Gtk.HPaned();

    var rightPanel = new VBox();
    rightPanel.PackStart(TagGrid(tags),true,true,0);
    rightPanel.PackStart(new BottomNav(),false,false,0);

    splitPanel.Add1(new Label("File List Place Holder"));
    splitPanel.Add2(rightPanel);

    Add(splitPanel);
  }

  private Container TagGrid(string[] tags) {
    const uint MAX_ROWS = 12;
    const uint MAX_COLS = 6;
    var grid = new Gtk.Table(MAX_ROWS, MAX_COLS, true);
    uint row = 0;
    uint col = 0;
    foreach(string tag in tags) {
      Console.WriteLine("Tag: " + tag + "\tRow: " + row + "\tCol: " + col);
      grid.Attach(new CheckButton(tag),col % MAX_COLS, col +1 % MAX_COLS, row % MAX_ROWS, row + 1 % MAX_ROWS);
      col++;
      if(col % MAX_COLS == 0) {
        col = 0;
        row++;
      }
    }
    var scroller = new ScrolledWindow();
    scroller.AddWithViewport(grid);
    return scroller;
  }

  protected void OnDeleteEvent(object sender, DeleteEventArgs a) {
    Application.Quit();
    a.RetVal = true;
  }
}
