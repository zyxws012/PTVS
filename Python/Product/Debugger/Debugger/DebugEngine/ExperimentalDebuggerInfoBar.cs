﻿// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using Microsoft.PythonTools.Interpreter;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.PythonTools.Debugger.DebugEngine {
    internal class ExperimentalDebuggerInfoBar : IVsInfoBarUIEvents {
        private uint _adviseCookie;
        private IVsInfoBarUIElement _infoBar;

        public static ExperimentalDebuggerInfoBar Instance { get; } = new ExperimentalDebuggerInfoBar();

        public void OnClosed(IVsInfoBarUIElement infoBarUIElement) {
            infoBarUIElement.Unadvise(_adviseCookie);
            _infoBar = null;
        }

        public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem) {
            ((Action)actionItem.ActionContext)();
        }

        public void AddInfoBar() {
            if (ExperimentalOptions.GetUseVsCodeDebugger() || !ExperimentalOptions.GetPromptVsCodeDebuggerInfoBar() || _infoBar != null) {
                return;
            }

            var shell = (IVsShell)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out object infoBarHostObj);

            var infoBarHost = (IVsInfoBarHost)infoBarHostObj;
            var infoBarFactory = (IVsInfoBarUIFactory)ServiceProvider.GlobalProvider.GetService(typeof(SVsInfoBarUIFactory));

            Action enableExperimentalDebugger = () => {
                ExperimentalOptions.UseVsCodeDebugger = true;
                ExperimentalOptions.PromptVsCodeDebuggerInfoBar = false;
                _infoBar.Close();
            };

            Action dontShowAgainDebugger = () => {
                ExperimentalOptions.PromptVsCodeDebuggerInfoBar = false;
                _infoBar.Close();
            };

            var messages = new List<IVsInfoBarTextSpan>();
            var actions = new List<InfoBarActionItem>();

            messages.Add(new InfoBarTextSpan(Strings.ExpDebuggerInfoBarMessage));
            actions.Add(new InfoBarButton(Strings.ExpDebuggerInfoBarEnableButtonText, enableExperimentalDebugger));
            actions.Add(new InfoBarButton(Strings.ExpDebuggerInfoBarDontShowAgainButtonText, dontShowAgainDebugger));

            var infoBarModel = new InfoBarModel(messages, actions, KnownMonikers.StatusInformation, isCloseButtonVisible: true);
            _infoBar = infoBarFactory.CreateInfoBar(infoBarModel);
            infoBarHost.AddInfoBar(_infoBar);

            _infoBar.Advise(this, out uint cookie);
            _adviseCookie = cookie;
        }
    }
}
