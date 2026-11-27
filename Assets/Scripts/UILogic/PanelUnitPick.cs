using System;
using System.Collections.Generic;
using FairyGUI;
using PackageUnit;

namespace UILogic
{
    /// <summary>带搜索的选择面板。窗口和行都是 PackageUnit 里的组件。</summary>
    public class PanelUnitPick : FguiPanelBase
    {
        readonly List<string> _keys = new List<string>();
        readonly List<string> _titles = new List<string>();
        readonly List<int> _shown = new List<int>();
        UI_WindowPick _window;
        Action<string> _onPick;

        public static void Show(string title, IList<string> keys, IList<string> titles, Action<string> onPick)
        {
            FGUIPanelManager.Instance.ShowPanel<PanelUnitPick>(
                UI_WindowPick.PackageName,
                UI_WindowPick.ResName,
                new object[] { title, Copy(keys), Copy(titles), onPick },
                ShowTypeEnum.OverCurrent);
        }

        public override void Start(object[] data)
        {
            _window = (UI_WindowPick)FGUIComponent;
            _keys.AddRange(data != null && data.Length > 1 ? data[1] as List<string> : null ?? new List<string>());
            _titles.AddRange(data != null && data.Length > 2 ? data[2] as List<string> : null ?? new List<string>());
            _onPick = data != null && data.Length > 3 ? data[3] as Action<string> : null;
            var title = data != null && data.Length > 0 ? data[0] as string : null;
            _window.m_title.text = string.IsNullOrEmpty(title) ? "选择" : title;
            _window.m_search.text = "";
            _window.m_search.onChanged.Set(Fill);
            _window.m_mask.touchable = true;
            _window.m_mask.onClick.Set(Leave);
            _window.m_btnCancel.onClick.Set(Leave);
            _window.m_choices.itemRenderer = Render;
            Fill();
        }

        public override void OnClose()
        {
            _onPick = null;
            _window = null;
        }

        void Leave()
        {
            _onPick = null;
            FGUIPanelManager.Instance.Back();
        }

        void Fill()
        {
            if (_window == null)
            {
                return;
            }

            var query = (_window.m_search.text ?? "").Trim();
            _shown.Clear();
            for (var i = 0; i < _keys.Count; i++)
            {
                if (query.Length > 0
                    && _titles[i].IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0
                    && _keys[i].IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                _shown.Add(i);
            }

            _window.m_choices.numItems = _shown.Count;
            _window.m_empty.visible = _shown.Count == 0;
        }

        void Render(int index, GObject obj)
        {
            var choice = _shown[index];
            var button = (UI_ItemPick)obj;
            button.title = _titles[choice];
            button.SetSize(_window.m_choices.width, button.height);
            button.onClick.Set(() =>
            {
                var pick = _onPick;
                var key = _keys[choice];
                _onPick = null;
                FGUIPanelManager.Instance.Back();
                if (pick != null)
                {
                    pick(key);
                }
            });
        }

        static List<string> Copy(IList<string> source)
        {
            var copy = new List<string>();
            if (source == null)
            {
                return copy;
            }

            for (var i = 0; i < source.Count; i++)
            {
                copy.Add(source[i] ?? "");
            }

            return copy;
        }
    }
}
