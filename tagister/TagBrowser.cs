using System;
using Gtk;
namespace tagster {
  
  public class FileBrowser : Gtk.Container {

    private HBox FileUIComponents;


    public enum Listings {
      All, Tagged, Untagged
    }

    public FileBrowser(string directory) : base() {
      
    }

  }

  public class UserInputDialog : Gtk.Dialog {

    public Entry UserInput;

    public UserInputDialog() : base() {
      Title = "New Tag";
      UserInput = new Entry();
      VBox.Add(UserInput);
      AddButton(Stock.Cancel, ResponseType.Cancel);
      AddButton(Stock.Ok, ResponseType.Ok); 
      ShowAll();
      /*this.Response += (object obj, ResponseArgs a) => {
        Respond(a.ResponseId);
      };*/
    }
  }

  public class BottomNav : HBox {

    private Button newTag;

    public BottomNav() : base() {
      newTag = new Button("New Tag");

      newTag.Clicked += (object sender, EventArgs e) => {
        using(var dialog = new UserInputDialog()) {
          switch((ResponseType) dialog.Run()) {
            case ResponseType.Ok:
              if(dialog.UserInput.Text.Trim() == "") {
                var m = new MessageDialog(new Gtk.Window("Invalid Tag"),DialogFlags.Modal,MessageType.Error,ButtonsType.Ok,"Invalid Tag");
                m.Run();
                m.Destroy();
              }
              else {
                tagister.Tagster.db.AddTag(dialog.UserInput.Text.Trim());   
              }
              break;
            default:
              break;
          }
          dialog.Destroy();
        }
      };

      Add(newTag);
    }
  }

  public class TagGrid : Container {

    public List<Tag>

    public TagGrid() : base() {
      
    }


  }

  public class TagBrowser: Gtk.Window {

    public TagDB Database { get; set; }

    public TagBrowser() : base("Tagister.ninja") {
      
      var splitPanel = new Gtk.HPaned();

      var rightPanel = new VBox();
      rightPanel.PackStart(TagGrid(),true,true,0);
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

}