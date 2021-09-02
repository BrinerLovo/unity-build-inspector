using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using Object = UnityEngine.Object;
using System.Text.RegularExpressions;

public class ProjectBuildInspector : EditorWindow
{
    #region Variables
    public List<AssetInfo> AssetData = new List<AssetInfo>();
    public List<AssetInfo> ShowAssets = new List<AssetInfo>();
    public List<CategoryInfo> CategoryData = new List<CategoryInfo>();
    private Vector2 scrollPos;
    private int state = 0;
    private int TableSize = 100;
    private int indexTable = 0;
    private float mayorFileSize = 0;
    private int numberOfBuild = 0;
    private Dictionary<string, List<string>> FileTypes = new Dictionary<string, List<string>>()
    {
        {"Textures", new List<string>(){"png", "jpg","tga", "psd", "tif", "gif", "bmp", "iff", "pict"} },
        {"Meshes", new List<string>(){"fbx", "obj","dae", "3ds", "dxf", "skp"} },
        {"Sounds", new List<string>(){"wav", "mp3","ogg", "aif", "xm", "mod", "it", "s3m"} },
        {"Shaders", new List<string>(){"shader", "cginc"} },
        {"Scenes", new List<string>(){"scene"} },
        {"Scripts", new List<string>(){"cs", "dll", "js"} },
        {"Animations", new List<string>(){"anim", "controller"} },
        {"Materials", new List<string>(){"mat", "physicmaterial"} },
        {"Prefabs", new List<string>(){"prefab"} },
        {"Assets", new List<string>(){"asset"} },
        {"Fonts", new List<string>(){"otf", "ttf"} },
        {"LightMap", new List<string>(){"exr"} },
    };
    private List<string> FileTypesNames = new List<string>();
    private int currentType = 0;
    private int lastBuildID = 0; 
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        minSize = new Vector2(600, 600);
        FileTypesNames.Add("All");
        FileTypesNames.AddRange(FileTypes.Keys.ToArray());
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGUI()
    {
        DrawHeader();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical("box");
        if (haveData)
        {
            if (state == 0)
            {
                DrawRawData();
            }
            else if (state == 1)
            {
                DrawInBuildAssets();
            }
        }
        else
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("NO DATA YET.");
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

    }

