using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace doubanFMAPI.Utilities
{
    public class WebConnection
    {
        private static CookieContainer cookie = new CookieContainer();
        private static string DefaultUserAgent = "Chrome/14.0.835.186 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
        private enum Method
        {
            Get,
            Post,
        }

        /// <summary>
        /// Get方式获取网页内容
        /// </summary>
        /// <param name="url">网页地址</param>
        /// <param name="userAgent">用户代理</param>
        /// <param name="encoding">网页编码</param>
        /// <returns>返回内容</returns>
        public static string GetCommand(string url, string userAgent, Encoding encoding)
        {
            string getData = string.Empty;
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "GET";
                request.UserAgent = DefaultUserAgent;
                if (!string.IsNullOrEmpty(userAgent))
                {
                    request.UserAgent = userAgent;
                }
                request.CookieContainer = LoadCookie();
                using (HttpWebResponse responce = request.GetResponse() as HttpWebResponse)
                using (StreamReader sr = new StreamReader(responce.GetResponseStream(), encoding))
                    getData = sr.ReadToEnd();
            }
            catch
            {
                //throw;
            }
            return getData;
        }

        /// <summary>
        /// Get方式获取网页内容（重载），默认无代理且网页编码为UTF8
        /// </summary>
        /// <param name="url">网页地址</param>
        /// <returns>返回内容</returns>
        public static string GetCommand(string url)
        {
            return GetCommand(url, string.Empty, Encoding.UTF8);
        }

        /// <summary>
        /// Post方式获取网页内容
        /// </summary>
        /// <param name="baseUrl">请求基地址</param>
        /// <param name="parameters">请求地址参数列表</param>
        /// <param name="userAgent">用户代理</param>
        /// <param name="encoding">网页编码</param>
        /// <returns>返回内容</returns>
        public static string PostCommand(string baseUrl, IEnumerable<KeyValuePair<string, object>> parameters, string userAgent, Encoding encoding)
        {
            string getData = string.Empty;
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                int i = 0;
                foreach (var item in parameters)
                {
                    if (i > 0)
                    {
                        stringBuilder.AppendFormat("&{0}={1}", item.Key, item.Value);
                    }
                    else
                    {
                        stringBuilder.AppendFormat("{0}={1}", item.Key, item.Value);
                    }
                    i++;
                }
                byte[] content = encoding.GetBytes(stringBuilder.ToString());
                HttpWebRequest request = WebRequest.Create(baseUrl) as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = DefaultUserAgent;
                if (!string.IsNullOrEmpty(userAgent))
                {
                    request.UserAgent = userAgent;
                }
                request.CookieContainer = LoadCookie();
                using (Stream requestStream = request.GetRequestStream())
                    requestStream.Write(content, 0, content.Length);
                using (HttpWebResponse responce = request.GetResponse() as HttpWebResponse)
                using (StreamReader sr = new StreamReader(responce.GetResponseStream(), encoding))
                    getData = sr.ReadToEnd();
                SaveCookie();           //保存Cookies
            }
            catch
            {
                //throw;
            }
            return getData;
        }

        /// <summary>
        /// Post方式获取网页内容（重载），默认无代理且网页编码为UTF8
        /// </summary>
        /// <param name="baseUrl">请求基地址</param>
        /// <param name="parameters">请求地址参数列表</param>
        /// <returns>返回内容</returns>
        public static string PostCommand(string baseUrl, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            return PostCommand(baseUrl, parameters, string.Empty, Encoding.UTF8);
        }

        /// <summary>
        /// 保存Cookie
        /// </summary>
        /// <returns>返回是否正确保存Cookie结果</returns>
        public static bool SaveCookie()
        {
            BinarySerialization<CookieContainer> bs = new BinarySerialization<CookieContainer>(cookie);
            return bs.Serialization("cookie.dat");
        }

        /// <summary>
        /// 加载Cookie
        /// </summary>
        /// <returns>返回加载的Cookie</returns>
        public static CookieContainer LoadCookie()
        {
            BinarySerialization<CookieContainer> bs = new BinarySerialization<CookieContainer>(cookie);
            cookie = bs.DeSerialization("cookie.dat");
            if (cookie == null)
                cookie = new CookieContainer();
            return cookie;
        }

        /// <summary>
        /// 清除Cookie并保存
        /// </summary>
        public static void ClearCookie()
        {
            cookie = new CookieContainer();
            SaveCookie();
        }
    }
}
