using System;
using Gtk;
using System.Collections.Generic;

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

  public class TagGrid : ScrolledWindow {

    private List<Tag> tags;

    public List<Tag> Tags {
      set {
        tags = value;
        refreshWindow();
      }
    }

    public uint Rows { get; set; } 

    public uint Cols { get; set; }

    public TagGrid() : base() {
      Rows = 12;
      Cols = 5;
    }

    private void refreshWindow() {
      var grid = new Gtk.Table(Rows, Cols, true);
      uint row = 0;
      uint col = 0;
      foreach(Tag tag in tags) { 
        
        var button = new CheckButton(tag.Name);
        button.Active = tag.Set;
        button.Click += TagBoxClick;

        grid.Attach(button, col % Cols, col +1 % Cols, row % Rows, row + 1 % Rows);
        col++;
        if(col % Cols == 0) {
          col = 0;
          row++;
        }
      }
      AddWithViewport(grid);
    }

    public void TagBoxClick() {
    }


  }

  public class TagBrowser: Gtk.Window {

    public TagDB Database { get; set; }

    public TagBrowser() : base("Tagister.ninja") {
      
      var splitPanel = new Gtk.HPaned();

      var rightPanel = new VBox();

      var tagGrid = new TagGrid();

      rightPanel.PackStart(tagGrid, true,true,0);
      rightPanel.PackStart(new BottomNav(),false,false,0);

      splitPanel.Add1(new Label("File List Place Holder"));
      splitPanel.Add2(rightPanel);

      Add(splitPanel);
    }
      

    protected void OnDeleteEvent(object sender, DeleteEventArgs a) {
      Application.Quit();
      a.RetVal = true;
    }
  }

}