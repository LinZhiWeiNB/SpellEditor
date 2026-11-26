using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
using RebornFrame.GameData;
using RebornFrame.Spell;
using RebornFrame.Spell.Animation;
using RebornFrame.Spell.Particles;
using RebornFrame.Spell.Spine;
using RebornFrame.Spell.Vfx;

namespace SpellEditor
{
    public sealed class ResourceAsset
    {
        public string Key = "";
        public string Name = "";
        public string Directory = "";
        public string Kind = "";
        public string KindLabel = "";
        public bool Owned;
        public string SourceTitle = "";
        public string SourcePath = "";
        public string Folder = "";
        public string FilePath = "";
        public long Length;
    }

    /// <summary>
    /// 项目目录里的资源，以及已载入数据包里的资源。
    /// 选一个导出目录后，每个骨架单独成为一条 Spine 资源。
    /// 骨架认 .skel、.skel.bytes 和带 skeleton 字段的 .json；图集认 .atlas 和 .atlas.txt。
    /// 贴图按内容的 MD5 放进 Spine/_textures，重复导入不会再复制一份。
    /// </summary>
    public static partial class ResourceLibrary
    {
        public const string SpineKind = "reborn.spine.skeleton";
        public const string SpineFolder = "Spine";
        const string TextureFolderName = "_textures";
        const string PagesFileName = "pages.txt";
        public const string AnimationKind = GameDataKinds.AnimationBundle;
        public const string AnimationFolder = "Animation";
        public const string ParticlesKind = GameDataKinds.ParticlesBundle;
        public const string ParticlesFolder = "Particles";
        public const string AudioKind = "reborn.audio.clip";
        public const string AudioFolder = "Audio";
        public const string PluginsFolder = "plugins";

        const ushort PackVersion = 2;
        const string SetupFileName = "setup.txt";
        static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        public static uint PluginMask()
        {
            if (ProjectSession.AuthorId != 0)
            {
                return ProjectSession.GameRenderPlugins();
            }

            return ProjectSession.Archive == null ? 0u : ProjectSession.Archive.RenderPlugins;
        }

        public static bool SpineImportEnabled()
        {
            return (PluginMask() & GameDataRenderPlugins.Spine) != 0;
        }

        public static bool AnimationImportEnabled()
        {
            return (PluginMask() & GameDataRenderPlugins.Animation) != 0;
        }

        public static bool ParticlesImportEnabled()
        {
            return (PluginMask() & GameDataRenderPlugins.Particles) != 0;
        }

