namespace SpellEditor
{
    /// <summary>
    /// 编辑器在 Unity 里运行时使用的系统文件选择。
    /// </summary>
    public static class EditorDialog
    {
        public static string OpenFolder(string title)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFolderPanel(title, "", "");
#else
            return "";
#endif
        }

        public static string OpenByteFile(string title)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanel(title, "", "byte");
#else
            return "";
#endif
        }

        public static string OpenDescriptor(string title)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanel(title, "", "json");
#else
            return "";
#endif
        }
    }
}
