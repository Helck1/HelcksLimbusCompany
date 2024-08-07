using HarmonyLib;
using TMPro;
using MainUI;

namespace LimbusLocalizeDCLC
{
    internal class DCLC_TextUI
    {

        [HarmonyPatch(typeof(BattlePassUIPopup), nameof(BattlePassUIPopup.localizeHelper.Initialize))]
        [HarmonyPostfix]
        private static void BattlePass_Init(BattlePassUIPopup __instance)
        {
            __instance.dailyMissionButton.GetComponentInChildren<UIVerticalWriteLocalize>(true).horizontalText.GetComponentInChildren<TextMeshProUGUI>(true).m_fontAsset = LCB_Russian_Font.GetRussianFonts(3);
            __instance.dailyMissionButton.GetComponentInChildren<UIVerticalWriteLocalize>(true).horizontalText.GetComponentInChildren<TextMeshProUGUI>(true).fontMaterial = LCB_Russian_Font.GetRussianFonts(3).material;

            __instance.weeklyMissionButton.GetComponentInChildren<UIVerticalWriteLocalize>(true).horizontalText.GetComponentInChildren<TextMeshProUGUI>(true).m_fontAsset = LCB_Russian_Font.GetRussianFonts(3);
            __instance.weeklyMissionButton.GetComponentInChildren<UIVerticalWriteLocalize>(true).horizontalText.GetComponentInChildren<TextMeshProUGUI>(true).fontMaterial = LCB_Russian_Font.GetRussianFonts(3).material;

            __instance.seasonMissionButton.GetComponentInChildren<UIVerticalWriteLocalize>(true).horizontalText.GetComponentInChildren<TextMeshProUGUI>(true).m_fontAsset = LCB_Russian_Font.GetRussianFonts(3);
            __instance.seasonMissionButton.GetComponentInChildren<UIVerticalWriteLocalize>(true).horizontalText.GetComponentInChildren<TextMeshProUGUI>(true).fontMaterial = LCB_Russian_Font.GetRussianFonts(3).material;

        }

        
    }
}