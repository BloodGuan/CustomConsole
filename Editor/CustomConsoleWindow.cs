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
        IEnumerator vl = ListView(logListView.obj, "CN Box");
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












    public static IEnumerator ListView(object state, GUIStyle style, params GUILayoutOption[] options)
    {
        object obj = null;
        Assembly editorAsm = Assembly.GetAssembly(typeof(EditorGUILayout));
        Type ListViewGUI = editorAsm.GetType("UnityEditor.ListViewGUI");
        Type ListViewState = editorAsm.GetType("UnityEditor.ListViewState");
        Type[] tys = new Type[] { ListViewState, typeof(GUIStyle), typeof(GUILayoutOption[]) };
        MethodInfo ListView = ListViewGUI.GetMethod("ListView", tys);
        object[] args = new object[] { state, style, options };
        obj = ListView.Invoke(null, args);
        return (IEnumerator)obj;
    }

    [Serializable]
    public class ListViewState
    {
        [SerializeField]
        public object obj;

        private FieldInfo _row;
        private FieldInfo _selectionChanged;
        private FieldInfo _totalRows;
        private FieldInfo _scrollPos;
        private FieldInfo _rowHeight;
        private FieldInfo _ID;

        public ListViewState(int totalRows, int rowHeight)
        {
            Assembly editorAsm = Assembly.GetAssembly(typeof(EditorGUILayout));
            Type type = editorAsm.GetType("UnityEditor.ListViewState");
            obj = Activator.CreateInstance(type, totalRows, rowHeight);
            _row = type.GetField("row", BindingFlags.Public | BindingFlags.Instance);
            _selectionChanged = type.GetField("selectionChanged", BindingFlags.Public | BindingFlags.Instance);
            _totalRows = type.GetField("totalRows", BindingFlags.Public | BindingFlags.Instance);
            _scrollPos = type.GetField("scrollPos", BindingFlags.Public | BindingFlags.Instance);
            _rowHeight = type.GetField("rowHeight", BindingFlags.Public | BindingFlags.Instance);
            _ID = type.GetField("ID", BindingFlags.Public | BindingFlags.Instance);
        }
        public int row
        {
            get
            {
                return (int)_row.GetValue(obj);
            }
            set
            {
                _row.SetValue(obj, value);
            }
        }
        public bool selectionChanged
        {
            get
            {
                return (bool)_selectionChanged.GetValue(obj);
            }
            set
            {
                _selectionChanged.SetValue(obj, value);
            }
        }
        public int totalRows
        {
            get
            {
                return (int)_totalRows.GetValue(obj);
            }
            set
            {
                _totalRows.SetValue(obj, value);
            }
        }
        public Vector2 scrollPos
        {
            get
            {
                return (Vector2)_scrollPos.GetValue(obj);
            }
            set
            {
                _scrollPos.SetValue(obj, value);
            }
        }
        public int rowHeight
        {
            get
            {
                return (int)_rowHeight.GetValue(obj);
            }
            set
            {
                _rowHeight.SetValue(obj, value);
            }
        }
        public int ID
        {
            get
            {
                return (int)_ID.GetValue(obj);
            }
            set
            {
                _ID.SetValue(obj, value);
            }
        }
    }

    [Serializable]
    public class ListViewElement
    {
        [SerializeField]
        public object obj;

        private FieldInfo _row;
        private FieldInfo _column;
        private FieldInfo _position;

        public ListViewElement(object obj)
        {
            Assembly editorAsm = Assembly.GetAssembly(typeof(EditorGUILayout));
            Type type = editorAsm.GetType("UnityEditor.ListViewElement");
            this.obj = obj;
            _row = type.GetField("row", BindingFlags.Public | BindingFlags.Instance);
            _column = type.GetField("column", BindingFlags.Public | BindingFlags.Instance);
            _position = type.GetField("position", BindingFlags.Public | BindingFlags.Instance);
        }

        public int row
        {
            get
            {
                return (int)_row.GetValue(obj);
            }
            set
            {
                _row.SetValue(obj, value);
            }
        }

        public int column
        {
            get
            {
                return (int)_column.GetValue(obj);
            }
            set
            {
                _column.SetValue(obj, value);
            }
        }

        public Rect position
        {
            get
            {
                return (Rect)_position.GetValue(obj);
            }
            set
            {
                _position.SetValue(obj, value);
            }
        }
    }
}
