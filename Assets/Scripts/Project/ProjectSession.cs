using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf.Collections;
using RebornFrame.Spell;

namespace SpellEditor
{
    public sealed class LoadedSource
    {
        public string AbsolutePath;
        public string RelativePath;
        public string Title;
        public uint AuthorId;
        public bool AuthorKnown;
        public bool IsGame;
        public SpellModuleData Module;
        public string EditorVersion;
        public bool VersionMismatch;
        public string ParseWarning;
        public string ExportLocation;
        public string ModDirectory;
    }

    /// <summary>
    /// 当前打开的编辑器项目。运行时模块在 OwnModule，备注名和载入列表在存档里。
    /// </summary>
    public static class ProjectSession
    {
        public const string RuntimeVersion = "1.1.0";

        public static ProjectArchiveFile Archive { get; private set; }
        public static string ArchivePath { get; private set; }
        public static SpellModuleData OwnModule { get; private set; }
        public static readonly List<LoadedSource> Sources = new List<LoadedSource>();

        public static uint AuthorId => Archive == null ? 0u : Archive.AuthorId;

        public static bool IsOpen => Archive != null && !string.IsNullOrEmpty(ArchivePath);

        public static string Create(string name, string folder, uint authorId, List<string> sourcePaths, out string error)
        {
            error = null;
            name = (name ?? "").Trim();
            if (name.Length == 0)
            {
                error = "先填写项目名称。";
                return null;
            }

            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                error = "先选择一个存在的文件夹。";
                return null;
            }

            var fileName = SanitizeFileName(name);
            if (fileName.Length == 0)
            {
                error = "项目名称不能作为文件名。";
                return null;
            }

            var archivePath = Path.Combine(folder, fileName + ".byte");
            if (File.Exists(archivePath))
            {
                error = "这个文件夹里已经有同名的 .byte 存档。";
                return null;
            }

            if (sourcePaths == null)
            {
                sourcePaths = new List<string>();
            }

            if (authorId != 0 && !HasGameSource(sourcePaths, out error))
            {
                return null;
            }

            var archive = new ProjectArchiveFile
            {
                Format = ProjectArchiveFile.CurrentFormat,
                Name = name,
                AuthorId = authorId,
                RuntimeVersion = RuntimeVersion,
                Module = new SpellModuleData(),
            };
            for (int i = 0; i < sourcePaths.Count; i++)
            {
                archive.LoadedSources.Add(ToRelative(folder, sourcePaths[i]));
            }

            File.WriteAllBytes(archivePath, archive.Write());
            return Open(archivePath, out error) ? archivePath : null;
        }

        public static bool Open(string path, out string error)
        {
            error = null;
            Archive = null;
            ArchivePath = null;
            OwnModule = null;
            Sources.Clear();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                error = "找不到这个存档。";
                return false;
            }

            ProjectArchiveFile archive;
            try
            {
                archive = ProjectArchiveFile.Read(File.ReadAllBytes(path));
            }
            catch (Exception)
            {
                error = "这个文件不是当前编辑器能读的存档。";
                return false;
            }

            if (archive.Format != ProjectArchiveFile.CurrentFormat)
            {
                error = "这个存档的格式版本编辑器还不认识。";
                return false;
            }

            Archive = archive;
            ArchivePath = Path.GetFullPath(path);
            OwnModule = archive.Module ?? new SpellModuleData();
            archive.Module = OwnModule;
            var directory = Path.GetDirectoryName(ArchivePath);
            for (int i = 0; i < archive.LoadedSources.Count; i++)
            {
                var relative = archive.LoadedSources[i];
                var absolute = Resolve(directory, relative);
                if (!TryLoadSource(absolute, relative, out var source))
                {
                    continue;
                }

                Sources.Add(source);
            }

