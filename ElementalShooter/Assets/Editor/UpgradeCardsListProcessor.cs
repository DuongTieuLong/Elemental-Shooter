using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class UpgradeCardsListProcessor
{
    [MenuItem("Tools/Sync Upgrade Cards List")]
    public static void SyncUpgradeCards()
    {
        UpgradeManager manager = Object.FindAnyObjectByType<UpgradeManager>();
        if (manager == null)
        {
            Debug.LogError("Could not find UpgradeManager in the active scene.");
            return;
        }

        // Find all CardUpgrade assets in the project
        string[] guids = AssetDatabase.FindAssets("t:CardUpgrade");
        List<CardUpgrade> cards = new List<CardUpgrade>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardUpgrade card = AssetDatabase.LoadAssetAtPath<CardUpgrade>(path);
            if (card != null)
            {
                cards.Add(card);
            }
        }

        Undo.RecordObject(manager, "Sync Upgrade Cards List");
        manager.allUpgrades = cards;
        
        EditorUtility.SetDirty(manager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);

        Debug.Log($"Successfully synced {cards.Count} Upgrade Cards to UpgradeManager in active scene.");
    }
}
