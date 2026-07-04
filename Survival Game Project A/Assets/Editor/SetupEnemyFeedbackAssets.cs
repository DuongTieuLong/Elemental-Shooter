using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Component.Enemy.Feedback;

public class SetupEnemyFeedbackAssets : EditorWindow
{
    [MenuItem("Tools/Setup Enemy Feedback Assets")]
    public static void ShowWindow()
    {
        SetupAnimators();
        SetupPrefabs();
        Debug.Log("Enemy Feedback setup complete!");
    }

    private static void SetupAnimators()
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets/Animations/Enemy" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) continue;

            // Add parameters
            AddParameter(controller, "IsMoving", AnimatorControllerParameterType.Bool);
            AddParameter(controller, "Attack", AnimatorControllerParameterType.Trigger);
            AddParameter(controller, "Hit", AnimatorControllerParameterType.Trigger);
            AddParameter(controller, "Die", AnimatorControllerParameterType.Bool); // We use bool in controller
            
            // Optionally: add Die Trigger just in case
            AddParameter(controller, "DieTrigger", AnimatorControllerParameterType.Trigger);

            // Set up basic states and transitions if they don't exist
            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

            // Find or create states
            AnimatorState idleState = FindOrCreateState(rootStateMachine, "Idle");
            AnimatorState walkState = FindOrCreateState(rootStateMachine, "Walk");
            AnimatorState attackState = FindOrCreateState(rootStateMachine, "Attack");
            AnimatorState hitState = FindOrCreateState(rootStateMachine, "Hit");
            AnimatorState dieState = FindOrCreateState(rootStateMachine, "Die");

            // Setup AnyState transitions
            SetupAnyStateTransition(rootStateMachine, attackState, "Attack");
            SetupAnyStateTransition(rootStateMachine, hitState, "Hit");
            SetupAnyStateTransition(rootStateMachine, dieState, "Die", AnimatorConditionMode.If);

            // Setup Idle <-> Walk
            SetupTransition(idleState, walkState, "IsMoving", true);
            SetupTransition(walkState, idleState, "IsMoving", false);

            // Setup Attack -> Idle (Has Exit Time)
            SetupExitTransition(attackState, idleState);

            // Setup Hit -> Idle (Has Exit Time)
            SetupExitTransition(hitState, idleState);

            EditorUtility.SetDirty(controller);
        }
        AssetDatabase.SaveAssets();
    }

    private static void AddParameter(AnimatorController controller, string paramName, AnimatorControllerParameterType type)
    {
        foreach (var param in controller.parameters)
        {
            if (param.name == paramName) return;
        }
        controller.AddParameter(paramName, type);
    }

    private static AnimatorState FindOrCreateState(AnimatorStateMachine sm, string stateName)
    {
        foreach (var state in sm.states)
        {
            if (state.state.name == stateName) return state.state;
        }
        return sm.AddState(stateName);
    }

    private static void SetupTransition(AnimatorState from, AnimatorState to, string param, bool expectedValue)
    {
        foreach (var trans in from.transitions)
        {
            if (trans.destinationState == to) return;
        }
        var newTrans = from.AddTransition(to);
        newTrans.AddCondition(expectedValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, param);
        newTrans.hasExitTime = false;
        newTrans.duration = 0f;
    }

    private static void SetupAnyStateTransition(AnimatorStateMachine sm, AnimatorState to, string triggerParam, AnimatorConditionMode mode = AnimatorConditionMode.If)
    {
        foreach (var trans in sm.anyStateTransitions)
        {
            if (trans.destinationState == to) return;
        }
        var newTrans = sm.AddAnyStateTransition(to);
        newTrans.AddCondition(mode, 0, triggerParam);
        newTrans.hasExitTime = false;
        newTrans.duration = 0f;
    }

    private static void SetupExitTransition(AnimatorState from, AnimatorState to)
    {
        foreach (var trans in from.transitions)
        {
            if (trans.destinationState == to && trans.hasExitTime) return;
        }
        var newTrans = from.AddTransition(to);
        newTrans.hasExitTime = true;
        newTrans.exitTime = 1f; // End of animation
        newTrans.duration = 0f;
    }

    private static void SetupPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Enemies" });
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab.GetComponent<EnemyController>() == null) continue;

            // Ensure AudioSource
            if (prefab.GetComponent<AudioSource>() == null)
            {
                prefab.AddComponent<AudioSource>();
            }

            // Ensure Feedback components
            if (prefab.GetComponent<EnemyAnimationController>() == null)
            {
                prefab.AddComponent<EnemyAnimationController>();
            }
            if (prefab.GetComponent<EnemyAudioController>() == null)
            {
                prefab.AddComponent<EnemyAudioController>();
            }
            if (prefab.GetComponent<EnemyVFXController>() == null)
            {
                prefab.AddComponent<EnemyVFXController>();
            }

            EditorUtility.SetDirty(prefab);
        }
        AssetDatabase.SaveAssets();
    }
}
