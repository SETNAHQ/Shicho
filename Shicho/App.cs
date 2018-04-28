﻿extern alias Cities;

using Shicho.Core;

using ICities;
using UnityEngine;

using System;
using System.Collections.Generic;


namespace Shicho
{
    internal class App
        : MonoBehaviour
        , IDisposable
    {
        private static App instance_;
        public static App Instance { get => instance_; }
        internal static void SetInstance(App a) { instance_ = a; }

        public App()
        {
            try {
                Log.Info("initializing...");

                cfgTool_ = GameObject.Find(Mod.ModInfo.ID).AddComponent<Mod.ConfigTool>();
                R = new ColossalFramework.Math.Randomizer(GetDeviceSeed());

                // stir up
                for (var i = 0; i < 123; ++i) {
                    R.ULong64();
                }

                Log.Debug("loading prefabs...");
                pmgr_ = new PrefabManager();
                pmgr_.FetchAll();

                Log.Info("initialized!");

                var cams = GameObject.FindObjectsOfType<UnityEngine.Camera>();
                foreach (var cam in cams) {
                    //if (cam.name == "Main Camera") {
                        Log.Debug($"cam: {cam}, {cam.depthTextureMode}, depth: {cam.depth}");
                    //    cam.depth = -2;
                    //    cam.depthTextureMode = DepthTextureMode.Depth;
                    //}
                }
                UnityEngine.Shader.globalMaximumLOD = 999999999;
                Log.Debug($"LOD: {UnityEngine.Shader.globalMaximumLOD}");

            } catch (Exception e) {
                Log.Error($"failed to initialize: '{e}'");
            }
        }

        public void OnLevelLoad()
        {
            Log.Debug("loading props...");
            pcon_ = new PropManager();
            pcon_.Fetch();

            Log.Debug("loading traffic...");
            tcon_ = new TrafficController();
            tcon_.Fetch();
            Log.Debug(tcon_);

            Log.Debug("initializing flow generator...");
            fgen_ = new FlowGenerator(ref pmgr_, ref tcon_);
            fgen_.AddFactory(typeof(Cities::Citizen));
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

        public void LoadConfig()
        {
            cfg_.Load();
        }

        public void SaveConfig()
        {
            cfg_.Save();
        }

        public void ChangeKeyBinding(Input.KeyMod? mod, KeyCode? key = null)
        {
            if (mod.HasValue) {
                cfg_.boundKeyMod = mod.Value;
            }

            if (key.HasValue) {
                cfg_.boundKey = key.Value;
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            cfgTool_.Populate(helper, cfg_);
        }

        public static ulong GetDeviceSeed()
        {
            return 1145144545191912345;
        }

        public void Dispose()
        {
            pmgr_.Dispose();
            tcon_.Dispose();
            fgen_.Dispose();
            citizens_.Clear();
        }

        private Mod.Config cfg_ = new Mod.Config();
        private Mod.ConfigTool cfgTool_;
        internal ColossalFramework.Math.Randomizer R;

        private PrefabManager pmgr_;

        private PropManager pcon_;
        private TrafficController tcon_;

        private FlowGenerator fgen_;
        private Dictionary<Game.CitizenID, Game.Citizen> citizens_ = new Dictionary<Game.CitizenID, Game.Citizen>();
    }
}