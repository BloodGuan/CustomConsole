using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public class CustomConsoleWindow : EditorWindow
{
    private static CustomConsoleWindow _instance;
    public static CustomConsoleWindow instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = ScriptableObject.CreateInstance<CustomConsoleWindow>();
            }
            return _instance;
        }
    }

    [MenuItem("Custom/Console")]
    public static void OpenWindow()
    {
        instance.Show(true);
        instance.Focus();
    }

    public CustomConsoleWindow()
    {
        logListView = new ListViewState(0, 30);
    }

    ListViewState logListView;
    object spl = null;
    bool toggle = false;
    Vector2 textScroll = Vector2.zero;
    void OnGUI()
    {
        DrawToolbar();
        DrawList();
    }
    void DrawToolbar()
    {
        GUILayout.BeginHorizontal("Toolbar");
        GUILayout.Button("Button", "toolbarbutton");
        toggle = GUILayout.Toggle(toggle, "Toggle", "toolbarbutton");
        GUILayout.EndHorizontal();
    }
    void DrawList()
    {
        Event current = Event.current;
        string text = "hhhh\nhhhh\nhhhh\nhhhh\nhhhh\nhhhh\nhhhh\nhhhh\nhhhh\nhhhh\nhhhh\nhhhh\nhhhh";
        this.logListView.totalRows = 55;

        spl = EditorDrawTools.BeginVerticalSplit(spl);
        int controlID = GUIUtility.GetControlID(FocusType.Native);
        IEnumerator vl = ListViewGUI.ListView(logListView.obj, "CN Box");
        while (vl.MoveNext())
        {
            ListViewElement listViewElement = new ListViewElement(vl.Current);
            if (current.type == EventType.MouseDown && current.button == 0 && listViewElement.position.Contains(current.mousePosition))
            {
                if (current.clickCount == 2)
                {
                    Debug.Log("双击了第" + logListView.row + "行");
                }
            }
            if (current.type == EventType.Repaint)
            {
                GUIStyle gUIStyle = (listViewElement.row % 2 != 0) ? "CN EntryBackEven" : "CN EntryBackodd";
                gUIStyle.Draw(listViewElement.position, false, false, this.logListView.row == listViewElement.row, false);
                GUIContent gUIContent = new GUIContent();
                gUIContent.text = listViewElement.row.ToString() + "\n" + text;
                GUIStyle styleForErrorMode = "CN EntryWarn";
                styleForErrorMode.Draw(listViewElement.position, gUIContent, controlID, this.logListView.row == listViewElement.row);
            }
            if (this.logListView.totalRows == 0 || this.logListView.row >= this.logListView.totalRows || this.logListView.row < 0)
            {
                Debug.Log("取消选中");
            }
            else
            {
                Debug.Log("选中第" + logListView.row + "行");
            }
        }
        EditorDrawTools.MidVerticalSplit();
        textScroll = GUILayout.BeginScrollView(textScroll);
        for (int i = 0; i < 5; i++)
        {
            GUILayout.Label(text);
            GUILayout.Label("--------------------------------------------------------------");
        }
        GUILayout.EndScrollView();
        EditorDrawTools.EndVerticalSplit();
    }









    #region Reflection
    public class ListViewGUI
    {
        private static MethodInfo method_ListView;
        public static IEnumerator ListView(object state, GUIStyle style, params GUILayoutOption[] options)
        {
            if (method_ListView == null)
            {
                Assembly editorAsm = Assembly.GetAssembly(typeof(EditorGUILayout));
                Type ListViewGUI = editorAsm.GetType("UnityEditor.ListViewGUI");
                Type ListViewState = editorAsm.GetType("UnityEditor.ListViewState");
                Type[] tys = new Type[] { ListViewState, typeof(GUIStyle), typeof(GUILayoutOption[]) };
                method_ListView = ListViewGUI.GetMethod("ListView", tys);
            }
            object[] args = new object[] { state, style, options };
            return (IEnumerator)method_ListView.Invoke(null, args);
        }
    }

    [Serializable]
    public class ListViewState
    {
        [SerializeField]
        public object obj;

        private static Type m_type;
        private static FieldInfo m_field_row;
        private static FieldInfo m_field_selectionChanged;
        private static FieldInfo m_field_totalRows;
        private static FieldInfo m_field_scrollPos;
        private static FieldInfo m_field_rowHeight;
        private static FieldInfo m_field_ID;

        private static Type type
        {
            get
            {
                if (m_type == null)
                {
                    Assembly editorAsm = Assembly.GetAssembly(typeof(EditorGUILayout));
                    m_type = editorAsm.GetType("UnityEditor.ListViewState");
                }
                return m_type;
            }
        }
        private static FieldInfo field_row
        {
            get
            {
                if (m_field_row == null)
                {
                    m_field_row = type.GetField("row", BindingFlags.Public | BindingFlags.Instance);
                }
                return m_field_row;
            }
        }
        private static FieldInfo field_selectionChanged
        {
            get
            {
                if (m_field_selectionChanged == null)
                {
                    m_field_selectionChanged = type.GetField("selectionChanged", BindingFlags.Public | BindingFlags.Instance);
                }
                return m_field_selectionChanged;
            }
        }
        private static FieldInfo field_totalRows
        {
            get
            {
                if (m_field_totalRows == null)
                {
                    m_field_totalRows = type.GetField("totalRows", BindingFlags.Public | BindingFlags.Instance);
                }
                return m_field_totalRows;
            }
        }
        private static FieldInfo field_scrollPos
        {
            get
            {
                if (m_field_scrollPos == null)
                {
                    m_field_scrollPos = type.GetField("scrollPos", BindingFlags.Public | BindingFlags.Instance);
                }
                return m_field_scrollPos;
            }
        }
        private static FieldInfo field_rowHeight
        {
            get
            {
                if (m_field_rowHeight == null)
                {
                    m_field_rowHeight = type.GetField("rowHeight", BindingFlags.Public | BindingFlags.Instance);
                }
                return m_field_rowHeight;
            }
        }
        private static FieldInfo field_ID
        {
            get
            {
                if (m_field_ID == null)
                {
                    m_field_ID = type.GetField("ID", BindingFlags.Public | BindingFlags.Instance);
                }
                return m_field_ID;
            }
        }

        public ListViewState(int totalRows, int rowHeight)
        {
            obj = Activator.CreateInstance(type, totalRows, rowHeight);
        }
        public int row
        {
            get
            {
                return (int)field_row.GetValue(obj);
            }
            set
            {
                field_row.SetValue(obj, value);
            }
        }
        public bool selectionChanged
        {
            get
            {
                return (bool)field_selectionChanged.GetValue(obj);
            }
            set
            {
                field_selectionChanged.SetValue(obj, value);
            }
        }
        public int totalRows
        {
            get
            {
                return (int)field_totalRows.GetValue(obj);
            }
            set
            {
                field_totalRows.SetValue(obj, value);
            }
        }
        public Vector2 scrollPos
        {
            get
            {
                return (Vector2)field_scrollPos.GetValue(obj);
            }
            set
            {
                field_scrollPos.SetValue(obj, value);
            }
        }
        public int rowHeight
        {
            get
            {
                return (int)field_rowHeight.GetValue(obj);
            }
            set
            {
                field_rowHeight.SetValue(obj, value);
            }
        }
        public int ID
        {
            get
            {
                return (int)field_ID.GetValue(obj);
            }
            set
            {
                field_ID.SetValue(obj, value);
            }
        }
    }

    [Serializable]
    public class ListViewElement
    {
        [SerializeField]
        public object obj;

        private static Type m_type;
        private static FieldInfo m_field_row;
        private static FieldInfo m_field_column;
        private static FieldInfo m_field_position;

        private static Type type
        {
            get
            {
                if (m_type == null)
                {
                    Assembly editorAsm = Assembly.GetAssembly(typeof(EditorGUILayout));
                    m_type = editorAsm.GetType("UnityEditor.ListViewElement");
                }
                return m_type;
            }
        }
        private static FieldInfo field_row
        {
            get
            {
                if (m_field_row == null)
                {
                    m_field_row = type.GetField("row", BindingFlags.Public | BindingFlags.Instance);
                }
                return m_field_row;
            }
        }
        private static FieldInfo field_column
        {
            get
            {
                if (m_field_column == null)
                {
                    m_field_column = type.GetField("column", BindingFlags.Public | BindingFlags.Instance);
                }
                return m_field_column;
            }
        }
        private static FieldInfo field_position
        {
            get
            {
                if (m_field_position == null)
                {
                    m_field_position = type.GetField("position", BindingFlags.Public | BindingFlags.Instance);
                }
                return m_field_position;
            }
        }

        public ListViewElement(object obj)
        {
            this.obj = obj;
        }

        public int row
        {
            get
            {
                return (int)field_row.GetValue(obj);
            }
            set
            {
                field_row.SetValue(obj, value);
            }
        }

        public int column
        {
            get
            {
                return (int)field_column.GetValue(obj);
            }
            set
            {
                field_column.SetValue(obj, value);
            }
        }

        public Rect position
        {
            get
            {
                return (Rect)field_position.GetValue(obj);
            }
            set
            {
                field_position.SetValue(obj, value);
            }
        }
    }
    #endregion
}
