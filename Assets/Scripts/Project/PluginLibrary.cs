using System;
using System.Collections.Generic;
using System.IO;

namespace GameContentEditor
{
    /// <summary>
    /// 项目 plugins 目录里的插件。每个插件是一个子文件夹，里面有 plugin.txt 和入口 lua。
    /// </summary>
    public static class PluginLibrary
    {
        public const string ManifestName = "plugin.txt";

        public sealed class PluginInfo
        {
            public string Id = "";
            public string Name = "";
            public string Entry = "";
            public string FolderName = "";
            public string Directory = "";
        }

        public static string Root()
        {
            if (!ProjectSession.IsOpen || string.IsNullOrEmpty(ProjectSession.ArchivePath))
            {
                return "";
            }

            var project = Path.GetDirectoryName(ProjectSession.ArchivePath);
            if (string.IsNullOrEmpty(project))
            {
                return "";
            }

            return Path.Combine(project, ResourceLibrary.PluginsFolder);
        }

        public static List<PluginInfo> List()
        {
            var list = new List<PluginInfo>();
            var root = Root();
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return list;
            }

            string[] folders;
            try
            {
                folders = Directory.GetDirectories(root);
            }
            catch (Exception)
            {
                return list;
            }

            Array.Sort(folders, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < folders.Length; i++)
            {
                var info = Read(folders[i]);
                if (info != null)
                {
                    list.Add(info);
                }
            }

            return list;
        }

        public static bool Import(string source, out string folderName, out string error)
        {
            folderName = "";
            error = null;
            var root = Root();
            if (string.IsNullOrEmpty(root))
            {
                error = "还没有打开项目。";
                return false;
            }

            if (string.IsNullOrEmpty(source) || !Directory.Exists(source))
            {
                error = "找不到这个目录。";
                return false;
            }

            source = Path.GetFullPath(source);
            var name = Path.GetFileName(source.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!SafeSegment(name))
            {
                error = "这个目录名不能作为插件文件夹。";
                return false;
            }

            var info = Read(source);
            if (info == null)
            {
                error = "目录里没有可用的 plugin.txt。";
                return false;
            }

            if (!File.Exists(Path.Combine(source, info.Entry)))
            {
                error = "plugin.txt 指向的 lua 不在这个目录里。";
                return false;
            }

            var destination = Path.GetFullPath(Path.Combine(root, name));
            if (string.Equals(source, destination, StringComparison.OrdinalIgnoreCase))
            {
                error = "它已经在项目的 plugins 里。";
                return false;
            }

            var prefix = source.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (destination.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                error = "不能把包含 plugins 的上层目录再导入进去。";
                return false;
            }

            try
            {
                Directory.CreateDirectory(root);
                if (Directory.Exists(destination))
                {
                    Directory.Delete(destination, true);
                }

                CopyFolder(source, destination);
            }
            catch (Exception)
            {
                error = "插件没有复制进项目。";
                return false;
            }

            folderName = name;
            return true;
        }

        public static bool Delete(string folderName, out string error)
        {
            error = null;
            var root = Root();
            if (string.IsNullOrEmpty(root))
            {
                error = "还没有打开项目。";
                return false;
            }

            if (!SafeSegment(folderName))
            {
                error = "没有这个插件。";
                return false;
            }

            var path = Path.Combine(root, folderName);
            if (!Directory.Exists(path))
            {
                error = "没有这个插件。";
                return false;
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (Exception)
            {
                error = "插件没有删掉。";
                return false;
            }

            return true;
        }

        public static PluginInfo Read(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return null;
            }

            var manifest = Path.Combine(directory, ManifestName);
            if (!File.Exists(manifest))
            {
                return null;
            }

            string text;
            try
            {
                text = File.ReadAllText(manifest);
            }
            catch (Exception)
            {
                return null;
            }

            var id = "";
            var name = "";
            var entry = "";
            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].TrimEnd('\r').Trim();
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }

                var tab = line.IndexOf('\t');
                if (tab <= 0)
                {
                    continue;
                }

                var key = line.Substring(0, tab).Trim();
                var value = line.Substring(tab + 1).Trim();
                if (key == "id")
                {
                    id = value;
                }
                else if (key == "name")
                {
                    name = value;
                }
                else if (key == "entry")
                {
                    entry = value;
                }
            }

            if (!SafeFile(entry))
            {
                return null;
            }

            var folderName = Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (name.Length == 0)
            {
                name = folderName;
            }

            if (id.Length == 0)
            {
                id = folderName;
            }

            return new PluginInfo
            {
                Id = id,
                Name = name,
                Entry = entry,
                FolderName = folderName,
                Directory = directory,
            };
        }

        static void CopyFolder(string source, string destination)
        {
            Directory.CreateDirectory(destination);
            var files = Directory.GetFiles(source);
            for (var i = 0; i < files.Length; i++)
            {
                var leaf = Path.GetFileName(files[i]);
                if (leaf.Length == 0 || leaf[0] == '.' || leaf.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                File.Copy(files[i], Path.Combine(destination, leaf), true);
            }

            var children = Directory.GetDirectories(source);
            for (var i = 0; i < children.Length; i++)
            {
                var leaf = Path.GetFileName(children[i]);
                if (leaf.Length == 0 || leaf[0] == '.')
                {
                    continue;
                }

                CopyFolder(children[i], Path.Combine(destination, leaf));
            }
        }

        static bool SafeSegment(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "." || name == "..")
            {
                return false;
            }

            return name.IndexOfAny(new[] { '/', '\\', ':' }) < 0;
        }

        static bool SafeFile(string name)
        {
            return SafeSegment(name) && name.IndexOf('.') > 0;
        }
    }
}
