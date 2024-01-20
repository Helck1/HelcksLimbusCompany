using BattleUI;
using HarmonyLib;

namespace LimbusLocalize
{
    public static class HLC_SpriteUI
    {
        [HarmonyPatch(typeof(ParryingTypoUI), nameof(ParryingTypoUI.SetParryingTypoData))]
        [HarmonyPrefix]
        private static void ParryingTypoUI_SetParryingTypoData(ParryingTypoUI __instance)
          => __instance.img_parryingTypo.sprite = HLC_ReadmeManager.ReadmeSprites["HLC_Combo"];
    }
}
