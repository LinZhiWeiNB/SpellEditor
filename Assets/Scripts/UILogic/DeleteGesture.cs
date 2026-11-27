using System;
using System.Collections.Generic;
using FairyGUI;
using UnityEngine;

namespace UILogic
{
    /// <summary>
    /// 当前页的删除键。Windows 用 Delete，Mac 用 Command+退格，Mac 键盘上的 Delete 同样算。
    /// 输入框有焦点时不触发。只有最上面、并且正在显示的页面会收到。
    /// </summary>
    public static class DeleteGesture
    {
        sealed class Binding
        {
            public object Owner;
            public Action Run;
        }

        static readonly List<Binding> Stack = new List<Binding>();
        static readonly EventCallback1 OnKey = Handle;

        public static void Bind(object owner, Action run)
        {
            if (owner == null || run == null)
                return;
            Unbind(owner);
            if (Stack.Count == 0)
                Stage.inst.onKeyDown.Add(OnKey);
            Stack.Add(new Binding { Owner = owner, Run = run });
        }

        public static void Unbind(object owner)
        {
            if (owner == null)
                return;
            for (var i = Stack.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(Stack[i].Owner, owner))
                    Stack.RemoveAt(i);
            }

            if (Stack.Count == 0)
                Stage.inst.onKeyDown.Remove(OnKey);
        }

        static void Handle(EventContext context)
        {
            var input = context.inputEvent;
            if (input == null || !Is(input))
                return;
            if (Stage.inst.focus is GTextInput)
                return;
            if (Stack.Count == 0)
                return;
            var top = Stack[Stack.Count - 1];
            if (!ReferenceEquals(FGUIPanelManager.Instance.GetCurrentPanel(), top.Owner))
                return;
            top.Run();
            context.StopPropagation();
        }

        public static bool Is(InputEvent input)
        {
            if (input == null)
                return false;
            if (input.keyCode == KeyCode.Delete)
                return true;
            var mac = Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.OSXPlayer;
            return mac && input.keyCode == KeyCode.Backspace && input.command;
        }
    }
}