        public static bool IsSpine(string kind)
        {
            return string.Equals(kind, SpineKind, StringComparison.Ordinal)
                   || (kind ?? "").IndexOf("spine", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsAnimation(string kind)
        {
            return string.Equals(kind, AnimationKind, StringComparison.Ordinal)
                   || (kind ?? "").IndexOf("animation", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsParticles(string kind)
        {
            return string.Equals(kind, ParticlesKind, StringComparison.Ordinal)
                   || (kind ?? "").IndexOf("particle", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsAudio(string kind)
        {
            return string.Equals(kind, AudioKind, StringComparison.Ordinal)
                   || (kind ?? "").IndexOf("audio", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsBundle(string kind)
        {
            return IsAnimation(kind) || IsParticles(kind);
        }

        public static string Label(string kind)
        {
            if (IsSpine(kind))
            {
                return "Spine";
            }

            if (IsAnimation(kind))
            {
                return "动画";
            }

            if (IsParticles(kind))
            {
                return "粒子";
            }

            if (IsAudio(kind))
            {
                return "音频";
            }

            return string.IsNullOrEmpty(kind) ? "未分类" : kind;
        }

        public static List<ResourceAsset> List()
        {
            var list = new List<ResourceAsset>();
            AddOwn(list);
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                AddForeign(list, ProjectSession.Sources[i]);
            }

            return list;
        }

        public static List<string> FileNames(ResourceAsset asset)
        {
            var names = new List<string>();
            if (asset == null)
            {
                return names;
            }

            if (asset.Owned && IsSpine(asset.Kind) && TryCollect(asset.Folder, out var parts, out _))
            {
                names.Add(Path.GetFileName(parts.SkeletonPath));
                names.Add(Path.GetFileName(parts.AtlasPath));
                for (var i = 0; i < parts.PageNames.Count; i++)
                {
                    names.Add(parts.PageNames[i]);
                }

                return names;
            }

            if (asset.Owned && Directory.Exists(asset.Folder))
            {
                var files = Directory.GetFiles(asset.Folder);
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < files.Length; i++)
                {
                    var name = Path.GetFileName(files[i]);
                    if (name.Length == 0 || name[0] == '.' || name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    names.Add(name);
                }

                return names;
            }

            if (TryReadSpine(asset, out var bundle))
            {
                names.Add(bundle.SkeletonFile);
                names.Add(bundle.AtlasFile);
                for (var i = 0; i < bundle.Pages.Count; i++)
                {
                    names.Add(bundle.Pages[i].FileName);
                }
            }

            return names;
        }

        public static byte[] FirstPng(ResourceAsset asset, out int pageCount)
        {
            pageCount = 0;
            if (asset == null)
            {
                return null;
            }

            if (asset.Owned && IsSpine(asset.Kind) && TryCollect(asset.Folder, out var parts, out _) && parts.Pngs.Count > 0)
            {
                pageCount = parts.Pngs.Count;
                return File.ReadAllBytes(parts.Pngs[0]);
            }

            if (!TryReadSpine(asset, out var bundle) || bundle.Pages.Count == 0)
            {
                return null;
            }

            pageCount = bundle.Pages.Count;
            return bundle.Pages[0].Bytes;
        }

        public sealed class SpineDocument
        {
            public string SkeletonName = "";
            public byte[] Skeleton = Array.Empty<byte>();
            public bool Binary;
            public string Atlas = "";
            public readonly List<SpinePageBytes> Pages = new List<SpinePageBytes>();
        }

        public sealed class SpinePageBytes
        {
            public string Name = "";
            public byte[] Bytes = Array.Empty<byte>();
        }

        /// <summary>
        /// 读出骨架、图集和贴图，供预览用 Spine 运行时加载。
        /// 本项目读目录里的文件，数据包读打包内容。
        /// </summary>
        public static bool TryOpenSpine(ResourceAsset asset, out SpineDocument document, out string error)
        {
            document = null;
            error = null;
            if (asset == null || !IsSpine(asset.Kind))
            {
                error = "这不是 Spine 资源。";
                return false;
            }

            if (asset.Owned)
            {
                if (!TryCollect(asset.Folder, out var parts, out error))
                {
                    return false;
                }

                try
                {
                    document = ReadOwnedSpine(parts);
                }
                catch (Exception)
                {
                    document = null;
                    error = "这份 Spine 没有读出来。";
                    return false;
                }

                if (document.Skeleton.Length == 0 || document.Atlas.Length == 0 || document.Pages.Count == 0)
                {
                    document = null;
                    error = "这份 Spine 没有读出来。";
                    return false;
                }

                return true;
            }

            if (!TryReadSpine(asset, out var bundle))
            {
                error = "这份 Spine 没有读出来。";
                return false;
            }

            document = FromBundle(bundle);
            if (document.Skeleton.Length == 0 || document.Atlas.Length == 0 || document.Pages.Count == 0)
            {
                document = null;
                error = "这份 Spine 没有读出来。";
                return false;
            }

            return true;
        }

        /// <summary>读出这份 Spine 的群组、触发器和外观槽位。没有记录时文本为空。</summary>
        public static bool TryReadSpineSetup(ResourceAsset asset, out string text, out string error)
        {
            text = "";
            error = null;
            if (asset == null || !IsSpine(asset.Kind))
            {
                return false;
            }

            if (asset.Owned)
            {
                if (string.IsNullOrEmpty(asset.Folder))
                {
                    return false;
                }

                var path = Path.Combine(asset.Folder, SetupFileName);
                if (!File.Exists(path))
                {
                    return true;
                }

                try
                {
                    text = File.ReadAllText(path, Utf8);
                    return true;
                }
                catch (Exception)
                {
                    error = "外观设置没有读出来。";
                    return false;
                }
            }

            if (!TryReadSpine(asset, out var bundle))
            {
                error = "这份 Spine 没有读出来。";
                return false;
            }

            text = bundle.Setup ?? "";
            return true;
        }

        /// <summary>把群组、触发器和外观槽位写回本项目的 Spine 目录。</summary>
        public static bool TryWriteSpineSetup(ResourceAsset asset, string text, out string error)
        {
            error = null;
            if (asset == null || !asset.Owned || string.IsNullOrEmpty(asset.Folder) || !IsSpine(asset.Kind))
            {
                error = "这份 Spine 不在本项目里，改动留在这次预览里。";
                return false;
            }

            try
            {
                Directory.CreateDirectory(asset.Folder);
                File.WriteAllText(Path.Combine(asset.Folder, SetupFileName), text ?? "", Utf8);
                return true;
            }
            catch (Exception)
            {
                error = "外观设置没有写进去。";
                return false;
            }
        }

        static SpineDocument ReadOwnedSpine(SpineParts parts)
        {
            var name = Path.GetFileName(parts.SkeletonPath);
            var document = new SpineDocument
            {
                SkeletonName = name,
                Skeleton = File.ReadAllBytes(parts.SkeletonPath),
                Binary = IsBinarySkeleton(name),
                Atlas = File.ReadAllText(parts.AtlasPath, Utf8),
            };
            for (var i = 0; i < parts.Pngs.Count; i++)
            {
                var pageName = i < parts.PageNames.Count ? parts.PageNames[i] : Path.GetFileName(parts.Pngs[i]);
                document.Pages.Add(new SpinePageBytes
                {
                    Name = pageName,
                    Bytes = File.ReadAllBytes(parts.Pngs[i]),
                });
            }

            return document;
        }

        static SpineDocument FromBundle(SpineBundle bundle)
        {
            var document = new SpineDocument
            {
                SkeletonName = bundle.SkeletonFile ?? "",
                Skeleton = bundle.Skeleton ?? Array.Empty<byte>(),
                Binary = IsBinarySkeleton(bundle.SkeletonFile),
                Atlas = bundle.Atlas ?? "",
            };
            for (var i = 0; i < bundle.Pages.Count; i++)
            {
                document.Pages.Add(new SpinePageBytes
                {
                    Name = bundle.Pages[i].FileName,
                    Bytes = bundle.Pages[i].Bytes,
                });
            }

            return document;
        }

        static bool IsBinarySkeleton(string name)
        {
            return !string.IsNullOrEmpty(name)
                   && (name.EndsWith(".skel", StringComparison.OrdinalIgnoreCase)
                       || name.EndsWith(".skel.bytes", StringComparison.OrdinalIgnoreCase));
        }

        public static List<string> References(string key)
        {
            return ResourceReferences.Labels(key);
        }

        public static void CollectSpellReferences(Action<string, string, string> add)
        {
            var module = ProjectSession.OwnModule;
            if (module == null || add == null)
            {
                return;
            }

            for (var i = 0; i < module.Spells.Count; i++)
            {
                var spell = module.Spells[i];
                if (spell == null)
                {
                    continue;
                }

                var label = "技能 " + new SpellId(spell.Id);
                for (var t = 0; t < spell.PresentationTracks.Count; t++)
                {
                    var key = TrackKey(spell.PresentationTracks[t]);
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    add(ResourceReferences.SpellTrack(spell.Id, t), key, label);
                }
            }
        }

        public sealed class SpineImportItem
        {
            public string Stem = "";
            public string SuggestedKey = "";
            public string Label = "";
            public string SkeletonPath = "";
            public string AtlasPath = "";
            public List<string> PageNames = new List<string>();
            public List<string> PagePaths = new List<string>();
        }

        public static bool TryPlanSpineImport(string folder, out List<SpineImportItem> items, out string warning, out string error)
        {
            items = new List<SpineImportItem>();
            warning = "";
            error = null;
            if (!ProjectSession.IsOpen)
            {
                error = "还没有打开项目。";
                return false;
            }

            if (!SpineImportEnabled())
            {
                error = "当前项目没有启用 Spine。";
                return false;
            }

            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                error = "找不到这个目录。";
                return false;
            }

            var spineRoot = SpineRoot();
            if (!string.IsNullOrEmpty(spineRoot) && IsUnder(folder, spineRoot))
            {
                error = "请选择 Spine 的导出目录。";
                return false;
            }

            if (!DiscoverSpines(folder, out var planned, out warning, out error))
            {
                return false;
            }

            for (var i = 0; i < planned.Count; i++)
            {
                var item = planned[i];
                items.Add(new SpineImportItem
                {
                    Stem = item.Stem,
                    SuggestedKey = item.Key,
                    Label = item.Label,
                    SkeletonPath = item.SkeletonPath,
                    AtlasPath = item.AtlasPath,
                    PageNames = item.PageNames,
                    PagePaths = item.PagePaths,
                });
            }

            return true;
        }

        public static List<string> SameNameKeys(string stem)
        {
            var keys = new List<string>();
            if (string.IsNullOrEmpty(stem))
            {
                return keys;
            }

            var assets = List();
            for (var i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (asset.Owned && IsSpine(asset.Kind) && string.Equals(asset.Name, stem, StringComparison.OrdinalIgnoreCase))
                {
                    keys.Add(asset.Key);
                }
            }

            keys.Sort(StringComparer.OrdinalIgnoreCase);
            return keys;
        }

        public static bool TrySpineKey(string stem, string directory, out string key, out string error)
        {
            key = "";
            if (!TryNormalizeDirectory(directory, out var parent, out error))
            {
                return false;
            }

            if (!ValidSegment(stem))
            {
                error = "骨骼名不能作为资源名。";
                return false;
            }

            key = parent.Length == 0 ? stem : parent + "/" + stem;
            return true;
        }

        public static ResourceAsset CommitSpine(SpineImportItem item, string key, out int reused, out string error)
        {
            reused = 0;
            error = null;
            if (item == null || string.IsNullOrEmpty(key))
            {
                error = "没有选择路径。";
                return null;
            }

            var existed = Directory.Exists(FolderOf(key));
            try
            {
                var destination = FolderOf(key);
                if (existed)
                {
                    ClearFolderFiles(destination);
                }

                Directory.CreateDirectory(destination);
                File.Copy(item.SkeletonPath, Path.Combine(destination, Path.GetFileName(item.SkeletonPath)), true);
                File.Copy(item.AtlasPath, Path.Combine(destination, Path.GetFileName(item.AtlasPath)), true);
                var planned = new SpinePlanned
                {
                    PageNames = item.PageNames,
                    PagePaths = item.PagePaths,
                };
                WritePages(destination, planned, ref reused);
                PruneTextures();
                return OwnAsset(key, destination);
            }
            catch (Exception)
            {
                error = "资源没有复制进项目目录。";
                return null;
            }
        }

        public static ResourceAsset ImportSpine(string folder, out string summary, out string error)
        {
            summary = "";
            error = null;
            if (!ProjectSession.IsOpen)
            {
                error = "还没有打开项目。";
                return null;
            }

            if (!SpineImportEnabled())
            {
                error = "当前项目没有启用 Spine。";
                return null;
            }

            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                error = "找不到这个目录。";
                return null;
            }

            var spineRoot = SpineRoot();
            if (!string.IsNullOrEmpty(spineRoot) && IsUnder(folder, spineRoot))
            {
                error = "请选择 Spine 的导出目录。";
                return null;
            }

            if (!DiscoverSpines(folder, out var planned, out var warning, out error))
            {
                return null;
            }

            for (var i = 0; i < planned.Count; i++)
            {
                if (SameNameKeys(planned[i].Stem).Count > 0)
                {
                    error = planned[i].Stem + " 和项目里已有的骨骼同名，请选择要覆盖的路径或填写新路径。";
                    return null;
                }
            }

            ResourceAsset first = null;
            var reused = 0;
            var notes = new List<string>();
            try
            {
                for (var i = 0; i < planned.Count; i++)
                {
                    var item = planned[i];
                    var destination = FolderOf(item.Key);
                    if (Directory.Exists(destination))
                    {
                        ClearFolderFiles(destination);
                    }

                    Directory.CreateDirectory(destination);
                    File.Copy(item.SkeletonPath, Path.Combine(destination, Path.GetFileName(item.SkeletonPath)), true);
                    File.Copy(item.AtlasPath, Path.Combine(destination, Path.GetFileName(item.AtlasPath)), true);
                    WritePages(destination, item, ref reused);
                    if (first == null)
                    {
                        first = OwnAsset(item.Key, destination);
                    }

                    notes.Add(item.Label);
                }
            }
            catch (Exception)
            {
                error = "资源没有复制进项目目录。";
                return null;
            }

            PruneTextures();
            summary = planned.Count == 1
                ? "已导入 " + notes[0] + "。"
                : "已导入 " + planned.Count + " 个骨骼：" + string.Join("；", notes) + "。";
            if (reused > 0)
            {
                summary += " 其中 " + reused + " 张贴图项目里已经有了。";
            }

            if (!string.IsNullOrEmpty(warning))
            {
                summary += " 未导入：" + warning + "。";
            }

            return first;
        }

        public static bool Move(ResourceAsset asset, string directory, out string newKey, out string error)
        {
            newKey = asset == null ? "" : asset.Key;
            error = null;
            if (asset == null || !asset.Owned)
            {
                error = "这份资源不能改目录。";
                return false;
            }

            if (!TryNormalizeDirectory(directory, out var parent, out error))
            {
                return false;
            }

            var fileBacked = !string.IsNullOrEmpty(asset.FilePath);
            var leaf = fileBacked ? Path.GetFileName(asset.FilePath) : asset.Name;
            var key = parent.Length == 0 ? leaf : parent + "/" + leaf;
            if (key == asset.Key)
            {
                return true;
            }

            var destination = fileBacked ? FileOf(asset.Kind, key) : FolderOf(key);
            if (fileBacked ? File.Exists(destination) : Directory.Exists(destination))
            {
                error = "这个目录下已经有同名资源。";
                return false;
            }

            var prefabs = fileBacked && IsBundle(asset.Kind) ? ReadPrefabs(asset, out _) : null;
            var oldLocation = fileBacked ? asset.FilePath : asset.Folder;
            try
            {
                var parentFolder = Path.GetDirectoryName(destination);
                if (!string.IsNullOrEmpty(parentFolder))
                {
                    Directory.CreateDirectory(parentFolder);
                }

                if (fileBacked)
                {
                    File.Move(asset.FilePath, destination);
                }
                else
                {
                    Directory.Move(asset.Folder, destination);
                }

                PruneEmptyParents(Path.GetDirectoryName(oldLocation), CategoryRoot(FolderName(asset.Kind)));
            }
            catch (Exception)
            {
                error = "目录没有改成。";
                return false;
            }

            Retarget(asset.Key, key, false);
            if (prefabs != null)
            {
                for (var i = 0; i < prefabs.Count; i++)
                {
                    var suffix = prefabs[i].Key.Length > asset.Key.Length && prefabs[i].Key.StartsWith(asset.Key, StringComparison.Ordinal)
                        ? prefabs[i].Key.Substring(asset.Key.Length)
                        : "";
                    if (suffix.Length > 0)
                    {
                        Retarget(prefabs[i].Key, key + suffix, false);
                    }
                }
            }

            ProjectSession.Save();
            ForgetBundle(asset);
            newKey = key;
            return true;
        }

        public static bool Delete(ResourceAsset asset, out string error)
        {
            error = null;
            if (asset == null || !asset.Owned)
            {
                error = "这份资源不能删除。";
                return false;
            }

            try
            {
                if (!string.IsNullOrEmpty(asset.FilePath))
                {
                    if (File.Exists(asset.FilePath))
                    {
                        File.Delete(asset.FilePath);
                    }

                    PruneEmptyParents(Path.GetDirectoryName(asset.FilePath), CategoryRoot(FolderName(asset.Kind)));
                }
                else if (Directory.Exists(asset.Folder))
                {
                    Directory.Delete(asset.Folder, true);
                    PruneEmptyParents(Path.GetDirectoryName(asset.Folder), SpineRoot());
                    if (IsSpine(asset.Kind))
                    {
                        PruneTextures();
                    }
                }

                ForgetBundle(asset);
            }
            catch (Exception)
            {
                error = "资源没有删掉。";
                return false;
            }

            return true;
        }

        public static void CollectOwnFolders(string kind, List<string> folders)
        {
            folders.Clear();
            var root = CategoryRoot(FolderName(kind));
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return;
            }

            WalkFolders(kind, root, root, folders);
        }

        public static bool CreateFolder(string kind, string parent, string name, out string directory, out string error)
        {
            directory = "";
            error = null;
            if (!TryNormalizeDirectory(parent, out var folder, out error))
            {
                return false;
            }

            name = (name ?? "").Trim();
            if (!ValidSegment(name) || ReservedFolder(name))
            {
                error = "目录名不能为空，也不能使用 .. 或贴图共用目录的名字。";
                return false;
            }

            directory = folder.Length == 0 ? name : folder + "/" + name;
            var path = FolderPath(kind, directory);
            if (string.IsNullOrEmpty(path))
            {
                error = "还没有打开项目。";
                directory = "";
                return false;
            }

            if (Directory.Exists(path) || File.Exists(path))
            {
                error = "这个位置已经有同名目录或资源。";
                directory = "";
                return false;
            }

            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception)
            {
                error = "目录没有建出来。";
                directory = "";
                return false;
            }

            return true;
        }

        public static bool RenameFolder(string kind, string directory, string newName, out string newDirectory, out string error)
        {
            newDirectory = "";
            error = null;
            if (!TryNormalizeDirectory(directory, out var folder, out error) || folder.Length == 0)
            {
                error = "这个目录不能改名。";
                return false;
            }

            newName = (newName ?? "").Trim();
            if (!ValidSegment(newName) || ReservedFolder(newName))
            {
                error = "目录名不能为空，也不能使用 .. 或贴图共用目录的名字。";
                return false;
            }

            var slash = folder.LastIndexOf('/');
            var parent = slash < 0 ? "" : folder.Substring(0, slash);
            if (string.Equals(slash < 0 ? folder : folder.Substring(slash + 1), newName, StringComparison.Ordinal))
            {
                newDirectory = folder;
                return true;
            }

            newDirectory = parent.Length == 0 ? newName : parent + "/" + newName;
            var from = FolderPath(kind, folder);
            var to = FolderPath(kind, newDirectory);
            if (string.IsNullOrEmpty(from) || !Directory.Exists(from))
            {
                error = "这个目录已经不在了。";
                newDirectory = "";
                return false;
            }

            if (Directory.Exists(to) || File.Exists(to))
            {
                error = "这个位置已经有同名目录或资源。";
                newDirectory = "";
                return false;
            }

            var affected = new List<ResourceAsset>();
            var own = new List<ResourceAsset>();
            AddOwn(own);
            var oldKeys = new List<string>();
            var newKeys = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < own.Count; i++)
            {
                if (!KindEquals(own[i].Kind, kind) || !UnderDirectory(own[i].Key, folder))
                {
                    continue;
                }

                affected.Add(own[i]);
                if (IsBundle(own[i].Kind))
                {
                    var prefabs = ReadPrefabs(own[i], out _);
                    for (var p = 0; p < prefabs.Count; p++)
                    {
                        QueueKey(prefabs[p].Key, folder, newDirectory, seen, oldKeys, newKeys);
                    }
                }

                QueueKey(own[i].Key, folder, newDirectory, seen, oldKeys, newKeys);
            }

            try
            {
                Directory.Move(from, to);
            }
            catch (Exception)
            {
                error = "目录没有改成。";
                newDirectory = "";
                return false;
            }

            for (var i = 0; i < oldKeys.Count; i++)
            {
                Retarget(oldKeys[i], newKeys[i], false);
            }

            for (var i = 0; i < affected.Count; i++)
            {
                ForgetBundle(affected[i]);
            }

            if (oldKeys.Count > 0)
            {
                ProjectSession.Save();
            }

            return true;
        }

        public static bool DeleteFolder(string kind, string directory, out int removed, out string error)
        {
            removed = 0;
            error = null;
            if (!TryNormalizeDirectory(directory, out var folder, out error) || folder.Length == 0)
            {
                error = "类别根目录不能删除。";
                return false;
            }

            var path = FolderPath(kind, folder);
            var own = new List<ResourceAsset>();
            AddOwn(own);
            var doomed = new List<ResourceAsset>();
            for (var i = 0; i < own.Count; i++)
            {
                if (KindEquals(own[i].Kind, kind) && UnderDirectory(own[i].Key, folder))
                {
                    doomed.Add(own[i]);
                }
            }

            if ((string.IsNullOrEmpty(path) || !Directory.Exists(path)) && doomed.Count == 0)
            {
                error = "这个目录已经不在了。";
                return false;
            }

            doomed.Sort((left, right) => right.Key.Length.CompareTo(left.Key.Length));
            for (var i = 0; i < doomed.Count; i++)
            {
                if (!Delete(doomed[i], out error))
                {
                    return false;
                }

                removed++;
            }

            try
            {
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    PruneEmptyParents(Path.GetDirectoryName(path), CategoryRoot(FolderName(kind)));
                }
            }
            catch (Exception)
            {
                error = "目录没有删掉。";
                return false;
            }

            return true;
        }

        static void WalkFolders(string kind, string root, string folder, List<string> folders)
        {
            if (string.Equals(Path.GetFileName(folder), TextureFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!SameDirectory(folder, root))
            {
                if (IsSpine(kind) && TryCollect(folder, out _, out _))
                {
                    return;
                }

                folders.Add(Path.GetRelativePath(root, folder).Replace('\\', '/'));
            }

            string[] children;
            try
            {
                children = Directory.GetDirectories(folder);
            }
            catch (Exception)
            {
                return;
            }

            Array.Sort(children, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < children.Length; i++)
            {
                WalkFolders(kind, root, children[i], folders);
            }
        }

        static void QueueKey(string key, string folder, string newDirectory, HashSet<string> seen, List<string> oldKeys, List<string> newKeys)
        {
            if (!UnderDirectory(key, folder) || !seen.Add(key))
            {
                return;
            }

            oldKeys.Add(key);
            newKeys.Add(newDirectory + key.Substring(folder.Length));
        }

        static bool UnderDirectory(string key, string directory)
        {
            return !string.IsNullOrEmpty(directory)
                   && (key ?? "").StartsWith(directory + "/", StringComparison.Ordinal);
        }

        static bool KindEquals(string assetKind, string kind)
        {
            if (IsSpine(kind))
            {
                return IsSpine(assetKind);
            }

            if (IsAnimation(kind))
            {
                return IsAnimation(assetKind);
            }

            if (IsParticles(kind))
            {
                return IsParticles(assetKind);
            }

            if (IsAudio(kind))
            {
                return IsAudio(assetKind);
            }

            return string.Equals(assetKind, kind, StringComparison.Ordinal);
        }

        static bool ReservedFolder(string name)
        {
            return string.Equals(name, TextureFolderName, StringComparison.OrdinalIgnoreCase);
        }

        static string FolderPath(string kind, string directory)
        {
            var root = CategoryRoot(FolderName(kind));
            if (string.IsNullOrEmpty(root))
            {
                return "";
            }

            if (string.IsNullOrEmpty(directory))
            {
                return root;
            }

            return Path.Combine(root, directory.Replace('/', Path.DirectorySeparatorChar));
        }

        public static bool TryExportEntries(List<GameDataEntry> entries, out string error)
        {
            error = null;
            var own = new List<ResourceAsset>();
            AddOwn(own);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < own.Count; i++)
            {
                if (own[i].Key.Length == 0 || own[i].Key == GameDataKinds.SpellModuleKey || !seen.Add(own[i].Key))
                {
                    error = own[i].Key + " 不能作为资源键。";
                    return false;
                }

                if (own[i].Kind == SpineKind)
                {
                    if (!TryPack(own[i].Folder, out var data, out error))
                    {
                        error = own[i].Key + "：" + error;
                        return false;
                    }

                    entries.Add(new GameDataEntry(own[i].Key, SpineKind, data));
                    continue;
                }

                if (string.IsNullOrEmpty(own[i].FilePath) || !File.Exists(own[i].FilePath))
                {
                    error = own[i].Key + "：找不到资源文件。";
                    return false;
                }

                entries.Add(new GameDataEntry(own[i].Key, own[i].Kind, File.ReadAllBytes(own[i].FilePath)));
            }

            return true;
        }

        public static bool MoveStorage(string oldDirectory, string newDirectory, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(oldDirectory) || string.IsNullOrEmpty(newDirectory))
            {
                return true;
            }

            var folders = new[] { SpineFolder, AnimationFolder, ParticlesFolder, AudioFolder, PluginsFolder };
            var referencesFrom = Path.Combine(oldDirectory, ResourceReferences.FileName);
            var referencesTo = Path.Combine(newDirectory, ResourceReferences.FileName);
            if (File.Exists(referencesFrom) && File.Exists(referencesTo))
            {
                error = "新目录里已经有 " + ResourceReferences.FileName + "，原来的引用记录还留在旧目录。";
                return false;
            }

            for (var i = 0; i < folders.Length; i++)
            {
                var from = Path.Combine(oldDirectory, folders[i]);
                if (!Directory.Exists(from))
                {
                    continue;
                }

                var to = Path.Combine(newDirectory, folders[i]);
                if (Directory.Exists(to))
                {
                    error = "新目录里已经有 " + folders[i] + " 文件夹，原来的资源还留在旧目录。";
                    return false;
                }
            }

            try
            {
                for (var i = 0; i < folders.Length; i++)
                {
                    var from = Path.Combine(oldDirectory, folders[i]);
                    if (!Directory.Exists(from))
                    {
                        continue;
                    }

                    Directory.Move(from, Path.Combine(newDirectory, folders[i]));
                }
            }
            catch (Exception)
            {
                error = "新存档已经写好，资源目录还留在原来的位置。";
                return false;
            }

            return ResourceReferences.MoveFile(oldDirectory, newDirectory, out error);
        }

