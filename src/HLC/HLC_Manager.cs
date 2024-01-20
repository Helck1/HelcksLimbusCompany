using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using MainUI;
using MainUI.Gacha;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using ILObject = Il2CppSystem.Object;
using UObject = UnityEngine.Object;

namespace LimbusLocalize
{
    public class HLC_Manager : MonoBehaviour
    {
        static HLC_Manager()
        {
            ClassInjector.RegisterTypeInIl2Cpp<HLC_Manager>();
            GameObject obj = new(nameof(HLC_Manager));
            DontDestroyOnLoad(obj);
            obj.hideFlags |= HideFlags.HideAndDontSave;
            Instance = obj.AddComponent<HLC_Manager>();
        }
        public static HLC_Manager Instance;
        public HLC_Manager(IntPtr ptr) : base(ptr) { }
        void OnApplicationQuit() => LCB_HLCMod.CopyLog();
        public static void OpenGlobalPopup(string description, string title = null, string close = "Отменить", string confirm = "Принять", Action confirmEvent = null, Action closeEvent = null)
        {
            if (!GlobalGameManager.Instance) { return; }
            TextOkUIPopup globalPopupUI = GlobalGameManager.Instance.globalPopupUI;
            TMP_FontAsset fontAsset = LCB_Russian_Font.tmprussianfonts[2];
            if (fontAsset)
            {
                TextMeshProUGUI btn_canceltmp = globalPopupUI.btn_cancel.GetComponentInChildren<TextMeshProUGUI>(true);
                btn_canceltmp.font = fontAsset;
                btn_canceltmp.fontMaterial = fontAsset.material;
                UITextDataLoader btn_canceltl = globalPopupUI.btn_cancel.GetComponentInChildren<UITextDataLoader>(true);
                btn_canceltl.enabled = false;
                btn_canceltmp.text = close;
                TextMeshProUGUI btn_oktmp = globalPopupUI.btn_ok.GetComponentInChildren<TextMeshProUGUI>(true);
                btn_oktmp.font = fontAsset;
                btn_oktmp.fontMaterial = fontAsset.material;
                UITextDataLoader btn_oktl = globalPopupUI.btn_ok.GetComponentInChildren<UITextDataLoader>(true);
                btn_oktl.enabled = false;
                btn_oktmp.text = confirm;
                globalPopupUI.tmp_title.font = fontAsset;
                globalPopupUI.tmp_title.fontMaterial = fontAsset.material;
                void TextLoaderEnabled() { btn_canceltl.enabled = true; btn_oktl.enabled = true; }
                confirmEvent += TextLoaderEnabled;
                closeEvent += TextLoaderEnabled;
            }
            globalPopupUI._titleObject.SetActive(!string.IsNullOrEmpty(title));
            globalPopupUI.tmp_title.text = title;
            globalPopupUI.tmp_description.text = description;
            globalPopupUI._confirmEvent = confirmEvent;
            globalPopupUI._closeEvent = closeEvent;
            globalPopupUI.btn_cancel.gameObject.SetActive(!string.IsNullOrEmpty(close));
            globalPopupUI._gridLayoutGroup.cellSize = new Vector2(!string.IsNullOrEmpty(close) ? 500 : 700, 100f);
            globalPopupUI.Open();
        }
        public static void InitLocalizes(DirectoryInfo directory)
        {
            foreach (FileInfo fileInfo in directory.GetFiles())
            {
                var value = File.ReadAllText(fileInfo.FullName);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                Localizes[fileNameWithoutExtension] = value;
            }
            foreach (DirectoryInfo directoryInfo in directory.GetDirectories())
            {
                InitLocalizes(directoryInfo);
            }

        }
        public static Dictionary<string, string> Localizes = new();
        public static Action FatalErrorAction;
        public static string FatalErrorlog;
        #region Блокировка безвредных предупреждений
        [HarmonyPatch(typeof(Logger), nameof(Logger.Log), new Type[]
        {
            typeof(LogType),
            typeof(ILObject)
        })]
        [HarmonyPrefix]
        private static bool Log(Logger __instance, LogType __0, ILObject __1)
        {
            if (__0 == LogType.Warning)
            {
                string LogString = Logger.GetString(__1);
                if (!LogString.Contains("DOTWEEN"))
                    __instance.logHandler.LogFormat(__0, null, "{0}", new ILObject[] { LogString });
                return false;
            }
            return true;
        }
        [HarmonyPatch(typeof(Logger), nameof(Logger.Log), new Type[]
        {
            typeof(LogType),
            typeof(ILObject),
            typeof(UObject)
        })]
        [HarmonyPrefix]
        private static bool Log(Logger __instance, LogType logType, ILObject message, UObject context)
        {
            if (logType == LogType.Warning)
            {
                string LogString = Logger.GetString(message);
                if (!LogString.Contains("Material"))
                    __instance.logHandler.LogFormat(logType, context, "{0}", new ILObject[] { LogString });
                return false;
            }
            return true;
        }
        #endregion
        #region Исправление ошибок
        [HarmonyPatch(typeof(GachaEffectEventSystem), nameof(GachaEffectEventSystem.LinkToCrackPosition))]
        [HarmonyPrefix]
        private static bool LinkToCrackPosition(GachaEffectEventSystem __instance, GachaCrackController[] crackList)
            => __instance._parent.EffectChainCamera;
        [HarmonyPatch(typeof(PersonalityVoiceJsonDataList), nameof(PersonalityVoiceJsonDataList.GetDataList))]
        [HarmonyPrefix]
        private static bool PersonalityVoiceGetDataList(PersonalityVoiceJsonDataList __instance, int personalityId, ref LocalizeTextDataRoot<TextData_PersonalityVoice> __result)
        {
            if (!__instance._voiceDictionary.TryGetValueEX(personalityId.ToString(), out LocalizeTextDataRoot<TextData_PersonalityVoice> localizeTextDataRoot))
            {
                Debug.LogError("PersonalityVoice no id:" + personalityId.ToString());
                localizeTextDataRoot = new LocalizeTextDataRoot<TextData_PersonalityVoice>() { dataList = new Il2CppSystem.Collections.Generic.List<TextData_PersonalityVoice>() };
            }
            __result = localizeTextDataRoot;
            return false;
        }
        #endregion
        [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
        [HarmonyPostfix]
        public static void CheckModActions()
        {
            if (HLC_UpdateChecker.UpdateCall != null)
                OpenGlobalPopup("Доступное обновление" + HLC_UpdateChecker.Updatelog + "!\nЗакрыть игру и скачать обновление\nДоступно обновление мода\nНажмите OK, чтобы закрыть игру и перейти к скачиванию\nИзменить" + HLC_UpdateChecker.Updatelog + "Перетащите содержимое архива в папку с игрой", "Мод обновлен!", null, "OK", () =>
                {
                    HLC_UpdateChecker.UpdateCall.Invoke();
                    HLC_UpdateChecker.UpdateCall = null;
                    HLC_UpdateChecker.Updatelog = string.Empty;
                });
            else if (FatalErrorAction != null)
                OpenGlobalPopup(FatalErrorlog, "Критическая ошибка мода!", null, "Откройте ссылку HLC", () =>
                {
                    FatalErrorAction.Invoke();
                    FatalErrorAction = null;
                    FatalErrorlog = string.Empty;
                });
        }
    }
}
