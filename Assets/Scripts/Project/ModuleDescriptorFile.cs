using System;
using System.IO;
using UnityEngine;

namespace SpellEditor
{
    /// <summary>
    /// 导出时附在 SpellModuleData 旁边的描述文件。
    /// </summary>
    [Serializable]
    public sealed class ModuleDescriptor
    {
        public const int CurrentFormat = 1;
        public const string Extension = ".module.json";

        public int format = CurrentFormat;
        public string editor_version = "";
        public string role = "";
        public string export_location = "";
        public string mod_directory = "";
        public string name = "";
        public string description = "";
        public string directory_name = "";
        public string module_data = "";
    }

    public static class ModuleDescriptorFile
    {
        public static void Write(string path, ModuleDescriptor descriptor)
        {
            File.WriteAllText(path, JsonUtility.ToJson(descriptor, true));
        }

        public static bool TryRead(string path, out ModuleDescriptor descriptor, out string error)
        {
            descriptor = null;
            error = null;
            try
            {
                var text = File.ReadAllText(path);
                descriptor = JsonUtility.FromJson<ModuleDescriptor>(text);
            }
            catch (Exception)
            {
                descriptor = null;
            }

            if (descriptor == null || descriptor.format != ModuleDescriptor.CurrentFormat || string.IsNullOrEmpty(descriptor.module_data))
            {
                error = "请选择描述文件，而不是数据二进制。";
                descriptor = null;
                return false;
            }

            return true;
        }
    }
}
