using System;
using Gtk;

public class TagBrowser: Gtk.Window {
  
  public TagBrowser() : base(Gtk.WindowType.Toplevel) {

    //var splitPanel = new Gtk.Paned();

    //splitPanel.Add1(new Label("File List Place Holder"));

    var grid = new Gtk.Table(5, 6, true);

  }

  protected void OnDeleteEvent(object sender, DeleteEventArgs a) {
    Application.Quit();
    a.RetVal = true;
  }
}
