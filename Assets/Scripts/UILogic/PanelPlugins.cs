using System.Collections.Generic;
using FairyGUI;
using PackageMain;
using SpellEditor;
using UnityEngine;

namespace UILogic
{
    public class PanelPlugins : FguiPanelBase
    {
        UI_PanelPlugins view;
        readonly List<PluginLibrary.PluginInfo> _items = new List<PluginLibrary.PluginInfo>();

        public override void Start(object[] data)
        {
            view = (UI_PanelPlugins)FGUIComponent;
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            view.m_btnImport.onClick.Set(Import);
            view.m_btnDelete.onClick.Set(Delete);
            view.m_list.itemRenderer = RenderItem;
            view.m_list.onClickItem.Set(OnClickItem);
            Refresh("");
        }

        void Import()
        {
            var folder = EditorDialog.OpenFolder("选择插件目录");
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            if (!PluginLibrary.Import(folder, out var name, out var error))
            {
                Say(error ?? "没有导入。", true);
                return;
            }

            Refresh(name);
            Say("已导入 " + name + "。", false);
        }

        void Delete()
        {
            var index = view.m_list.selectedIndex;
            if (index < 0 || index >= _items.Count)
            {
                Say("先选一个插件。", true);
                return;
            }

            var item = _items[index];
            if (!PluginLibrary.Delete(item.FolderName, out var error))
            {
                Say(error ?? "没有删除。", true);
                return;
            }

            Refresh("");
            Say("已删除 " + item.Name + "。", false);
        }

        void OnClickItem()
        {
            view.m_message.text = "";
        }

        void Refresh(string selectName)
        {
            _items.Clear();
            _items.AddRange(PluginLibrary.List());
            view.m_list.numItems = _items.Count;
            var index = -1;
            if (!string.IsNullOrEmpty(selectName))
            {
                for (var i = 0; i < _items.Count; i++)
                {
                    if (_items[i].FolderName == selectName || _items[i].Name == selectName)
                    {
                        index = i;
                        break;
                    }
                }
            }

            view.m_list.selectedIndex = index;
            if (_items.Count == 0)
            {
                view.m_message.color = new Color32(0x8d, 0x88, 0x7e, 0xff);
                view.m_message.text = "还没有插件。";
            }
        }

        void RenderItem(int index, GObject obj)
        {
            var row = (UI_ItemPlugin)obj;
            var item = _items[index];
            row.title = item.Name;
            row.m_meta.text = item.Id + "  ·  " + item.Entry;
        }

        void Say(string text, bool problem)
        {
            view.m_message.color = problem
                ? new Color32(0xe0, 0x6a, 0x6a, 0xff)
                : new Color32(0xb7, 0xd4, 0xa8, 0xff);
            view.m_message.text = text ?? "";
        }
    }
}
