using PackageMain;
using GameContentEditor;

namespace UILogic
{
    public class PanelExport : FguiPanelBase
    {
        UI_PanelExport view;
        bool _mod;

        public override void Start(object[] data)
        {
            view = (UI_PanelExport)FGUIComponent;
            _mod = ProjectSession.AuthorId != 0;
            view.m_cRole.selectedIndex = _mod ? 1 : 0;
            view.m_version.text = "数据包会自动写入当前编辑器版本 " + EditorVersion.Current + "。";
            view.m_cResult.selectedIndex = (int)UI_PanelExport.EcResult.空;
            view.m_error.text = "";
            LoadSettings();
            view.m_btnBack.onClick.Set(() => FGUIPanelManager.Instance.Back());
            view.m_btnLocation.onClick.Add(PickFolder);
            view.m_btnExport.onClick.Add(Export);
        }

        public override void OnClose()
        {
            Remember();
        }

        void PickFolder()
        {
            var folder = EditorDialog.OpenFolder("导出位置");
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            view.m_inputLocation.text = folder;
            view.m_cResult.selectedIndex = (int)UI_PanelExport.EcResult.空;
            view.m_error.text = "";
        }

        void LoadSettings()
        {
            var archive = ProjectSession.Archive;
            if (archive == null)
            {
                return;
            }

            view.m_inputLocation.text = archive.ExportLocation ?? "";
            view.m_inputModDir.text = archive.ExportModDirectory ?? "";
            view.m_inputName.text = archive.ExportName ?? "";
            view.m_inputIntro.text = archive.ExportDescription ?? "";
            view.m_inputDir.text = archive.ExportDirectoryName ?? "";
        }

        void Remember()
        {
            var archive = ProjectSession.Archive;
            if (archive == null || view == null || view.isDisposed)
            {
                return;
            }

            archive.ExportLocation = view.m_inputLocation.text ?? "";
            archive.ExportModDirectory = view.m_inputModDir.text ?? "";
            archive.ExportName = view.m_inputName.text ?? "";
            archive.ExportDescription = view.m_inputIntro.text ?? "";
            archive.ExportDirectoryName = view.m_inputDir.text ?? "";
            ProjectSession.Save();
        }

        void Export()
        {
            Remember();
            var error = _mod
                ? ProjectExport.ExportMod(view.m_inputName.text, view.m_inputIntro.text, view.m_inputDir.text)
                : ProjectExport.ExportGame(view.m_inputLocation.text, view.m_inputModDir.text);
            view.m_cResult.selectedIndex = error == null
                ? (int)UI_PanelExport.EcResult.成功
                : (int)UI_PanelExport.EcResult.警告;
            view.m_error.text = error ?? "已写出数据包。";
        }
    }
}
