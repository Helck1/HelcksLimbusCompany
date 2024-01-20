using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace LimbusLocalize
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class LCB_HLCMod : BasePlugin
    {
        public static ConfigFile HLC_Settings;
        public static string ModPath;
        public static string GamePath;
        public const string GUID = "Com.Bright.LocalizeLimbusCompany";
        public const string NAME = "HelcksLimbusCompany";
        public const string VERSION = "0.0.1";
        public const string AUTHOR = "Bright";
        public const string HLCLink = "https://github.com/Helck1/HelcksLimbusCompany";
        public static Action<string, Action> LogFatalError { get; set; }
        public static Action<string> LogError { get; set; }
        public static Action<string> LogWarning { get; set; }
        public static void OpenHLCURL() => Application.OpenURL(HLCLink);
        public static void OpenGamePath() => Application.OpenURL(GamePath);
        public override void Load()
        {
            HLC_Settings = Config;
            LogError = (string log) => { Log.LogError(log); Debug.LogError(log); };
            LogWarning = (string log) => { Log.LogWarning(log); Debug.LogWarning(log); };
            LogFatalError = (string log, Action action) => { HLC_Manager.FatalErrorlog += log + "\n"; LogError(log); HLC_Manager.FatalErrorAction = action; HLC_Manager.CheckModActions(); };
            ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            GamePath = new DirectoryInfo(Application.dataPath).Parent.FullName;
            HLC_UpdateChecker.StartAutoUpdate();
            try
            {
                Harmony harmony = new(NAME);
                if (HLC_Russian_Setting.IsUseRussian.Value)
                {
                    HLC_Manager.InitLocalizes(new DirectoryInfo(ModPath + "/Localize/RU"));
                    harmony.PatchAll(typeof(LCB_Russian_Font));
                    harmony.PatchAll(typeof(HLC_ReadmeManager));
                    harmony.PatchAll(typeof(HLC_LoadingManager));
                    harmony.PatchAll(typeof(HLC_SpriteUI));
                }
                harmony.PatchAll(typeof(HLC_Manager));
                harmony.PatchAll(typeof(HLC_Russian_Setting));
                if (!LCB_Russian_Font.AddRussianFont(ModPath + "/tmprussianfonts"))
                    LogFatalError("Отсутствует русский шрифт. Пожалуйста, посетите GitHub мода и убедитесь, что у Вас все установленно согласено инструкции", OpenHLCURL);
            }
            catch (Exception e)
            {
                LogFatalError("Возникла неизвестная критическая ошибка!!!", () => { CopyLog(); OpenGamePath(); OpenHLCURL(); });
                LogError(e.ToString());
            }
        }
        public static void CopyLog()
        {
            File.Copy(GamePath + "/BepInEx/LogOutput.log", GamePath + "/Latest.log", true);
            File.Copy(Application.consoleLogPath, GamePath + "/Player.log", true);
        }
    }
}
