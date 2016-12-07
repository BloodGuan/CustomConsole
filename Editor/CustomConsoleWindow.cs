using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;

public class CustomConsoleWindow : EditorWindow
{
    public static List<LogEntry> logEntries = new List<LogEntry>();

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
        string log = "+";
        for (int i = 0; i < 10; i++)
        {
            log += log;
        }
        Debug.Log(log);
        instance.Show(true);
        instance.Focus();
    }

    public CustomConsoleWindow()
    {
        logListView = new ListViewState(0, 32);
    }

    ListViewState logListView;
    object spl = null;
    bool toggle = false;
    Vector2 textScroll = Vector2.zero;
    string selectLogText = "";

    void OnGUI()
    {
        WindowGUIStyles.Load();
        DrawToolbar();
        DrawList();
    }
    void DrawToolbar()
    {
        GUILayout.BeginHorizontal(WindowGUIStyles.Toolbar);
        if (GUILayout.Button("Clear", WindowGUIStyles.ToolbarButton))
        {
            Clear();
        }
        if (GUILayout.Button("Add Log", WindowGUIStyles.ToolbarButton))
        {
            Debug.Log("Log");
            LogEntry add = new LogEntry();
            add.type = LogType.Log;
            add.message = "Log";
            add.stackTrace = LogEntry.GetStackTrace();
            logEntries.Add(add);
            Repaint();
        }
        if (GUILayout.Button("Add Err", WindowGUIStyles.ToolbarButton))
        {
            LogEntry add = new LogEntry();
            add.type = LogType.Error;
            add.message = "Err";
            logEntries.Add(add);
            Repaint();
        }
        if (GUILayout.Button("Add War", WindowGUIStyles.ToolbarButton))
        {
            LogEntry add = new LogEntry();
            add.type = LogType.Warning;
            add.message = "War";
            logEntries.Add(add);
            Repaint();
        }
        toggle = GUILayout.Toggle(toggle, "Toggle", WindowGUIStyles.ToolbarButton);
        GUILayout.EndHorizontal();
    }
    void DrawList()
    {
        Event current = Event.current;
        this.logListView.totalRows = logEntries.Count;

        spl = EditorDrawTools.BeginVerticalSplit(spl);
        GUIContent logContent = new GUIContent();
        int controlID = GUIUtility.GetControlID(FocusType.Native);
        ListViewGUI.ForeachListView(ListViewGUI.ListView(logListView.obj, WindowGUIStyles.Box),
        (listViewElement) =>
        {
            if (current.type == EventType.MouseDown && current.button == 0 && listViewElement.position.Contains(current.mousePosition))
            {
                if (current.clickCount == 2)
                {
                    logEntries[logListView.row].GetDoubleClick();
                }
            }
            if (current.type == EventType.Repaint)
            {
                GUIStyle backgroundStyle = (listViewElement.row % 2 != 0) ? WindowGUIStyles.LogBackground_1 : WindowGUIStyles.LogBackground_2;
                backgroundStyle.Draw(listViewElement.position, false, false, this.logListView.row == listViewElement.row, false);
                logContent.text = logEntries[listViewElement.row].GetTwoLine();
                GUIStyle entryStyle = WindowGUIStyles.GetEntryStyle(logEntries[listViewElement.row].type);
                entryStyle.Draw(listViewElement.position, logContent, controlID, this.logListView.row == listViewElement.row);
            }
            if (this.logListView.totalRows == 0 || this.logListView.row >= this.logListView.totalRows || this.logListView.row < 0)
            {
                selectLogText = "";
            }
            else
            {
                selectLogText = logEntries[logListView.row].GetAllLine();
            }
        });
        EditorDrawTools.MidVerticalSplit();
        textScroll = GUILayout.BeginScrollView(textScroll);
        float minHeight = ((GUIStyle)"CN Message").CalcHeight(new GUIContent(selectLogText), base.position.width);
        EditorGUILayout.SelectableLabel(selectLogText, "CN Message", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinHeight(minHeight));
        GUILayout.EndScrollView();
        EditorDrawTools.EndVerticalSplit();
    }


    void Clear()
    {
        logEntries.Clear();
        Repaint();
    }





    #region GUIStyles
    public class WindowGUIStyles
    {
        private static bool inited = false;
        public static GUIStyle Toolbar;
        public static GUIStyle ToolbarButton;
        public static GUIStyle Box;
        public static GUIStyle LogBackground_1;
        public static GUIStyle LogBackground_2;
        public static GUIStyle ErrorStyle;
        public static GUIStyle LogStyle;
        public static GUIStyle WarningStyle;

        public static void Load()
        {
            if (inited) return;
            inited = true;

            Toolbar = "Toolbar";
            ToolbarButton = "ToolbarButton";
            Box = "CN Box";
            LogBackground_1 = "CN EntryBackEven";
            LogBackground_2 = "CN EntryBackodd";
            ErrorStyle = "CN EntryError";
            LogStyle = "CN EntryInfo";
            WarningStyle = "CN EntryWarn";
        }
        public static GUIStyle GetEntryStyle(LogType type)
        {
            switch (type)
            {
                case LogType.Log:
                    return LogStyle;
                case LogType.Warning:
                    return WarningStyle;
                default:
                    return ErrorStyle;
            }
        }
    }
    #endregion
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
        public static void ForeachListView(IEnumerator ie, Action<ListViewElement> foreachAction)
        {
            while (ie.MoveNext())
            {
                if (foreachAction != null)
                {
                    foreachAction(new ListViewElement(ie.Current));
                }
            }
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
public class LogEntry
{
    public LogType type;
    public string message;
    public string stackTrace;

    public LogEntry()
    {
 
    }
    public LogEntry(LogType type, string messag, string stackTrace)
    {

    }

    public void GetDoubleClick()
    {
 
    }
    public string GetTwoLine()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        string[] messageLines = message.Split('\n');
        if (messageLines.Length >= 2)
        {
            sb.AppendLine(messageLines[0]);
            sb.AppendLine(messageLines[1]);
        }
        else
        {
            sb.AppendLine(message);
        }

        return sb.ToString();
    }
    public string GetAllLine()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine(message);
        sb.AppendLine(stackTrace);
        return sb.ToString();
    }

    public static string GetStackTrace()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, true);
        System.Diagnostics.StackFrame[] sfs = st.GetFrames();
        
        for (int i = 0; i < sfs.Length; i++)
        {
            System.Diagnostics.StackFrame item = sfs[i];
            string fileName = item.GetFileName();
            MethodBase method = item.GetMethod();
            ParameterInfo[] parameters = method.GetParameters();
            // part.1 --- 类名:方法名(参数类型)
            sb.Append(method.ReflectedType.FullName);
            sb.Append(":");
            sb.Append(method.Name);
            sb.Append("(");
            for (int j = 0; j < parameters.Length; j++)
            {
                sb.Append(parameters[j].ParameterType.FullName);
                sb.Append(" ");
                sb.Append(parameters[j].Name);
                if (j + 1 < parameters.Length)
                    sb.Append(",");
            }
            sb.Append(")");
            // part.1 --- end

            // part.2 --- (at 路径:行数)
            
            if (IsFileInDataPath(fileName))
            {
                sb.Append(" (");
                sb.Append("at ");
                sb.Append(ReplaceDataPath(fileName));
                sb.Append(":");
                sb.Append(item.GetFileLineNumber());
                sb.Append(")");
            }
            // part.2 --- end

            if (i + 1 < sfs.Length)
                sb.Append("\n");
        }

        return sb.ToString();
    }
    private static bool IsFileInDataPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) return false;
        string dataPath = Application.dataPath;
        dataPath = dataPath.Replace('\\', '/');
        fullPath = fullPath.Replace('\\', '/');
        return fullPath.StartsWith(dataPath);
    }
    private static string ReplaceDataPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath)) return "";
        string dataPath = Application.dataPath;
        dataPath = dataPath.Replace('\\', '/');
        fullPath = fullPath.Replace('\\', '/');
        return "Assets" + fullPath.Replace(dataPath, "");
    }
}