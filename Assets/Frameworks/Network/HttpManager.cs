using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using System.IO;

#pragma warning disable 219

namespace Framework.Network
{
    public partial class HttpManager
    {
        private static HttpManager instance = null;
        public static HttpManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new HttpManager();

                return instance;
            }

            private set { instance = value; }
        }

        //给URL添加随机后缀，以绕过CDN缓存强制访问
        static System.Random m_rand = new System.Random();
        public static string AppendRandomStringToUrl(string strUrl)
        {
            char cSep = (strUrl.IndexOf('?') >= 0) ? '&' : '?';
            string strRandom = m_rand.Next().ToString();
            return string.Format("{0}{1}r={2}", strUrl, cSep, strRandom);
        }

        //异步下载UTF8编码的文件
        public static IEnumerable AsyncDownloadUTF8FileByUrl(string strUrl, int iTimeout, CReturnTuple<bool, string> ret)
        {
            ret.value_0 = false;

            CDownloadState downloadState = new CDownloadState();
            CWebClient webClient = new CWebClient(iTimeout);
            webClient.DownloadDataCompleted += (object sender, DownloadDataCompletedEventArgs arg) =>
            {
                if (arg.Cancelled)
                    downloadState.Cancel = true;
                else if (arg.Error != null)
                    downloadState.Ex = arg.Error;
                else
                    downloadState.Result = arg.Result;

                downloadState.Done = true;
            };

            webClient.DownloadDataAsync(new Uri(AppendRandomStringToUrl(strUrl)));

            while (!downloadState.Done)
                yield return null;

            if (!downloadState.Cancel && downloadState.Ex == null)
            {
                ret.value_0 = true;
                ret.value_1 = Encoding.UTF8.GetString(downloadState.Result);
            }
        }

        public static bool HttpGet(string url,string userAgent, CookieCollection cookies, int Timeout, out string errMsg, out string returnData)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            //request.UserAgent = DefaultUserAgent;
            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            request.Timeout = Timeout;
            
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            HttpWebResponse res = null;
            errMsg = "";
            returnData = "";

            try
            {
                   res = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                errMsg = ex.Message;
            }
            if (res!= null)
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(res.GetResponseStream(), Encoding.UTF8);
                string retString = reader.ReadToEnd();
                
                try
                {
                    returnData = retString;
                }
                catch (Exception)
                {
                    returnData = retString;
                }
                if (request != null)
                {
                    request.Abort();
                }
                if (res != null)
                {
                    res.Close();
                }
            }

            return true;
        }

        public static bool HttpPost(string Url, string postDataStr,bool KeepAlive, int Timeout, out string errMsg, out string returnData)
        {
                //System.GC.Collect();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postDataStr.Length;
                request.KeepAlive = KeepAlive;
                request.Timeout = Timeout;
                byte[] payload;
                payload = System.Text.Encoding.UTF8.GetBytes(postDataStr);
                request.ContentLength = payload.Length;
                System.IO.Stream writer = request.GetRequestStream();
                writer.Write(payload, 0, payload.Length);
                writer.Close();
                HttpWebResponse res = null;
                errMsg = "";
                try
                {
                    System.Net.ServicePointManager.DefaultConnectionLimit = 200;
                    res = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                //res = (HttpWebResponse)ex.Response;
                    errMsg = ex.Message;

                }

                System.IO.StreamReader reader = new System.IO.StreamReader(res.GetResponseStream(), Encoding.UTF8);
                string retString = reader.ReadToEnd();
                
                try
                {
                    returnData = retString;
                }
                catch (Exception)
                {
                    returnData = retString;
                }
                if (request != null)
                {
                    request.Abort();
                }
                if (res != null)
                {
                    res.Close();
                }
                return true;
        }

        //---------------------------------------------------------------------------------------------
        //异步方法

        public class RequestState
        {
            public const int BUFFER_SIZE = 1024 * 64;
            public byte[] m_bufferRead;
            public HttpWebRequest m_request;
            public HttpWebResponse m_response;
            public Stream m_streamResponse;
            public IHttpDownloadCallback m_downloadCallback;
            public FileStream m_continueFileStream;
            public object m_param;
            public RequestState()
            {
                m_bufferRead = new byte[BUFFER_SIZE];
                m_request = null;
                m_streamResponse = null;
            }
        }

        public void BeginDownload(string url, IHttpDownloadCallback callback, object param)
        {
            BeginDownload(url, callback, null, param);
        }

        public void BeginDownload(string url, IHttpDownloadCallback callback, FileStream continueFileStream, object param)
        {
            RequestState requestState = new RequestState();
            requestState.m_downloadCallback = callback;
            requestState.m_param = param;

            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.Timeout = 10000;
                request.ReadWriteTimeout = 10000;
                Debug.Log("begin send [get] rul:" + url);

                if (continueFileStream != null)
                {
                    requestState.m_continueFileStream = continueFileStream;
                    continueFileStream.Seek(continueFileStream.Length, SeekOrigin.Current);
                    request.AddRange((int)(continueFileStream.Length));
                }

                requestState.m_request = request;

                IAsyncResult result = request.BeginGetResponse(ResponseCallback, requestState);
            }
            catch (Exception e)
            {
                callback.OnError(param, e);
            }
        }

        public void ResponseCallback(IAsyncResult ar)
        {
            RequestState requestState = ar.AsyncState as RequestState;
            try
            {
                HttpWebRequest request = requestState.m_request;

                requestState.m_response = request.EndGetResponse(ar) as HttpWebResponse;
                Stream stream = requestState.m_response.GetResponseStream();
                requestState.m_downloadCallback.OnEndGetResponse(requestState.m_response);
                requestState.m_downloadCallback.OnPrepare(requestState.m_response.ContentLength, requestState.m_continueFileStream, requestState.m_param);

                requestState.m_streamResponse = stream;
                stream.BeginRead(requestState.m_bufferRead, 0, RequestState.BUFFER_SIZE, _ReadCallback, requestState);
            }
            catch (Exception e)
            {
                requestState.m_downloadCallback.OnError(requestState.m_param, e);
            }
        }

        private void _ReadCallback(IAsyncResult ar)
        {
            RequestState requestState = ar.AsyncState as RequestState;
            try
            {
                int readLen = requestState.m_streamResponse.EndRead(ar);
                if (readLen > 0)
                {
                    byte[] buffer = new byte[readLen];
                    Array.Copy(requestState.m_bufferRead, 0, buffer, 0, readLen);

                    requestState.m_downloadCallback.OnProcess(buffer, requestState.m_param);
                    requestState.m_streamResponse.BeginRead(requestState.m_bufferRead, 0, RequestState.BUFFER_SIZE, _ReadCallback, requestState);
                }
                else
                {
                    requestState.m_streamResponse.Close();
                    requestState.m_response.Close();
                    requestState.m_downloadCallback.OnComplate(requestState.m_param);
                }
            }
            catch (Exception e)
            {
                requestState.m_downloadCallback.OnError(requestState.m_param, e);
            }
        }

    }


    //下载相关
    public class CDownloadState
    {
        public volatile System.Exception Ex;
        public volatile bool Cancel;
        public volatile bool Done;
        public volatile byte[] Result;
    }

    public class CWebClient : WebClient
    {
        readonly int m_iTimeout;

        public CWebClient(int iTimeout = 60 * 1000)
        {
            m_iTimeout = iTimeout;
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = m_iTimeout;
            return w;
        }
    }

    public interface IHttpDownloadCallback
    {
        void OnPrepare(long length, FileStream continueFileStream, object param);

        void OnEndGetResponse(HttpWebResponse response);

        void OnProcess(byte[] buffer, object param);

        void OnComplate(object param);

        void OnError(object param, Exception e);
    }
}