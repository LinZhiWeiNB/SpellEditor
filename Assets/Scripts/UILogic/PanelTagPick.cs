using System;
using System.Collections.Generic;
using FairyGUI;
using RebornFrame.Spell;
using SpellEditor;
using UnityEngine;

namespace UILogic
{
    /// <summary>左侧搜索已有标签，右侧新建。选中或创建后交给调用方并关闭。</summary>
    public class PanelTagPick : FguiPanelBase
    {
        const string Package = "PackageSpell";
        const string Panel = "PanelTagPick";

        readonly List<ulong> _ids = new List<ulong>();
        readonly List<int> _shown = new List<int>();
        GComponent _view;
        Action<ulong> _onPick;

        public static bool Show(Action<ulong> onPick)
        {
            if (!PanelLayer.Published(Package, Panel))
            {
                Debug.LogWarning("PackageSpell 里还没有 PanelTagPick，先在 FairyGUI 发布技能包。");
                return false;
            }

            FGUIPanelManager.Instance.ShowPanel<PanelTagPick>(
                Package,
                Panel,
                new object[] { onPick },
                ShowTypeEnum.OverCurrent);
            return true;
        }

        public override void Start(object[] data)
        {
            _view = FGUIComponent;
            _onPick = data != null && data.Length > 0 ? data[0] as Action<ulong> : null;
            Collect();
            var search = Input("search");
            var name = Input("inputName");
            search.text = "";
            name.text = "";
            Label("createWarn").text = "";
            search.onChanged.Set(Fill);
            PanelLayer.Mask(_view, Leave);
            (_view.GetChild("btnCancel") as GButton).onClick.Set(Leave);
            (_view.GetChild("btnCreate") as GButton).onClick.Set(Create);
            var list = _view.GetChild("choices") as GList;
            list.itemRenderer = Render;
            Fill();
        }

        public override void OnClose()
        {
            _onPick = null;
            _view = null;
        }

        void Leave()
        {
            _onPick = null;
            FGUIPanelManager.Instance.Back();
        }

        void Collect()
        {
            _ids.Clear();
            Add(ProjectSession.OwnModule);
            for (var i = 0; i < ProjectSession.Sources.Count; i++)
            {
                Add(ProjectSession.Sources[i].Module);
            }
        }

        void Add(SpellModuleData module)
        {
            if (module == null)
            {
                return;
            }

            for (var i = 0; i < module.Tags.Count; i++)
            {
                var tag = module.Tags[i];
                if (tag != null && tag.Id != 0)
                {
                    _ids.Add(tag.Id);
                }
            }
        }

        void Fill()
        {
            if (_view == null)
            {
                return;
            }

            var query = (Input("search").text ?? "").Trim();
            _shown.Clear();
            for (var i = 0; i < _ids.Count; i++)
            {
                if (Matches(_ids[i], query))
                {
                    _shown.Add(i);
                }
            }

            var list = _view.GetChild("choices") as GList;
            list.numItems = _shown.Count;
            _view.GetChild("empty").visible = _shown.Count == 0;
        }

        static bool Matches(ulong id, string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return true;
            }

            var path = ComTagEditor.Path(id);
            var parts = path.Split(new[] { " / " }, StringSplitOptions.None);
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            var parsed = new TagId(id);
            return parsed.LocalId.ToString().IndexOf(query, StringComparison.Ordinal) >= 0
                   || parsed.ToString().IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        void Render(int index, GObject obj)
        {
            var choice = _shown[index];
            var button = (GButton)obj;
            var id = _ids[choice];
            button.title = ComTagEditor.Path(id);
            button.SetSize((_view.GetChild("choices") as GList).width, button.height);
            button.onClick.Set(() => Use(id));
        }

        void Create()
        {
            var name = (Input("inputName").text ?? "").Trim();
            if (name.Length == 0)
            {
                Label("createWarn").text = "先填写名称。";
                return;
            }

            var id = ComTagEditor.CreateRoot(name);
            if (id == 0)
            {
                Label("createWarn").text = "没能创建标签。";
                return;
            }

            Use(id);
        }

        void Use(ulong id)
        {
            var pick = _onPick;
            _onPick = null;
            FGUIPanelManager.Instance.Back();
            if (pick != null)
            {
                pick(id);
            }
        }

        GTextInput Input(string name)
        {
            return _view.GetChild(name) as GTextInput;
        }

        GTextField Label(string name)
        {
            return _view.GetChild(name) as GTextField;
        }
    }
}
