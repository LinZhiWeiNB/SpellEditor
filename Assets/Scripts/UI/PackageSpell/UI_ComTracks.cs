/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ComTracks : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ComTracks";
        public GList m_tracks;
        public UI_ComKeys m_keys;
        public const string URL = "ui://skl4p8n2s0uwe";

        public static UI_ComTracks CreateInstance()
        {
            return (UI_ComTracks)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_tracks = (GList)GetChildAt(0);
            m_keys = (UI_ComKeys)GetChildAt(1);
        }
    }
}