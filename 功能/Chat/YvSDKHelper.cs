using System;
using System.Collections.Generic;
using FrameWork;
using UnityEngine;
using YunvaIM;

namespace Game
{
    public class YvSDKHelper : Singleton<YvSDKHelper>
    {
        #region Cached Message Info
        private class MessageInfo
        {
            public bool isOther;
            public string name;
            public string messageBody;
            public MessageInfo(bool isOther, string name, string messageBody)
            {
                this.isOther = isOther;
                this.name = name;
                this.messageBody = messageBody;
            }
        }

        private class CachedMessageInfos
        {
            private int capacity = 100;
            public Queue<MessageInfo> messageInfos;
            public CachedMessageInfos(int capacity = 100)
            {
                messageInfos = new Queue<MessageInfo>();
                this.capacity = capacity;
            }

            public void AddMessage(MessageInfo info)
            {
                List<int> s = new List<int>();
                while (messageInfos.Count >= capacity)
                {
                    messageInfos.Dequeue();
                }
                messageInfos.Enqueue(info);
            }

            public void GetCachedMessages()
            {
                foreach(var messageInfo in messageInfos)
                {
                    if(YvSDKHelper.Instance.onGetCachedHistoryMessage != null)
                    {
                        YvSDKHelper.Instance.onGetCachedHistoryMessage(messageInfo.isOther, messageInfo.name, messageInfo.messageBody);
                    }
                }
            }

            public void SetCapacity(int capacity)
            {
                if(capacity > 0)
                {
                    while(messageInfos.Count > capacity)
                    {
                        messageInfos.Dequeue();
                    }
                    this.capacity = capacity;
                }
            }
        }
        #endregion

        #region Info
        public static YvSDKHelper theInstance
        {
            get { return Instance; }
        }
        public Action onInited;
        public Action<ImThirdLoginResp> onLogin;
        public Action<ImChannelLoginResp> onLoginChannel;
        public Action<ImChannelSendMsgResp> onSendMessage;
        public Action<string> onGetChannelHistory;
        public Action<bool, string, string> onGetCachedHistoryMessage;
        public Action<string,string> onReceiveChannelMessage;

        private CachedMessageInfos cachedMessages = new CachedMessageInfos();
        #endregion

        #region Init
        public void Init(Action onInited)
        {
            EventListenerManager.AddListener(ProtocolEnum.IM_CHANNEL_MESSAGE_NOTIFY, OnChannelMessageNotify);
            this.onInited = onInited;
            YvSDK_Init();
        }

        public void Release()
        {
            EventListenerManager.RemoveListener(ProtocolEnum.IM_CHANNEL_MESSAGE_NOTIFY, OnChannelMessageNotify);
            onInited = null;
            YvSDK_Logout();
        }

        /// <summary>
        /// 聊天SDK初始化
        /// </summary>
        public void YvSDK_Init()
        {
            int ret = YunVaImSDK.instance.YunVa_Init(0, 1003127, Application.persistentDataPath, false, false);
            if(ret != 0)
            {
                Debug.LogError("YvSDK Init False!");
            }
            else
            {
                if (onInited != null)
                {
                    onInited();
                }
            }
        }

        /// <summary>
        /// 聊天SDK登陆，只有一个世界频道
        /// </summary>
        /// <param name="nickName">昵称</param>
        /// <param name="userId">用户id</param>
        public void YvSDK_Login(string nickName, string userId)
        {
            string ttFormat = "{{\"nickname\":\"{0}\",\"uid\":\"{1}\"}}";
            //string tt = string.Format(ttFormat, nickName, userId);
            string tt = string.Format(ttFormat, "mike" + userId, userId);
            string[] wildcard = new string[2];
            wildcard[0] = "0x001";
            wildcard[1] = "0x002";
            YunVaImSDK.instance.YunVaOnLogin(tt, "12345678a", wildcard, 0, (data) =>
            {
                if (data.result == 0)
                {
                    Debug.Log("Success Login!!!!!!!!!!");
                }
                else
                {
                    Debug.Log("FAIL Login!!!!!!!!!!!!!!!!");
                }
                if (onLogin != null)
                {
                    onLogin(data);
                }
            },
            (data1) =>
            {
                if (data1.result == 0)
                {
                    Debug.Log(string.Format("频道登录成功..."));
                }
                else
                {
                    Debug.Log(string.Format("频道登录失败...") + data1.msg);
                }
                if (onLoginChannel != null)
                {
                    onLoginChannel(data1);
                }
            });
        }

        /// <summary>
        /// 登出
        /// </summary>
        public void YvSDK_Logout()
        {
            YunVaImSDK.instance.YunVaLogOut();
        }
        #endregion

        #region Method
        /// <summary>
        /// 发送频道文本信息
        /// </summary>
        public void YvSDK_SendChannelTextMessage(string name, string message)
        {
            int ret = YunVaImSDK.instance.SendChannelTextMessage(message, "0x001", (data) =>
            {
                if (data.result == 0)
                {
                    var text = "发送成功" + ",expand:" + data.expand + ",flag:" + data.flag;
                    Debug.Log(text);
                    //todo:改为正式名称
                    MessageInfo messageInfo = new MessageInfo(false, name, message);
                    cachedMessages.AddMessage(messageInfo);
                }
                else
                {
                    var text = "发送失败";
                    Debug.Log(text);
                }
                if (onSendMessage != null)
                {
                    onSendMessage(data);
                }

            }, "is ext", "is flag");
        }

        /// <summary>
        /// 查看频道历史
        /// </summary>
        public void YvSDK_GetChannelHistoryMsg()
        {
            string retText = "";
            YunVaImSDK.instance.getChannelHistoryMsg(0, -15, "0x001", (data) =>
            {
                for (int i = 0; i < data.channelHisList.Count; i++)
                {
                    retText = retText + "\n" + data.channelHisList[i].nickName + ":" + data.channelHisList[i].messageBody;
                }
                Debug.Log("@@@@@@@@@@@@Get History Str:!!@@@@@@@@@@2");
                if (onGetChannelHistory != null)
                {
                    onGetChannelHistory(retText);
                }
            });
        }

        public void GetCachedHistoryMessages()
        {
            cachedMessages.GetCachedMessages();
        }

        #endregion

        #region CallBack
        public void OnChannelMessageNotify(object data)
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!Receive Channel Message:!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            ImChannelMessageNotify channelNotify = data as ImChannelMessageNotify;
            Debug.Log(channelNotify.userId);
            Debug.Log(channelNotify.nickname);
            Debug.Log(channelNotify.messageBody);
            Debug.Log("Recive Channel Notify::" + onReceiveChannelMessage == null);
            MessageInfo message = new MessageInfo(true, channelNotify.nickname, channelNotify.messageBody);
            cachedMessages.AddMessage(message);
            if (onReceiveChannelMessage != null)
            {
                onReceiveChannelMessage(channelNotify.nickname, channelNotify.messageBody);
            }
        }

        #endregion
    }
}
