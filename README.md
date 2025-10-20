# Unity-Duelyst-Animations

![Alt Text](https://raw.githubusercontent.com/josfeldmann/Unity-Duelyst-Animations/refs/heads/main/DuelystUnits.gif)

[Duelyst](https://github.com/open-duelyst/duelyst) is an open source trading card game that has a lot of beautiful animated pixel art characters. I wanted to port those animations into Unity animation clips and animator controllers so Unity Devs could use them more easily. If you just want to download a the finished animations you can download a unitypackage file from [releases](https://github.com/josfeldmann/Unity-Duelyst-Animations/releases).

# HOW TO REGENERATE THE ANIMATION FILES
In order to use these animations in your game you may want to change some of their properties or set them up differently from how I have them. You can easily regenerate the animation files by:
  1. Opening this project in Unity
  2. Delete all existing animations under Assets\Duelyst-Sprites\Spritesheets\Units
  3. Pull up the "Duelyst Animation Importer Window" under Window > Duelyst Animation Importer

![Alt Text](https://raw.githubusercontent.com/josfeldmann/Unity-Duelyst-Animations/refs/heads/main/Tutorial1.png)
     
  4. Press the "1. Create Sprites" Button to recut the sprite sheets (This will take some time)
  5. Press the "2. Create Animations" Button to regenerate the animations (This will take som time as well)

If you want to change some of the logic I use in order to generate the files you will most likely need to examine the [DuelystAnimationImporterWindow.cs](https://github.com/josfeldmann/Unity-Duelyst-Animations/blob/main/Assets/Duelyst-Sprites/Scripts/Editor/DuelystAnimationImporterWindow.cs) file and make changes there. All of the import logic is in that script.


