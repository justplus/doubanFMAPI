using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doubanFMAPI.Interfaces;
using doubanFMAPI.Entities;

namespace doubanFMAPI
{
    public class API
    {
        private static API api;
        private UserAPI userAPI;
        private SongAPI songAPI;
        private ChannelAPI channelAPI;
        private OpenChannelAPI openChannelAPI;
        private MoreInfoAPI moreInfoAPI;

        #region 构造函数
        private API()
        {
            userAPI = new UserAPI();
            songAPI = new SongAPI();
            channelAPI = new ChannelAPI();
            openChannelAPI = new OpenChannelAPI();
            moreInfoAPI = new MoreInfoAPI();
        }

        /// <summary>
        /// 获取API对象，单例模式
        /// </summary>
        /// <returns>返回API实例</returns>
        public static API GetInstance()
        {
            if (api == null)
                api = new API();
            return api;
        }
        #endregion

        #region 登陆及用户信息相关
        /// <summary>
        /// 加载或者更新验证码
        /// </summary>
        /// <returns>返回验证码图片的Url地址,若返回空字符串，表示网络连接出错或其他异常</returns>
        public string UpdateCaptcha()
        {
            return userAPI.UpdateCaptcha();
        }

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
            userAPI.Login(userName, password, captcha, remember);
            if (userAPI.isLoginCompleted && loginCompletedEvent != null)
            {
                loginCompletedEvent(userAPI.result);
                userAPI.isLoginCompleted = false;
            }
        }

        /// <summary>
        /// 判断是否已经登录豆瓣FM
        /// </summary>
        /// <returns>-1：网络连接出错 0：未登录 1：已登录</returns>
        public int HasLogined()
        {
            return userAPI.HasLogined();
        }

        /// <summary>
        /// 获取登录用户信息
        /// </summary>
        /// <returns>返回登陆用户信息,未登录用户返回null</returns>
        public void GetLoginedUserInfo()
        {
            userAPI.GetLoginedUserInfo();
            if (userAPI.isLoginCompleted && loginCompletedEvent != null)
            {
                loginCompletedEvent(userAPI.result);
                userAPI.isLoginCompleted = false;
            }
        }

        /// <summary>
        /// 退出豆瓣账户
        /// </summary>
        public void Logout()
        {
            userAPI.Logout();
        }

        public delegate void LoginCompleted(Entities.User.LoginResult result);
        public event LoginCompleted loginCompletedEvent;
        #endregion

        #region 播放歌曲相关(普通兆赫)
        /// <summary>
        /// 加载新的播放列表
        /// 启动应用程序时只需要提供channelID参数即可,播放过程中切换兆赫，需要切换兆赫是播放歌曲的ID号和播放进度
        /// 推荐算法需求（下同）
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="SongPT">加红心时歌曲进度(单位：0.1秒)</param>
        public void GetNewPlayList(int channelID, string songID = "", double SongPT = 0.0)
        {
            songAPI.GetNewPlayList(channelID, songID, SongPT);
            RaisePlayListEvent();
        }

        /// <summary>
        /// 加红心,可能会更换新列表
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">加红心时歌曲进度(单位：0.1秒)</param>
        public void Like(int channelID, string songID, double songPT)
        {
            songAPI.Like(channelID, songID, songPT);
            RaisePlayListEvent();
        }

        /// <summary>
        /// 取消红心,可能会更换新列表
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">取消红心时歌曲进度(单位：0.1秒)</param>
        public void UnLike(int channelID, string songID, double songPT)
        {
            songAPI.UnLike(channelID, songID, songPT);
            RaisePlayListEvent();
        }

        /// <summary>
        /// 不再播放当前歌曲,可能会更换新列表
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">不再播放时歌曲进度(单位：0.1秒)</param>
        public void Ban(int channelID, string songID, double songPT)
        {
            songAPI.Ban(channelID, songID, songPT);
            RaisePlayListEvent();
        }

        /// <summary>
        /// 跳过该曲,可能会更换新列表
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">跳过时歌曲进度(单位：0.1秒)</param>
        public void Next(int channelID, string songID, double songPT)
        {
            songAPI.Next(channelID, songID, songPT);
            RaisePlayListEvent();
        }

