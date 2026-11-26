using System;
using System.Collections.Generic;
using FairyGUI;
using PackageUnit;

namespace UILogic
{
    /// <summary>带搜索的选择弹窗。窗口和行都是 PackageUnit 里的组件。</summary>
    sealed class UnitPickDialog
    {
        readonly List<string> _keys = new List<string>();
        readonly List<string> _titles = new List<string>();
        readonly List<int> _shown = new List<int>();
        UI_WindowPick _window;
        Action<string> _onPick;

        public void Open(string title, IList<string> keys, IList<string> titles, Action<string> onPick)
        {
            Close();
            _window = UI_WindowPick.CreateInstance();
            if (_window == null)
            {
                return;
            }

            _keys.Clear();
            _titles.Clear();
            if (keys != null)
            {
                for (var i = 0; i < keys.Count; i++)
                {
                    _keys.Add(keys[i] ?? "");
                    _titles.Add(titles != null && i < titles.Count ? titles[i] ?? "" : keys[i] ?? "");
                }
            }

            _onPick = onPick;
            var root = GRoot.inst;
            _window.SetSize(root.width, root.height);
            _window.AddRelation(root, RelationType.Size);
            _window.sortingOrder = 1000;
            root.AddChild(_window);
            _window.m_title.text = string.IsNullOrEmpty(title) ? "选择" : title;
            _window.m_search.text = "";
            _window.m_search.onChanged.Set(Fill);
            _window.m_mask.onClick.Set(Close);
            _window.m_btnCancel.onClick.Set(Close);
            _window.m_choices.itemRenderer = Render;
            Fill();
        }

        public void Close()
        {
            _onPick = null;
            if (_window != null)
            {
                _window.Dispose();
                _window = null;
            }
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
                Close();
                if (pick != null)
                {
                    pick(key);
                }
            });
        }
    }
}
