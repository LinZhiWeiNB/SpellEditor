using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameContentEditor
{
    public sealed class RecentProject
    {
        public string Path;
        public string Name;
        public uint AuthorId;
        public long OpenedAt;
    }

    /// <summary>
    /// 最近打开的项目只记在本机，不写进项目存档。
    /// </summary>
    public static class RecentProjects
    {
        const int Limit = 12;

        public static List<RecentProject> Load()
        {
            var result = new List<RecentProject>();
            var file = FilePath();
            if (!File.Exists(file))
            {
                return result;
            }

            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split('\t');
                if (parts.Length < 4 || !File.Exists(parts[0]))
                {
                    continue;
                }

                if (!uint.TryParse(parts[2], out var authorId) || !long.TryParse(parts[3], out var openedAt))
                {
                    continue;
                }

                result.Add(new RecentProject
                {
                    Path = parts[0],
                    Name = parts[1],
                    AuthorId = authorId,
                    OpenedAt = openedAt,
                });
            }

            return result;
        }

        public static void Touch(string path, string name, uint authorId)
        {
            var items = Load();
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (string.Equals(items[i].Path, path, StringComparison.Ordinal))
                {
                    items.RemoveAt(i);
                }
            }

            items.Insert(0, new RecentProject
            {
                Path = path,
                Name = name ?? "",
                AuthorId = authorId,
                OpenedAt = DateTime.Now.Ticks,
            });
            if (items.Count > Limit)
            {
                items.RemoveRange(Limit, items.Count - Limit);
            }

            Write(items);
        }

        public static void Forget(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var items = Load();
            var changed = false;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (string.Equals(items[i].Path, path, StringComparison.OrdinalIgnoreCase))
                {
                    items.RemoveAt(i);
                    changed = true;
                }
            }

            if (changed)
            {
                Write(items);
            }
        }

        static void Write(List<RecentProject> items)
        {
            var lines = new string[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                lines[i] = item.Path + "\t" + item.Name.Replace('\t', ' ') + "\t" + item.AuthorId + "\t" + item.OpenedAt;
            }

            File.WriteAllLines(FilePath(), lines);
        }

        static string FilePath()
        {
            return Path.Combine(Application.persistentDataPath, "recent-projects.txt");
        }
    }
}