        /// <summary>
        /// 自然结束该曲
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">结束时歌曲进度，为歌曲总长度(单位：0.1秒)</param>
        public void NaturalEnd(int channelID, string songID, double songPT)
        {
            songAPI.NaturalEnd(channelID, songID, songPT);
            RaisePlayListEvent();
        }

        public delegate void PlayListChanged(IEnumerable<Entities.Song.SongEntity> playlist);
        public delegate void PlayListLoadFailed();
        public delegate void CurrentSongChanged(Entities.Song.SongEntity song);
        public delegate void PlayListEmptyed();
        public event PlayListChanged playListChangedEvent;
        public event PlayListLoadFailed playListLoadFailedEvent;
        public event CurrentSongChanged currentSongChangedEvent;
        public event PlayListEmptyed playListEmptyedEvent;
        private void RaisePlayListEvent()
        {
            if (songAPI.isCurrentSongChanged && currentSongChangedEvent != null)
            {
                currentSongChangedEvent(songAPI.currentSong);
            }
            if (songAPI.isPlayListChanged && playListChangedEvent != null)
                playListChangedEvent(songAPI.playList);
            if (songAPI.isPlayListLoadFailed && playListLoadFailedEvent != null)
                playListLoadFailedEvent();
            songAPI.isCurrentSongChanged = false;
            songAPI.isPlayListChanged = false;
            songAPI.isPlayListLoadFailed = false;
        }
        #endregion

        #region 频道操作相关

        /// <summary>
        /// 获取初始兆赫
        /// </summary>
        /// <param name="logined">用户是否登陆</param>
        /// <returns>初始兆赫</returns>
        public doubanFMAPI.Entities.Channel.ChannelEntity GetInitChannel(bool logined)
        {
            if (!logined)
                return channelAPI.GetInitChannel();
            else
                return new doubanFMAPI.Entities.Channel.ChannelEntity() { Introduction = "私人兆赫", Name = "私人兆赫", SongsCount = "...", Cover = "Images/Private.png", Banner = "Images/Private.png", Creator = new doubanFMAPI.Entities.Channel.ChannelCreator() { Name = "豆瓣FM" }, ChannelID = 0, HotSongs = new List<String>() };
        }
        /// <summary>
        /// 获取热门兆赫列表
        /// </summary>
        /// <param name="start">起始值</param>
        /// <param name="limit">每次获取的兆赫数目</param>
        public void GetHotChannels(int start, int limit)
        {
            channelAPI.GetHotChannels(start, limit);
            RaiseChannelEvent();
        }

        /// <summary>
        /// 获取上升最快兆赫列表
        /// </summary>
        /// <param name="start">起始值</param>
        /// <param name="limit">每次获取的兆赫数目</param>
        public void GetUpTrendingChannels(int start, int limit)
        {
            channelAPI.GetUpTrendingChannels(start, limit);
            RaiseChannelEvent();
        }

        /// <summary>
        /// 搜索兆赫
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="start">起始值</param>
        /// <param name="limit">每次获取的兆赫数目</param>
        public void GetSearchChannels(string keyword, int start, int limit)
        {
            channelAPI.GetSearchChannels(keyword, start, limit);
            RaiseChannelEvent();
        }

        /// <summary>
        /// 获取用户收藏的兆赫列表
        /// </summary>
        /// <param name="userName">用户名</param>
        public void GetFavoriateChannels(string userName)
        {
            channelAPI.GetFavoriateChannels(userName);
            RaiseChannelEvent();
        }

        /// <summary>
        /// 判断某兆赫是否在收藏列表中
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="channelID">兆赫ID号</param>
        /// <returns>返回是否在收藏列表中</returns>
        public bool IsFavorite(string userName, int channelID)
        {
            return channelAPI.IsFavorite(userName, channelID);
        }
        /// <summary>
        /// 添加兆赫到收藏列表
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="channel">兆赫</param>
        public void AddToFavoriateChannels(string userName, Entities.Channel.ChannelEntity channel)
        {
            channelAPI.AddToFavoriateChannels(userName, channel);
            RaiseChannelEvent();
        }

