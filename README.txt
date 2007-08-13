MplayerBuddy v0.5
-------------------------

written by Sumit Khanna
Licensed as GNU GPL 
http://penguindreams.org
sumdog@gmail.com


Description
--------------
MplayerBuddy is a media bookmarking application designed to keep track of how far a viewer has watching videos. It uses mplayer as its back-end media player, is written in C# using the gtksharp toolkit, is designed to run with mono on Linux and is excellent for people with very short attention spans who constantly start and stop several videos like myself.


Usage
-------
Start MplayerBuddy (using the command mono ./MplayerBuddy.exe in the current working directory) and then drag files into the playlist window. Double clicking on a file starts mplayer. Right clicking brings up a context menu. 


Requirements
----------------
MplayerBuddy requires mono, gtk-sharp 2 and mplayer. I have tested this application on a Gentoo Linux system. It does build using Microsoft's .NET environment so long as gtk-sharp is installed, however I have not tested it using a windows build of mplayer. 


Development 
----------------
The package contains both monodevelop project files as well as Visual Studio 2005 project files so it can be developed on either platform. An nant build file is also included. 


Known Issues
-----------------
When exiting the application, the mplayer windows will remain open. As soon as you unpause them they do exit. I'm not sure why mono isn't completely killing the process. 

You can add the same video file twice. I'll fix this in a later release.

The video listing doesn't refresh on its own. You must resize the window or do something which causes the TreeView to refresh. 


Disclaimer 
------------------
This is free software and on top of that, it is beta software. Use at your own risk. This software comes with no warranty implied or otherwise. 

