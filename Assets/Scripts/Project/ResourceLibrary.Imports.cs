using System;
using System.Collections.Generic;
using System.IO;
using RebornFrame.GameData;
using UnityEngine;

namespace SpellEditor
{
    public sealed class ResourcePrefab
    {
        public string Name = "";
        public string Key = "";
        public string Note = "";
    }

    public static partial class ResourceLibrary
    {
        sealed class PrefabCache
        {
            public long Stamp;
            public bool Opened;
            public List<ResourcePrefab> Prefabs;
        }

        static readonly Dictionary<string, PrefabCache> PrefabCaches = new Dictionary<string, PrefabCache>();

        public static int ReferenceCount(ResourceAsset asset)
        {
            if (asset == null || !asset.Owned)
            {
                return 0;
            }

            if (!IsBundle(asset.Kind))
            {
                return References(asset.Key).Count;
            }

            var prefabs = ReadPrefabs(asset, out var opened);
            if (!opened || prefabs.Count == 0)
            {
                return References(asset.Key).Count;
            }

            var count = 0;
            for (var i = 0; i < prefabs.Count; i++)
            {
                count += References(prefabs[i].Key).Count;
            }

            return count;
        }

        public static string StorageName(ResourceAsset asset)
        {
            return asset == null ? "" : FolderName(asset.Kind);
        }

        public static ResourceAsset ImportAnimation(string file, out string error)
        {
            if (!AnimationImportEnabled())
            {
                error = "当前项目没有启用动画。";
                return null;
            }

            return ImportBundle(file, AnimationFolder, AnimationKind, out error);
        }

        public static ResourceAsset ImportParticles(string file, out string error)
        {
            if (!ParticlesImportEnabled())
            {
                error = "当前项目没有启用粒子。";
                return null;
            }

            return ImportBundle(file, ParticlesFolder, ParticlesKind, out error);
        }

        public static ResourceAsset ImportAudio(string file, out string error)
        {
            error = null;
            if (!IsAudioFile(file))
            {
                error = "音频使用 wav、mp3 或 ogg。";
                return null;
            }

            return ImportFile(file, AudioFolder, AudioKind, out error);
        }

        public static int EntryCount(ResourceAsset asset)
        {
            return ReadPrefabs(asset, out _).Count;
        }

