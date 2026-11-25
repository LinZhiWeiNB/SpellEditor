"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.genCode = void 0;
const csharp_1 = require("csharp");
const CodeWriter_1 = require("./CodeWriter");
function genCode(handler) {
    let settings = handler.project.GetSettings("Publish").codeGeneration;
    let codePkgName = handler.ToFilename(handler.pkg.name); //convert chinese to pinyin, remove special chars etc.
    let exportCodePath = handler.exportCodePath + '/' + codePkgName;
    let namespaceName = codePkgName;
    let isMonoGame = handler.project.type == csharp_1.FairyEditor.ProjectType.MonoGame;
    if (settings.packageName)
        namespaceName = settings.packageName + '.' + namespaceName;
    //CollectClasses(stripeMemeber, stripeClass, fguiNamespace)
    let classes = handler.CollectClasses(settings.ignoreNoname, false, null);
    handler.SetupCodeFolder(exportCodePath, "cs"); //check if target folder exists, and delete old files
    let getMemberByName = settings.getMemberByName;
    let classCnt = classes.Count;

    let writer = new CodeWriter_1.default();
    for (let i = 0; i < classCnt; i++) {
        let classInfo = classes.get_Item(i);
        let members = classInfo.members;
        writer.reset();
        writer.writeln('using FairyGUI;');
        writer.writeln('using FairyGUI.Utils;');
        writer.writeln();
        writer.writeln('namespace %s', namespaceName);
        writer.startBlock();
        writer.writeln('public partial class %s : %s', classInfo.className, classInfo.superClassName);
        writer.startBlock();

        writer.writeln('public const string PackageName = "%s";',handler.pkg.name);
        writer.writeln('public const string ResName = "%s";',classInfo.resName);
        
        let memberCnt = members.Count;
        for (let j = 0; j < memberCnt; j++) {
            let memberInfo = members.get_Item(j);
            if (memberInfo.res != null && memberInfo.res.owner.name != handler.pkg.name) {
                writer.writeln('public %s.%s%s %s;', memberInfo.res.owner, settings.classNamePrefix, memberInfo.res.name, memberInfo.varName);
            }
            else {
                writer.writeln('public %s %s;', memberInfo.type, memberInfo.varName);
            }
            if (memberInfo.group == 1) {
                // 如果是控制器生成枚举
                var obj = csharp_1.FairyEditor.FObjectFactory.CreateObject(classInfo.res).topmost;
                var fController = obj.GetController(memberInfo.name);
                var pages = fController.GetPages();
                if (pages.get_Item(0).name != "") {
                    writer.writeln();
                    writer.writeln('public enum E%s', memberInfo.name);
                    writer.startBlock();
                    for (var num = 0; num < pages.Count; num++) {
                        var fControllerPage = pages.get_Item(num);
                        writer.writeln('/// <summary>');
                        writer.writeln('/// %s', fControllerPage.remark);
                        writer.writeln('/// <summary>');
                        writer.writeln('%s = %s,', fControllerPage.name, num);
                    }
                    writer.endBlock();
                    writer.writeln();
                    writer.writeln('public E%s GetSelectedIndexE%s()', memberInfo.name, memberInfo.name);
                    writer.startBlock();
                    writer.writeln('return (E%s)%s.selectedIndex;', memberInfo.name, memberInfo.varName);
                    writer.endBlock();
                    writer.writeln();
                }
            }
        }
        writer.writeln('public const string URL = "ui://%s%s";', handler.pkg.id, classInfo.resId);
        writer.writeln();
        writer.writeln('public static %s CreateInstance()', classInfo.className);
        writer.startBlock();
        writer.writeln('return (%s)UIPackage.CreateObject(PackageName, ResName);', classInfo.className);
        writer.endBlock();
        writer.writeln();
        if (isMonoGame) {
            writer.writeln("protected override void OnConstruct()");
            writer.startBlock();
        }
        else {
            writer.writeln('public override void ConstructFromXML(XML xml)');
            writer.startBlock();
            writer.writeln('base.ConstructFromXML(xml);');
            writer.writeln();
        }
        for (let j = 0; j < memberCnt; j++) {
            let memberInfo = members.get_Item(j);
            if (memberInfo.group == 0) {
                if (memberInfo.res != null && memberInfo.res.owner.name != handler.pkg.name) {
                    if (getMemberByName) {
                        //writer.writeln('%s = (%s)GetChild("%s");', memberInfo.varName, memberInfo.type, memberInfo.name);
                        writer.writeln('%s = (%s.%s%s)GetChild("%s");', memberInfo.varName, memberInfo.res.owner, settings.classNamePrefix, memberInfo.res.name, memberInfo.name);
                    }
                    else {
                        //writer.writeln('%s = (%s)GetChildAt(%s);', memberInfo.varName, memberInfo.type, memberInfo.index);
                        writer.writeln('%s = (%s.%s%s)GetChildAt(%s);', memberInfo.varName, memberInfo.res.owner, settings.classNamePrefix, memberInfo.res.name, memberInfo.index);
                    }
                }
                else {
                    if (getMemberByName)
                        writer.writeln('%s = (%s)GetChild("%s");', memberInfo.varName, memberInfo.type, memberInfo.name);
                    else
                        writer.writeln('%s = (%s)GetChildAt(%s);', memberInfo.varName, memberInfo.type, memberInfo.index);
                }
            }
            else if (memberInfo.group == 1) {
                if (getMemberByName)
                    writer.writeln('%s = GetController("%s");', memberInfo.varName, memberInfo.name);
                else
                    writer.writeln('%s = GetControllerAt(%s);', memberInfo.varName, memberInfo.index);
            }
            else {
                if (getMemberByName)
                    writer.writeln('%s = GetTransition("%s");', memberInfo.varName, memberInfo.name);
                else
                    writer.writeln('%s = GetTransitionAt(%s);', memberInfo.varName, memberInfo.index);
            }
        }
        writer.endBlock();
        writer.endBlock(); //class
        writer.endBlock(); //namepsace
        writer.save(exportCodePath + '/' + classInfo.className + '.cs');
    }
    writer.reset();
    let binderName = codePkgName + 'Binder';
    writer.writeln('using FairyGUI;');
    writer.writeln();
    writer.writeln('namespace %s', namespaceName);
    writer.startBlock();
    writer.writeln('public class %s', binderName);
    writer.startBlock();
    writer.writeln('public static void BindAll()');
    writer.startBlock();
    for (let i = 0; i < classCnt; i++) {
        let classInfo = classes.get_Item(i);
        writer.writeln('UIObjectFactory.SetPackageItemExtension(%s.URL, typeof(%s));', classInfo.className, classInfo.className);
    }
    writer.endBlock(); //bindall
    writer.endBlock(); //class
    writer.endBlock(); //namespace
    writer.save(exportCodePath + '/' + binderName + '.cs');
}
exports.genCode = genCode;
