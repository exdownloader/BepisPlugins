﻿using BepInEx;
using BepInEx.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DynamicTranslationLoader
{
    public class DynamicTranslator : BaseUnityPlugin
    {
        public override string ID => "com.bepis.bepinex.dynamictranslator";
        public override string Name => "Dynamic Translator";
        public override Version Version => new Version("2.0");

        private static Dictionary<string, string> translations = new Dictionary<string, string>();
        private static List<string> untranslated = new List<string>();

        void Awake()
        {
            string[] translation = Directory.GetFiles(Path.Combine(Utility.PluginsDirectory, "translation"), "*.txt", SearchOption.AllDirectories)
                .SelectMany(file => File.ReadAllLines(file))
                .ToArray();

            for (int i = 0; i < translation.Length; i++)
            {
                string line = translation[i];
                if (!line.Contains('='))
                    continue;

                string[] split = line.Split('=');

                translations[split[0]] = split[1];
            }

            Hooks.InstallHooks();

            TranslateAll();
        }


        void LevelFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            TranslateScene(scene);
        }

        public static string Translate(string input)
        {
            if (translations.ContainsKey(input))
                return translations[input];

            if (!untranslated.Contains(input) &&
                !translations.ContainsValue(input.Trim()))
            {
                untranslated.Add(input);
            }

            return input;
        }

        void TranslateAll()
        {
            foreach (TextMeshProUGUI gameObject in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
            {
                //gameObject.text = "Harsh is shit";

                gameObject.text = Translate(gameObject.text);
            }
        }

        void TranslateScene(Scene scene)
        {
            foreach (GameObject obj in scene.GetRootGameObjects())
                foreach (TextMeshProUGUI gameObject in obj.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    //gameObject.text = "Harsh is shit";

                    gameObject.text = Translate(gameObject.text);
                }
        }

        void Dump()
        {
            string output = "";

            foreach (var kv in translations)
                output += $"{kv.Key.Trim()}={kv.Value.Trim()}\r\n";

            foreach (var text in untranslated)
                if(!Regex.Replace(text, @"[\d-]", string.Empty).IsNullOrWhiteSpace()
                        && !text.Contains("Reset"))
                    output += $"{text.Trim()}=\r\n";

            File.WriteAllText("dumped-tl.txt", output);
        }

        #region MonoBehaviour
        void OnEnable()
        {
            SceneManager.sceneLoaded += LevelFinishedLoading;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= LevelFinishedLoading;
        }

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F10))
            {
                Dump();
                BepInLogger.Log($"Text dumped to \"{Path.GetFullPath("dumped-tl.txt")}\"", true);
            }
        }
        #endregion
    }
}