        static void AddOwn(List<ResourceAsset> list)
        {
            var root = SpineRoot();
            if (!string.IsNullOrEmpty(root) && Directory.Exists(root))
            {
                WalkOwn(root, root, list);
            }

            AddFileTree(list, AnimationFolder, AnimationKind);
            AddFileTree(list, ParticlesFolder, ParticlesKind);
            AddFileTree(list, AudioFolder, AudioKind);
        }

        static void WalkOwn(string root, string folder, List<ResourceAsset> list)
        {
            if (string.Equals(Path.GetFileName(folder), TextureFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (TryCollect(folder, out _, out _) && !SameDirectory(folder, root))
            {
                var relative = Path.GetRelativePath(root, folder).Replace('\\', '/');
                list.Add(OwnAsset(relative, folder));
                return;
            }

            string[] children;
            try
            {
                children = Directory.GetDirectories(folder);
            }
            catch (Exception)
            {
                return;
            }

            Array.Sort(children, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < children.Length; i++)
            {
                WalkOwn(root, children[i], list);
            }
        }

        static void AddForeign(List<ResourceAsset> list, LoadedSource source)
        {
            if (source == null || string.IsNullOrEmpty(source.AbsolutePath))
            {
                return;
            }

            if (!GameDataFile.TryReadInfo(source.AbsolutePath, out var info, out _))
            {
                return;
            }

            for (var i = 0; i < info.Entries.Count; i++)
            {
                var entry = info.Entries[i];
                if (entry.Kind == GameDataKinds.SpellModule || string.IsNullOrEmpty(entry.Key))
                {
                    continue;
                }

                SplitKey(entry.Key, out var directory, out var leaf);
                list.Add(new ResourceAsset
                {
                    Key = entry.Key,
                    Name = DisplayName(leaf, entry.Kind),
                    Directory = directory,
                    Kind = entry.Kind,
                    KindLabel = Label(entry.Kind),
                    Owned = false,
                    SourceTitle = string.IsNullOrEmpty(source.Title) ? "数据源" : source.Title,
                    SourcePath = source.AbsolutePath,
                    Length = entry.Length,
                });
            }
        }

        static ResourceAsset OwnAsset(string key, string folder)
        {
            SplitKey(key, out var directory, out var name);
            return new ResourceAsset
            {
                Key = key,
                Name = name,
                Directory = directory,
                Kind = SpineKind,
                KindLabel = "Spine",
                Owned = true,
                SourceTitle = "本项目",
                Folder = folder,
            };
        }

        static void Retarget(string oldKey, string newKey)
        {
            Retarget(oldKey, newKey, true);
        }

        static void Retarget(string oldKey, string newKey, bool save)
        {
            var units = ProjectSession.Archive == null ? null : ProjectSession.Archive.Units;
            if (units != null)
            {
                for (var i = 0; i < units.Count; i++)
                {
                    if (units[i].SpineKey == oldKey)
                    {
                        units[i].SpineKey = newKey;
                    }
                }
            }

            RetargetModule(ProjectSession.OwnModule, oldKey, newKey);
            UnitDiffs.Retarget(oldKey, newKey);
            ResourceReferences.Retarget(oldKey, newKey);
            if (save)
            {
                ProjectSession.Save();
            }
        }

        static void RetargetModule(SpellModuleData module, string oldKey, string newKey)
        {
            if (module == null)
            {
                return;
            }

            for (var i = 0; i < module.Spells.Count; i++)
            {
                var spell = module.Spells[i];
                if (spell == null)
                {
                    continue;
                }

                for (var t = 0; t < spell.PresentationTracks.Count; t++)
                {
                    RewriteTrack(spell.PresentationTracks[t], oldKey, newKey);
                }
            }
        }

        static string TrackKey(SpellPresentationTrack track)
        {
            if (track == null || track.Parameters == null || track.Parameters.Length == 0)
            {
                return "";
            }

            try
            {
                if (track.TypeId == SpineSpellTracks.TypeId.Value)
                {
                    return SpineSpellTrackConfig.Parser.ParseFrom(track.Parameters).DataKey ?? "";
                }

                if (track.TypeId == ParticleSpellTracks.TypeId.Value)
                {
                    return ParticleSpellTrackConfig.Parser.ParseFrom(track.Parameters).PrefabKey ?? "";
                }

                if (track.TypeId == VfxSpellTracks.TypeId.Value)
                {
                    return VfxSpellTrackConfig.Parser.ParseFrom(track.Parameters).PrefabKey ?? "";
                }

                if (track.TypeId == AnimationSpellTracks.TypeId.Value)
                {
                    return AnimationSpellTrackConfig.Parser.ParseFrom(track.Parameters).ClipKey ?? "";
                }
            }
            catch (Exception)
            {
            }

            return "";
        }

        static void RewriteTrack(SpellPresentationTrack track, string oldKey, string newKey)
        {
            if (track == null || TrackKey(track) != oldKey)
            {
                return;
            }

            try
            {
                if (track.TypeId == SpineSpellTracks.TypeId.Value)
                {
                    var config = SpineSpellTrackConfig.Parser.ParseFrom(track.Parameters);
                    config.DataKey = newKey;
                    track.Parameters = config.ToByteString();
                }
                else if (track.TypeId == ParticleSpellTracks.TypeId.Value)
                {
                    var config = ParticleSpellTrackConfig.Parser.ParseFrom(track.Parameters);
                    config.PrefabKey = newKey;
                    track.Parameters = config.ToByteString();
                }
                else if (track.TypeId == VfxSpellTracks.TypeId.Value)
                {
                    var config = VfxSpellTrackConfig.Parser.ParseFrom(track.Parameters);
                    config.PrefabKey = newKey;
                    track.Parameters = config.ToByteString();
                }
                else if (track.TypeId == AnimationSpellTracks.TypeId.Value)
                {
                    var config = AnimationSpellTrackConfig.Parser.ParseFrom(track.Parameters);
                    config.ClipKey = newKey;
                    track.Parameters = config.ToByteString();
                }
            }
            catch (Exception)
            {
            }
        }

        static bool TryReadSpine(ResourceAsset asset, out SpineBundle bundle)
        {
            bundle = null;
            if (asset == null || asset.Owned || string.IsNullOrEmpty(asset.SourcePath) || !IsSpine(asset.Kind))
            {
                return false;
            }

            if (!GameDataFile.TryReadEntry(asset.SourcePath, asset.Key, out var data, out _))
            {
                return false;
            }

            return TryUnpack(data, out bundle);
        }

        static bool TryPack(string folder, out byte[] data, out string error)
        {
            data = null;
            if (!TryCollect(folder, out var parts, out error))
            {
                return false;
            }

            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream, Utf8, true);
            writer.Write((byte)'S');
            writer.Write((byte)'P');
            writer.Write((byte)'N');
            writer.Write((byte)'E');
            writer.Write(PackVersion);
            WriteBlock(writer, Path.GetFileName(parts.SkeletonPath), File.ReadAllBytes(parts.SkeletonPath));
            WriteString(writer, Path.GetFileName(parts.AtlasPath));
            WriteString(writer, File.ReadAllText(parts.AtlasPath));
            writer.Write(parts.Pngs.Count);
            for (var i = 0; i < parts.Pngs.Count; i++)
            {
                var pageName = i < parts.PageNames.Count ? parts.PageNames[i] : Path.GetFileName(parts.Pngs[i]);
                WriteBlock(writer, pageName, File.ReadAllBytes(parts.Pngs[i]));
            }

            var setupPath = Path.Combine(folder, SetupFileName);
            var setup = "";
            if (File.Exists(setupPath))
            {
                setup = File.ReadAllText(setupPath, Utf8);
            }

            WriteString(writer, setup);

            writer.Flush();
            data = stream.ToArray();
            return true;
        }

        static bool TryUnpack(byte[] data, out SpineBundle bundle)
        {
            bundle = null;
            try
            {
                using (var stream = new MemoryStream(data, false))
                using (var reader = new BinaryReader(stream, Utf8, true))
                {
                    if (reader.ReadByte() != (byte)'S'
                        || reader.ReadByte() != (byte)'P'
                        || reader.ReadByte() != (byte)'N'
                        || reader.ReadByte() != (byte)'E')
                    {
                        return false;
                    }

                    var version = reader.ReadUInt16();
                    if (version < 1 || version > PackVersion)
                    {
                        return false;
                    }

                    var packed = new SpineBundle();
                    packed.SkeletonFile = ReadString(reader);
                    packed.Skeleton = ReadBytes(reader);
                    packed.AtlasFile = ReadString(reader);
                    packed.Atlas = ReadString(reader);
                    var count = reader.ReadInt32();
                    if (count < 0 || count > 256)
                    {
                        return false;
                    }

                    for (var i = 0; i < count; i++)
                    {
                        var page = new SpinePage
                        {
                            FileName = ReadString(reader),
                            Bytes = ReadBytes(reader),
                        };
                        packed.Pages.Add(page);
                    }

                    if (version >= 2)
                    {
                        packed.Setup = ReadString(reader);
                    }

                    bundle = packed;
                    return true;
                }
            }
            catch (Exception)
            {
                bundle = null;
                return false;
            }
        }

        static bool DiscoverSpines(string root, out List<SpinePlanned> planned, out string warning, out string error)
        {
            planned = new List<SpinePlanned>();
            warning = "";
            error = null;
            var files = new List<string>();
            CollectFiles(root, files);
            var images = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var groups = new Dictionary<string, SpineGroup>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < files.Count; i++)
            {
                var name = Path.GetFileName(files[i]);
                if (name.Length == 0 || name[0] == '.' || name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (IsImage(name))
                {
                    if (!images.TryGetValue(name, out var list))
                    {
                        list = new List<string>();
                        images[name] = list;
                    }

                    list.Add(files[i]);
                    continue;
                }

                var stem = SpineStem(files[i]);
                if (stem.Length == 0 || !groups.TryGetValue(GroupKey(files[i], stem), out var group))
                {
                    if (stem.Length == 0)
                    {
                        continue;
                    }

                    group = new SpineGroup { Stem = stem, Directory = Path.GetDirectoryName(files[i]) };
                    groups[GroupKey(files[i], stem)] = group;
                }

                if (name.EndsWith(".skel.bytes", StringComparison.OrdinalIgnoreCase))
                {
                    group.Bytes = files[i];
                }
                else if (name.EndsWith(".skel", StringComparison.OrdinalIgnoreCase))
                {
                    group.Binary = files[i];
                }
                else if (name.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && LooksLikeSpineJson(files[i]))
                {
                    group.Json = files[i];
                }
                else if (name.EndsWith(".atlas.txt", StringComparison.OrdinalIgnoreCase))
                {
                    group.AtlasTxt = files[i];
                }
                else if (name.EndsWith(".atlas", StringComparison.OrdinalIgnoreCase))
                {
                    group.Atlas = files[i];
                }
            }

            var problems = new List<string>();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in groups)
            {
                var group = pair.Value;
                var skeleton = group.Binary ?? group.Bytes ?? group.Json;
                if (skeleton == null)
                {
                    continue;
                }

                var atlas = group.AtlasTxt ?? group.Atlas;
                if (atlas == null)
                {
                    problems.Add(group.Stem + " 缺少图集");
                    continue;
                }

                var pageNames = AtlasPages(atlas);
                if (pageNames.Count == 0)
                {
                    problems.Add(group.Stem + " 的图集里没有贴图");
                    continue;
                }

                var pagePaths = new List<string>();
                var missing = false;
                for (var i = 0; i < pageNames.Count; i++)
                {
                    if (!FindImage(group.Directory, pageNames[i], images, out var image))
                    {
                        problems.Add(group.Stem + " 缺少贴图 " + pageNames[i]);
                        missing = true;
                        break;
                    }

                    pagePaths.Add(image);
                }

                if (missing)
                {
                    continue;
                }

                var key = ImportKey(root, group.Directory, group.Stem);
                if (key.Length == 0)
                {
                    problems.Add(group.Stem + " 的文件名不能作为资源名");
                    continue;
                }

                var number = 2;
                var unique = key;
                while (!used.Add(unique))
                {
                    unique = key + "-" + number;
                    number++;
                }

                var variant = group.Binary != null || group.Bytes != null ? "二进制" : "JSON";
                var atlasKind = group.AtlasTxt != null ? "atlas.txt" : "atlas";
                var label = group.Stem + "（" + variant + "、" + atlasKind + "，贴图 " + pageNames.Count + "）";
                if ((group.Binary != null || group.Bytes != null) && group.Json != null)
                {
                    label += "，另有 JSON 未使用";
                }

                planned.Add(new SpinePlanned
                {
                    Stem = group.Stem,
                    Key = unique,
                    SkeletonPath = skeleton,
                    AtlasPath = atlas,
                    Label = label,
                    PageNames = pageNames,
                    PagePaths = pagePaths,
                });
            }

            if (planned.Count == 0)
            {
                error = problems.Count == 0
                    ? "这个目录里没有认出来的 Spine 导出。需要骨架、图集和贴图。"
                    : string.Join("；", problems) + "。";
                return false;
            }

            planned.Sort((left, right) => string.Compare(left.Key, right.Key, StringComparison.OrdinalIgnoreCase));
            if (problems.Count > 0)
            {
                warning = string.Join("；", problems);
            }

            return true;
        }

