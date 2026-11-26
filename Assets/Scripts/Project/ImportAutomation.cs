using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MoonSharp.Interpreter;

namespace SpellEditor
{
    /// <summary>
    /// 资源导入之后调用已安装的 Lua 插件。
    /// job.kind 是 spine、animation、particles、audio，不限于某一种资源。
    /// job.files 是这份资源的全部文件，job:read 读出内容。贴图如果存在共用目录，也按页面名放进来。
    /// 插件写下 job.setup 时，Spine 存成目录里的 setup.txt，其他资源在同目录写 setup.txt。
    /// job:write 只接受资源目录下的单个文件名。
    /// </summary>
    public static class ImportAutomation
    {
        public static string Apply(ResourceAsset asset, string sourcePath)
        {
            if (asset == null || !asset.Owned)
            {
                return "";
            }

            var plugins = PluginLibrary.List();
            if (plugins.Count == 0)
            {
                return "";
            }

            var job = new Job
            {
                Kind = LuaKind(asset.Kind),
                Name = asset.Name ?? "",
                Source = sourcePath ?? "",
                Key = asset.Key ?? "",
            };
            CollectFiles(asset, job.FileBytes);
            AddLines(job.Entries, ResourceLibrary.DetailLines(asset));
            job.Setup = ReadExistingSetup(asset);
            var existingSetup = job.Setup;

            var notes = new List<string>();
            var files = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < plugins.Count; i++)
            {
                Run(plugins[i], job, files, notes);
            }

            var folder = OutputFolder(asset);
            if (!string.IsNullOrEmpty(job.Setup) && !string.Equals(job.Setup, existingSetup, StringComparison.Ordinal))
            {
                files["setup.txt"] = job.Setup;
            }

            if (files.Count > 0 && string.IsNullOrEmpty(folder))
            {
                notes.Add("插件写了文件，但这份资源没有目录。");
            }
            else
            {
                WriteFiles(asset, folder, files, notes);
            }

            var result = notes.Count == 0 ? "" : string.Join(" ", notes);
            if (result.Length > 0)
            {
                UnityEngine.Debug.Log("导入插件：" + asset.Key + " " + result);
            }

            return result;
        }

        static void Run(PluginLibrary.PluginInfo plugin, Job job, Dictionary<string, string> files, List<string> notes)
        {
            var entry = Path.Combine(plugin.Directory, plugin.Entry);
            if (!File.Exists(entry))
            {
                notes.Add(plugin.Name + " 的入口不在。");
                return;
            }

            try
            {
                var script = new Script(CoreModules.Preset_HardSandbox);
                var code = File.ReadAllText(entry);
                script.DoString(code, null, plugin.Entry);
                var function = script.Globals.Get("on_import");
                if (function.Type != DataType.Function)
                {
                    notes.Add(plugin.Name + " 没有 on_import。");
                    return;
                }

                var table = Build(script, job);
                try
                {
                    script.Call(function, table);
                }
                finally
                {
                    TakeSetup(table, job);
                }

                foreach (var pair in job.Pending)
                {
                    files[pair.Key] = pair.Value;
                }

                notes.AddRange(job.Notes);
                job.Pending.Clear();
                job.Notes.Clear();
            }
            catch (Exception ex)
            {
                job.Pending.Clear();
                job.Notes.Clear();
                UnityEngine.Debug.LogException(ex);
                notes.Add(plugin.Name + " 没有跑完：" + FirstLine(ex.Message));
            }
        }

        static void TakeSetup(Table table, Job job)
        {
            if (table == null)
            {
                return;
            }

            var setup = table.Get("setup");
            if (setup.Type == DataType.String && setup.String.Length > 0)
            {
                job.Setup = setup.String;
            }
        }

        static Table Build(Script script, Job job)
        {
            var table = new Table(script);
            table["kind"] = job.Kind;
            table["name"] = job.Name;
            table["source"] = job.Source;
            table["key"] = job.Key;
            table["setup"] = job.Setup ?? "";
            table["files"] = Strings(script, FileNames(job));
            table["entries"] = Strings(script, job.Entries);
            table["read"] = DynValue.NewCallback((context, args) =>
            {
                var name = Arg(args, 1);
                if (name.Length == 0 || !job.FileBytes.TryGetValue(name, out var bytes) || bytes == null)
                {
                    return DynValue.Nil;
                }

                return DynValue.NewString(AsBytes(bytes));
            });
            table["note"] = DynValue.NewCallback((context, args) =>
            {
                var text = Arg(args, 1);
                if (text.Length > 0)
                {
                    job.Notes.Add(text);
                }

                return DynValue.Nil;
            });
            table["write"] = DynValue.NewCallback((context, args) =>
            {
                var name = Arg(args, 1);
                if (!PlainFile(name))
                {
                    job.Notes.Add("插件要写的文件名不能用：" + name);
                    return DynValue.Nil;
                }

                job.Pending[name] = Arg(args, 2);
                return DynValue.Nil;
            });
            return table;
        }

        static Table Strings(Script script, List<string> values)
        {
            var table = new Table(script);
            for (var i = 0; i < values.Count; i++)
            {
                table.Append(DynValue.NewString(values[i] ?? ""));
            }

            return table;
        }

