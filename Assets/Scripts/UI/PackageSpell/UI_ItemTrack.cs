/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ItemTrack : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ItemTrack";
        public GTextField m_title;
        public GList m_keys;
        public const string URL = "ui://skl4p8n2mni05";

        public static UI_ItemTrack CreateInstance()
        {
            return (UI_ItemTrack)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChildAt(0);
            m_keys = (GList)GetChildAt(1);
        }
    }
}