        /// <summary>
        /// 从收藏列表中移除兆赫
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="channelID">兆赫ID号</param>
        public void RemoveFromFavoriateChannels(string userName, int channelID)
        {
            channelAPI.RemoveFromFavoriateChannels(userName, channelID);
            RaiseChannelEvent();
        }

        public delegate void ChannelChanged(Entities.Channel.GetChannelCollection channelInfo);
        public delegate void ChannelLoadFailed();
        public event ChannelChanged channelChangedEvent;
        public event ChannelLoadFailed channelLoadFailedEvent;
        private void RaiseChannelEvent()
        {
            if (channelAPI.isChannelChanged && channelChangedEvent != null)
                channelChangedEvent(channelAPI.channelInfo);
            if (channelAPI.isChanneLoadFailed && channelLoadFailedEvent != null)
                channelLoadFailedEvent();
            channelAPI.isChanneLoadFailed = false;
            channelAPI.isChannelChanged = false;
        }
        #endregion

        #region 歌手专辑兆赫操作相关
        /// <summary>
        /// 获取歌手兆赫播放列表
        /// </summary>
        /// <param name="singerName">歌手名</param>
        public void GetSingerFMNewPlayList(string singerName)
        {
            openChannelAPI.GetSingerFMNewPlayList(singerName);
            RaiseOpenChannelEvent();
        }

        /// <summary>
        /// 获取专辑兆赫播放列表
        /// </summary>
        /// <param name="albumName">专辑名</param>
        public void GetAlbumFMNewPlayList(string albumName)
        {
            openChannelAPI.GetAlbumFMNewPlayList(albumName);
            RaiseOpenChannelEvent();
        }

        /// <summary>
        /// 加红心,不更换新列表
        /// </summary>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">加红心时歌曲进度(单位：0.1秒)</param>
        public void Like(string songID, double songPT)
        {
            openChannelAPI.Like(songID, songPT);
        }

        /// <summary>
        /// 取消红心，不更换列表
        /// </summary>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">取消红心时歌曲进度(单位：0.1秒)</param>
        public void UnLike(string songID, double songPT)
        {
            openChannelAPI.UnLike(songID, songPT);
        }

        /// <summary>
        /// 不再播放，在歌手或者专辑兆赫均为下一首
        /// </summary>
        public void Ban()
        {
            openChannelAPI.Ban();
            RaiseOpenChannelEvent();
        }

        /// <summary>
        /// 跳过该曲，在歌手或者专辑兆赫均为下一首
        /// </summary>
        public void Next()
        {
            openChannelAPI.Next();
            RaiseOpenChannelEvent();
        }

        /// <summary>
        /// 自然结束，在歌手或者专辑兆赫均为下一首
        /// </summary>
        public void NaturalEnd()
        {
            openChannelAPI.NaturalEnd();
            RaiseOpenChannelEvent();
        }

        private void RaiseOpenChannelEvent()
        {
            if (openChannelAPI.isCurrentSongChanged && currentSongChangedEvent != null)
                currentSongChangedEvent(openChannelAPI.currentSong);
            if (openChannelAPI.isPlayListChanged && playListChangedEvent != null)
                playListChangedEvent(openChannelAPI.playList);
            if (openChannelAPI.isPlayListLoadFailed && playListLoadFailedEvent != null)
                playListLoadFailedEvent();
            if (openChannelAPI.isPlayListEmpty && playListEmptyedEvent != null)
                playListEmptyedEvent();
            openChannelAPI.isCurrentSongChanged = false;
            openChannelAPI.isPlayListChanged = false;
            openChannelAPI.isPlayListLoadFailed = false;
            openChannelAPI.isPlayListEmpty = false;
        }
        #endregion

        #region 歌词操作及其他变量
        /// <summary>
        /// 获取歌曲歌词(适合NormalFM来获取歌词)
        /// </summary>
        /// <param name="song">歌曲对象</param>
        public void GetLyric(ref Entities.Song.SongEntity song)
        {
            moreInfoAPI.GetLyric(ref song);
        }

        /// <summary>
        /// 兆赫类型
        /// </summary>
        public enum FMType
        {
            NormalFM,
            SingerFM,
            AlbumFM
        }
        #endregion
    }
}
