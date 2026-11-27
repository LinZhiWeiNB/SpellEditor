namespace GameContentEditor
{
    /// <summary>
    /// 编辑器自己的版本。和 com.reborn.spell 的运行时版本不是一回事。
    /// </summary>
    public static class EditorVersion
    {
        public const string Current = "1.0.0";
        public const string Repository = "https://github.com/LinZhiWeiNB/GameContentEditor";

        public static string ReleasePage(string version)
        {
            version = (version ?? "").Trim();
            if (version.Length == 0)
            {
                return Repository + "/releases";
            }

            if (version[0] == 'v' || version[0] == 'V')
            {
                version = version.Substring(1);
            }

            return Repository + "/releases/tag/v" + version;
        }

        public static bool Matches(string version)
        {
            version = (version ?? "").Trim();
            if (version.Length > 0 && (version[0] == 'v' || version[0] == 'V'))
            {
                version = version.Substring(1);
            }

            return version == Current;
        }
    }
}
