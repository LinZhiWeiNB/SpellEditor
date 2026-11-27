/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ComTimeline : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ComTimeline";
        public GGraph m_timePane;
        public UI_ComMarks m_marks;
        public UI_ComTracks m_tracks;
        public UI_ComProgress m_progress;
        public UI_ScrollBarH m_scrollH;
        public UI_ScrollBarV m_scrollV;
        public const string URL = "ui://skl4p8n2time01";

        public static UI_ComTimeline CreateInstance()
        {
            return (UI_ComTimeline)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_timePane = (GGraph)GetChildAt(0);
            m_marks = (UI_ComMarks)GetChildAt(1);
            m_tracks = (UI_ComTracks)GetChildAt(2);
            m_progress = (UI_ComProgress)GetChildAt(3);
            m_scrollH = (UI_ScrollBarH)GetChildAt(4);
            m_scrollV = (UI_ScrollBarV)GetChildAt(5);
        }
    }
}