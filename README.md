## Project Build Inspector
Is a Unity 3D Editor tool to facilitate the work of inspector game build files.

Unity after a succesfully build compilation, writes the build final stats in the Editor.log file, information useful like in which game files the size is partitioned,
and the included project files in the build, this information result useful if you wanna reduce your game weight, so what this tools do is fetch this information and output in a editor window where you can readably see the information along with some more information like see each file included in the build along with their respective size.

## How to use
After you create a build in your project, in the Editor top menu, go to Lovatto -> Tools Build Inspector.
In the new window, click the top left button 'Load Last' -> it may take a few seconds (or more depend on the size of your project) to fetch and build all the information,
once it finish you will see something like this:

![previewWindow](Info/window-preview.png?raw=true "Build Inspector Window")

At the top, you will see all the information of your game build size and in the bottom list, you will see the heavy files included in the build, if you click on the file name it will automatically ping the file in your project view window so you can change the Import file settings to reduce the size.
