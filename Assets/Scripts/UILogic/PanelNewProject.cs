using System.Collections.Generic;
using FairyGUI;
using PackageLaunch;
using SpellEditor;
using UnityEngine;

namespace UILogic
{
    public class PanelNewProject : FguiPanelBase
    {
        static readonly string[] RoleTitles = { "游戏开发者", "模组作者" };
        static readonly string[] RoleDescs =
        {
            "编写游戏本体。不用填写作者 ID，编辑器会为这份存档保存游戏开发者身份。",
            "在游戏数据之上编写自己的内容。需要一个作者 ID，并选择要一起载入的数据源。",
        };

        UI_WindowNewProject view;
        string _folderHint;
        string _sourceHint;
        readonly List<LoadedSource> _sources = new List<LoadedSource>();

        public override void Start(object[] data)
        {
            view = (UI_WindowNewProject)FGUIComponent;
            _folderHint = view.m_folderHint.text;
            _sourceHint = view.m_sourceHint.text;
            view.m_inputName.text = "";
            view.m_inputFolder.text = "";
            view.m_inputAuthor.text = "";
            view.m_inputAuthor.restrict = "[0-9]";
            view.m_sourceHint.text = "选择描述文件来添加。属于游戏的显示为游戏数据源，其余显示为模组数据源。模组需要一份游戏数据源。";
            _sourceHint = view.m_sourceHint.text;
            view.m_btnBack.onClick.Add(() => FGUIPanelManager.Instance.Back());
            view.m_btnFolder.onClick.Add(PickFolder);
            view.m_btnAddSource.onClick.Add(AddSource);
            view.m_btnCreate.onClick.Add(Create);
            view.m_roles.itemRenderer = RenderRole;
            view.m_roles.numItems = RoleTitles.Length;
            view.m_roles.selectedIndex = 0;
            view.m_sources.itemRenderer = RenderSource;
            view.m_sources.numItems = 0;
        }

        void RenderRole(int index, GObject item)
        {
            var row = (UI_ItemRole)item;
            row.title = RoleTitles[index];
            row.m_desc.text = RoleDescs[index];
        }

        void RenderSource(int index, GObject item)
        {
            var source = _sources[index];
            var row = (UI_ItemSource)item;
            row.m_title.text = source.Title;
            row.m_kind.text = ProjectSession.KindLabel(source);
            row.m_kind.color = new Color32(0xb7, 0xb2, 0xa8, 0xff);
            row.m_path.onClick.Clear();
            row.m_path.touchable = false;
            row.m_path.singleLine = true;
            row.m_path.autoSize = AutoSizeType.None;
            row.m_path.color = new Color32(0x8d, 0x88, 0x7e, 0xff);
            row.m_path.text = source.AbsolutePath;
            row.height = 52;
            if (source.VersionMismatch || !string.IsNullOrEmpty(source.ParseWarning))
            {
                row.m_kind.color = new Color32(0xe0, 0xb0, 0x60, 0xff);
                if (source.VersionMismatch)
                {
                    row.m_kind.text += " · 版本不符";
                }

                row.m_path.touchable = source.VersionMismatch;
                row.m_path.singleLine = false;
                row.m_path.autoSize = AutoSizeType.Height;
                row.m_path.color = new Color32(0xe0, 0xb0, 0x60, 0xff);
                row.m_path.text = WarningText(source);
                if (source.VersionMismatch)
                {
                    var version = source.EditorVersion;
                    row.m_path.onClick.Set(() => Application.OpenURL(EditorVersion.ReleasePage(version)));
                }

                row.height = Mathf.Max(52, row.m_path.y + row.m_path.height + 8);
            }

            row.m_remove.data = source.AbsolutePath;
            row.m_remove.onClick.Set(RemoveSource);
        }

        void PickFolder()
        {
            var folder = EditorDialog.OpenFolder("选择文件夹");
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            view.m_inputFolder.text = folder;
            view.m_folderHint.text = _folderHint;
        }

        void AddSource()
        {
            var path = EditorDialog.OpenDescriptor("添加数据源");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            for (int i = 0; i < _sources.Count; i++)
            {
                if (_sources[i].AbsolutePath == path)
                {
                    return;
                }
            }

            if (!ProjectSession.TryLoadSource(path, path, out var source))
            {
                view.m_sourceHint.text = "请选择描述文件，而不是数据二进制。";
                return;
            }

            _sources.Add(source);
            view.m_sourceHint.text = _sourceHint;
            view.m_sources.numItems = _sources.Count;
        }

        void RemoveSource(EventContext context)
        {
            var path = (string)((GObject)context.sender).data;
            for (int i = _sources.Count - 1; i >= 0; i--)
            {
                if (_sources[i].AbsolutePath == path)
                {
                    _sources.RemoveAt(i);
                }
            }

            view.m_sources.numItems = _sources.Count;
        }

        static string WarningText(LoadedSource source)
        {
            var text = "";
            if (source.VersionMismatch)
            {
                var found = string.IsNullOrEmpty(source.EditorVersion) ? "未知" : source.EditorVersion;
                text = "编辑器版本 " + found + " 与当前 " + EditorVersion.Current
                       + " 不一致，可能无法正常解析。建议下载对应版本的编辑器。";
            }

            if (!string.IsNullOrEmpty(source.ParseWarning))
            {
                if (text.Length > 0)
                {
                    text += " ";
                }

                text += source.ParseWarning;
            }

            return text;
        }

        void Create()
        {
            var authorId = 0u;
            if (view.m_cRole.selectedIndex == (int)UI_WindowNewProject.EcRole.模组作者)
            {
                if (!uint.TryParse(view.m_inputAuthor.text, out authorId) || authorId == 0)
                {
                    view.m_sourceHint.text = "模组作者需要填写一个非 0 的作者 ID。";
                    return;
                }
            }

            var paths = new List<string>(_sources.Count);
            for (int i = 0; i < _sources.Count; i++)
            {
                paths.Add(_sources[i].AbsolutePath);
            }

            var created = ProjectSession.Create(
                view.m_inputName.text,
                view.m_inputFolder.text,
                authorId,
                paths,
                out var error);
            if (created == null)
            {
                if (authorId != 0 && error == "模组需要一份游戏数据源。")
                {
                    view.m_sourceHint.text = error;
                }
                else
                {
                    view.m_folderHint.text = error;
                }

                return;
            }

            FGUIPanelManager.Instance.ShowPanel<PanelMain>(
                PackageMain.UI_WindowMain.PackageName,
                PackageMain.UI_WindowMain.ResName,
                showType: ShowTypeEnum.CloseCurrent);
        }
    }
}
