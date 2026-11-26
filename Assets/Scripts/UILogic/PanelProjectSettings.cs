using System.IO;
using FairyGUI;
using PackageLaunch;
using PackageMain;
using RebornFrame.GameData;
using SpellEditor;
using UnityEngine;

namespace UILogic
{
    public class PanelProjectSettings : FguiPanelBase
    {
        UI_PanelProjectSettings view;
        bool _mod;
        uint _plugins;
        string _pendingFolder;

        public override void Start(object[] data)
        {
            view = (UI_PanelProjectSettings)FGUIComponent;
            _mod = ProjectSession.AuthorId != 0;
            _plugins = _mod ? ProjectSession.GameRenderPlugins() : ProjectSession.Archive.RenderPlugins;
            _pendingFolder = Path.GetDirectoryName(ProjectSession.ArchivePath) ?? "";
            view.m_cRole.selectedIndex = _mod
                ? (int)UI_PanelProjectSettings.EcRole.模组作者
                : (int)UI_PanelProjectSettings.EcRole.游戏开发者;
            view.m_inputName.text = ProjectSession.Archive.Name ?? "";
            view.m_folder.text = _pendingFolder;
            view.m_cMessage.selectedIndex = (int)UI_PanelProjectSettings.EcMessage.空;
            view.m_message.text = "";
            BindCheck(view.m_checkParticles, GameDataRenderPlugins.Particles);
            BindCheck(view.m_checkVfx, GameDataRenderPlugins.Vfx);
            BindCheck(view.m_checkSpine, GameDataRenderPlugins.Spine);
            BindCheck(view.m_checkAnimation, GameDataRenderPlugins.Animation);
            view.m_btnBack.onClick.Set(RequestClose);
            view.m_btnSave.onClick.Add(Save);
            view.m_btnMigrate.onClick.Add(PickFolder);
            if (_mod)
            {
                view.m_btnAddSource.onClick.Add(AddSource);
                view.m_sources.itemRenderer = RenderSource;
                view.m_sources.numItems = ProjectSession.Sources.Count;
            }
        }

        void BindCheck(GButton check, uint bit)
        {
            check.selected = (_plugins & bit) != 0;
            if (_mod)
            {
                return;
            }

            check.onChanged.Add(() =>
            {
                if (check.selected)
                {
                    _plugins |= bit;
                }
                else
                {
                    _plugins &= ~bit;
                }
            });
        }

        void PickFolder()
        {
            var folder = EditorDialog.OpenFolder("迁移目录");
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            _pendingFolder = folder;
            view.m_folder.text = folder;
            view.m_cMessage.selectedIndex = (int)UI_PanelProjectSettings.EcMessage.空;
            view.m_message.text = "";
        }

        void Save()
        {
            if (!Apply(out var error))
            {
                Show(error);
                return;
            }

            view.m_cMessage.selectedIndex = (int)UI_PanelProjectSettings.EcMessage.成功;
            view.m_message.text = "已保存。";
        }

        void RequestClose()
        {
            if (!Apply(out var error))
            {
                Show(error);
                return;
            }

            FGUIPanelManager.Instance.Back();
        }

        bool Apply(out string error)
        {
            var ok = ProjectSession.UpdateSettings(view.m_inputName.text, _pendingFolder, _plugins, out error);
            if (ok || error == "新存档已经写好，原来的文件还在。")
            {
                _pendingFolder = Path.GetDirectoryName(ProjectSession.ArchivePath) ?? _pendingFolder;
                view.m_folder.text = _pendingFolder;
            }

            return ok;
        }

        void Show(string error)
        {
            view.m_cMessage.selectedIndex = error == "新存档已经写好，原来的文件还在。"
                ? (int)UI_PanelProjectSettings.EcMessage.警告
                : (int)UI_PanelProjectSettings.EcMessage.错误;
            view.m_message.text = error ?? "";
        }

        void AddSource()
        {
            var path = EditorDialog.OpenDataPackage("添加数据源");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (!ProjectSession.TryAddSource(path, out var error))
            {
                Show(error);
                return;
            }

            view.m_cMessage.selectedIndex = (int)UI_PanelProjectSettings.EcMessage.空;
            view.m_message.text = "";
            view.m_sources.numItems = ProjectSession.Sources.Count;
        }

        void RenderSource(int index, GObject item)
        {
            var source = ProjectSession.Sources[index];
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

            var path = source.AbsolutePath;
            row.m_remove.onClick.Set(() => RemoveSource(path));
        }

        void RemoveSource(string path)
        {
            if (!ProjectSession.TryRemoveSource(path, out var error))
            {
                Show(error);
                return;
            }

            view.m_cMessage.selectedIndex = (int)UI_PanelProjectSettings.EcMessage.空;
            view.m_message.text = "";
            view.m_sources.numItems = ProjectSession.Sources.Count;
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
    }
}
