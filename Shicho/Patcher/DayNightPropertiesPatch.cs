﻿extern alias Cities;

using Harmony;

namespace Shicho.Patcher.DayNightPropertiesPatch
{
    [HarmonyPatch(typeof(Cities::DayNightProperties))]
    [HarmonyPatch("UpdateLighting")]
    [HarmonyAfter(new string[] { "com.ronyx.relight", "com.ronyx.relight.biaspatch" })]
    class UpdateLighting
    {
        static void Postfix()
        {
            var light = App.Instance.MainLight;
            if (!light) return;

            lock (App.Config.GraphicsLock) {
                if (App.Config.Graphics.shadowBias) {
                    light.shadowBias = App.Config.Graphics.shadowBias.Value;
                }
            }
        }
    }
}
