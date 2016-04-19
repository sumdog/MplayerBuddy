using System;
using Gtk;

public class TagBrowser: Gtk.Window {
  
  public TagBrowser(String[] tags) : base("Tagister.ninja") {
    
    var splitPanel = new Gtk.HPaned();

    splitPanel.Add1(new Label("File List Place Holder"));

    var grid = new Gtk.Table(5, 6, true);
    foreach(string tag in tags) {
      grid.Add(new CheckButton(tag));
    }

    Add(splitPanel);
  }

  protected void OnDeleteEvent(object sender, DeleteEventArgs a) {
    Application.Quit();
    a.RetVal = true;
  }
}
