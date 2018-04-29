﻿using Shicho.Core;

using ColossalFramework.UI;

using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Shicho.Tool
{
    using Shicho.GUI;
    using UInput = UnityEngine.Input;

    class SupportTool : ToolBase
    {
        public override void Awake()
        {
            base.Awake();
            Log.Debug($"{Config}, {Config != null}");
            if (Config.TabIndex > Tabs.Length) {
                Config.TabIndex = 0;
            }
        }

        public void Start()
        {
            Window.Show();
        }

        public void Update()
        {
            var keyMod = Input.KeyMod.None;

            if (UInput.GetKey(KeyCode.LeftControl) || UInput.GetKey(KeyCode.RightControl)) {
                keyMod |= Input.KeyMod.Ctrl;
            }
            if (UInput.GetKey(KeyCode.LeftAlt) || UInput.GetKey(KeyCode.RightAlt)) {
                keyMod |= Input.KeyMod.Alt;
            }
            if (UInput.GetKey(KeyCode.LeftShift) || UInput.GetKey(KeyCode.RightShift)) {
                keyMod |= Input.KeyMod.Shift;
            }

            if (UInput.GetKeyDown(App.Config.mainKey.Code)) {
                if ((App.Config.mainKey.Mod & keyMod) == App.Config.mainKey.Mod) {
                    Config.IsVisible = !Config.IsVisible;
                    SetVisible(Config.IsVisible);
                }
            }
        }

        class ShadowBiasMethod
        {
        }

        private delegate void TabContentFunc();
        readonly string[] Tabs = new[] {"Graphics", "Log"};

        const float HUnit = 24f;
        const int sliderWidth = 200;

        static class Defaults
        {
            public const int FontSize = 16;

            public const int TitleFontSize = 16;
            public const int TabFontSize = 16;

            public const float TabHeight = TabFontSize * 2.2f;
            public const float Top = TitleFontSize * 1.1f + TabHeight;
        }

        public static readonly Rect DefaultRect = new Rect(
            0f, 0f, 280f, 800f
        );

        public override TabbedWindowConfig ConfigProxy {
            get => App.Config.GUI.SupportTool;
            set => App.Config.GUI.SupportTool = value;
        }
    }
}
