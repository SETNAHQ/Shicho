﻿using Shicho.Core;

using ColossalFramework.UI;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;


namespace Shicho.Tool
{
    using Helper = GUI.Helper;
    using GUI.Extension;

    interface ITool : GUI.IConfigurableComponent<GUI.TabbedWindowConfig>
    {
        GUI.TabbedWindowConfig ConfigProxy { get; }
        GUI.TabTemplate[] Tabs { get; }

        void SetVisible(bool flag);
    }

    abstract class ToolBase
        : MonoBehaviour
        , ITool
    {
        public virtual void Awake()
        {
            win_ = UIView.GetAView().AddUIComponent(typeof(GUI.Window)) as GUI.Window;
        }

        public virtual void Start()
        {
            win_.Config = ConfigProxy;
            Config.Rect.RelocateIn(Helper.ScreenRectAsUI);

            win_.position = Config.Rect.position;
            win_.size = Config.Rect.size;

            //Log.Debug($"Window: {win_.position}, {win_.size}");

            win_.eventClosed += (c, param) => {
                SetVisible(false);
            };
            win_.eventSizeChanged += (c, size) => {
                Config.Rect.size = size;
            };
            win_.eventPositionChanged += (c, pos) => {
                Config.Rect.position = pos;
            };

            if (Config.SelectedTabIndex > Tabs.Length) {
                Config.SelectedTabIndex = 0;
            }

            win_.Content.clipChildren = true;
            win_.Content.SetAutoLayout(LayoutDirection.Vertical);

            {
                tabs_ = win_.Content.AddUIComponent<UITabstrip>();
                //tabs_.autoSize = false;
                tabs_.clipChildren = true;
                tabs_.width = win_.Content.width;
                tabs_.backgroundSprite = "GenericTabDisabled";
                tabs_.color = Helper.RGB(20, 20, 40);
                tabs_.zOrder = 2;
                //tabs_.clipChildren = true;
                //Log.Debug($"Tab: {tabs_.position}, {tabs_.size}");

                //tabs_.relativePosition = Vector2.zero;
                tabs_.tabPages = win_.Content.AddUIComponent<UITabContainer>();
                var container = tabs_.tabContainer;

                container.relativePosition = Vector2.zero;
                container.width = tabs_.width;
                container.clipChildren = true;
                container.height = win_.Content.parent.height - tabs_.height;

                foreach (var v in Tabs.Select((tab, i) => new {tab, i})) {
                    {
                        var btn = tabs_.AddTab(v.tab.name, true);
                        tabs_.selectedIndex = v.i; // important for setting current target obj

                        btn.tabIndex = v.i; // misc value
                        btn.relativePosition = Vector2.zero;
                        btn.text = "";
                        btn.tooltip = v.tab.name;
                        v.tab.icons.AssignTo(ref btn);

                        btn.normalBgSprite = "GenericTab";
                        btn.hoveredBgSprite = "GenericTabHovered";
                        btn.pressedBgSprite = "GenericTabPressed";
                        btn.focusedBgSprite = "GenericTabFocused";

                        //btn.spritePadding = Helper.Padding(4, 22);
                        btn.width = btn.height = 32;
                        tabs_.height = Math.Max(tabs_.height, btn.height);
                    }

                    var page = container.components[v.i] as UIPanel;
                    //Log.Debug($"{page.name}");

                    {
                        //var bg = page.AddUIComponent<UITiledSprite>();
                        //bg.spriteName = "MenuPanel2";
                        //bg.size = page.size;
                        //bg.SendToBack();
                        //bg.tileScale = new Vector2(
                        //    bg.width / bg.spriteInfo.width,
                        //    bg.height / bg.spriteInfo.height
                        //);
                    }
                    //page.clipChildren = true;
                    tabPages_.Add(v.tab.name, page);

                    //page.relativePosition = Vector2.zero;
                    page.isVisible = false;
                    page.autoSize = false;
                    page.width = tabs_.width;
                    page.height = win_.Content.height - tabs_.height;

                    // auto defocus
                    {
                        page.canFocus = true;
                        page.eventClicked += (c, p) => {
                            //Log.Debug($"source: {c}, {p.source}");

                            if (p.source == c) {
                                page.Focus();
                            }
                        };
                    }

                    //Log.Debug($"{page.position}, {page.size}");

                    Window.Content.eventSizeChanged += (c, size) => {
                        page.width = size.x;
                        page.height = c.height - tabs_.height;
                    };

                    // page.relativePosition = Vector2.zero;
                    page.SetAutoLayout(LayoutDirection.Vertical);

                    //var bg = page.AddUIComponent<UITiledSprite>();
                    //bg.spriteName = "InfoPanelBack";
                    //bg.width = 200;
                    //bg.height = 400;
                    //bg.relativePosition = Vector2.zero;

                    //var desc = page.AddUIComponent<UILabel>();
                    //desc.text = $"'{v.tab.name}' here!!!";
                    //desc.zOrder = 1;

                    // Log.Debug($"tc: {tabs_.size} {win_.Content.size} | {page.position} {page.size}");
                }

                //Log.Debug($"tab.size: {tabs_.size}");

                //Log.Debug($"si: {Config.SelectedTabIndex}");
                tabs_.startSelectedIndex = Config.SelectedTabIndex;
                tabs_.selectedIndex = Config.SelectedTabIndex;
                //tabs_.tabIndex = Config.SelectedTabIndex;

                tabs_.eventSelectedIndexChanged += (c, i) => {
                    //Log.Info($"tab: {c.tabIndex}, {i}");
                    Config.SelectedTabIndex = i;
                };
            }
        }

        protected void SelectTab(int i)
        {
            tabs_.selectedIndex = i;
            tabs_.tabPages.components[i].Show();
        }

        public virtual void OnDestroy()
        {
            // very important
            // contains root gameObjct for all Colossal UI components
            Helper.DeepDestroy(win_);
            Destroy(win_);
            win_ = null;
            UnityEngine.Resources.UnloadUnusedAssets();
        }

        public void SetVisible(bool flag)
        {
            Config.IsVisible = flag;

            if (flag) {
                Window.Show();

            } else {
                Window.Hide();
            }
        }

        public string Title {
            get => win_.Title;
            protected set => win_.Title = value;
        }

        private GUI.Window win_;
        protected GUI.Window Window { get => win_; }
        public GUI.TabbedWindowConfig Config { get => win_.Config; set => win_.Config = value; }
        public abstract GUI.TabbedWindowConfig ConfigProxy { get; }

        public GUI.TabTemplate[] Tabs { get; protected set; }
        private UITabstrip tabs_;

        private Dictionary<string, UIPanel> tabPages_ = new Dictionary<string, UIPanel>();
        public UIPanel TabPage(string name) => tabPages_[name];
    }
}