        static bool TryCollect(string folder, out SpineParts parts, out string error)
        {
            parts = null;
            error = null;
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                error = "找不到这个目录。";
                return false;
            }

            string[] files;
            try
            {
                files = Directory.GetFiles(folder);
            }
            catch (Exception)
            {
                error = "这个目录没有读出来。";
                return false;
            }

            string binary = null;
            string bytes = null;
            string json = null;
            string atlasTxt = null;
            string atlas = null;
            var localImages = new List<string>();
            for (var i = 0; i < files.Length; i++)
            {
                var name = Path.GetFileName(files[i]);
                if (name.Length == 0 || name[0] == '.')
                {
                    continue;
                }

                if (name.EndsWith(".skel.bytes", StringComparison.OrdinalIgnoreCase))
                {
                    bytes = files[i];
                }
                else if (name.EndsWith(".skel", StringComparison.OrdinalIgnoreCase))
                {
                    binary = files[i];
                }
                else if (name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    json = files[i];
                }
                else if (name.EndsWith(".atlas.txt", StringComparison.OrdinalIgnoreCase))
                {
                    atlasTxt = files[i];
                }
                else if (name.EndsWith(".atlas", StringComparison.OrdinalIgnoreCase))
                {
                    atlas = files[i];
                }
                else if (IsImage(name))
                {
                    localImages.Add(files[i]);
                }
            }

