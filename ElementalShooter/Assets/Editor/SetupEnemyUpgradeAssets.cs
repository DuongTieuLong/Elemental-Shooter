using UnityEditor;
using UnityEngine;

public class SetupEnemyUpgradeAssets : EditorWindow
{
    [MenuItem("Tools/Setup Enemy Assets")]
    public static void RunSetup()
    {
        string vfxFolder = "Assets/Prefabs/VFX";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(vfxFolder))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "VFX");
        }

        string trailPrefabPath = vfxFolder + "/BossTrailPrefab.prefab";
        
        // 1. Tạo hoặc tải BossTrailPrefab
        GameObject trailPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(trailPrefabPath);
        if (trailPrefab == null)
        {
            GameObject trailObj = new GameObject("BossTrailPrefab");
            var sr = trailObj.AddComponent<SpriteRenderer>();
            var knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            if (knobSprite != null) sr.sprite = knobSprite;
            sr.color = new Color(1f, 0.25f, 0f, 0.45f);
            sr.sortingOrder = -1;

            var col = trailObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var hazard = trailObj.AddComponent<HazardArea>();
            hazard.Setup(null, 4f, 3.2f, 0.5f);

            trailPrefab = PrefabUtility.SaveAsPrefabAsset(trailObj, trailPrefabPath);
            GameObject.DestroyImmediate(trailObj);
            Debug.Log("Created BossTrailPrefab at " + trailPrefabPath);
        }

        // 2. Tạo thư mục chứa ScriptableObject nếu chưa có
        string folderPath = "Assets/Scripts/DataSO/EnemiesWave";
        if (!AssetDatabase.IsValidFolder("Assets/Scripts/DataSO"))
        {
            AssetDatabase.CreateFolder("Assets/Scripts", "DataSO");
        }
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Scripts/DataSO", "EnemiesWave");
        }

        // Tải các prefab cơ sở
        var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab not found at Assets/Prefabs/Enemy.prefab! Can't create ScriptableObjects.");
            return;
        }

        // 2.1 Melee EnemyData
        string meleePath = folderPath + "/Melee_EnemyData.asset";
        var meleeData = AssetDatabase.LoadAssetAtPath<EnemyData>(meleePath);
        if (meleeData == null)
        {
            meleeData = ScriptableObject.CreateInstance<EnemyData>();
            AssetDatabase.CreateAsset(meleeData, meleePath);
        }
        meleeData.prefab = enemyPrefab;
        meleeData.speed = 2.4f;
        meleeData.health = 75f;
        meleeData.stopDistance = 0f;
        EditorUtility.SetDirty(meleeData);

        // 2.2 Ranged EnemyData
        string rangedPath = folderPath + "/Ranged_EnemyData.asset";
        var rangedData = AssetDatabase.LoadAssetAtPath<EnemyData>(rangedPath);
        if (rangedData == null)
        {
            rangedData = ScriptableObject.CreateInstance<EnemyData>();
            AssetDatabase.CreateAsset(rangedData, rangedPath);
        }
        rangedData.prefab = enemyPrefab;
        rangedData.speed = 1.9f;
        rangedData.health = 55f;
        rangedData.stopDistance = 4.5f;
        EditorUtility.SetDirty(rangedData);

        // 2.3 Elite EnemyData
        string elitePath = folderPath + "/Elite_EnemyData.asset";
        var eliteData = AssetDatabase.LoadAssetAtPath<EnemyData>(elitePath);
        if (eliteData == null)
        {
            eliteData = ScriptableObject.CreateInstance<EnemyData>();
            AssetDatabase.CreateAsset(eliteData, elitePath);
        }
        eliteData.prefab = enemyPrefab;
        eliteData.speed = 2.8f;
        eliteData.health = 220f;
        eliteData.stopDistance = 0f;
        EditorUtility.SetDirty(eliteData);

        // 2.4 Boss EnemyData
        string bossPath = folderPath + "/Boss_EnemyData.asset";
        var bossData = AssetDatabase.LoadAssetAtPath<EnemyData>(bossPath);
        if (bossData == null)
        {
            bossData = ScriptableObject.CreateInstance<EnemyData>();
            AssetDatabase.CreateAsset(bossData, bossPath);
        }
        bossData.prefab = enemyPrefab;
        bossData.speed = 1.6f;
        bossData.health = 900f;
        bossData.stopDistance = 0f;
        EditorUtility.SetDirty(bossData);

        // Gán lại tham chiếu base prefab trong HazardArea nếu đã tạo xong
        var hazardComponent = trailPrefab.GetComponent<HazardArea>();
        if (hazardComponent != null)
        {
            hazardComponent.Setup(trailPrefab, 4f, 3.2f, 0.5f);
            EditorUtility.SetDirty(trailPrefab);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created and configured simplified Melee, Ranged, Elite, and Boss EnemyData assets successfully!");
    }
}