            RecentProjects.Touch(ArchivePath, archive.Name, archive.AuthorId);
            return true;
        }

        public static void Save()
        {
            if (!IsOpen)
            {
                return;
            }

            Archive.Module = OwnModule;
            File.WriteAllBytes(ArchivePath, Archive.Write());
        }

        public static string AttributeName(ulong id, uint localId)
        {
            if (Archive != null && Archive.AttributeNames.TryGetValue(id, out var name) && !string.IsNullOrEmpty(name))
            {
                return name;
            }

            return "属性 " + localId;
        }

        public static void SetAttributeName(ulong id, string name)
        {
            if (!IsOpen)
            {
                return;
            }

            name = (name ?? "").Trim();
            if (name.Length == 0)
            {
                Archive.AttributeNames.Remove(id);
            }
            else
            {
                Archive.AttributeNames[id] = name;
            }

            Save();
        }

        public static void MoveAttributeName(ulong fromId, ulong toId)
        {
            if (Archive == null || fromId == toId)
            {
                return;
            }

            if (Archive.AttributeNames.TryGetValue(fromId, out var name))
            {
                Archive.AttributeNames.Remove(fromId);
                Archive.AttributeNames[toId] = name;
            }
        }

        public static bool TryChangeLocalId(AttributeDefinitionConfig definition, uint localId, out string error)
        {
            error = null;
            var next = AttributeId.Combine(AuthorId, localId).Value;
            if (definition.Id == next)
            {
                return true;
            }

            var attributes = OwnModule.Attributes;
            for (int i = 0; i < attributes.Count; i++)
            {
                if (!ReferenceEquals(attributes[i], definition) && attributes[i].Id == next)
                {
                    error = "当前数据源里已经有这个属性 ID。";
                    return false;
                }
            }

            for (int i = 0; i < Sources.Count; i++)
            {
                var foreign = FindAttribute(Sources[i].Module, next);
                if (foreign != null && !SameDefinition(definition, foreign))
                {
                    error = "这个标识和已载入的数据源冲突。";
                    return false;
                }
            }

            MoveAttributeName(definition.Id, next);
            definition.Id = next;
            Save();
            return true;
        }

        public static void DeleteAttribute(AttributeDefinitionConfig definition)
        {
            if (!IsOpen || definition == null)
            {
                return;
            }

            OwnModule.Attributes.Remove(definition);
            Archive.AttributeNames.Remove(definition.Id);
            Save();
        }

        public static bool TryLoadSource(string absolutePath, string relativePath, out LoadedSource source)
        {
            source = null;
            if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
            {
                return false;
            }

            if (!ModuleDescriptorFile.TryRead(absolutePath, out var descriptor, out _))
            {
                return false;
            }

            var directory = Path.GetDirectoryName(absolutePath);
            var modulePath = Resolve(directory, descriptor.module_data);
            SpellModuleData module = null;
            string parseWarning = null;
            if (!File.Exists(modulePath))
            {
                parseWarning = "找不到描述文件指向的数据，可能无法正常解析。";
            }
            else if (!TryReadModule(modulePath, out module, out _))
            {
                parseWarning = "描述文件指向的数据没能解析，可能无法正常使用。";
            }

            var authorKnown = TryAuthorId(module, out var authorId);
            var isGame = (authorKnown && authorId == 0) || (!authorKnown && descriptor.role == "game");
            source = new LoadedSource
            {
                AbsolutePath = absolutePath,
                RelativePath = string.IsNullOrEmpty(relativePath) ? absolutePath : relativePath,
                Title = SourceTitle(descriptor, absolutePath),
                AuthorId = authorId,
                AuthorKnown = authorKnown,
                IsGame = isGame,
                Module = module,
                EditorVersion = descriptor.editor_version ?? "",
                VersionMismatch = !EditorVersion.Matches(descriptor.editor_version),
                ParseWarning = parseWarning,
                ExportLocation = descriptor.export_location ?? "",
                ModDirectory = !string.IsNullOrEmpty(descriptor.mod_directory)
                    ? descriptor.mod_directory
                    : descriptor.directory_name ?? "",
            };
            return true;
        }

        public static bool TryReadModule(string path, out SpellModuleData module, out string error)
        {
            module = null;
            error = null;
            try
            {
                module = SpellModuleData.Parser.ParseFrom(File.ReadAllBytes(path));
                return true;
            }
            catch (Exception)
            {
                error = "这个文件不是数据源。";
                return false;
            }
        }

        public static string KindLabel(LoadedSource source)
        {
            if (source.IsGame)
            {
                return "游戏数据源";
            }

            if (source.AuthorKnown)
            {
                return "模组数据源 · 作者 " + source.AuthorId;
            }

            return "模组数据源";
        }

        public static bool SameDefinition(AttributeDefinitionConfig left, AttributeDefinitionConfig right)
        {
            return left.ValueKind == right.ValueKind
                   && left.Behavior == right.Behavior
                   && left.NonNegative == right.NonNegative
                   && left.ScaleCurrentWithMax == right.ScaleCurrentWithMax;
        }

        static bool HasGameSource(List<string> sourcePaths, out string error)
        {
            error = "模组需要一份游戏数据源。";
            for (int i = 0; i < sourcePaths.Count; i++)
            {
                if (!TryLoadSource(sourcePaths[i], sourcePaths[i], out var source))
                {
                    continue;
                }

                if (source.IsGame && source.Module != null)
                {
                    error = null;
                    return true;
                }
            }

            return false;
        }

        static string SourceTitle(ModuleDescriptor descriptor, string path)
        {
            if (!string.IsNullOrEmpty(descriptor.name))
            {
                return descriptor.name;
            }

            var file = Path.GetFileNameWithoutExtension(path);
            const string suffix = ".module";
            if (file.EndsWith(suffix))
            {
                file = file.Substring(0, file.Length - suffix.Length);
            }

            return file;
        }

        static bool TryAuthorId(SpellModuleData module, out uint authorId)
        {
            authorId = 0;
            if (module == null)
            {
                return false;
            }

            if (FirstId(module.Attributes, out authorId)) return true;
            if (FirstId(module.Tags, out authorId)) return true;
            if (FirstId(module.Buffs, out authorId)) return true;
            if (FirstId(module.Spells, out authorId)) return true;
            return false;
        }

        static bool FirstId<T>(RepeatedField<T> items, out uint authorId) where T : class
        {
            authorId = 0;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null)
                {
                    continue;
                }

                authorId = new AttributeId(ReadId(items[i])).AuthorId;
                return true;
            }

            return false;
        }

        static ulong ReadId(object item)
        {
            switch (item)
            {
                case AttributeDefinitionConfig attribute:
                    return attribute.Id;
                case TagDefinitionConfig tag:
                    return tag.Id;
                case BuffDefinitionConfig buff:
                    return buff.Id;
                case SpellData spell:
                    return spell.Id;
                default:
                    return 0;
            }
        }

        static AttributeDefinitionConfig FindAttribute(SpellModuleData module, ulong id)
        {
            if (module == null)
            {
                return null;
            }

            var attributes = module.Attributes;
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i].Id == id)
                {
                    return attributes[i];
                }
            }

            return null;
        }

        static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var chars = name.ToCharArray();
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

            return new string(chars).Trim();
        }

        public static string ToRelative(string directory, string absolutePath)
        {
            var from = Path.GetFullPath(directory);
            if (!from.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                from += Path.DirectorySeparatorChar;
            }

            var relative = Uri.UnescapeDataString(new Uri(from).MakeRelativeUri(new Uri(Path.GetFullPath(absolutePath))).ToString());
            return relative.Replace('\\', '/');
        }

        public static string Resolve(string directory, string relative)
        {
            var parts = (relative ?? "").Replace('\\', '/');
            return Path.GetFullPath(Path.Combine(directory, parts));
        }
    }
}