            var skeleton = binary ?? bytes ?? json;
            var atlasPath = atlasTxt ?? atlas;
            if (skeleton == null || atlasPath == null)
            {
                error = "Spine 资源里需要骨架和图集。";
                return false;
            }

            var pageNames = new List<string>();
            var pagePaths = new List<string>();
            var pagesFile = Path.Combine(folder, PagesFileName);
            if (File.Exists(pagesFile))
            {
                if (!ReadPages(pagesFile, pageNames, pagePaths, out error))
                {
                    return false;
                }
            }
            else
            {
                localImages.Sort(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < localImages.Count; i++)
                {
                    pageNames.Add(Path.GetFileName(localImages[i]));
                    pagePaths.Add(localImages[i]);
                }
            }

            if (pagePaths.Count == 0)
            {
                error = "Spine 资源里没有贴图。";
                return false;
            }

            parts = new SpineParts
            {
                SkeletonPath = skeleton,
                AtlasPath = atlasPath,
                Pngs = pagePaths,
                PageNames = pageNames,
            };
            return true;
        }

        static void WritePages(string folder, SpinePlanned item, ref int reused)
        {
            var text = new StringBuilder();
            for (var i = 0; i < item.PageNames.Count; i++)
            {
                var stored = PlaceTexture(item.PagePaths[i], out var already);
                if (already)
                {
                    reused++;
                }

                text.Append(item.PageNames[i]);
                text.Append('\t');
                text.Append(stored);
                text.Append('\n');
            }

            File.WriteAllText(Path.Combine(folder, PagesFileName), text.ToString(), Utf8);
        }

