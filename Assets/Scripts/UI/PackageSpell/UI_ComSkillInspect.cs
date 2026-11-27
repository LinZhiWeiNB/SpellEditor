/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace PackageSpell
{
    public partial class UI_ComSkillInspect : GComponent
    {
        public const string PackageName = "PackageSpell";
        public const string ResName = "ComSkillInspect";
        public GGraph m_cardTags;
        public GTextField m_labelTags;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddTag;
        public PackageAtrribute.UI_ButtonCheck m_castLink;
        public UI_ButtonTip m_btnCastTip;
        public GTextField m_labelMatch;
        public PackageResource.UI_ComboValue m_comboMatch;
        public GList m_tags;
        public GGraph m_cardConditions;
        public GTextField m_labelConditions;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddCondition;
        public GList m_conditions;
        public GGraph m_cardBuffs;
        public GTextField m_labelBuffs;
        public PackageAtrribute.UI_ButtonSecondary m_btnAddBuff;
        public GList m_buffs;
        public const string URL = "ui://skl4p8n2skin1";

        public static UI_ComSkillInspect CreateInstance()
        {
            return (UI_ComSkillInspect)UIPackage.CreateObject(PackageName, ResName);
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_cardTags = (GGraph)GetChildAt(0);
            m_labelTags = (GTextField)GetChildAt(1);
            m_btnAddTag = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(2);
            m_castLink = (PackageAtrribute.UI_ButtonCheck)GetChildAt(3);
            m_btnCastTip = (UI_ButtonTip)GetChildAt(4);
            m_labelMatch = (GTextField)GetChildAt(5);
            m_comboMatch = (PackageResource.UI_ComboValue)GetChildAt(6);
            m_tags = (GList)GetChildAt(7);
            m_cardConditions = (GGraph)GetChildAt(8);
            m_labelConditions = (GTextField)GetChildAt(9);
            m_btnAddCondition = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(10);
            m_conditions = (GList)GetChildAt(11);
            m_cardBuffs = (GGraph)GetChildAt(12);
            m_labelBuffs = (GTextField)GetChildAt(13);
            m_btnAddBuff = (PackageAtrribute.UI_ButtonSecondary)GetChildAt(14);
            m_buffs = (GList)GetChildAt(15);
        }
    }
}