using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;


public class EditorDrawTools
{
    public static bool DrawButton(string name, float minWidth = 70f)
    {
        return GUILayout.Button(name, GUILayout.MinWidth(minWidth), GUILayout.ExpandWidth(false));
    }
    public static bool DrawButton(string name, Color color, float minWidth = 70f)
    {
        Color temp = GUI.color;
        GUI.color = color;
        bool r = GUILayout.Button(name, GUILayout.MinWidth(minWidth), GUILayout.ExpandWidth(false));
        GUI.color = temp;
        return r;
    }

    private static object NewSplitterState()
    {
        Assembly editorAsm = Assembly.GetAssembly(typeof(EditorGUILayout));
        Type SplitterState = editorAsm.GetType("UnityEditor.SplitterState");
        return Activator.CreateInstance(SplitterState, new float[]
        {
            70f,
            30f
        }, new int[]
        {
            32,
            32
        }, null);
    }
    public static object BeginVerticalSplit(object state, params GUILayoutOption[] options)
    {
        if (state == null) state = NewSplitterState();
        Assembly editorAsm = Assembly.GetAssembly(typeof(EditorGUILayout));
        Type SplitterGUILayout = editorAsm.GetType("UnityEditor.SplitterGUILayout");
        MethodInfo BeginSplit = SplitterGUILayout.GetMethod("BeginSplit", BindingFlags.Static | BindingFlags.Public);
        object[] args = new object[] { state, GUIStyle.none, true, options };
        BeginSplit.Invoke(null, args);
        EditorGUIUtility.SetIconSize(new Vector2(32f, 32f));
        return state;
    }
    public static void MidVerticalSplit()
    {
        EditorGUIUtility.SetIconSize(Vector2.zero);
        EditorGUILayout.BeginVertical("CN Box");
    }
    public static void EndVerticalSplit()
    {
        GUILayout.EndVertical();
        Type type = typeof(GUILayoutUtility);
        MethodInfo EndLayoutGroup = type.GetMethod("EndLayoutGroup", BindingFlags.Static | BindingFlags.NonPublic);
        EndLayoutGroup.Invoke(null, null);
    }
}