        static string PlaceTexture(string source, out bool reused)
        {
            var hash = Md5Hex(source);
            var ext = Path.GetExtension(source);
            if (ext.Length == 0)
            {
                ext = ".png";
            }

            var stored = hash + ext.ToLowerInvariant();
            var destination = Path.Combine(TextureRoot(), stored);
            Directory.CreateDirectory(TextureRoot());
            reused = File.Exists(destination);
            if (!reused)
            {
                File.Copy(source, destination, false);
            }

            return stored;
        }

        static bool ReadPages(string file, List<string> names, List<string> paths, out string error)
        {
            error = null;
            string[] lines;
            try
            {
                lines = File.ReadAllLines(file);
            }
            catch (Exception)
            {
                error = "贴图记录没有读出来。";
                return false;
            }

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length == 0)
                {
                    continue;
                }

                var parts = lines[i].Split('\t');
                if (parts.Length < 2 || parts[0].Length == 0 || parts[1].Length == 0)
                {
                    continue;
                }

                var path = Path.Combine(TextureRoot(), parts[1]);
                if (!File.Exists(path))
                {
                    error = "缺少贴图 " + parts[0] + "。";
                    return false;
                }

                names.Add(parts[0]);
                paths.Add(path);
            }

            if (names.Count > 0)
            {
                return true;
            }

            error = "贴图记录是空的。";
            return false;
        }

        static void PruneTextures()
        {
            var root = TextureRoot();
            if (!Directory.Exists(root))
            {
                return;
            }

            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CollectUsedTextures(SpineRoot(), used);
            string[] files;
            try
            {
                files = Directory.GetFiles(root);
            }
            catch (Exception)
            {
                return;
            }

            for (var i = 0; i < files.Length; i++)
            {
                if (!used.Contains(Path.GetFileName(files[i])))
                {
                    try
                    {
                        File.Delete(files[i]);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        static void CollectUsedTextures(string folder, HashSet<string> used)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                return;
            }

            var pages = Path.Combine(folder, PagesFileName);
            if (File.Exists(pages))
            {
                string[] lines;
                try
                {
                    lines = File.ReadAllLines(pages);
                }
                catch (Exception)
                {
                    lines = Array.Empty<string>();
                }

                for (var i = 0; i < lines.Length; i++)
                {
                    var parts = lines[i].Split('\t');
                    if (parts.Length >= 2 && parts[1].Length > 0)
                    {
                        used.Add(parts[1]);
                    }
                }
            }

            string[] children;
            try
            {
                children = Directory.GetDirectories(folder);
            }
            catch (Exception)
            {
                return;
            }

            for (var i = 0; i < children.Length; i++)
            {
                if (string.Equals(Path.GetFileName(children[i]), TextureFolderName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                CollectUsedTextures(children[i], used);
            }
        }

        static void CollectFiles(string folder, List<string> files)
        {
            if (string.Equals(Path.GetFileName(folder), TextureFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                files.AddRange(Directory.GetFiles(folder));
                var children = Directory.GetDirectories(folder);
                for (var i = 0; i < children.Length; i++)
                {
                    CollectFiles(children[i], files);
                }
            }
            catch (Exception)
            {
            }
        }

        static List<string> AtlasPages(string atlasPath)
        {
            var pages = new List<string>();
            string[] lines;
            try
            {
                lines = File.ReadAllLines(atlasPath);
            }
            catch (Exception)
            {
                return pages;
            }

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Length == 0 || line[0] == ' ' || line[0] == '\t')
                {
                    continue;
                }

                var name = line.Trim();
                if (IsImage(name))
                {
                    pages.Add(name);
                }
            }

            return pages;
        }

        static bool FindImage(string directory, string pageName, Dictionary<string, List<string>> images, out string path)
        {
            path = "";
            var beside = Path.Combine(directory ?? "", pageName);
            if (File.Exists(beside))
            {
                path = beside;
                return true;
            }

            if (images != null && images.TryGetValue(pageName, out var hits) && hits.Count > 0)
            {
                path = hits[0];
                return true;
            }

            return false;
        }

        static bool LooksLikeSpineJson(string path)
        {
            try
            {
                using (var stream = File.OpenRead(path))
                {
                    var count = (int)Math.Min(4096, stream.Length);
                    var buffer = new byte[count];
                    var read = stream.Read(buffer, 0, count);
                    var text = Encoding.UTF8.GetString(buffer, 0, read);
                    return text.IndexOf("\"skeleton\"", StringComparison.Ordinal) >= 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        static string ImportKey(string root, string directory, string stem)
        {
            if (!ValidSegment(stem))
            {
                return "";
            }

            var relative = "";
            if (!string.IsNullOrEmpty(directory) && !SameDirectory(directory, root))
            {
                relative = Path.GetRelativePath(root, directory).Replace('\\', '/');
            }

            if (relative.Length == 0 || relative == ".")
            {
                return stem;
            }

            var parts = relative.Split('/');
            for (var i = 0; i < parts.Length; i++)
            {
                if (!ValidSegment(parts[i]))
                {
                    return "";
                }
            }

            return relative + "/" + stem;
        }

        static string GroupKey(string path, string stem)
        {
            return Path.GetFullPath(Path.GetDirectoryName(path) ?? "") + "\n" + stem;
        }

        static string SpineStem(string path)
        {
            var name = Path.GetFileName(path);
            const string bytes = ".skel.bytes";
            const string atlasTxt = ".atlas.txt";
            if (name.EndsWith(bytes, StringComparison.OrdinalIgnoreCase))
            {
                return name.Substring(0, name.Length - bytes.Length);
            }

            if (name.EndsWith(atlasTxt, StringComparison.OrdinalIgnoreCase))
            {
                return name.Substring(0, name.Length - atlasTxt.Length);
            }

            if (name.EndsWith(".atlas", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith(".skel", StringComparison.OrdinalIgnoreCase)
                || name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFileNameWithoutExtension(name);
            }

            return "";
        }

        static string Md5Hex(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(stream);
                var text = new StringBuilder(hash.Length * 2);
                for (var i = 0; i < hash.Length; i++)
                {
                    text.Append(hash[i].ToString("x2"));
                }

                return text.ToString();
            }
        }

        static void ClearFolderFiles(string folder)
        {
            var files = Directory.GetFiles(folder);
            for (var i = 0; i < files.Length; i++)
            {
                if (string.Equals(Path.GetFileName(files[i]), SetupFileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                File.Delete(files[i]);
            }
        }

        static bool IsUnder(string path, string root)
        {
            var full = Path.GetFullPath(path);
            var rootFull = Path.GetFullPath(root);
            if (SameDirectory(full, rootFull))
            {
                return true;
            }

            if (!rootFull.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                rootFull += Path.DirectorySeparatorChar;
            }

            return full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase);
        }

        static string TextureRoot()
        {
            return Path.Combine(SpineRoot(), TextureFolderName);
        }

        static bool IsImage(string name)
        {
            return name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase);
        }

        static bool TryNormalizeDirectory(string text, out string directory, out string error)
        {
            directory = "";
            error = null;
            text = (text ?? "").Trim().Replace('\\', '/');
            while (text.StartsWith("/", StringComparison.Ordinal))
            {
                text = text.Substring(1);
            }

            while (text.EndsWith("/", StringComparison.Ordinal))
            {
                text = text.Substring(0, text.Length - 1);
            }

            if (text.Length == 0)
            {
                return true;
            }

            var parts = text.Split('/');
            for (var i = 0; i < parts.Length; i++)
            {
                if (!ValidSegment(parts[i]))
                {
                    error = "目录里不要使用空段或 .. 。";
                    directory = "";
                    return false;
                }
            }

            directory = string.Join("/", parts);
            return true;
        }

        static bool ValidSegment(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "." || name == "..")
            {
                return false;
            }

            var invalid = Path.GetInvalidFileNameChars();
            for (var i = 0; i < name.Length; i++)
            {
                if (name[i] == '/' || name[i] == '\\')
                {
                    return false;
                }

                for (var j = 0; j < invalid.Length; j++)
                {
                    if (name[i] == invalid[j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        static void SplitKey(string key, out string directory, out string name)
        {
            key = (key ?? "").Replace('\\', '/');
            var slash = key.LastIndexOf('/');
            if (slash < 0)
            {
                directory = "";
                name = key;
                return;
            }

            directory = key.Substring(0, slash);
            name = key.Substring(slash + 1);
        }

        static string SpineRoot()
        {
            if (!ProjectSession.IsOpen)
            {
                return "";
            }

            var project = Path.GetDirectoryName(ProjectSession.ArchivePath);
            if (string.IsNullOrEmpty(project))
            {
                return "";
            }

            return Path.Combine(project, SpineFolder);
        }

        static string FolderOf(string key)
        {
            var folder = SpineRoot();
            var parts = (key ?? "").Split('/');
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    folder = Path.Combine(folder, parts[i]);
                }
            }

            return folder;
        }

        static void CopyTo(string file, string folder)
        {
            File.Copy(file, Path.Combine(folder, Path.GetFileName(file)), false);
        }

        static void PruneEmptyParents(string folder, string root)
        {
            if (string.IsNullOrEmpty(root) || string.IsNullOrEmpty(folder))
            {
                return;
            }

            var rootFull = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
            while (!string.IsNullOrEmpty(folder) && !SameDirectory(folder, root))
            {
                if (!Path.GetFullPath(folder).StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (Directory.Exists(folder) && Directory.GetFileSystemEntries(folder).Length == 0)
                {
                    Directory.Delete(folder);
                    folder = Path.GetDirectoryName(folder);
                    continue;
                }

                break;
            }
        }

        static bool SameDirectory(string left, string right)
        {
            return string.Equals(
                Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }

        static void WriteBlock(BinaryWriter writer, string name, byte[] bytes)
        {
            WriteString(writer, name);
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        static void WriteString(BinaryWriter writer, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? "");
            writer.Write(bytes.Length);
            writer.Write(bytes);
        }

        static string ReadString(BinaryReader reader)
        {
            return Encoding.UTF8.GetString(ReadBytes(reader));
        }

        static byte[] ReadBytes(BinaryReader reader)
        {
            var length = reader.ReadInt32();
            if (length < 0 || length > 64 * 1024 * 1024)
            {
                throw new InvalidDataException();
            }

            var bytes = reader.ReadBytes(length);
            if (bytes.Length != length)
            {
                throw new EndOfStreamException();
            }

            return bytes;
        }

        sealed class SpinePlanned
        {
            public string Stem;
            public string Key;
            public string SkeletonPath;
            public string AtlasPath;
            public string Label;
            public List<string> PageNames;
            public List<string> PagePaths;
        }

        sealed class SpineGroup
        {
            public string Stem;
            public string Directory;
            public string Binary;
            public string Bytes;
            public string Json;
            public string AtlasTxt;
            public string Atlas;
        }

        sealed class SpineParts
        {
            public string SkeletonPath;
            public string AtlasPath;
            public List<string> Pngs;
            public List<string> PageNames;
        }

        sealed class SpineBundle
        {
            public string SkeletonFile = "";
            public byte[] Skeleton = Array.Empty<byte>();
            public string AtlasFile = "";
            public string Atlas = "";
            public string Setup = "";
            public readonly List<SpinePage> Pages = new List<SpinePage>();
        }

        sealed class SpinePage
        {
            public string FileName = "";
            public byte[] Bytes = Array.Empty<byte>();
        }
    }
}
