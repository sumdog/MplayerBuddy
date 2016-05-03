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

    public event EventHandler<Tag> NewTag;

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
                if(NewTag != null) {
                  NewTag(this, new Tag { Name = dialog.UserInput.Text.Trim(), Set = false });   
                }
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

    public uint Rows { 
      get { return grid.NRows; } 
      set { grid.NRows = value; } 
    } 

    public uint Cols { 
      get { return grid.NColumns; } 
      set { grid.NColumns = value; } 
    }

    public TagGrid() : base() {
      grid = new Gtk.Table(0,0,true);
      Rows = 12;
      Cols = 5;
    }

    private Gtk.Table grid;

    private void refreshWindow() {
      foreach(Widget r in grid.Children) {
        Remove(r);
        r.Destroy();
      }
      uint row = 0;
      uint col = 0;
      foreach(Tag tag in tags) { 
        
        var button = new CheckButton(tag.Name);
        button.Active = tag.Set;
        button.Clicked += TagBoxClick;

        grid.Attach(button, col % Cols, col +1 % Cols, row % Rows, row + 1 % Rows);
        col++;
        if(col % Cols == 0) {
          col = 0;
          row++;
        }
      }

      AddWithViewport(grid);
      ShowAll();
    }

    public void TagBoxClick(object sender, EventArgs e) {
      Console.WriteLine(sender.ToString());
    }


  }

  public class TagBrowser: Gtk.Window {

    private TagDB database;

    public TagDB Database { 
      get {
        return database;
      } 
      set {
        database = value;
        tagGrid.Tags = value.Tags;
      }
    }

    private TagGrid tagGrid;

    public TagBrowser() : base("Tagister.ninja") {
      
      var splitPanel = new Gtk.HPaned();

      var rightPanel = new VBox();

      tagGrid = new TagGrid();

      var bottomNav = new BottomNav();
      bottomNav.NewTag += (object sender, Tag e) => {
        Database.AddTag(e.Name);
        tagGrid.Tags = Database.Tags;
      };

      rightPanel.PackStart(tagGrid, true,true,0);
      rightPanel.PackStart(bottomNav,false,false,0);

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