        public static bool Matches(ResourceAsset asset, string query)
        {
            if (asset == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(query))
            {
                return true;
            }

            if (Has(asset.Name, query) || Has(asset.Key, query) || Has(asset.KindLabel, query)
                || Has(asset.SourceTitle, query) || Has(asset.Directory, query))
            {
                return true;
            }

            if (!IsBundle(asset.Kind))
            {
                return false;
            }

            var prefabs = ReadPrefabs(asset, out _);
            for (var i = 0; i < prefabs.Count; i++)
            {
                if (Has(prefabs[i].Name, query) || Has(prefabs[i].Key, query))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<string> DetailLines(ResourceAsset asset)
        {
            var lines = new List<string>();
            if (asset == null)
            {
                return lines;
            }

            if (IsBundle(asset.Kind))
            {
                var prefabs = ReadPrefabs(asset, out var opened);
                if (!opened)
                {
                    lines.Add("这个 AssetBundle 没有读出来。");
                    return lines;
                }

                if (prefabs.Count == 0)
                {
                    lines.Add("这个 AssetBundle 里没有预制体。");
                    return lines;
                }

                for (var i = 0; i < prefabs.Count; i++)
                {
                    var note = prefabs[i].Note;
                    lines.Add(note.Length == 0
                        ? prefabs[i].Name + "  ·  " + prefabs[i].Key
                        : prefabs[i].Name + "  ·  " + prefabs[i].Key + "  ·  " + note);
                }

                return lines;
            }

            if (IsAudio(asset.Kind))
            {
                lines.Add(Path.GetFileName(asset.Key));
                return lines;
            }

            return FileNames(asset);
        }

        public static List<string> DetailReferences(ResourceAsset asset)
        {
            var lines = new List<string>();
            if (asset == null)
            {
                return lines;
            }

            if (!asset.Owned)
            {
                lines.Add("其他数据源的资源不记录引用。");
                return lines;
            }

            if (IsBundle(asset.Kind))
            {
                var prefabs = ReadPrefabs(asset, out var opened);
                if (!opened)
                {
                    lines.Add("没有读出预制体，看不到引用。");
                    return lines;
                }

                if (prefabs.Count == 0)
                {
                    AddReferenceLine(lines, asset.Key, "整包");
                    return lines;
                }

                for (var i = 0; i < prefabs.Count; i++)
                {
                    AddReferenceLine(lines, prefabs[i].Key, prefabs[i].Name);
                }

                return lines;
            }

            var refs = References(asset.Key);
            if (refs.Count == 0)
            {
                lines.Add("没有引用。");
            }
            else
            {
                lines.AddRange(refs);
            }

            return lines;
        }

        static ResourceAsset ImportBundle(string file, string folderName, string kind, out string error)
        {
            var asset = ImportFile(file, folderName, kind, out error);
            if (asset == null)
            {
                return null;
            }

            var prefabs = ReadPrefabs(asset, out var opened);
            if (!opened)
            {
                RemoveImported(asset);
                error = "这不是当前平台能读取的 Unity AssetBundle。";
                return null;
            }

            if (prefabs.Count == 0)
            {
                RemoveImported(asset);
                error = "这个 AssetBundle 里没有预制体。";
                return null;
            }

            return asset;
        }

        static ResourceAsset ImportFile(string file, string folderName, string kind, out string error)
        {
            error = null;
            if (!ProjectSession.IsOpen)
            {
                error = "还没有打开项目。";
                return null;
            }

            if (string.IsNullOrEmpty(file) || !File.Exists(file))
            {
                error = "找不到这个文件。";
                return null;
            }

            var leaf = Path.GetFileName(file);
            if (!ValidSegment(leaf))
            {
                error = "这个文件名不能作为资源名。";
                return null;
            }

            var root = CategoryRoot(folderName);
            var destination = Path.Combine(root, leaf);
            if (File.Exists(destination))
            {
                error = "项目里已经有同名资源。";
                return null;
            }

            try
            {
                Directory.CreateDirectory(root);
                File.Copy(file, destination, false);
            }
            catch (Exception)
            {
                error = "资源没有复制进项目目录。";
                return null;
            }

            return new ResourceAsset
            {
                Key = leaf,
                Name = DisplayName(leaf, kind),
                Directory = "",
                Kind = kind,
                KindLabel = Label(kind),
                Owned = true,
                SourceTitle = "本项目",
                FilePath = destination,
                Length = new FileInfo(destination).Length,
            };
        }

        static void RemoveImported(ResourceAsset asset)
        {
            try
            {
                if (asset != null && File.Exists(asset.FilePath))
                {
                    File.Delete(asset.FilePath);
                }
            }
            catch (Exception)
            {
            }

            ForgetBundle(asset);
        }

        static void AddFileTree(List<ResourceAsset> list, string folderName, string kind)
        {
            var root = CategoryRoot(folderName);
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return;
            }

            WalkFiles(root, root, kind, list);
        }

        static void WalkFiles(string root, string folder, string kind, List<ResourceAsset> list)
        {
            string[] files;
            try
            {
                files = Directory.GetFiles(folder);
            }
            catch (Exception)
            {
                files = Array.Empty<string>();
            }

            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < files.Length; i++)
            {
                var leaf = Path.GetFileName(files[i]);
                if (leaf.Length == 0 || leaf[0] == '.' || leaf.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (IsAudio(kind) && !IsAudioFile(files[i]))
                {
                    continue;
                }

                var relative = Path.GetRelativePath(root, files[i]).Replace('\\', '/');
                SplitKey(relative, out var directory, out var name);
                list.Add(new ResourceAsset
                {
                    Key = relative,
                    Name = DisplayName(name, kind),
                    Directory = directory,
                    Kind = kind,
                    KindLabel = Label(kind),
                    Owned = true,
                    SourceTitle = "本项目",
                    FilePath = files[i],
                    Length = new FileInfo(files[i]).Length,
                });
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
                WalkFiles(root, children[i], kind, list);
            }
        }

        static List<ResourcePrefab> ReadPrefabs(ResourceAsset asset, out bool opened)
        {
            opened = false;
            if (asset == null || !IsBundle(asset.Kind))
            {
                return new List<ResourcePrefab>();
            }

            if (!string.IsNullOrEmpty(asset.FilePath) && File.Exists(asset.FilePath))
            {
                var info = new FileInfo(asset.FilePath);
                return LoadCached(asset.FilePath, info.LastWriteTimeUtc.Ticks ^ info.Length, asset.Key, asset.Kind, null, asset.FilePath, out opened);
            }

            if (!asset.Owned && !string.IsNullOrEmpty(asset.SourcePath)
                && GameDataFile.TryReadEntry(asset.SourcePath, asset.Key, out var data, out _))
            {
                var cacheId = asset.SourcePath + "\n" + asset.Key;
                return LoadCached(cacheId, data.LongLength, asset.Key, asset.Kind, data, null, out opened);
            }

            return new List<ResourcePrefab>();
        }

        static List<ResourcePrefab> LoadCached(string cacheId, long stamp, string bundleKey, string kind, byte[] data, string path, out bool opened)
        {
            if (PrefabCaches.TryGetValue(cacheId, out var cached) && cached.Stamp == stamp)
            {
                opened = cached.Opened;
                return cached.Prefabs;
            }

            AssetBundle bundle = null;
            try
            {
                bundle = data != null ? AssetBundle.LoadFromMemory(data) : AssetBundle.LoadFromFile(path);
            }
            catch (Exception)
            {
                bundle = null;
            }

            var prefabs = new List<ResourcePrefab>();
            opened = bundle != null;
            if (bundle != null)
            {
                try
                {
                    CollectPrefabs(bundle, bundleKey, kind, prefabs);
                }
                finally
                {
                    bundle.Unload(true);
                }
            }

            PrefabCaches[cacheId] = new PrefabCache
            {
                Stamp = stamp,
                Opened = opened,
                Prefabs = prefabs,
            };
            return prefabs;
        }

        static void CollectPrefabs(AssetBundle bundle, string bundleKey, string kind, List<ResourcePrefab> prefabs)
        {
            var names = bundle.GetAllAssetNames();
            Array.Sort(names, StringComparer.OrdinalIgnoreCase);
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < names.Length; i++)
            {
                var go = bundle.LoadAsset<GameObject>(names[i]);
                if (go == null)
                {
                    continue;
                }

                var suffix = GameDataBundles.PrefabSuffix(names[i]);
                if (suffix.Length == 0)
                {
                    suffix = "prefab";
                }

                var unique = suffix;
                var number = 2;
                while (!used.Add(unique))
                {
                    unique = suffix + "-" + number;
                    number++;
                }

                var slash = unique.LastIndexOf('/');
                prefabs.Add(new ResourcePrefab
                {
                    Name = slash < 0 ? unique : unique.Substring(slash + 1),
                    Key = bundleKey + "/" + unique,
                    Note = PrefabNote(go, kind),
                });
            }
        }

