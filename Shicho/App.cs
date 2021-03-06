﻿extern alias Cities;

using Shicho.Core;

using ICities;
using ColossalFramework.UI;
using UnityEngine;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Shicho
{
    class App
        : MonoBehaviour
        , IDisposable
    {
        public void Awake()
        {
            isDive_ = false;
            cfgToolObj_ = new GameObject($"{gameObject.name}.ConfigTool");
            cfgToolObj_.transform.parent = gameObject.transform;
            cfgToolObj_.SetActive(true);

            supportToolObj_ = new GameObject($"{gameObject.name}.SupportTool");
            supportToolObj_.transform.parent = gameObject.transform;

            diveObj_ = new GameObject($"{gameObject.name}.Dive");
            diveObj_.transform.parent = gameObject.transform;
            diveObj_.SetActive(false);

            try {
                // Log.Info("initializing...");
                R = new ColossalFramework.Math.Randomizer(GetDeviceSeedUL());

                // stir up
                for (var i = 0; i < 123; ++i) {
                    R.ULong64();
                }

                InitGUI();

                // Log.Info("initialized");

            } catch (Exception e) {
                Log.Error($"failed to initialize: '{e}'");
            }
        }

        private bool isDive_;
        public bool IsDive { get => isDive_; }

        public void Update()
        {
            var keyMod = SInput.GetMod();
            if (SInput.HasOnlyKeyDown(keyMod, App.Config.diveKey)) {
                if (dive_ != null) {
                    isDive_ = !isDive_;

                    var cc = ToolsModifierControl.cameraController;
                    if (isDive_) {
                        cc.m_freeCameraInertia = 0;
                        cc.m_freeCamera = true;

                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.None;

                        dive_.gameObject.SetActive(true);

                    } else {
                        // must come first
                        dive_.gameObject.SetActive(false);
                        // supportTool_.gameObject.SetActive(!isDive_);

                        Cursor.visible = true;
                        cc.m_freeCamera = false;
                    }
                }
            }
        }

        public void InitGameMode()
        {
            Log.Debug($"loading fonts...");
            GUI.FontStore.Instance.Load();

            LoadLevelData();

            supportTool_ = supportToolObj_.AddComponent<Tool.SupportTool>();
            dive_ = diveObj_.AddComponent<Dive.DiveController>();
        }

        private void InitGUI()
        {
            cfgTool_ = cfgToolObj_.AddComponent<Tool.ConfigTool>();
        }

        private void InitPhysics()
        {

        }

        private void LoadLevelData()
        {
            Log.Debug("loading prefabs...");
            pmgr_ = new PrefabManager();
            //pmgr_.FetchAll();

            Log.Debug("loading props...");
            pcon_ = new PropManager();
            //pcon_.Fetch();

            Log.Debug("loading traffic...");
            tcon_ = new TrafficController();
            //tcon_.Fetch();

            Log.Debug("initializing flow generator...");
            fgen_ = new FlowGenerator(ref pmgr_, ref tcon_);
            // fgen_.AddFactory(typeof(Cities::Citizen));
        }

        public void UnloadLevelData()
        {
            if (pmgr_ != null) pmgr_.Dispose();
            if (tcon_ != null) tcon_.Dispose();
            if (fgen_ != null) fgen_.Dispose();
        }

        private void UnloadAllData()
        {
            UnloadLevelData();
            citizens_.Clear();
            GUI.FontStore.Instance.Unload();
            GUI.FontStore.Deinit();
        }

        public void SetFlow(
            Cities::ItemClass.Service service,
            uint flow
        ) {
            if (service != Cities::ItemClass.Service.Citizen) {
                throw new NotImplementedException("service != Cities::ItemClass.Service.Citizen");
            }

            for (uint i = 0; i < flow; ++i) {
                var c = new Game.Citizen(20 + i, 3);
                //Log.Info($"created: {c}");

                citizens_.Add(c.ID, c);
            }
        }

        internal static int GetDeviceSeedI()
        {
            return 1145144545;
        }

        internal static ulong GetDeviceSeedUL()
        {
            return 1145144545191912345;
        }

        public void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            //Log.Debug("Dispose()");

            try {
                GameObject.DestroyImmediate(diveObj_);
                diveObj_ = null;

                GameObject.DestroyImmediate(cfgToolObj_);
                cfgToolObj_ = null;

                GameObject.DestroyImmediate(supportToolObj_);
                supportToolObj_ = null;

                foreach (var c in GetComponentsInChildren<MonoBehaviour>()) {
                    //Log.Debug($"\"{c.name}\": [{c.gameObject}] {c.tag}, {c}");
                    Destroy(c.gameObject);
                }
                UnityEngine.Resources.UnloadUnusedAssets();


            } catch (Exception e) {
                Log.Error($"could not dispose object: {e}");

            } finally {
                // at last, delete all data
                UnloadAllData();
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            cfgTool_.Populate(helper);
        }

        public void SaveConfig()
        {
            if (supportTool_) {
                Config.GUI.SupportTool = supportTool_.Config.Clone() as GUI.TabbedWindowConfig;
            }
            Config.Save(ConfigPath);
        }

        public static App Instance { get => Bootstrapper.AppInstance; }

        // TODO: cache this (GetComponent should be slow)
        public Light MainLight {
            get => GameObject.FindWithTag("MainLight")?.GetComponent<Light>();
        }

        public const string ConfigPath = Mod.ModInfo.ID + ".xml";
        public static Mod.Config Config { get => Bootstrapper.Instance.Config; }

        private Tool.SupportTool supportTool_ = null;

        private GameObject cfgToolObj_ = null, supportToolObj_ = null, diveObj_ = null;
        private Dive.DiveController dive_ = null;
        public Tool.ConfigTool cfgTool_ = null;

        internal ColossalFramework.Math.Randomizer R;

        private PrefabManager pmgr_;

        private PropManager pcon_;
        private TrafficController tcon_;

        private FlowGenerator fgen_;
        private Dictionary<Game.CitizenID, Game.Citizen> citizens_ = new Dictionary<Game.CitizenID, Game.Citizen>();
    }
}
