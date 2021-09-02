## Project Build Inspector
Is a Unity 3D Editor tool to facilitate the work of inspector game build files.

Unity after a succesfully build compilation, writes the build final stats in the Editor.log file, information useful like in which game files the size is partitioned,
and the included project files in the build, this information result useful if you wanna reduce your game weight, so what this tools do is fetch this information and output in a editor window where you can readably see the information along with some more information like see each file included in the build along with their respective size.

## How to use
After you create a build in your project, in the Editor top menu, go to Lovatto -> Tools Build Inspector.
In the new window, click the top left button 'Load Last' -> it may take a few seconds (or more depend on the size of your project) to fetch and build all the information,
once it finish you will see something like this:

![previewWindow](https://user-images.githubusercontent.com/16416509/119120944-6db26780-ba5f-11eb-9cdd-fc8500207f4d.png)
