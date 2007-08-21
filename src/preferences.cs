/*
 * preferences.cs - MplayerBuddy
 * author: Sumit Khanna
 * penguindreams.org (see site for licence)
 *
 * Contains the Preference Window for MplayerBuddy
 *    
 */
using System;
using System.Collections.Generic;
using System.Text;
using Gtk;

namespace org.penguindreams.MplayerBuddy
{
    public class Preferences : Window
    {
    
        private VBox mainbox;
        
        private CheckButton saveOnExit, customPath;
        
        private Button ok, cancel;
        
        private Entry mplayerPath, mplayerArgs;
                  
        public Preferences() : base("MplayerBuddy Preferences") {
        
            //initalize 
            mainbox = new VBox();
            saveOnExit = new CheckButton("Save Window Position on Exit");
            customPath = new CheckButton("Custom mplayer Path");
            ok = new Button(Stock.Ok);
            cancel = new Button(Stock.Cancel);
            mplayerPath = new Entry(MplayerBuddy.conf.mplayerCommand);
            mplayerArgs = new Entry(MplayerBuddy.conf.mplayerArgs);
            
            //button box
            ButtonBox buttons = new HButtonBox();
            buttons.Add(ok);
            buttons.Add(cancel);
            
            //pull current values
            saveOnExit.Active = MplayerBuddy.conf.saveSettingsOnExit;
            customPath.Active = MplayerBuddy.conf.useCustomPath;
            mplayerPath.Sensitive = customPath.Active;

            mplayerArgs.Text = MplayerBuddy.conf.mplayerArgs;
            mplayerPath.Text = (MplayerBuddy.conf.mplayerCommand != null) ? MplayerBuddy.conf.mplayerCommand : "";
            
            //event handlers
            ok.Clicked += onClick;
            cancel.Clicked += onClick;
            customPath.Toggled += onToggle;
            
            //put stuff together
            mainbox.PackStart(saveOnExit);
            mainbox.PackStart(customPath);
            mainbox.PackStart(mplayerPath);
            mainbox.PackStart(new Label("Mplayer Arguments"));
            mainbox.PackStart(mplayerArgs);
            mainbox.PackStart(buttons);
            
            //layout
            Add(mainbox);
            Resize(250, 200);
            Modal = true;
        }
        
        private void writeSettingsToConfig() {
            Config c = MplayerBuddy.conf;
            
            c.mplayerArgs = mplayerArgs.Text;
            c.mplayerCommand = mplayerPath.Text;
            c.saveSettingsOnExit = saveOnExit.Active;
            c.useCustomPath = customPath.Active;
        }
        
        public void onToggle(object o, EventArgs a) {
            if(o == customPath) {
                mplayerPath.Sensitive = customPath.Active;
            }
        }
        
        public void onClick(object o, EventArgs a) {
            if(o == ok) {
                writeSettingsToConfig();
                MplayerBuddy.conf.saveConfig();
                Destroy();
            }
            else if (o == cancel) {
                Destroy();
            }
        }
    
    }
}
