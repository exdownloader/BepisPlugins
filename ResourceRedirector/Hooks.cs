﻿using BepInEx;
using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ResourceRedirector
{
    static class Hooks
    {
        public static void InstallHooks()
        {
            var harmony = HarmonyInstance.Create("com.bepis.bepinex.resourceredirector");


            MethodInfo original = AccessTools.Method(typeof(AssetBundleManager), "LoadAsset", new[] { typeof(string), typeof(string), typeof(Type), typeof(string) });

            HarmonyMethod postfix = new HarmonyMethod(typeof(Hooks).GetMethod("LoadAssetPostHook"));

            harmony.Patch(original, null, postfix);


            original = AccessTools.Method(typeof(AssetBundleManager), "LoadAssetBundle", new[] { typeof(string), typeof(bool), typeof(string) });

            postfix = new HarmonyMethod(typeof(Hooks).GetMethod("LoadAssetBundlePostHook"));

            harmony.Patch(original, null, postfix);



            original = AccessTools.Method(typeof(AssetBundleManager), "LoadAssetAsync", new[] { typeof(string), typeof(string), typeof(Type), typeof(string) });

            postfix = new HarmonyMethod(typeof(Hooks).GetMethod("LoadAssetAsyncPostHook"));

            harmony.Patch(original, null, postfix);



            original = AccessTools.Method(typeof(AssetBundleManager), "LoadAllAsset", new[] { typeof(string), typeof(Type), typeof(string) });

            postfix = new HarmonyMethod(typeof(Hooks).GetMethod("LoadAllAssetPostHook"));

            harmony.Patch(original, null, postfix);



            original = AccessTools.Method(typeof(ChaListControl), "LoadListInfoAll");

            postfix = new HarmonyMethod(typeof(Hooks).GetMethod("LoadListInfoAllPostHook"));

            harmony.Patch(original, null, postfix);
        }

        public static void LoadListInfoAllPostHook(ChaListControl __instance)
        {
            string listPath = Path.Combine(ResourceRedirector.EmulatedDir, @"list\characustom");

            if (Directory.Exists(listPath))
                foreach (string csvPath in Directory.GetFiles(listPath, "*.csv", SearchOption.AllDirectories))
                {
                    ListLoader.LoadCSV(File.OpenRead(csvPath));
                }

            ListLoader.LoadAllLists(__instance);
        }

        public static void LoadAssetPostHook(ref AssetBundleLoadAssetOperation __result, string assetBundleName, string assetName, Type type, string manifestAssetBundleName)
        {
            //BepInLogger.Log($"{assetBundleName} : {assetName} : {type.FullName} : {manifestAssetBundleName ?? ""}");

            __result = ResourceRedirector.HandleAsset(assetBundleName, assetName, type, manifestAssetBundleName, ref __result);
        }

        public static void LoadAssetBundlePostHook(string assetBundleName, bool isAsync, string manifestAssetBundleName)
        {
            //BepInLogger.Log($"{assetBundleName} : {manifestAssetBundleName} : {isAsync}");
        }

        public static void LoadAssetAsyncPostHook(ref AssetBundleLoadAssetOperation __result, string assetBundleName, string assetName, Type type, string manifestAssetBundleName)
        {
            //BepInLogger.Log($"{assetBundleName} : {assetName} : {type.FullName} : {manifestAssetBundleName ?? ""}", true);

            __result = ResourceRedirector.HandleAsset(assetBundleName, assetName, type, manifestAssetBundleName, ref __result);
        }

        public static void LoadAllAssetPostHook(ref AssetBundleLoadAssetOperation __result, string assetBundleName, Type type, string manifestAssetBundleName = null)
        {
            //BepInLogger.Log($"{assetBundleName} : {type.FullName} : {manifestAssetBundleName ?? ""}");

            if (assetBundleName == "sound/data/systemse/brandcall/00.unity3d" ||
                assetBundleName == "sound/data/systemse/titlecall/00.unity3d")
            {
                string dir = $"{BepInEx.Common.Utility.PluginsDirectory}\\introclips";

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var files = Directory.GetFiles(dir, "*.wav");

                if (files.Length == 0)
                    return;

                List<UnityEngine.Object> loadedClips = new List<UnityEngine.Object>();

                foreach (string path in files)
                    loadedClips.Add(AssetLoader.LoadAudioClip(path, AudioType.WAV));

                __result = new AssetBundleLoadAssetOperationSimulation(loadedClips.ToArray());
            }
        }
    }
}
