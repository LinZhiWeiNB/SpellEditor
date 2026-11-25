using FairyGUI;
using PackageMain;
using SpellEditor;
using UnityEngine;

namespace UILogic
{
    public class ComExport
    {
        readonly GComponent _root;
        readonly GTextInput _location;
        readonly GTextInput _modDirectory;
        readonly GTextInput _name;
        readonly GTextInput _intro;
        readonly GTextInput _directory;
        readonly GTextField _error;
        readonly Controller _role;
        readonly GObject _gameFields;
        readonly GObject _modFields;
        readonly bool _mod;

        public ComExport(GComponent parent)
        {
            _mod = ProjectSession.AuthorId != 0;
            var published = UIPackage.GetByName(UI_PanelExport.PackageName);
            if (published != null && published.GetItemByName(UI_PanelExport.ResName) != null)
            {
                var view = UI_PanelExport.CreateInstance();
                _root = view;
                _location = view.m_inputLocation;
                _modDirectory = view.m_inputModDir;
                _name = view.m_inputName;
                _intro = view.m_inputIntro;
                _directory = view.m_inputDir;
                _error = view.m_error;
                _role = view.m_cRole;
                _gameFields = view.m_gameFields;
                _modFields = view.m_modFields;
                view.m_version.text = "描述文件会自动写入当前编辑器版本 " + EditorVersion.Current + "。";
                view.m_btnBack.onClick.Add(Close);
                view.m_btnLocation.onClick.Add(PickFolder);
                view.m_btnExport.onClick.Add(Export);
            }
            else
            {
                GButton pick;
                GButton export;
                GButton back;
                _root = Build(
                    out _location,
                    out _modDirectory,
                    out _name,
                    out _intro,
                    out _directory,
                    out _error,
                    out _gameFields,
                    out _modFields,
                    out pick,
                    out export,
                    out back);
                _role = null;
                back.onClick.Add(Close);
                pick.onClick.Add(PickFolder);
                export.onClick.Add(Export);
            }

            _intro.singleLine = false;
            _directory.restrict = "[a-zA-Z0-9_\\-]";
            _error.text = "";
            ApplyRole();
            _root.name = "export";
            _root.SetSize(parent.width, parent.height);
            _root.AddRelation(parent, RelationType.Size);
            parent.AddChild(_root);
        }

        void ApplyRole()
        {
            if (_role != null)
            {
                _role.selectedIndex = _mod ? 1 : 0;
                return;
            }

            _gameFields.visible = !_mod;
            _modFields.visible = _mod;
        }

        void PickFolder()
        {
            var folder = EditorDialog.OpenFolder("导出位置");
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            _location.text = folder;
            _error.text = "";
        }

        void Export()
        {
            var error = _mod
                ? ProjectExport.ExportMod(_name.text, _intro.text, _directory.text)
                : ProjectExport.ExportGame(_location.text, _modDirectory.text);
            _error.color = error == null
                ? new Color32(0xb7, 0xd4, 0xa8, 0xff)
                : new Color32(0xe0, 0xb0, 0x60, 0xff);
            _error.text = error ?? "已写出描述文件和数据。";
        }

        void Close()
        {
            _root.RemoveFromParent();
            _root.Dispose();
        }

        static GComponent Build(
            out GTextInput location,
            out GTextInput modDirectory,
            out GTextInput name,
            out GTextInput intro,
            out GTextInput directory,
            out GTextField error,
            out GObject gameFields,
            out GObject modFields,
            out GButton pick,
            out GButton export,
            out GButton back)
        {
            var root = new GComponent();
            root.SetSize(1600, 900);
            var bg = new GGraph();
            bg.DrawRect(1600, 900, 0, Color.clear, new Color32(0x16, 0x18, 0x1d, 0xff));
            bg.SetSize(1600, 900);
            bg.AddRelation(root, RelationType.Size);
            root.AddChild(bg);

            back = PackageLaunch.UI_ButtonGhost.CreateInstance();
            back.title = "返回";
            back.SetXY(48, 24);
            back.SetSize(88, 36);
            root.AddChild(back);

            var title = NewText(root, 400, 28, 280, 36, 22, new Color32(0xe8, 0xe6, 0xe3, 0xff), true);
            title.text = "导出项目";
            var version = NewText(root, 400, 72, 800, 22, 13, new Color32(0x8d, 0x88, 0x7e, 0xff), false);
            version.text = "描述文件会自动写入当前编辑器版本 " + EditorVersion.Current + "。";
            error = NewText(root, 400, 100, 800, 22, 13, new Color32(0xe0, 0xb0, 0x60, 0xff), false);

            var game = new GComponent();
            game.SetXY(400, 140);
            game.SetSize(800, 144);
            root.AddChild(game);
            Label(game, 0, 0, "导出位置");
            location = Input(game, 0, 28, 640, 36, "选择要写出描述文件和数据的文件夹");
            pick = PackageAtrribute.UI_ButtonSecondary.CreateInstance();
            pick.title = "选择文件夹";
            pick.SetXY(652, 26);
            pick.SetSize(148, 40);
            game.AddChild(pick);
            Label(game, 0, 80, "模组目录");
            modDirectory = Input(game, 0, 108, 800, 36, "模组将放在这个目录下");
            gameFields = game;

            var mod = new GComponent();
            mod.SetXY(400, 140);
            mod.SetSize(800, 284);
            root.AddChild(mod);
            Label(mod, 0, 0, "模组名");
            name = Input(mod, 0, 28, 800, 36, "");
            Label(mod, 0, 80, "模组介绍");
            intro = Input(mod, 0, 108, 800, 80, "");
            Label(mod, 0, 220, "模组目录名");
            directory = Input(mod, 0, 248, 800, 36, "仅英语，例如 RunePack");
            modFields = mod;

            export = PackageAtrribute.UI_ButtonPrimary.CreateInstance();
            export.title = "导出";
            export.SetXY(1040, 26);
            export.SetSize(160, 40);
            root.AddChild(export);

            return root;
        }

        static GTextField NewText(GComponent parent, float x, float y, float w, float h, int size, Color color, bool bold)
        {
            var text = new GTextField();
            text.SetXY(x, y);
            text.SetSize(w, h);
            text.autoSize = AutoSizeType.None;
            var format = text.textFormat;
            format.size = size;
            format.color = color;
            format.bold = bold;
            text.textFormat = format;

            parent.AddChild(text);
            return text;
        }

        static void Label(GComponent parent, float x, float y, string value)
        {
            var text = NewText(parent, x, y, 240, 24, 14, new Color32(0xb7, 0xb2, 0xa8, 0xff), false);
            text.text = value;
        }

        static GTextInput Input(GComponent parent, float x, float y, float w, float h, string prompt)
        {
            var box = new GGraph();
            box.SetXY(x, y);
            box.SetSize(w, h);
            box.DrawRect(w, h, 1, new Color32(0x3a, 0x3e, 0x48, 0xff), new Color32(0x14, 0x16, 0x1b, 0xff));
            parent.AddChild(box);
            var input = new GTextInput();
            input.SetXY(x + 12, y);
            input.SetSize(w - 24, h);
            input.autoSize = AutoSizeType.None;
            var format = input.textFormat;
            format.size = 15;
            format.color = new Color32(0xe8, 0xe6, 0xe3, 0xff);
            input.textFormat = format;
            input.promptText = prompt;
            parent.AddChild(input);
            return input;
        }
    }
}
