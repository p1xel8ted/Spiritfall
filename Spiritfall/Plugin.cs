﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spiritfall;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public partial class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "p1xel8ted.spiritfall.ultrawide";
    private const string PluginName = "Spiritfall Ultra-Wide Fixes & Tweaks";
    private const string PluginVersion = "0.0.1";

    private static ManualLogSource Log { get; set; }
    private static Harmony _harmony;

    private static ConfigEntry<bool> _modEnabled;
    private static Dictionary<string, ConfigEntry<bool>> _uiElements;
    private static ConfigEntry<bool> _quitToDesktop;

    private void Awake()
    {
        Log = Logger;
        _harmony = new Harmony(PluginGuid);
        InitConfiguration();
        ApplyPatches(this, null);
    }

    private void InitConfiguration()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        _modEnabled = Config.Bind("1. General", "Enabled", true, $"Toggle {PluginName}");
        _modEnabled.SettingChanged += ApplyPatches;

        _uiElements = new Dictionary<string, ConfigEntry<bool>>
        {
            {"WelcomePopup", Config.Bind("2. Main Menu Tweaks", "WelcomePopup", false, "Toggle Welcome Popup")},
            {
                "EarlyAccessBanner",
                Config.Bind("2. Main Menu Tweaks", "EarlyAccessBanner", true, "Toggle Early Access Banner")
            },
            {"DiscordButton", Config.Bind("2. Main Menu Tweaks", "DiscordButton", true, "Toggle Discord Button")},
            {"TwitterButton", Config.Bind("2. Main Menu Tweaks", "TwitterButton", false, "Toggle Twitter Button")},
            {"FeedbackButton", Config.Bind("2. Main Menu Tweaks", "FeedbackButton", false, "Toggle Feedback Button")},
            {"RoadmapButton", Config.Bind("2. Main Menu Tweaks", "RoadmapButton", true, "Toggle Roadmap Button")},
            {"CreditsButton", Config.Bind("2. Main Menu Tweaks", "CreditsButton", true, "Toggle Credits Button")}
        };

        _quitToDesktop = Config.Bind("3. In Game Tweaks", "QuitToDesktop", true,
            "Re-enables the Quit to Desktop button in the pause menu. (It's hidden by default)");

        foreach (var uiElement in _uiElements.Values)
        {
            uiElement.SettingChanged += ToggleAll;
        }
    }

    private void OnActiveSceneChanged(Scene arg0, Scene arg1)
    {
        Log.LogInfo("Active scene changed. Old: " + arg0.name + " New: " + arg1.name);
        ToggleAll(this, null);
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Log.LogInfo("Scene loaded. Scene: " + arg0.name + " Mode: " + arg1);
        ToggleAll(this, null);
    }

    private static GameObject GetGameObject(string name)
    {
        return GameObject.Find(name);
    }

    private static void ToggleAll(object sender, EventArgs eventArgs)
    {
        if (_uiElements.Values.Any(a => a.Value))
        {
            var uiElementPaths = new Dictionary<string, string>
            {
                {"WelcomePopup", "CanvasMainMenu/WelcomePopup"},
                {"EarlyAccessBanner", "CanvasMainMenu/MenuPanel/EarlyAccessBanner"},
                {"DiscordButton", "CanvasMainMenu/MenuPanel/DiscordButton"},
                {"TwitterButton", "CanvasMainMenu/MenuPanel/TwitterButton"},
                {"FeedbackButton", "CanvasMainMenu/MenuPanel/FeedbackButton"},
                {"RoadmapButton", "CanvasMainMenu/MenuPanel/Content/Roadmap"},
                {"CreditsButton", "CanvasMainMenu/MenuPanel/Content/Credits"}
            };
            foreach (var uiElement in _uiElements)
            {
                var gameObject = GetGameObject(uiElementPaths[uiElement.Key]);
                if (gameObject != null)
                {
                    gameObject.SetActive(uiElement.Value.Value);
                }
            }

            var pauseMenuTwitterButton =
                GetGameObject("CanvasPauseMenu/PauseMenu/Content/MarketingButtons/TwitterButton");
            if (pauseMenuTwitterButton != null)
            {
                pauseMenuTwitterButton.SetActive(_uiElements["TwitterButton"].Value);
            }

            if (!_uiElements["FeedbackButton"].Value)
            {
                RemoveFeedbackButtonInPauseMenu();
            }
        }

        if (_quitToDesktop.Value)
        {
            var pauseMenuQuitButton = GetGameObject("CanvasPauseMenu/PauseMenu/Content/Quit");
            if (pauseMenuQuitButton != null)
            {
                pauseMenuQuitButton.SetActive(true);
            }

            RemoveFeedbackButtonInPauseMenu();
        }

        void RemoveFeedbackButtonInPauseMenu()
        {

            var pauseMenuFeedBack = GetGameObject("CanvasPauseMenu/PauseMenu/Content/Feedback");
            if (pauseMenuFeedBack != null)
            {
                var spaceOne = GetGameObject("CanvasPauseMenu/PauseMenu/Content/_Space");
                if (spaceOne != null)
                {
                    spaceOne.SetActive(false);
                }

                var spaceTwo = GetGameObject("CanvasPauseMenu/PauseMenu/Content/_Space_01");
                if (spaceTwo != null)
                {
                    spaceTwo.SetActive(false);
                }

                pauseMenuFeedBack.SetActive(false);
            }
        }
    }

    private static void ApplyPatches(object sender, EventArgs e)
    {
        if (_modEnabled.Value)
        {
            Log.LogInfo($"Applying patches for {PluginName}");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        else
        {
            Log.LogInfo($"Removing patches for {PluginName}");
            _harmony.UnpatchSelf();
        }
    }
}