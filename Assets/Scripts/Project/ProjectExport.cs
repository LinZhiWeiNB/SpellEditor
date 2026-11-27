using System.IO;
using Google.Protobuf;
using RebornFrame.GameData;

namespace GameContentEditor
{
    public static class ProjectExport
    {
        public static bool HasSettings()
        {
            var archive = ProjectSession.Archive;
            if (archive == null)
            {
                return false;
            }

            if (ProjectSession.AuthorId != 0)
            {
                return (archive.ExportName ?? "").Trim().Length > 0
                       && IsEnglishDirectory((archive.ExportDirectoryName ?? "").Trim());
            }

            return (archive.ExportLocation ?? "").Trim().Length > 0
                   && (archive.ExportModDirectory ?? "").Trim().Length > 0;
        }

        public static string ExportStored()
        {
            var archive = ProjectSession.Archive;
            if (archive == null)
            {
                return "还没有打开项目。";
            }

            if (ProjectSession.AuthorId != 0)
            {
                return ExportMod(archive.ExportName, archive.ExportDescription, archive.ExportDirectoryName);
            }

            return ExportGame(archive.ExportLocation, archive.ExportModDirectory);
        }

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
            return WritePackage(exportLocation, stem, new GameDataContent
            {
                EditorVersion = EditorVersion.Current,
                Role = GameDataRole.Game,
                ExportLocation = Path.GetFullPath(exportLocation),
                ModDirectory = modDirectory,
                Name = ProjectSession.Archive.Name,
                RenderPlugins = ProjectSession.Archive.RenderPlugins,
            });
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
            return WritePackage(folder, directoryName, new GameDataContent
            {
                EditorVersion = EditorVersion.Current,
                Role = GameDataRole.Mod,
                Name = modName,
                Description = intro.Trim(),
                DirectoryName = directoryName,
            });
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

        static string WritePackage(string folder, string stem, GameDataContent content)
        {
            ProjectSession.Save();
            content.Entries.Add(new GameDataEntry(
                GameDataKinds.SpellModuleKey,
                GameDataKinds.SpellModule,
                ProjectSession.OwnModule.ToByteArray()));
            var presets = ProjectSession.Archive == null ? null : ProjectSession.Archive.UnitPresets;
            if (presets != null && presets.Count > 0)
            {
                content.Entries.Add(new GameDataEntry(
                    UnitPresetPackage.Key,
                    UnitPresetPackage.Kind,
                    UnitPresetPackage.Encode(presets)));
            }
            if (!ResourceLibrary.TryExportEntries(content.Entries, out var resourceError))
            {
                return resourceError;
            }

            UnitDiffs.Export();
            try
            {
                GameDataFile.Write(Path.Combine(folder, stem + GameDataFile.Extension), content);
            }
            catch (IOException)
            {
                return "数据包没有写出去。";
            }

            return null;
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

            error = "需要一份写有模组目录的游戏数据包。";
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
