using System.Collections.Generic;
using FairyGUI;
using PackageLaunch;
using SpellEditor;
using UnityEngine;

namespace UILogic
{
    public class PanelNewProject : FguiPanelBase
    {
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
            view.m_btnBack.onClick.Add(() => FGUIPanelManager.Instance.Back());
            view.m_btnFolder.onClick.Add(PickFolder);
            view.m_btnAddSource.onClick.Add(AddSource);
            view.m_btnCreate.onClick.Add(Create);
            view.m_roles.selectedIndex = 0;
            view.m_sources.itemRenderer = RenderSource;
            view.m_sources.numItems = 0;
        }

        void RenderSource(int index, GObject item)
        {
            var source = _sources[index];
            var row = (UI_ItemSource)item;
            row.m_title.text = source.Title;
            row.m_kind.text = ProjectSession.KindLabel(source);
            row.m_path.onClick.Clear();
            var warned = source.VersionMismatch || !string.IsNullOrEmpty(source.ParseWarning);
            row.m_cState.selectedIndex = source.VersionMismatch
                ? (int)UI_ItemSource.EcState.版本不符
                : warned
                    ? (int)UI_ItemSource.EcState.警告
                    : (int)UI_ItemSource.EcState.正常;
            if (source.VersionMismatch)
            {
                row.m_kind.text += " · 版本不符";
            }

            row.m_path.text = warned ? WarningText(source) : source.AbsolutePath;
            if (source.VersionMismatch)
            {
                var version = source.EditorVersion;
                row.m_path.onClick.Set(() => Application.OpenURL(EditorVersion.ReleasePage(version)));
            }

            row.height = Mathf.Max(52, row.m_path.y + row.m_path.height + 8);

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
            ShowFolderHint(_folderHint, false);
        }

        void AddSource()
        {
            var path = EditorDialog.OpenDataPackage("添加数据源");
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
                view.m_sourceHint.text = "请选择数据包。";
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

        void ShowFolderHint(string text, bool error)
        {
            view.m_folderHint.text = text;
            view.m_cFolder.selectedIndex = error
                ? (int)UI_WindowNewProject.EcFolder.错误
                : (int)UI_WindowNewProject.EcFolder.说明;
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
                    ShowFolderHint(error, true);
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
