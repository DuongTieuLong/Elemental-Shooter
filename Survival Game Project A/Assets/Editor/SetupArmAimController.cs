using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SetupArmAimController
{
    //[InitializeOnLoadMethod]
    private static void AutoSetup()
    {
        // Kiểm tra xem scene active có hợp lệ không
        Scene activeScene = SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(activeScene.path)) return;

        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning("[SetupArmAimController] Không tìm thấy Player GameObject trong active scene.");
            return;
        }

        // Kiểm tra xem đã có PlayerArmAimController chưa
        var controller = player.GetComponent<PlayerArmAimController>();
        bool addedNew = false;
        if (controller == null)
        {
            controller = player.AddComponent<PlayerArmAimController>();
            addedNew = true;
            Debug.Log("[SetupArmAimController] Đã thêm component PlayerArmAimController vào Player.");
        }

        // Tự động tìm kiếm các bộ phận tay nếu chưa được gán
        var serializedObject = new SerializedObject(controller);
        var leftArmProp = serializedObject.FindProperty("leftArm");
        var rightArmProp = serializedObject.FindProperty("rightArm");

        bool modified = false;

        if (leftArmProp.objectReferenceValue == null)
        {
            Transform leftArm = player.transform.Find("Body/Left Arm");
            if (leftArm != null)
            {
                leftArmProp.objectReferenceValue = leftArm;
                modified = true;
                Debug.Log("[SetupArmAimController] Đã tự động gán Left Arm.");
            }
        }

        if (rightArmProp.objectReferenceValue == null)
        {
            Transform rightArm = player.transform.Find("Body/Right Arm");
            if (rightArm != null)
            {
                rightArmProp.objectReferenceValue = rightArm;
                modified = true;
                Debug.Log("[SetupArmAimController] Đã tự động gán Right Arm.");
            }
        }

        // Gán các góc lệch mặc định nếu cần thiết
        var rightOffsetProp = serializedObject.FindProperty("rightArmAngleOffset");
        var leftOffsetProp = serializedObject.FindProperty("leftArmAngleOffset");
        var armLengthProp = serializedObject.FindProperty("armLength");
        var verticalOffsetProp = serializedObject.FindProperty("verticalOffset");

        if (rightOffsetProp.floatValue == 0f)
        {
            rightOffsetProp.floatValue = 180f;
            modified = true;
        }

        if (leftOffsetProp.floatValue == 0f)
        {
            leftOffsetProp.floatValue = 0f;
            modified = true;
        }

        if (armLengthProp.floatValue == 0f || armLengthProp.floatValue == 1.0f)
        {
            armLengthProp.floatValue = 1.0f;
            modified = true;
        }

        if (verticalOffsetProp.floatValue == 0f || verticalOffsetProp.floatValue == 0.3f)
        {
            verticalOffsetProp.floatValue = 0.35f;
            modified = true;
        }

        if (addedNew || modified)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(player);
            EditorSceneManager.MarkSceneDirty(activeScene);
            bool saveResult = EditorSceneManager.SaveScene(activeScene);
            Debug.Log($"[SetupArmAimController] Đã lưu Scene tự động: {saveResult}");
        }
    }
}
