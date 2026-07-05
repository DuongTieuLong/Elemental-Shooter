using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public static class SetupUpgradeUI
{
    [MenuItem("Tools/Setup Upgrade UI Layout")]
    public static void SetupLayout()
    {
        UpgradeUI upgradeUI = Object.FindAnyObjectByType<UpgradeUI>();
        if (upgradeUI == null)
        {
            Debug.LogError("Could not find UpgradeUI in active scene.");
            return;
        }

        Transform panelTransform = upgradeUI.UpgradePanel.transform;

        // 1. Setup Reroll Button
        Transform rerollBtnTrans = panelTransform.Find("RerollButton");
        GameObject rerollBtnGo;
        if (rerollBtnTrans == null)
        {
            rerollBtnGo = new GameObject("RerollButton");
            rerollBtnGo.transform.SetParent(panelTransform, false);
        }
        else
        {
            rerollBtnGo = rerollBtnTrans.gameObject;
        }

        // RectTransform
        RectTransform rtBtn = rerollBtnGo.GetComponent<RectTransform>();
        if (rtBtn == null) rtBtn = rerollBtnGo.AddComponent<RectTransform>();
        rtBtn.anchorMin = new Vector2(0.5f, 0f);
        rtBtn.anchorMax = new Vector2(0.5f, 0f);
        rtBtn.pivot = new Vector2(0.5f, 0.5f);
        rtBtn.anchoredPosition = new Vector2(0f, -420f); // Pos at bottom of screen
        rtBtn.sizeDelta = new Vector2(220f, 55f);

        // LayoutElement to ignore HorizontalLayoutGroup
        LayoutElement leBtn = rerollBtnGo.GetComponent<LayoutElement>();
        if (leBtn == null) leBtn = rerollBtnGo.AddComponent<LayoutElement>();
        leBtn.ignoreLayout = true;

        // Image component for button look
        Image btnImg = rerollBtnGo.GetComponent<Image>();
        if (btnImg == null) btnImg = rerollBtnGo.AddComponent<Image>();
        btnImg.color = new Color(0.85f, 0.45f, 0.1f); // Dark orange

        // Button component
        Button btn = rerollBtnGo.GetComponent<Button>();
        if (btn == null) btn = rerollBtnGo.AddComponent<Button>();

        // Text child
        Transform txtTrans = rerollBtnGo.transform.Find("Text (TMP)");
        GameObject txtGo;
        if (txtTrans == null)
        {
            txtGo = new GameObject("Text (TMP)");
            txtGo.transform.SetParent(rerollBtnGo.transform, false);
        }
        else
        {
            txtGo = txtTrans.gameObject;
        }

        RectTransform rtTxt = txtGo.GetComponent<RectTransform>();
        if (rtTxt == null) rtTxt = txtGo.AddComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = Vector2.zero;
        rtTxt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmpText = txtGo.GetComponent<TextMeshProUGUI>();
        if (tmpText == null) tmpText = txtGo.AddComponent<TextMeshProUGUI>();
        tmpText.fontSize = 22;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.text = "Reroll (3/3)";
        tmpText.color = Color.white;

        // 2. Setup Player Stats Text
        Transform statsTrans = panelTransform.Find("PlayerStatsText");
        GameObject statsGo;
        if (statsTrans == null)
        {
            statsGo = new GameObject("PlayerStatsText");
            statsGo.transform.SetParent(panelTransform, false);
        }
        else
        {
            statsGo = statsTrans.gameObject;
        }

        RectTransform rtStats = statsGo.GetComponent<RectTransform>();
        if (rtStats == null) rtStats = statsGo.AddComponent<RectTransform>();
        rtStats.anchorMin = new Vector2(0f, 0.5f);
        rtStats.anchorMax = new Vector2(0f, 0.5f);
        rtStats.pivot = new Vector2(0f, 0.5f);
        rtStats.anchoredPosition = new Vector2(-680f, 0f); // Left side of cards
        rtStats.sizeDelta = new Vector2(340f, 500f);

        LayoutElement leStats = statsGo.GetComponent<LayoutElement>();
        if (leStats == null) leStats = statsGo.AddComponent<LayoutElement>();
        leStats.ignoreLayout = true;

        TextMeshProUGUI statsTmp = statsGo.GetComponent<TextMeshProUGUI>();
        if (statsTmp == null) statsTmp = statsGo.AddComponent<TextMeshProUGUI>();
        statsTmp.fontSize = 24;
        statsTmp.alignment = TextAlignmentOptions.TopLeft;
        statsTmp.text = "CHỈ SỐ NHÂN VẬT";

        // Bind references to UpgradeUI
        Undo.RecordObject(upgradeUI, "Setup Upgrade UI Layout");
        upgradeUI.rerollButton = btn;
        upgradeUI.rerollText = tmpText;
        upgradeUI.playerStatsText = statsTmp;

        EditorUtility.SetDirty(upgradeUI);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(upgradeUI.gameObject.scene);

        Debug.Log("Upgrade UI layout setup successfully completed.");
    }
}
