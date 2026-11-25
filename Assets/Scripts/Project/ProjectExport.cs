using System.IO;
using Google.Protobuf;

namespace SpellEditor
{
    public static class ProjectExport
    {
        public static string ExportGame(string exportLocation, string modDirectory)
        {
            exportLocation = (exportLocation ?? "").Trim();
            modDirectory = (modDirectory ?? "").Trim();
            if (exportLocation.Length == 0)
            {
                return "先填写导出位置。";
            }

            if (modDirectory.Length == 0)
            {
                return "先填写模组目录。";
            }

            Directory.CreateDirectory(exportLocation);
            var stem = FileStem(ProjectSession.Archive.Name);
            WritePackage(exportLocation, stem, new ModuleDescriptor
            {
                format = ModuleDescriptor.CurrentFormat,
                editor_version = EditorVersion.Current,
                role = "game",
                export_location = Path.GetFullPath(exportLocation),
                mod_directory = modDirectory,
                name = ProjectSession.Archive.Name,
                module_data = stem + ".byte",
            });
            return null;
        }

        public static string ExportMod(string modName, string intro, string directoryName)
        {
            modName = (modName ?? "").Trim();
            intro = intro ?? "";
            directoryName = (directoryName ?? "").Trim();
            if (modName.Length == 0)
            {
                return "先填写模组名。";
            }

            if (!IsEnglishDirectory(directoryName))
            {
                return "模组目录名只能使用英语字母、数字、下划线或连字符，并且以字母开头。";
            }

            if (!TryModRoot(out var root, out var error))
            {
                return error;
            }

            var folder = Path.Combine(root, directoryName);
            Directory.CreateDirectory(folder);
            WritePackage(folder, directoryName, new ModuleDescriptor
            {
                format = ModuleDescriptor.CurrentFormat,
                editor_version = EditorVersion.Current,
                role = "mod",
                name = modName,
                description = intro.Trim(),
                directory_name = directoryName,
                module_data = directoryName + ".byte",
            });
            return null;
        }

        public static bool IsEnglishDirectory(string name)
        {
            if (string.IsNullOrEmpty(name) || !IsAsciiLetter(name[0]))
            {
                return false;
            }

            for (int i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (!IsAsciiLetter(c) && !char.IsDigit(c) && c != '_' && c != '-')
                {
                    return false;
                }
            }

            return true;
        }

        static void WritePackage(string folder, string stem, ModuleDescriptor descriptor)
        {
            ProjectSession.Save();
            var bytePath = Path.Combine(folder, stem + ".byte");
            var descPath = Path.Combine(folder, stem + ModuleDescriptor.Extension);
            File.WriteAllBytes(bytePath, ProjectSession.OwnModule.ToByteArray());
            ModuleDescriptorFile.Write(descPath, descriptor);
        }

        static bool TryModRoot(out string root, out string error)
        {
            root = null;
            error = null;
            for (int i = 0; i < ProjectSession.Sources.Count; i++)
            {
                var source = ProjectSession.Sources[i];
                if (!source.IsGame || string.IsNullOrEmpty(source.ModDirectory))
                {
                    continue;
                }

                root = source.ModDirectory;
                if (!Path.IsPathRooted(root))
                {
                    var basis = string.IsNullOrEmpty(source.ExportLocation)
                        ? Path.GetDirectoryName(source.AbsolutePath)
                        : source.ExportLocation;
                    root = Path.GetFullPath(Path.Combine(basis, source.ModDirectory));
                }

                return true;
            }

            error = "需要一份写有模组目录的游戏描述文件。";
            return false;
        }

        static string FileStem(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var chars = (name ?? "Module").ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                for (int j = 0; j < invalid.Length; j++)
                {
                    if (chars[i] == invalid[j])
                    {
                        chars[i] = '_';
                        break;
                    }
                }
            }

            var stem = new string(chars).Trim();
            return stem.Length == 0 ? "Module" : stem;
        }

        static bool IsAsciiLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }
    }
}
