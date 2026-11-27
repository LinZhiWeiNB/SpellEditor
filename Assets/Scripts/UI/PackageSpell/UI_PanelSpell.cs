/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_PanelSpell : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "PanelSpell";
        public UI_ComInspect m_inspect;
        public UI_ComFunc m_func;
        public UI_ComTimeline m_timeline;
        public GGraph m_vSplitL;
        public GGraph m_vSplitR;
        public GGraph m_hSplit;
        public GGraph m_previewHit;
        public PackageAtrribute.UI_ButtonSecondary m_btnResetCamera;
        public PackageAtrribute.UI_ButtonSecondary m_btnPreviewMode;
        public UI_ModeToggle m_modeToggle;
        public GGraph m_previewBorder;
        public const string URL = "ui://skl4p8n2mns01";

        public static UI_PanelSpell CreateInstance()
        {
            return (UI_PanelSpell)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_inspect = (UI_ComInspect)GetChildAt(0);
            m_func = (UI_ComFunc)GetChildAt(1);
            m_timeline = (UI_ComTimeline)GetChildAt(2);
            m_vSplitL = (GGraph)GetChildAt(3);
            m_vSplitR = (GGraph)GetChildAt(4);
            m_hSplit = (GGraph)GetChildAt(5);
            m_previewHit = (GGraph)GetChildAt(6);
            m_btnResetCamera = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(7);
            m_btnPreviewMode = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(8);
            m_modeToggle = (UI_ModeToggle)GetChildAt(9);
            m_previewBorder = (GGraph)GetChildAt(10);
        }
    }
}