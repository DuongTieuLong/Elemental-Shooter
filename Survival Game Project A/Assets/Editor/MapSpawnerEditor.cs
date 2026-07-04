using UnityEngine;
using UnityEditor;

public class MapSpawnerEditor : EditorWindow
{
    // Mảng chứa các Prefab Sprite (Cây, Cỏ, Đá dạng 2D)
    private GameObject[] prefabsToSpawn = new GameObject[15];

    // Cấu hình vùng spawn 2D
    private Vector2 spawnAreaCenter = Vector2.zero;
    private Vector2 spawnAreaSize = new Vector2(30f, 20f); // Chiều rộng (X) và Chiều cao (Y)
    private int spawnCount = 50;

    // Thứ tự hiển thị (Sorting Order) ngẫu nhiên để tạo chiều sâu cho game 2D
    private int minSortingOrder = 0;
    private int maxSortingOrder = 10;

    [MenuItem("Tools/2D Map Spawner")]
    public static void ShowWindow()
    {
        GetWindow<MapSpawnerEditor>("2D Spawner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Cấu hình Spawner 2D", EditorStyles.boldLabel);

        // Kéo thả Prefab 2D vào đây
        prefabsToSpawn[0] = (GameObject)EditorGUILayout.ObjectField("Prefab Cây 2D:", prefabsToSpawn[0], typeof(GameObject), false);
        prefabsToSpawn[1] = (GameObject)EditorGUILayout.ObjectField("Prefab Cỏ 2D:", prefabsToSpawn[1], typeof(GameObject), false);
        prefabsToSpawn[2] = (GameObject)EditorGUILayout.ObjectField("Prefab Đá 2D:", prefabsToSpawn[2], typeof(GameObject), false);
        prefabsToSpawn[3] = (GameObject)EditorGUILayout.ObjectField("Prefab Đá 2D:", prefabsToSpawn[3], typeof(GameObject), false);
        prefabsToSpawn[4] = (GameObject)EditorGUILayout.ObjectField("Prefab Đá 2D:", prefabsToSpawn[4], typeof(GameObject), false);
        prefabsToSpawn[5] = (GameObject)EditorGUILayout.ObjectField("Prefab Đá 2D:", prefabsToSpawn[5], typeof(GameObject), false);
        prefabsToSpawn[6] = (GameObject)EditorGUILayout.ObjectField("Prefab Đá 2D:", prefabsToSpawn[6], typeof(GameObject), false);
        prefabsToSpawn[7] = (GameObject)EditorGUILayout.ObjectField("Prefab Đá 2D:", prefabsToSpawn[7], typeof(GameObject), false);
        prefabsToSpawn[8] = (GameObject)EditorGUILayout.ObjectField("Prefab Đá 2D:", prefabsToSpawn[8], typeof(GameObject), false);

        EditorGUILayout.Space();

        // Cấu hình tọa độ
        spawnAreaCenter = EditorGUILayout.Vector2Field("Tâm vùng Spawn (X, Y):", spawnAreaCenter);
        spawnAreaSize = EditorGUILayout.Vector2Field("Kích thước vùng (Rộng, Cao):", spawnAreaSize);
        spawnCount = EditorGUILayout.IntField("Số lượng spawn:", spawnCount);

        EditorGUILayout.Space();
        GUILayout.Label("Cấu hình Layer hiển thị (Tùy chọn)", EditorStyles.miniBoldLabel);
        minSortingOrder = EditorGUILayout.IntField("Sorting Order nhỏ nhất:", minSortingOrder);
        maxSortingOrder = EditorGUILayout.IntField("Sorting Order lớn nhất:", maxSortingOrder);

        EditorGUILayout.Space();

        if (GUILayout.Button("Spawn Sprite Ngẫu Nhiên", GUILayout.Height(40)))
        {
            Spawn2DObjects();
        }
    }

    private void Spawn2DObjects()
    {
        bool hasPrefab = false;
        foreach (var p in prefabsToSpawn)
        {
            if (p != null) { hasPrefab = true; break; }
        }

        if (!hasPrefab)
        {
            Debug.LogError("Vui lòng kéo ít nhất một Prefab 2D vào bảng!");
            return;
        }

        // Tạo nhóm cha để quản lý Hierarchy cho sạch
        GameObject groupParent = GameObject.Find("Generated_2D_Map_Objects");
        if (groupParent == null)
        {
            groupParent = new GameObject("Generated_2D_Map_Objects");
        }

        int successCount = 0;

        for (int i = 0; i < spawnCount; i++)
        {
            // 1. Tính toán vị trí X, Y ngẫu nhiên trong khung hình chữ nhật
            float randomX = Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2);
            float randomY = Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2, spawnAreaCenter.y + spawnAreaSize.y / 2);
            Vector3 spawnPosition = new Vector3(randomX, randomY, 0f); // Z luôn bằng 0 trong 2D chuẩn

            // 2. Lấy ngẫu nhiên một Prefab đã kéo vào
            GameObject selectedPrefab = null;
            while (selectedPrefab == null)
            {
                selectedPrefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
            }

            // 3. Tạo Object trong Editor Mode
            GameObject spawnedObj = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            spawnedObj.transform.position = spawnPosition;
            spawnedObj.transform.SetParent(groupParent.transform);

            // Xoay nhẹ một chút cho tự nhiên (nếu thích, không thì xóa dòng này đi)
            spawnedObj.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));

            // 4. Tự động chỉnh sửa Sorting Order để các Sprite không bị đè lên nhau một cách hoàn hảo
            SpriteRenderer spriteRenderer = spawnedObj.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Cách 1: Random ngẫu nhiên theo chỉ số bạn nhập
                spriteRenderer.sortingOrder = Random.Range(minSortingOrder, maxSortingOrder + 1);

                // Mẹo bổ sung cho game Top-Down/Twin-Stick: Thường các cụ sẽ dùng cơ chế dựa trên Y (càng ở dưới càng hiển thị đè lên trên)
                // Bạn có thể bật tính năng "Transparency Sort Mode" trong Project Settings -> Graphics sang Custom Axis (0, 1, 0) nhé!
            }

            // Lưu lại thao tác để có thể Ctrl + Z nếu không ưng ý
            Undo.RegisterCreatedObjectUndo(spawnedObj, "Spawn 2D Object");
            successCount++;
        }

        if (successCount > 0)
        {
            // Báo cho Unity biết Scene đã thay đổi để lưu lại được
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"Đã rải xong {successCount} Sprites lên bản đồ 2D!");
        }
    }
}
