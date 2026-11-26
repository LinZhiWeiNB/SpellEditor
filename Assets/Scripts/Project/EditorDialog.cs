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

        public static string OpenDataPackage(string title)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanel(title, "", "bin");
#else
            return "";
#endif
        }

        public static string OpenAssetBundle(string title)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanelWithFilters(title, "", new[]
            {
                "AssetBundle", "ab,bundle,unity3d",
                "所有文件", "*",
            });
#else
            return "";
#endif
        }

        public static string OpenAudioFile(string title)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.OpenFilePanelWithFilters(title, "", new[]
            {
                "音频", "wav,mp3,ogg,aiff,aif",
                "所有文件", "*",
            });
#else
            return "";
#endif
        }
    }
}
