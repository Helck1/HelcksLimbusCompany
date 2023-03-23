﻿using Il2Cpp;
using Il2CppLocalSave;
using Il2CppMainUI;
using Il2CppServer;
using Il2CppSimpleJSON;
using Il2CppSystem.Collections.Generic;
using Il2CppTMPro;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using static MelonLoader.MelonLogger;

namespace LimbusLocalize
{/*{
      "id": id
      "version": 版本,
      "type": 1贡献 0更新,
      "startDate": 开始时间,
      "endDate": 结束时间,
      "sprNameList": ["图片key"],
      "title_KR": "左侧标题",
      "content_KR": {\"formatKey\":\"SubTitle\",\"formatValue\":\"<size=49>标题</size>\"} HyperLink超链接 Image图片 Text正常文本 PassTable通行证任务点数更改特供
    },*/

    public static class ReadmeManager
    {
        public static NoticeUIPopup NoticeUIInstance;
        public static RedDotWriggler _redDot_Notice;
        public static void Initialize()
        {
            NoticeUIInstance.Initialize();
            Action _back = delegate () { Close(); };
            NoticeUIInstance.btn_back._onClick.RemoveAllListeners();
            NoticeUIInstance.btn_back._onClick.AddListener(_back);
            InitReadmeSprites();
            NoticeUIInstance.btn_systemNotice.GetComponentInChildren<UITextDataLoader>(true).enabled = false;
            NoticeUIInstance.btn_systemNotice.GetComponentInChildren<TextMeshProUGUI>(true).text = "更新公告";
            NoticeUIInstance.btn_eventNotice.GetComponentInChildren<UITextDataLoader>(true).enabled = false;
            NoticeUIInstance.btn_eventNotice.GetComponentInChildren<TextMeshProUGUI>(true).text = "贡献,反馈,赞助";
        }

        public static void Open()
        {
            NoticeUIInstance.Open();
            NoticeUIInstance._popupPanel.Open();
            List<Notice> notices = ReadmeList;
            Func<Notice, bool> findsys = (Notice x) => x.noticeType == NOTICE_TYPE.System;
            NoticeUIInstance._systemNotices = notices.FindAll(findsys);
            Func<Notice, bool> findeve = (Notice x) => x.noticeType == NOTICE_TYPE.Event;
            NoticeUIInstance._eventNotices = notices.FindAll(findeve);
            NoticeUIInstance.EventTapClickEvent();
            (NoticeUIInstance.btn_eventNotice as UISelectedButton).SetSelected(true);
        }
        private static void InitReadmeSprites()
        {
            ReadmeSprites = new Dictionary<string, Sprite>();

            foreach (FileInfo fileInfo in new DirectoryInfo(LimbusLocalizeMod.path + "/Localize/Readme").GetFiles().Where(f => f.Extension != ".json"))
            {
                Texture2D texture2D = new(2, 2);
                ImageConversion.LoadImage(texture2D, File.ReadAllBytes(fileInfo.FullName));
                Sprite value = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                texture2D.name = fileNameWithoutExtension;
                value.name = fileNameWithoutExtension;
                ReadmeSprites[fileNameWithoutExtension] = value;
            }

        }
        public static Sprite GetReadmeSprite(string key)
        {
            return ReadmeSprites[key];
        }
        public static void InitReadmeList()
        {
            foreach (var notices in JSONNode.Parse(File.ReadAllText(LimbusLocalizeMod.path + "/Localize/Readme/Readme.json"))[0].AsArray.m_List)
            {
                ReadmeList.Add(new Notice(JsonUtility.FromJson<NoticeFormat>(notices.ToString()), LOCALIZE_LANGUAGE.KR));
            }
        }
        public static List<Notice> ReadmeList = new();
        public static Dictionary<string, Sprite> ReadmeSprites = new();

        public static void Close()
        {
            Singleton<UserLocalSaveDataRoot>.Instance.NoticeRedDotSaveModel.Save();
            NoticeUIInstance._popupPanel.Close();
            UpdateNoticeRedDot();
        }
        public static void UpdateNoticeRedDot()
        {
            _redDot_Notice.gameObject.SetActive(IsValidRedDot());
        }
        public static bool IsValidRedDot()
        {
            int i = 0;
            int count = ReadmeList.Count;
            while (i < count)
            {
                if (!Singleton<UserLocalSaveDataRoot>.Instance.NoticeRedDotSaveModel.TryCheckId(ReadmeList[i].ID))
                {
                    return true;
                }
                i++;
            }
            return false;
        }
    }
}