    /// <summary>
    /// 
    /// </summary>
    void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal("box");
        numberOfBuild = EditorGUILayout.IntField(numberOfBuild, GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight));
        if (GUILayout.Button("Load Last", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            CountBuildReports();
            LoadBuildData(lastBuildID - 1);
            BuildData();
        }
        if (GUILayout.Button("Load Data", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            LoadBuildData(numberOfBuild - 1);
        }
        GUILayout.Space(2);
        GUI.enabled = haveData && state == 0;
        if (GUILayout.Button("Build Data", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            BuildData();
        }

        if (GUILayout.Button("Count Builds", EditorStyles.toolbarButton, GUILayout.Width(100)))
        {
            CountBuildReports();
        }
        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawRawData()
    {
        GUILayout.Label($"Used Assets found: {AssetData.Count}");
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawInBuildAssets()
    {
        DrawCategorys();

        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("Asset", GUILayout.Width(200));
        if (GUILayout.Button("Size", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            ShowAssets = ShowAssets.OrderBy(x => x.SizeKB).ToList();
        }
        GUILayout.Space(20);
        GUILayout.Label("%", GUILayout.Width(50));
        GUILayout.Label("Path", GUILayout.Width(100));
        GUI.enabled = indexTable > 0;
        if (GUILayout.Button("<", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            indexTable -= TableSize;
            indexTable = Mathf.Clamp(indexTable, 0, int.MaxValue);
        }
        GUI.enabled = true;
        int page = ShowAssets.Count > 0 ? indexTable / ShowAssets.Count : 1;
        GUILayout.Label($"{ Mathf.Clamp(indexTable,1,int.MaxValue) } / {ShowAssets.Count - 1}",GUILayout.Width(75));
        GUI.enabled = indexTable < ShowAssets.Count - 1;
        if (GUILayout.Button(">", EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            int remaing = (ShowAssets.Count - 1) - indexTable;
            int add = (remaing > TableSize) ? TableSize : remaing;
            indexTable += add;
        }
        GUI.enabled = true;
        GUILayout.Space(10);
        currentType = EditorGUILayout.Popup(currentType, FileTypesNames.ToArray(), EditorStyles.toolbarPopup,GUILayout.Width(125));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        scrollPos = GUILayout.BeginScrollView(scrollPos);
        int showNumber = ShowAssets.Count > TableSize ? TableSize : ShowAssets.Count - 1;
        string typeName = string.Empty;
        if (currentType != 0)//if by category
        {
            typeName = FileTypesNames[currentType];
            showNumber = ShowAssets.Count;
        }
        for (int i = 0; i < showNumber; i++)
        {
            int index = i + indexTable;
            if (index > ShowAssets.Count - 1)
                continue;

            AssetInfo data = ShowAssets[index];
            if(currentType != 0)
            {
                if (!FileTypes[typeName].Exists(x => x == data.Extension)) continue;
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(data.Name, EditorStyles.toolbarButton, GUILayout.Width(200)))
            {
                Object obj = AssetDatabase.LoadAssetAtPath(data.Path, typeof(Object)) as Object;
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
            GUILayout.Space(10);
            if (mayorFileSize > 1000)
            {
                GUI.color = Color.Lerp(Color.white, Color.red, data.SizeKB / mayorFileSize);
            }
            GUILayout.Label(data.Size,  GUILayout.Width(60));
            GUI.color = Color.white;
            GUILayout.Space(10);
            GUILayout.Label(data.Percentage, GUILayout.Width(50));
            GUILayout.Label(data.Path, EditorStyles.miniLabel, GUILayout.Width(Screen.width - 225));
            EditorGUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawCategorys()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal("box");
        GUILayout.Label("Type", GUILayout.Width(125));
        GUILayout.Label("Size", GUILayout.Width(60));
        GUILayout.Label("Build %", GUILayout.Width(50));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        for (int i = 0; i < CategoryData.Count; i++)
        {
            CategoryInfo data = CategoryData[i];
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(data.Name, EditorStyles.helpBox, GUILayout.Width(125));
            GUILayout.Label(data.Size, EditorStyles.helpBox, GUILayout.Width(60));
            GUILayout.Label(data.Percentage, EditorStyles.helpBox, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 
    /// </summary>
    private void LoadBuildData(int buildList)
    {
        state = 0;
        AssetData.Clear();
        CategoryData.Clear();
        GetLists(ref AssetData, buildList);
        Repaint();
    }

    /// <summary>
    /// 
    /// </summary>
    void BuildData()
    {
        if (!haveData) return;
        CategoryData.Clear();
        ShowAssets.Clear();
        for (int i = 0; i < AssetData.Count; i++)
        {
            AssetInfo info = AssetData[i];
            string raw = info.RawLine.Trim();
            Char last = raw[raw.Length - 1];
            if (last == ':') continue;

            if (last == '%')
            {
                GetCategory(raw);
            }
            else
            {
                if (GetAssetData(raw, ref info))
                    ShowAssets.Add(info);
            }
        }
        state = 1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="raw"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    bool GetAssetData(string raw, ref AssetInfo info)
    {
        if (raw.Substring(raw.Length - 3) == " mb" || raw.Substring(raw.Length - 3) == " gb" || raw.Substring(raw.Length - 3) == " tb") return false;
        
        string size = raw.Substring(0, raw.IndexOf(" ", raw.IndexOf(" ") + 1)).Trim();
        string percentage = string.IsNullOrEmpty(size) ? "--" : raw.Substring(size.Length, raw.IndexOf("%") - size.Length + 1).Trim();
        int thirdSpace = raw.IndexOf("%") + 1;
        string path = raw.Substring(thirdSpace, raw.Length - thirdSpace).Trim();
        string name = Path.GetFileName(path);
        string extension = Path.GetExtension(path).ToLower();
        if (!string.IsNullOrEmpty(extension)){ extension = extension.Substring(1, extension.Length - 1); }

        info.Path = path;
        info.Name = name;
        info.Size = size;
        info.Extension = extension;
        info.Percentage = percentage;
        mayorFileSize = Mathf.Max(info.ParseSize(), mayorFileSize);
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="raw"></param>
    void GetCategory(string raw)
    {
        CategoryInfo cd = new CategoryInfo();
        //get name
        string name = raw.Substring(0, raw.IndexOf("  "));
        //get size
        string size = raw.Replace(name, "");
        size = size.Trim();
        size = size.Substring(0, size.IndexOf("\t")).Trim();
        //get percentage
        int lastTubular = raw.LastIndexOf("\t");
        string percentage = raw.Substring(lastTubular, raw.Length - lastTubular).Trim();

        cd.Name = name;
        cd.Size = size;
        cd.Percentage = percentage;
        CategoryData.Add(cd);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string[] GetAllAssets()
    {
        string[] tmpAssets1 = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
        string[] tmpAssets2 = Array.FindAll(tmpAssets1, name => !name.EndsWith(".meta"));
        string[] allAssets;

        allAssets = Array.FindAll(tmpAssets2, name => !name.EndsWith(".unity"));

        for (int i = 0; i < allAssets.Length; i++)
        {
            allAssets[i] = allAssets[i].Substring(allAssets[i].IndexOf("/Assets") + 1);
            allAssets[i] = allAssets[i].Replace(@"\", "/");
        }

        return allAssets;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assetResult"></param>
    /// <param name="buildID"></param>
    public void GetLists(ref List<AssetInfo> assetResult, int buildID)
    {
        assetResult.Clear();

        string LocalAppData = string.Empty;
        string UnityEditorLogfile = string.Empty;

        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            UnityEditorLogfile = LocalAppData + "\\Unity\\Editor\\Editor.log";
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            UnityEditorLogfile = LocalAppData + "/Library/Logs/Unity/Editor.log";
        }

        try
        {
            // Have to use FileStream to get around sharing violations!
            FileStream FS = new FileStream(UnityEditorLogfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader SR = new StreamReader(FS);
            string text = SR.ReadToEnd();//I find easier read the whole string data that read line by line
            SR.Dispose();
            FS.Close();

            string[] split = text.Split("\n"[0]);
            int textPart = 0;
            int buildNumber = 0;
            for (int i = 0; i < split.Length; i++)//search in the text file from top to bottom
            {
                if (string.IsNullOrEmpty(split[i])) continue;
                if (textPart == 0)//if we have not found the build log part yet
                {
                    if (split[i].Contains("Build Report"))//Build log part
                    {
                        if (buildNumber < buildID)
                        {
                            buildNumber++;
                            continue;
                        }
                        textPart = 1;
                    }
                    continue;
                }
                else if (textPart < 2) { textPart++; }//skip next line
                if (split[i].Contains("------")) { break; }//finally of the build log, don't continue checking other lines

                AssetInfo ai = new AssetInfo();
                ai.RawLine = split[i];//add the raw line
                assetResult.Add(ai);
            }
        }
        catch (Exception E)
        {
            Debug.LogError("Error: " + E);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CountBuildReports()
    {
        string LocalAppData = string.Empty;
        string UnityEditorLogfile = string.Empty;

        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            UnityEditorLogfile = LocalAppData + "\\Unity\\Editor\\Editor.log";
        }
        else if (Application.platform == RuntimePlatform.OSXEditor)
        {
            LocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            UnityEditorLogfile = LocalAppData + "/Library/Logs/Unity/Editor.log";
        }

        try
        {
            // Have to use FileStream to get around sharing violations!
            FileStream FS = new FileStream(UnityEditorLogfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader SR = new StreamReader(FS);
            string text = SR.ReadToEnd();//I find easier read the whole string data that read line by line
            SR.Dispose();
            FS.Close();

            string[] split = text.Split("\n"[0]);
            int buildNumber = 0;
            for (int i = 0; i < split.Length; i++)//search in the text file from top to bottom
            {
                if (string.IsNullOrEmpty(split[i])) continue;
                if (split[i].Contains("Build Report"))//Build log part
                {
                    buildNumber++;
                }
                continue;
            }
            lastBuildID = buildNumber;
            Debug.Log($"Number of builds: {buildNumber}");
        }
        catch (Exception E)
        {
            Debug.LogError("Error: " + E);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <param name="nth"></param>
    /// <returns></returns>
    public int IndexOfNth(string str, string value, int nth = 1)
    {
        int offset = str.IndexOf(value);
        for (int i = 1; i < nth; i++)
        {
            if (offset == -1) return -1;
            offset = str.IndexOf(value, offset + 1);
        }
        return offset;
    }

    public bool haveData { get { return AssetData.Count > 0; } }
    [MenuItem("Lovatto/Tools/Build Inspector")]
    static void Init()
    {
        ProjectBuildInspector window = (ProjectBuildInspector)EditorWindow.GetWindow(typeof(ProjectBuildInspector));
        window.Show();
    }

    [Serializable]
    public class AssetInfo
    {
        public string Name;
        public string Path;
        public string Extension;
        public string Size;
        public float SizeKB = 0;
        public string Percentage;

        public Color SizeColor = Color.white;
        public Object Asset;
        public string RawLine;

        public float ParseSize()
        {
            SizeKB = float.Parse(Regex.Match(Size, @"([-+]?[0-9]*\.?[0-9]+)").Value);
            if (Size.Contains("mb")) { SizeKB *= 1000; }
            return SizeKB;
        }
    }

    [Serializable]
    public class CategoryInfo
    {
        public string Name;
        public string Size;
        public string Percentage;
    }

    [Serializable]
    public enum InfoType
    {
        Asset = 0,
        Category = 1,
    }
}