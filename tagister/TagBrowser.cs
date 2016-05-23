using System;
using Gtk;
using System.Collections.Generic;
using org.penguindreams.MplayerBuddy;

namespace tagster {

  public class DisplayMessage {
  
    public static void ShowError(string msg) {
      var m = new MessageDialog(new Gtk.Window("Error"),DialogFlags.Modal,MessageType.Error,ButtonsType.Ok,msg);
      m.Run();
      m.Destroy();
    }

  }

  public class UserInputDialog : Dialog {

    public Entry UserInput;

    public UserInputDialog() : base() {
      Title = "New Tag";
      UserInput = new Entry();
      VBox.Add(UserInput);
      AddButton(Stock.Cancel, ResponseType.Cancel);
      AddButton(Stock.Ok, ResponseType.Ok); 
      ShowAll();
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
                DisplayMessage.ShowError("Invalid Tag");
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

    private Table grid;

    public event EventHandler<Tag> TagChange;

    private void refreshWindow() {
      foreach(Widget r in grid.Children) {
        Remove(r);
        r.Destroy();
      }
      uint row = 0;
      uint col = 0;
      foreach(Tag tag in tags) { 
        
        var button = new TagCheckButton(tag);
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
      if(TagChange != null) {
        var t = ((TagCheckButton)sender);
        t.Tag.Set = t.Active;
        TagChange(this, t.Tag);
      }
    }


  }

  public class TagBrowser: Window {

    private TagDB database;

    private FileTaggerView tree;

    public MPVWindow MPV { get; set; }

    public enum TagListing { All, Tagged, Untagged, Custom };

    private TagListing currentListing;

    public TagDB Database { 
      get {
        return database;
      } 
      set {
        database = value;
        tagGrid.Tags = value.Tags;
        tree.Model = new FileTaggerList( value.ListFiles() );
      }
    }

    private TagGrid tagGrid;

    public TagBrowser() : base("Tagister.ninja") {
      
      var splitPanel = new Gtk.HPaned();

      var rightPanel = new VBox();
      var leftPanel = new VBox();

      currentListing = TagListing.All;
      var cbListing = new ComboBox(new string[] { "All", "Tagged", "Untagged" });

      tagGrid = new TagGrid();
      tree = new FileTaggerView();

      leftPanel.PackStart(cbListing, false, true, 0);
      var sw = new ScrolledWindow();
      sw.Add(tree);
      leftPanel.PackStart(sw, true, true, 0);

      #region events

      cbListing.Changed += (object sender, EventArgs e) => {
        List<TFile> newFiles = null;
        switch(cbListing.ActiveText) {
          case "All":
            newFiles = database.ListFiles();
            break;
          case "Tagged":
            newFiles = database.ListFiles(true);
            break;
          case "Untagged":
            newFiles = database.ListFiles(false);
            break;
        }
        ((FileTaggerList)tree.Model).FileList = newFiles;
      };

      tree.Selection.Changed += (object o, EventArgs args) => {
        var s = SelectedFile();
        if(s != null) {
          tagGrid.Tags = database.TagsForFile(s);
          if(MPV != null) 
            MPV.LoadPlayer(new Viewer(System.IO.Path.Combine(Tagster.RepositoryRoot, s.File)));
        }
      };
        
      tagGrid.TagChange += (object sender, Tag e) => {
        var s = SelectedFile();
        if(s != null)
          database.UpdateTag(s, e);
      };

      var bottomNav = new BottomNav();
      bottomNav.NewTag += (object sender, Tag e) => {

        if(Database.Tags.Find( t => t.Name == e.Name ) != null) {
          DisplayMessage.ShowError("Tag already exists");
        }
        else {
          Database.AddTag(e.Name);
          var s = SelectedFile();
          if(s != null)
            tagGrid.Tags = database.TagsForFile(s);
          else
            tagGrid.Tags = Database.Tags;
        }
      };

      #endregion

      rightPanel.PackStart(tagGrid, true,true,0);
      rightPanel.PackStart(bottomNav,false,false,0);

      splitPanel.Add1(leftPanel);
      splitPanel.Add2(rightPanel);
      splitPanel.Position = 300;

      Add(splitPanel);
    }

    private TFile SelectedFile() {
      TreeIter selected;
      if(tree.Selection.GetSelected(out selected)) {
        return (TFile) tree.Model.GetValue(selected, 0);
      }   
      return null;
    }
      

    protected void OnDeleteEvent(object sender, DeleteEventArgs a) {
      Application.Quit();
      a.RetVal = true;
    }
  }

}