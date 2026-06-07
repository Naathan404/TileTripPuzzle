using UnityEngine;
using UnityEditor; // Bắt buộc phải có thư viện này
using System.Collections.Generic;
using System.IO;
using log4net.Core;

public class LevelEditor : EditorWindow
{
    // Cấu hình lưới
    private int gridSizeX = 14;
    private int gridSizeY = 18;
    private int currentLayerZ = 0; 
    private int levelID = 1;
    private int availableTileNumber = 4;
    private Vector2 tileSpacing = new Vector2(0.75f, 0.8f);

    private float cellSize = 20f; 
    private Dictionary<int, List<Vector2>> layerDataDict = new Dictionary<int, List<Vector2>>();

    // tạo menu Tools
    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        LevelEditor window = GetWindow<LevelEditor>("Level Editor");
        window.minSize = new Vector2(400, 600); 
    }

    // update
    private void OnGUI()
    {
        DrawHeader();
        DrawGridArea();
        DrawFooter();
    }

    /// <summary>
    /// Vẽ header
    /// </summary>
private void DrawHeader()
    {
        GUILayout.Label("THIẾT KẾ BẢN ĐỒ TILE MATCH", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Box Level ID
        GUILayout.BeginVertical("box");
        GUILayout.Label("ID màn chơi", EditorStyles.boldLabel);
        levelID = EditorGUILayout.IntField("LevelID: ", levelID);
        GUILayout.EndVertical();

        // Box Cấu Hình Khoảng Cách
        GUILayout.BeginVertical("box");
        GUILayout.Label("Cấu Hình Khoảng Cách", EditorStyles.boldLabel);
        tileSpacing = EditorGUILayout.Vector2Field("Khoảng cách thẻ (Spacing)", tileSpacing);
        GUILayout.EndVertical();

        // Box Danh Sách Tile ID (Mới thêm)
        GUILayout.BeginVertical("box");
        GUILayout.Label("Số loại tile", EditorStyles.boldLabel);
        availableTileNumber = EditorGUILayout.IntField("Số loại tile:", availableTileNumber);
        GUILayout.EndVertical();
        GUILayout.Space(10);

        // Thanh điều khiển Layer 
        GUILayout.BeginHorizontal();
        GUILayout.Label($"Đang vẽ ở Layer Z = {currentLayerZ}", GUILayout.Width(150));
        
        if (GUILayout.Button("-", GUILayout.Width(30))) 
            currentLayerZ = Mathf.Max(0, currentLayerZ - 1); 
            
        if (GUILayout.Button("+", GUILayout.Width(30))) 
            currentLayerZ++;
        
        if(GUILayout.Button("Clear", GUILayout.Width(100)))
        {
            if (layerDataDict.ContainsKey(currentLayerZ))
            {
                List<Vector2> currentLayerList = layerDataDict[currentLayerZ];
                currentLayerList.Clear();
            }
        }
            
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }


    /// <summary>
    /// Vẽ lưới để vẽ trực tiếp
    /// </summary>
    private void DrawGridArea()
    {
        if (!layerDataDict.ContainsKey(currentLayerZ))
        {
            layerDataDict[currentLayerZ] = new List<Vector2>();
        }

        List<Vector2> currentLayerList = layerDataDict[currentLayerZ];

        GUILayout.BeginVertical();

        for (int y = 0; y < gridSizeY; y++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int x = 0; x < gridSizeX; x++)
            {
                Vector2 currentPos = new Vector2(x, y);
                
                bool hasTile = currentLayerList.Contains(currentPos);
                bool isFootprint = !hasTile && (
                    currentLayerList.Contains(new Vector2(x - 1, y)) ||     // Bị đè bởi thẻ bên Trái
                    currentLayerList.Contains(new Vector2(x, y - 1)) ||     // Bị đè bởi thẻ bên Trên
                    currentLayerList.Contains(new Vector2(x - 1, y - 1))    // Bị đè bởi thẻ góc Trên-Trái
                );

                if (hasTile)
                    GUI.backgroundColor = Color.cyan; 
                else if (isFootprint)
                    GUI.backgroundColor = new Color(0.8f, 0.95f, 0.95f, 1f); 
                else
                    GUI.backgroundColor = Color.white; 
                    
                if (GUILayout.Button("", GUILayout.Width(cellSize), GUILayout.Height(cellSize)))
                {
                    if (hasTile)
                    {
                        currentLayerList.Remove(currentPos);
                    }
                    else
                    {
                        currentLayerList.Add(currentPos);
                    }
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        GUI.backgroundColor = Color.white; 
    }


    /// <summary>
    /// Vẽ Footer
    /// </summary>
    private void DrawFooter()
    {
        GUILayout.FlexibleSpace(); 

        // Nút xuất file
        GUI.backgroundColor = Color.green; 
        if (GUILayout.Button("Export to JSON", GUILayout.Height(40)))
        {
            ExportLevelData();
        }
        GUI.backgroundColor = Color.white;
    }

    /// <summary>
    /// Parse dữ liệu sang JSON
    /// </summary>
    private void ExportLevelData()
    {
        LevelData newLevel = new LevelData();
        newLevel.LevelID = levelID; 
        newLevel.LevelName = "Level_" + levelID; 
        newLevel.SpacingX = tileSpacing.x;
        newLevel.SpacingY = tileSpacing.y;
        newLevel.AvailableTileNumber = availableTileNumber;
        int totalTilesCount = 0;

        foreach (var kvp in layerDataDict)
        {
            int zIndex = kvp.Key;
            List<Vector2> gridPositions = kvp.Value;

            if (gridPositions.Count == 0) continue; 

            LayerData layerData = new LayerData();
            layerData.ZIndex = zIndex;

            foreach (Vector2 pos in gridPositions)
            {
                layerData.TilePositions.Add(new TilePosition(pos.x * 0.5f, pos.y * 0.5f));
                totalTilesCount++;
            }

            newLevel.Layers.Add(layerData);
        }

        newLevel.TotalTiles = totalTilesCount;

        if (totalTilesCount % 3 != 0)
        {
            EditorUtility.DisplayDialog("Lỗi", 
                $"Tổng số thẻ đang là {totalTilesCount}. Bắt buộc phải chia hết cho 3!\n.", 
                "OK con dê");
            return; 
        }


        string jsonString = JsonUtility.ToJson(newLevel, true);

        string folderPath = Application.dataPath + "/Resources/Levels";
        string filePath = folderPath + $"/Level_{levelID}.json";

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        
        File.WriteAllText(filePath, jsonString);

        AssetDatabase.Refresh();
        Debug.Log($"<color=green>[THÀNH CÔNG]</color> Đã lưu Level có {totalTilesCount} thẻ tại:\n{filePath}");
    }
}