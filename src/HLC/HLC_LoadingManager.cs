﻿using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LimbusLocalize
{
    public static class HLC_LoadingManager
    {
        static List<string> LoadingTexts = new();
        static string Touhou;
        static readonly string Raw = "<bounce f=0.5>NOW LOADING...</bounce>";
        public static ConfigEntry<bool> RandomAllLoadCG = LCB_HLCMod.HLC_Settings.Bind("HLC Settings", "RandomAllLoadCG", true, "Использовать случайный загрузочный экран ( true | false )");
        public static ConfigEntry<bool> RandomLoadText = LCB_HLCMod.HLC_Settings.Bind("HLC Settings", "RandomLoadText", true, "Использовать случайное загрузочное сообщение[NOW LOADING...] ( true | false )");
        public static int ArchiveCGId;
        static HLC_LoadingManager() => InitLoadingTexts();
        public static void InitLoadingTexts()
        {
            LoadingTexts = File.ReadAllLines(LCB_HLCMod.ModPath + "/Localize/Readme/LoadingTexts.md").ToList();
            for (int i = 0; i < LoadingTexts.Count; i++)
            {
                string LoadingText = LoadingTexts[i];
                LoadingTexts[i] = "<bounce f=0.5>" + LoadingText.Remove(0, 2) + "</bounce>";
            }
            Touhou = LoadingTexts[0];
            LoadingTexts.RemoveAt(0);
        }
        public static T SelectOne<T>(List<T> list)
            => list.Count == 0 ? default : list[Random.Range(0, list.Count)];
        [HarmonyPatch(typeof(LoadingSceneManager), nameof(LoadingSceneManager.Start))]
        [HarmonyPostfix]
        private static void LSM_Start(LoadingSceneManager __instance)
        {
            if (!RandomLoadText.Value)
                return;
            var loadingText = __instance._loadingText;
            loadingText.font = LCB_Russian_Font.tmprussianfonts[1];
            loadingText.fontMaterial = LCB_Russian_Font.tmprussianfonts[1].material;
            loadingText.fontSize = 46;
            int random = Random.Range(0, 100);
            if (random < 25)
                loadingText.text = Raw;
            else if (random < 50)
                loadingText.text = Touhou;
            else
                loadingText.text = SelectOne(LoadingTexts);
        }
        [HarmonyPatch(typeof(LoadingCGDataList), nameof(LoadingCGDataList.GetDataByStageNode))]
        [HarmonyPrefix]
        private static void RandomAllLoadCG_GetDataByStageNode(LoadingCGDataList __instance, ref int id, ref int SubChapterId)
        {
            if (!RandomAllLoadCG.Value)
                return;
            if (ArchiveCGId != id)
            {
                ArchiveCGId = id;
                __instance._loadingCGDataDic_Stage[1192] = AllLoadingCGData(__instance._loadingCGDataDic_Stage, id);
            }
            id = SubChapterId = 1192;
        }
        [HarmonyPatch(typeof(LoadingCGDataList), nameof(LoadingCGDataList.GetDataByDungeonNode))]
        [HarmonyPrefix]
        private static void RandomAllLoadCG_GetDataByDungeonNode(LoadingCGDataList __instance, ref int id, ref int SubChapterId)
        {
            if (!RandomAllLoadCG.Value)
                return;
            if (ArchiveCGId != id)
            {
                ArchiveCGId = id;
                __instance._loadingCGDataDic_Dungeon[1192] = AllLoadingCGData(__instance._loadingCGDataDic_Dungeon, id);
            }
            id = SubChapterId = 1192;
        }
        private static Il2CppSystem.Collections.Generic.List<LoadingCGData> AllLoadingCGData(Il2CppSystem.Collections.Generic.Dictionary<int, Il2CppSystem.Collections.Generic.List<LoadingCGData>> dic, int id)
        {
            LoadingCGData allloadingCGData = new()
            {
                id = 1192
            };
            Il2CppSystem.Collections.Generic.List<LoadingCGData> allloadingCGDatas = new();
            allloadingCGDatas.Add(allloadingCGData);

            int idx = dic.FindEntry(id / 100) + 1;

            for (int i = 0; i < idx; i++)
                foreach (var items in dic._entries[i].value)
                    if (items.id != 1192 && items.id <= id)
                        foreach (var item in items.clearCGList)
                            allloadingCGData.clearCGList.Add(item);
            return allloadingCGDatas;
        }
    }
}
