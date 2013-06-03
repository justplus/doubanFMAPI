using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using doubanFMAPI.Utilities;
using doubanFMAPI.Entities.User;

namespace doubanFMAPI.Interfaces
{
    internal class UserAPI
    {
        internal static readonly string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"doubanFM\userinfo.txt");
        private string captchaID;
        public bool isLoginCompleted = false;
        public LoginResult result = new LoginResult();
        /// <summary>
        /// 豆瓣用户登陆
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="captcha">验证码</param>
        /// <param name="remember">是否记住登陆</param>
        /// <returns>返回登陆结果</returns>
        public void Login(string userName, string password, string captcha, bool remember = true)
        {
            //LoginResult result = new LoginResult();
            //判断用户是否已经登录
            int isLogined = HasLogined();
            if (isLogined == 1)
                GetLoginedUserInfo();
            else if (isLogined == -1)
                result = null;
            else
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters.Add("source", "radio");
                parameters.Add("alias", userName);
                parameters.Add("form_password", password);
                parameters.Add("captcha_solution", captcha);
                parameters.Add("captcha_id", captchaID);
                parameters.Add("task", "sync_channel_list");
                if (remember)
                {
                    parameters.Add("remember", "on");
                }
                string s = WebConnection.PostCommand("http://douban.fm/j/login", parameters);
                result = LoginResult.Json2Object(s);
                if (result != null && result.R == 0)
                {
                    SaveUserInfo(s);
                }
            }
            isLoginCompleted = true;
        }

        /// <summary>
        /// 更新验证码
        /// </summary>
        /// <returns>返回验证码的Url</returns>
        public string UpdateCaptcha()
        {
            string data = WebConnection.GetCommand("http://douban.fm/j/new_captcha");
            if (!string.IsNullOrEmpty(data))
            {
                captchaID = data.Trim('\"');
                return string.Format(@"http://douban.fm/misc/captcha?size=m&id={0}", captchaID);
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// 判断是否已经登录豆瓣FM
        /// </summary>
        /// <returns>-1：网络连接出错 0：未登录 1：已登录</returns>
        public int HasLogined()
        {
            string getData = WebConnection.GetCommand(@"http://douban.fm/");
            if (string.IsNullOrEmpty(getData))
                return -1;
            Match match = Regex.Match(getData, @"豆瓣", RegexOptions.IgnoreCase);
            if (match == null || string.IsNullOrEmpty(match.Groups[0].Value))
                return -1;
            match = Regex.Match(getData, "http://www.douban.com/accounts/login");
            //System.Diagnostics.Debug.WriteLine(match.Groups[0].Value);
            if (!string.IsNullOrEmpty(match.Groups[0].Value))
                return 0;
            else
                return 1;
        }

        /// <summary>
        /// 保存登陆用户的信息
        /// </summary>
        /// <param name="json">用户信息json字符串</param>
        private void SaveUserInfo(string json)
        {
            using (StreamWriter writer = new StreamWriter(DataFolder))
            {
                writer.Write(json);
            }
        }
        /// <summary>
        /// 获取登录用户信息
        /// </summary>
        /// <returns>返回登陆用户信息,未登录用户返回null</returns>
        public void GetLoginedUserInfo()
        {
            /*if (HasLogined() != 1)
                return null;
            Result result = new Result();
            Entity user = new Entity();
            string getData = WebConnection.GetCommand(@"http://douban.fm/");
            MatchCollection mc = Regex.Matches(getData, @"<span id=""user_name"">([^{].*?)\s");
            user.Name = mc[0].Groups[1].Value;
            Match match = Regex.Match(getData, @"累积收听<span id=""rec_played"">(\d+)</span>首");
            user.Record.Played = Int32.Parse(match.Groups[1].Value);
            match = Regex.Match(getData, @"加红心<span id=""rec_liked"">(\d+)</span>首");
            user.Record.Liked = Int32.Parse(match.Groups[1].Value);
            match = Regex.Match(getData, @"<span id=""rec_banned"">(\d+)</span>首不再播放");
            user.Record.Banned = Int32.Parse(match.Groups[1].Value);
            match = Regex.Match(getData, @"http://www.douban.com/accounts/logout?.*?ck=(.*?)&amp");
            user.CK = match.Groups[1].Value;
            return result;*/
            string userInfoString;
            using (StreamReader reader = new StreamReader(DataFolder))
            {
                userInfoString = reader.ReadToEnd();
            }
            result = LoginResult.Json2Object(userInfoString);
            isLoginCompleted = true;
        }

        /// <summary>
        /// 登出豆瓣账户
        /// </summary>
        public void Logout()
        {
            WebConnection.GetCommand(string.Format(@"http://douban.fm/partner/logout?source=radio&ck={0}&no_login=y", result.User.CK));
            WebConnection.ClearCookie();
            SaveUserInfo(string.Empty);
        }
    }
}
