using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;


[System.Serializable]
public class AnimationData {

    public string name;
    public string filepathname;


    public Dictionary<string, List<FrameData>> animations = new Dictionary<string, List<FrameData>>();



}

[System.Serializable]
public class FrameData {

    public string frameName;
    public Tuple<Vector2, Vector2> frameCoord;
    public Vector2 offset;
    public bool rotated = false;
    public Vector2 sourceColor;
    public Vector2 sourceSize;

    public override string ToString() {
        return "Frame Name: " + frameName +
               " Frame Coord: " + frameCoord.ToString() +
               " Offset: " + offset.ToString() +
               " Rotated: " + rotated.ToString() +
               " Source Color: " + sourceColor.ToString() +
               " Source Size: " + sourceSize.ToString();
    }


}



public class DuelystAnimationImporterWindow : EditorWindow
{

    [MenuItem("Window/Duelyst Animation Importer")]

    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(DuelystAnimationImporterWindow));
    }


    private void OnGUI() {
        GUILayout.Label("Duelyst Animation Importer", EditorStyles.boldLabel);

        XMLDirectoryAssetPath = EditorGUILayout.TextField("XML Directory", XMLDirectoryAssetPath);
        PNGDirectoryAssetPath = EditorGUILayout.TextField("PNG Directory", PNGDirectoryAssetPath);
        exportPath = EditorGUILayout.TextField("Export Directory", exportPath);

        spritePivot = EditorGUILayout.Vector2Field("Sprite Pivot", spritePivot);
        clipFrameRate = EditorGUILayout.FloatField("Animation Frame Rate", clipFrameRate);
        firstX = EditorGUILayout.IntField("First X", firstX);

        if (GUILayout.Button("1. Create Sprite Sheets")) CreateSpriteSheets();
        if (GUILayout.Button("2. Create Animations")) CreateAnimations();

    }




    public string XMLDirectoryAssetPath = "Duelyst-Sprites\\Scripts\\XMLS\\";
    public string PNGDirectoryAssetPath = "Duelyst-Sprites\\Spritesheets\\Units";
    public string exportPath = "Duelyst-Sprites\\Animations\\Units";

    public List<AnimationData> datas = new List<AnimationData>();

    public Vector2 spritePivot = new Vector2(0.5f, 0.5f);

    public float clipFrameRate = 20;

    public int firstX = -1;
    public bool skipExisting = true;


    public List<string> toDo;


    private void CreateSpriteSheets() {

        datas = new List<AnimationData>();
        CreateDataFiles();
        CutSpriteSheets();
        
    }

    private void CreateAnimations() {
        MakeAnimationsFromCutSpriteSheeets();
        datas.Clear();
    }






    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void CreateDataFiles()
    {
        toDo = new List<string>();



        DirectoryInfo d = new DirectoryInfo(Application.dataPath + "//" + XMLDirectoryAssetPath); //Assuming Test is your Folder

        FileInfo[] Files = d.GetFiles("*.plist"); //Getting Text files

        int limit = 0;
        foreach (FileInfo file in Files) {

            if (firstX >= 0 && limit >= firstX) break;
            //Debug.Log(file.FullName);

            
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.Load(file.FullName);

            XmlNode node = xmlDoc.ChildNodes[2].FirstChild.ChildNodes[1];

            string s = node.InnerXml;

            //Debug.Log(node.ChildNodes.Count);

            int iter = 0;

            AnimationData animData = new AnimationData();


            string unitName = file.Name;

            unitName = unitName.Replace(".plist","");

            animData.name = unitName;

            toDo.Add(unitName);

            animData.filepathname = unitName + ".png";
            animData.animations = new Dictionary<string, List<FrameData>>();


            while (iter < node.ChildNodes.Count) {

                //Extract Key

                string key = node.ChildNodes[iter].InnerText;

                key = key.Replace(unitName+"_", "").Replace(".png", "");

                string[] split = key.Split("_");

                string animKey = split[0];
                int frameNumber = 0;
                try {
                   frameNumber = int.Parse(split[1]);
                } catch {

                    Debug.Log(key);

                }
                if (!animData.animations.ContainsKey(animKey)) {
                    animData.animations.Add(animKey, new List<FrameData>());
                }

                if (split.Length > 2) {
                    Debug.LogError("Key Messed up " + node.ChildNodes[iter].InnerText);
                }

                //Extract Frame Data

                XmlNode FrameNode = node.ChildNodes[iter + 1];

                FrameData frameData = new FrameData();

                frameData.frameName = split[0] + "_" + frameNumber.ToString("000");


                //Get Frame Coords

                string frSRC = FrameNode.ChildNodes[1].InnerText;
                string[] nums = frSRC.Replace("{", "").Replace("}", "").Split(',');
                frameData.frameCoord = new Tuple<Vector2, Vector2>( new Vector2(int.Parse(nums[0]), int.Parse(nums[1])), new Vector2(int.Parse(nums[2]), int.Parse(nums[3])));

                //Get Offset

                string offSRC = FrameNode.ChildNodes[3].InnerText;
                nums = offSRC.Replace("{", "").Replace("}", "").Split(',');
                frameData.offset = new Vector2(int.Parse(nums[0]), int.Parse(nums[1]));


                //Get Rotated
                frameData.rotated = !FrameNode.ChildNodes[5].OuterXml.Contains("false"); 

                //Get Source Color

                string srcclrSRC = FrameNode.ChildNodes[7].InnerText;
                nums = srcclrSRC.Replace("{", "").Replace("}", "").Split(',');
                frameData.sourceColor = new Vector2(int.Parse(nums[0]), int.Parse(nums[1]));

                //Get Source Size

                string srcSizeSRC = FrameNode.ChildNodes[9].InnerText;
                nums = srcSizeSRC.Replace("{", "").Replace("}", "").Split(',');
                frameData.sourceSize = new Vector2(int.Parse(nums[0]), int.Parse(nums[1]));


                animData.animations[animKey].Add(frameData);

                //Debug.Log("Key: " + key + " Value: " + frameData.ToString());

                iter += 2;
            }


           
            datas.Add(animData);

            limit++;


        }










    }

    void CutSpriteSheets() {

        foreach (AnimationData animData in datas) {

            string absoluteFilePath = Application.dataPath + "\\" + PNGDirectoryAssetPath + "\\" + animData.filepathname;
            string assetFilePath = "Assets\\" + PNGDirectoryAssetPath + "\\" + animData.filepathname;


            if (!File.Exists(absoluteFilePath)) Debug.LogError("Could Not Find SpriteSheet @ " + absoluteFilePath);

            Texture2D o = AssetDatabase.LoadAssetAtPath<Texture2D>("" + assetFilePath);

            //Debug.Log(o.name);

            string path = AssetDatabase.GetAssetPath(o);

           // Debug.Log(assetFilePath);

            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            ti.isReadable = true;
            ti.spriteImportMode = SpriteImportMode.Multiple;

            List<SpriteRect> newData = new List<SpriteRect>();

            foreach (KeyValuePair<string, List<FrameData>> kvPair in animData.animations) {


                string animName = kvPair.Key;
                List<FrameData> frameDatas = kvPair.Value;

                foreach (FrameData frameData in frameDatas) {

                    SpriteRect frameMetaData = new SpriteRect();
                    frameMetaData.pivot = spritePivot;
                    frameMetaData.name = frameData.frameName;
                    frameMetaData.rect = new Rect(frameData.frameCoord.Item1.x, o.height - frameData.frameCoord.Item1.y - frameData.frameCoord.Item2.y, frameData.frameCoord.Item2.x, frameData.frameCoord.Item2.y);
                    frameMetaData.alignment = SpriteAlignment.Center;

                    //Debug.Break();

                    newData.Add(frameMetaData);
                }
            }

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(o);
            dataProvider.InitSpriteEditorDataProvider();

            dataProvider.SetSpriteRects(newData.ToArray());

            dataProvider.Apply();

            // Reimport the asset to have the changes applied
            var assetImporter = dataProvider.targetObject as AssetImporter;
            assetImporter.SaveAndReimport();




        }

       
        



    }




    private void MakeAnimationsFromCutSpriteSheeets() {



        foreach (String s in toDo) {

            string pngPath = "Assets\\" + PNGDirectoryAssetPath + "\\" + s + ".png";

            string pngName = s;


            string fullControllerPath = Application.dataPath + "\\" + exportPath + "\\" + s + "\\" + pngName + ".controller";
            if (File.Exists(fullControllerPath)) File.Delete(fullControllerPath);



            

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);

            UnityEngine.Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(pngPath);

            Dictionary<string, List<Sprite>> anims = new Dictionary<string, List<Sprite>>();

            //Creating an animation list;

            foreach (UnityEngine.Object asset in allAssets) {

                if (asset is Sprite sprite) {
                    string spriteName = sprite.name.Split("_")[0];
                    if (!anims.ContainsKey(spriteName)) anims[spriteName] = new List<Sprite>();
                    anims[spriteName].Add(sprite);
                }
            }

            
            //Creating animator Controller

            if (!Directory.Exists(Application.dataPath + "\\" + exportPath + "\\" + s )) Directory.CreateDirectory(Application.dataPath + "\\" + exportPath + "\\" + s );

           


            AnimatorController controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets\\" + exportPath + "\\" + s + "\\" + pngName + ".controller");




            int aa = 0;

            foreach (KeyValuePair<string, List<Sprite>> kv in anims) {

                string animName = kv.Key.Split("_")[0];

                var stateMachine = controller.layers[0].stateMachine;
                var animState = stateMachine.AddState(animName, new Vector2(200, aa * 75));

                List<Sprite> sprites = kv.Value;

                string fullClipPath = Application.dataPath + "\\" + exportPath + "\\" + s + "\\" + pngName + "_" + animName + ".anim";

                if (File.Exists(fullClipPath)) File.Delete(fullClipPath);

                string clipPath = "Assets\\" + exportPath + "\\" + s + "\\" + pngName + "_" + animName + ".anim";

                AnimationClip clip = new AnimationClip();

                AssetDatabase.CreateAsset(clip, AssetDatabase.GenerateUniqueAssetPath(clipPath));


                clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);

                clip.frameRate = clipFrameRate;

                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);


                var spriteCurve = new ObjectReferenceKeyframe[sprites.Count];
                for (int i = 0; i < sprites.Count; ++i) {
                    var sprite = sprites[i];
                    spriteCurve[i] = new ObjectReferenceKeyframe {
                        time = i / clip.frameRate,
                        value = sprite
                    };
                }

                var spriteBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
                AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteCurve);

                

                animState.motion = clip;

                AssetDatabase.SaveAssets();

                aa++;

            }

            //Create animation clips and add to controller
        }
        
    }







}
