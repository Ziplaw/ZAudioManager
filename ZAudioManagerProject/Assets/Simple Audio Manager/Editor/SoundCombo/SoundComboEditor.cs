using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

public class AssetHandler
{
    [OnOpenAsset]
    public static bool OpenEditor(int instanceId, int line)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceId) as SoundCombo;
        if (obj != null)
        {
            SoundComboEditorWindow.Open(obj);
            return true;
        }

        return false;
    }
}


[CustomEditor(typeof(SoundCombo))]
public class SoundComboEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GUILayout.Label("Double Click me in the Project tab!");
        base.OnInspectorGUI();
    }
}