        static void WriteFiles(ResourceAsset asset, string folder, Dictionary<string, string> files, List<string> notes)
        {
            foreach (var pair in files)
            {
                if (pair.Key.Equals("setup.txt", StringComparison.OrdinalIgnoreCase) && ResourceLibrary.IsSpine(asset.Kind))
                {
                    if (!ResourceLibrary.TryWriteSpineSetup(asset, pair.Value, out var error))
                    {
                        notes.Add(error ?? "setup.txt 没有写进去。");
                    }
                    else if (!notes.Contains("已写入 setup.txt。"))
                    {
                        notes.Add("已写入 setup.txt。");
                    }

                    continue;
                }

                if (pair.Key.Equals("pages.txt", StringComparison.OrdinalIgnoreCase))
                {
                    notes.Add("插件不能改 pages.txt。");
                    continue;
                }

                try
                {
                    Directory.CreateDirectory(folder);
                    File.WriteAllText(Path.Combine(folder, pair.Key), pair.Value ?? "", new UTF8Encoding(false));
                    notes.Add("已写入 " + pair.Key + "。");
                }
                catch (Exception)
                {
                    notes.Add(pair.Key + " 没有写进去。");
                }
            }
        }

        static string ReadExistingSetup(ResourceAsset asset)
        {
            if (ResourceLibrary.IsSpine(asset.Kind))
            {
                return ResourceLibrary.TryReadSpineSetup(asset, out var text, out _) ? text ?? "" : "";
            }

            var folder = OutputFolder(asset);
            if (string.IsNullOrEmpty(folder))
            {
                return "";
            }

            var path = Path.Combine(folder, "setup.txt");
            if (!File.Exists(path))
            {
                return "";
            }

            try
            {
                return File.ReadAllText(path);
            }
            catch (Exception)
            {
                return "";
            }
        }

        static string OutputFolder(ResourceAsset asset)
        {
            if (!string.IsNullOrEmpty(asset.Folder) && Directory.Exists(asset.Folder))
            {
                return asset.Folder;
            }

            if (!string.IsNullOrEmpty(asset.FilePath))
            {
                return Path.GetDirectoryName(asset.FilePath) ?? "";
            }

            return "";
        }

        static string LuaKind(string kind)
        {
            if (ResourceLibrary.IsSpine(kind))
            {
                return "spine";
            }

            if (ResourceLibrary.IsAnimation(kind))
            {
                return "animation";
            }

            if (ResourceLibrary.IsParticles(kind))
            {
                return "particles";
            }

            if (ResourceLibrary.IsAudio(kind))
            {
                return "audio";
            }

            return kind ?? "";
        }

        static void CollectFiles(ResourceAsset asset, Dictionary<string, byte[]> files)
        {
            if (!string.IsNullOrEmpty(asset.Folder) && Directory.Exists(asset.Folder))
            {
                AddFolderFiles(asset.Folder, files);
            }
            else if (!string.IsNullOrEmpty(asset.FilePath) && File.Exists(asset.FilePath))
            {
                TryAddFile(files, Path.GetFileName(asset.FilePath), asset.FilePath);
            }

            if (!ResourceLibrary.IsSpine(asset.Kind) || !ResourceLibrary.TryOpenSpine(asset, out var document, out _))
            {
                return;
            }

            for (var i = 0; i < document.Pages.Count; i++)
            {
                var page = document.Pages[i];
                if (page == null || string.IsNullOrEmpty(page.Name) || page.Bytes == null || files.ContainsKey(page.Name))
                {
                    continue;
                }

                files[page.Name] = page.Bytes;
            }
        }

        static void AddFolderFiles(string folder, Dictionary<string, byte[]> files)
        {
            string[] paths;
            try
            {
                paths = Directory.GetFiles(folder);
            }
            catch (Exception)
            {
                return;
            }

            Array.Sort(paths, StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < paths.Length; i++)
            {
                var leaf = Path.GetFileName(paths[i]);
                if (leaf.Length == 0 || leaf[0] == '.' || leaf.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                TryAddFile(files, leaf, paths[i]);
            }
        }

        static void TryAddFile(Dictionary<string, byte[]> files, string name, string path)
        {
            if (string.IsNullOrEmpty(name) || files.ContainsKey(name))
            {
                return;
            }

            try
            {
                files[name] = File.ReadAllBytes(path);
            }
            catch (Exception)
            {
            }
        }

        static List<string> FileNames(Job job)
        {
            var names = new List<string>(job.FileBytes.Keys);
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        static string AsBytes(byte[] bytes)
        {
            var chars = new char[bytes.Length];
            for (var i = 0; i < bytes.Length; i++)
            {
                chars[i] = (char)bytes[i];
            }

            return new string(chars);
        }

        static void AddLines(List<string> into, List<string> lines)
        {
            if (lines == null)
            {
                return;
            }

            into.AddRange(lines);
        }

        static string Arg(CallbackArguments args, int index)
        {
            if (args == null || index < 0 || index >= args.Count)
            {
                return "";
            }

            var value = args[index];
            if (value.Type == DataType.String)
            {
                return value.String ?? "";
            }

            if (value.Type == DataType.Number || value.Type == DataType.Boolean)
            {
                return value.CastToString() ?? "";
            }

            return "";
        }

        static string FirstLine(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return "出错了。";
            }

            var line = message.Split('\n')[0].Trim();
            return line.Length == 0 ? "出错了。" : line;
        }

        static bool PlainFile(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "." || name == "..")
            {
                return false;
            }

            return name.IndexOfAny(new[] { '/', '\\', ':' }) < 0;
        }

        sealed class Job
        {
            public string Kind = "";
            public string Name = "";
            public string Source = "";
            public string Key = "";
            public string Setup = "";
            public readonly Dictionary<string, byte[]> FileBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
            public readonly List<string> Entries = new List<string>();
            public readonly List<string> Notes = new List<string>();
            public readonly Dictionary<string, string> Pending = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
