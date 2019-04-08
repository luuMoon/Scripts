using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;

namespace FrameWork
{
    /// <summary>
    /// 鉴于web需求的行为可预期
    /// </summary>
    public class AsynExecWorker
    {
        #region Vars
        private List<UnityCbTaskExecutor> workers = new List<UnityCbTaskExecutor>();
        private List<UnityCbTaskExecutor> tmpExecutors = new List<UnityCbTaskExecutor>();
        private int idCount = 0;
        private int tmpCount = 0;
        private static AsynExecWorker instance = null;
        public static AsynExecWorker Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new AsynExecWorker();
                }
                return instance;
            }
        }
        #endregion
        #region Public Method
        public AsynExecWorker()
        {
            ServicePointManager.DefaultConnectionLimit = 512;
        }
        public void UploadFile(string url,string uploadFile, string fileKey,string saveName, bool addPre =false, bool compress =false,
                               string contentType = null,string[] header = null, int timeOut = 10000, Action<bool,string> cb = null)
        {
            HttpUpLoadTask task = new HttpUpLoadTask(url, header,uploadFile, fileKey,saveName, addPre, compress, contentType, timeOut, cb);
            GetTempWorker().AddTask(task);
        }
        public void DownloadFile(int id, string url, string savePath, int timeOut, Action<bool,string> cb)
        {
            HttpDownloadTask task = new HttpDownloadTask(url, savePath, timeOut, cb);
            GetWorker(id).AddTask(task);
        }
        public void WebRequestGet(string url,Action<bool,string> cb,int id = 0)
        {
            WebTask task = new WebTask(url, "GET", cb);
            GetWorker(id).AddTask(task);
        }

        public void GetIpFromUrl(string url,bool ipv6,Action<string> cb)
        {
            GetIpTask task = new GetIpTask(url,ipv6,cb);
            GetTempWorker().AddTask(task);
        }

        public void AddTask(int id, WebTask task)
        {
            GetWorker(id).AddTask(task);
        }

        public void AddTmpTask(WebTask task)
        {
            GetTempWorker().AddTask(task);
        }
        public void UpdateDone()
        {
            if (idCount > 0)
            {
                for (int i = 0; i < idCount; ++i)
                    workers[i].UpdateDone();
            }

            if (tmpCount > 0)
            {
                for (int i = 0; i < tmpCount;)
                {
                    UnityCbTaskExecutor tmp = tmpExecutors[i];
                    if (tmp.UpdateDone())
                    {
                        tmpExecutors.RemoveAt(i);
                        tmpCount--;
                    }
                    else
                    {
                        ++i;
                    }
                }
            }
        }
        public void ShutDown()
        {
            if (idCount > 0)
            {
                for (int i = 0; i < idCount; ++i)
                    workers[i].ShutDown();
                workers.Clear();
                idCount = 0;
            }

            if (tmpCount > 0)
            {
                for (int i = 0; i < tmpCount; ++i)
                    tmpExecutors[i].ShutDown();
                tmpExecutors.Clear();
                tmpCount = 0;
            }
        }
        #endregion
        #region Private Method
        private UnityCbTaskExecutor GetWorker(int id,bool autoShutDown = false)
        {
            for (int i = 0; i < idCount; ++i)
            {
                if (workers[i].Id == id)
                    return workers[i];
            }

            UnityCbTaskExecutor we = new UnityCbTaskExecutor(id.ToString(), true, autoShutDown) {Id = id};
            workers.Add(we);
            ++idCount;
            return we;
        }

        private UnityCbTaskExecutor GetTempWorker()
        {
            UnityCbTaskExecutor we = new UnityCbTaskExecutor("tmp", true, true);
            tmpExecutors.Add(we);
            ++tmpCount;
            return we;
        }
        #endregion
    }
}