        static string PrefabNote(GameObject go, string kind)
        {
            if (IsParticles(kind))
            {
                return go.GetComponentInChildren<ParticleSystem>(true) != null ? "ParticleSystem" : "没有 ParticleSystem";
            }

            if (IsAnimation(kind))
            {
                return go.GetComponentInChildren<Animator>(true) != null ? "Animator" : "预制体";
            }

            return "";
        }

        static void AddReferenceLine(List<string> lines, string key, string title)
        {
            var refs = References(key);
            if (refs.Count == 0)
            {
                lines.Add(title + "  ·  没有引用");
                return;
            }

            lines.Add(title + "  ·  " + string.Join("、", refs));
        }

        static void ForgetBundle(ResourceAsset asset)
        {
            if (asset == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(asset.FilePath))
            {
                PrefabCaches.Remove(asset.FilePath);
            }

            if (!string.IsNullOrEmpty(asset.SourcePath))
            {
                PrefabCaches.Remove(asset.SourcePath + "\n" + asset.Key);
            }
        }

        static string DisplayName(string leaf, string kind)
        {
            if (!IsBundle(kind) && !IsAudio(kind))
            {
                return leaf ?? "";
            }

            var extension = Path.GetExtension(leaf ?? "");
            if (extension.Length == 0 || extension.Length >= (leaf ?? "").Length)
            {
                return leaf ?? "";
            }

            return leaf.Substring(0, leaf.Length - extension.Length);
        }

        static string FolderName(string kind)
        {
            if (IsAnimation(kind))
            {
                return AnimationFolder;
            }

            if (IsParticles(kind))
            {
                return ParticlesFolder;
            }

            if (IsAudio(kind))
            {
                return AudioFolder;
            }

            return SpineFolder;
        }

        static string CategoryRoot(string folderName)
        {
            if (!ProjectSession.IsOpen || string.IsNullOrEmpty(folderName))
            {
                return "";
            }

            var project = Path.GetDirectoryName(ProjectSession.ArchivePath);
            if (string.IsNullOrEmpty(project))
            {
                return "";
            }

            return Path.Combine(project, folderName);
        }

        static string FileOf(string kind, string key)
        {
            var path = CategoryRoot(FolderName(kind));
            var parts = (key ?? "").Split('/');
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    path = Path.Combine(path, parts[i]);
                }
            }

            return path;
        }

        static bool IsAudioFile(string path)
        {
            var extension = Path.GetExtension(path ?? "");
            return extension.Equals(".wav", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".mp3", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".ogg", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".aiff", StringComparison.OrdinalIgnoreCase)
                   || extension.Equals(".aif", StringComparison.OrdinalIgnoreCase);
        }

        static bool Has(string text, string query)
        {
            return (text ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
