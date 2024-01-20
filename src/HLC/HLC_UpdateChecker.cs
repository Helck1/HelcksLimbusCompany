using BepInEx.Configuration;
using Il2CppSystem.Threading;
using SimpleJSON;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using UnityEngine;
using UnityEngine.Networking;

namespace LimbusLocalize
{
    public static class HLC_UpdateChecker
    {
        public static ConfigEntry<bool> AutoUpdate = LCB_HLCMod.HLC_Settings.Bind("HLC Settings", "AutoUpdate", false, "Автоматически проверять и загружать обновления ( true | false )");
        public static ConfigEntry<URI> UpdateURI = LCB_HLCMod.HLC_Settings.Bind("HLC Settings", "UpdateURI", URI.GitHub, "Автоматическое обновление URI ( GitHub:по умолчанию | Mirror_OneDrive:Обновления могут появляться чуть позже)");
        public static void StartAutoUpdate()
        {
            if (AutoUpdate.Value)
            {
                LCB_HLCMod.LogWarning($"Check Mod Update From {UpdateURI.Value}");
                Action ModUpdate = CheckModUpdate;
                new Thread(ModUpdate).Start();
            }
        }
        static void CheckModUpdate()
        {
            string release_uri = UpdateURI.Value == URI.GitHub ?
                " "
                : " ";
            UnityWebRequest www = UnityWebRequest.Get(release_uri);
            www.timeout = 4;
            www.SendWebRequest();
            while (!www.isDone)
                Thread.Sleep(100);
            if (www.result != UnityWebRequest.Result.Success)
                LCB_HLCMod.LogWarning($"Не удается подключиться {UpdateURI.Value}!!!" + www.error);
            else
            {
                var latest = JSONNode.Parse(www.downloadHandler.text).AsObject;
                string latestReleaseTag = latest["tag_name"].Value;
                if (Version.Parse(LCB_HLCMod.VERSION) < Version.Parse(latestReleaseTag.Remove(0, 1)))
                {
                    string updatelog = "LimbusLocalize_BIE_" + latestReleaseTag;
                    Updatelog += updatelog + ".7z ";
                    string download_uri = UpdateURI.Value == URI.GitHub ?
                        $" "
                        : $" ";
                    var dirs = download_uri.Split('/');
                    string filename = LCB_HLCMod.GamePath + "/" + dirs[^1];
                    if (!File.Exists(filename))
                        DownloadFileAsync(download_uri, filename);
                    UpdateCall = UpdateDel;
                }
                LCB_HLCMod.LogWarning("Проверьте обновление русского шрифта");
                Action FontAssetUpdate = CheckRussianFontAssetUpdate;
                new Thread(FontAssetUpdate).Start();
            }
        }
        static void CheckRussianFontAssetUpdate()
        {
            string release_uri = UpdateURI.Value == URI.GitHub ?
                " "
                : " ";
            UnityWebRequest www = UnityWebRequest.Get(release_uri);
            string FilePath = LCB_HLCMod.ModPath + "/tmprussianfonts";
            var LastWriteTime = File.Exists(FilePath) ? int.Parse(TimeZoneInfo.ConvertTime(new FileInfo(FilePath).LastWriteTime, TimeZoneInfo.FindSystemTimeZoneById("Moscow Standard Time")).ToString("ddMMyy")) : 0;
            www.SendWebRequest();
            while (!www.isDone)
                Thread.Sleep(100);
            var latest = JSONNode.Parse(www.downloadHandler.text).AsObject;
            int latestReleaseTag = int.Parse(latest["tag_name"].Value);
            if (LastWriteTime < latestReleaseTag)
            {
                string updatelog = "tmprussianfonts_" + latestReleaseTag;
                Updatelog += updatelog + ".7z ";
                string download = UpdateURI.Value == URI.GitHub ?
                    $" "
                    : $" ";
                var dirs = download.Split('/');
                string filename = LCB_HLCMod.GamePath + "/" + dirs[^1];
                if (!File.Exists(filename))
                    DownloadFileAsync(download, filename);
                UpdateCall = UpdateDel;
            }
        }
        static void UpdateDel()
        {
            LCB_HLCMod.OpenGamePath();
            Application.Quit();
        }
        static void DownloadFileAsync(string uri, string filePath)
        {
            try
            {
                LCB_HLCMod.LogWarning("Download " + uri + " To " + filePath);
                using HttpClient client = new();
                using HttpResponseMessage response = client.GetAsync(uri).GetAwaiter().GetResult();
                using HttpContent content = response.Content;
                using FileStream fileStream = new(filePath, FileMode.Create);
                content.CopyToAsync(fileStream).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                if (ex is HttpRequestException httpException && httpException.StatusCode == HttpStatusCode.NotFound)
                    LCB_HLCMod.LogWarning($"{uri} 404 NotFound,No Resource");
                else
                    LCB_HLCMod.LogWarning($"{uri} Error!!!" + ex.ToString());
            }
        }
        public static void CheckReadmeUpdate()
        {
            UnityWebRequest www = UnityWebRequest.Get(" ");
            www.timeout = 1;
            www.SendWebRequest();
            string FilePath = LCB_HLCMod.ModPath + "/Localize/Readme/Readme.json";
            var LastWriteTime = new FileInfo(FilePath).LastWriteTime;
            while (!www.isDone)
            {
                Thread.Sleep(100);
            }
            if (www.result == UnityWebRequest.Result.Success && LastWriteTime < DateTime.Parse(www.downloadHandler.text))
            {
                UnityWebRequest www2 = UnityWebRequest.Get(" ");
                www2.SendWebRequest();
                while (!www2.isDone)
                {
                    Thread.Sleep(100);
                }
                File.WriteAllText(FilePath, www2.downloadHandler.text);
                HLC_ReadmeManager.InitReadmeList();
            }
        }
        public static string Updatelog;
        public static Action UpdateCall;
        public enum URI
        {
            GitHub,
            Mirror_OneDrive
        }
    }
}