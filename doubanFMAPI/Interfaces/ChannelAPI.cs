using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using doubanFMAPI.Utilities;
using doubanFMAPI.Entities.Channel;

namespace doubanFMAPI.Interfaces
{
    internal class ChannelAPI
    {
        internal static readonly string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"doubanFM\favchannels.txt");
        public GetChannelCollection channelInfo;
        //private GetChannelCollection favChannelInfo = new GetChannelCollection();
        private FavChannelCollection favChannels;
        public bool isChannelChanged = false;
        public bool isChanneLoadFailed = false;

        /// <summary>
        /// 获取初始兆赫
        /// </summary>
        /// <returns>返回初始兆赫</returns>
        public ChannelEntity GetInitChannel()
        {
            string getData = WebConnection.GetCommand("http://douban.fm/j/explore/hot_channels?start=0&limit=1");
            GetChannelResult getResult = GetChannelResult.Json2Object(getData);
            return getResult.Data.Channels[0];
        }
        /// <summary>
        /// 获取热门兆赫列表
        /// </summary>
        /// <param name="start">起始值</param>
        /// <param name="limit">每次获取的兆赫数目</param>
        public void GetHotChannels(int start,int limit)
        {
            string url = string.Format("http://douban.fm/j/explore/hot_channels?start={0}&limit={1}", start, limit);
            GetChannels(url);
        }

        /// <summary>
        /// 获取上升最快兆赫列表
        /// </summary>
        /// <param name="start">起始值</param>
        /// <param name="limit">每次获取的兆赫数目</param>
        public void GetUpTrendingChannels(int start, int limit)
        {
            string url = string.Format("http://douban.fm/j/explore/up_trending_channels?start={0}&limit={1}", start, limit);
            GetChannels(url);
        }

        /// <summary>
        /// 搜索兆赫
        /// </summary>
        /// <param name="keyword">关键字</param>
        /// <param name="start">起始值</param>
        /// <param name="limit">每次获取的兆赫数目</param>
        public void GetSearchChannels(string keyword, int start, int limit)
        {
            string url = string.Format("http://douban.fm/j/explore/search?query={0}&start={1}&limit={2}", keyword, start, limit);
            GetChannels(url);
        }

        /// <summary>
        /// 获取用户收藏的兆赫列表
        /// </summary>
        /// <param name="userName">用户名</param>
        public void GetFavoriateChannels(string userName)
        {
            GetFavChannels(userName);
            if (channelInfo == null)
                isChanneLoadFailed = true;
            else
                isChannelChanged = true;
        }

        /// <summary>
        /// 添加兆赫到收藏列表
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="channel">兆赫</param>
        public void AddToFavoriateChannels(string userName, ChannelEntity channel)
        {
            GetFavoriateChannels(userName);
            if (favChannels.AllFavChannels == null)
            {
                GetChannelCollection col = new GetChannelCollection();
                List<ChannelEntity> chList = new List<ChannelEntity>();
                chList.Add(channel);
                col.Channels = chList;
                col.ChannelCount = 1;
                FavChannelEntity entity = new FavChannelEntity() { UserName = userName, FavChannels = col };
                List<FavChannelEntity> list = new List<FavChannelEntity>();
                list.Add(entity);
                favChannels.AllFavChannels = list;
            }
            else
            {
                bool findUser = false;
                foreach (FavChannelEntity entity in favChannels.AllFavChannels)
                {
                    if (entity.UserName == userName)
                    {
                        entity.FavChannels.Channels.Add(channel);
                        entity.FavChannels.ChannelCount++;
                        findUser = true;
                        break;
                    }
                }
                if (!findUser)
                {
                    GetChannelCollection col = new GetChannelCollection();
                    List<ChannelEntity> chList = new List<ChannelEntity>();
                    chList.Add(channel);
                    col.Channels = chList;
                    col.ChannelCount = 1;
                    FavChannelEntity entity = new FavChannelEntity() { UserName = userName, FavChannels = col };
                    List<FavChannelEntity> list = new List<FavChannelEntity>();
                    list.Add(entity);
                    favChannels.AllFavChannels.Add(entity);
                }
            }
            using (StreamWriter writer = new StreamWriter(DataFolder))
            {
                writer.Write(FavChannelCollection.Object2Json(favChannels));
            }
        }

        /// <summary>
        /// 从收藏列表中移除兆赫
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="channelID">兆赫ID号</param>
        public void RemoveFromFavoriateChannels(string userName, int channelID)
        {
            GetFavoriateChannels(userName);
            foreach (FavChannelEntity entity in favChannels.AllFavChannels)
            {
                if (entity.UserName == userName)
                {
                    foreach (ChannelEntity en in entity.FavChannels.Channels)
                    {
                        if (en.ChannelID == channelID)
                        {
                            entity.FavChannels.Channels.Remove(en);
                            entity.FavChannels.ChannelCount--;
                            break;
                        }
                    }
                    break;
                }
            }
            using (StreamWriter writer = new StreamWriter(DataFolder))
            {
                writer.Write(FavChannelCollection.Object2Json(favChannels));
            }
        }

        /// <summary>
        /// 判断某兆赫是否在收藏列表中
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="channelID">兆赫ID号</param>
        /// <returns>返回是否在收藏列表中</returns>
        public bool IsFavorite(string userName, int channelID)
        {
            GetFavoriateChannels(userName);
            if (favChannels.AllFavChannels != null)
            {
                foreach (FavChannelEntity entity in favChannels.AllFavChannels)
                {
                    if (entity.UserName == userName)
                    {
                        foreach (ChannelEntity en in entity.FavChannels.Channels)
                        {
                            if (en.ChannelID == channelID)
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 获取兆赫列表
        /// </summary>
        /// <param name="url">Url</param>
        private void GetChannels(string url)
        {
            channelInfo = null;
            string getData = WebConnection.GetCommand(url);
            GetChannelResult getResult = GetChannelResult.Json2Object(getData);
            if (getResult == null || getResult.Status == false)
                isChanneLoadFailed = true;
            else
            {
                channelInfo = getResult.Data;
                isChannelChanged = true;
            }
        }

        private void GetFavChannels(string userName)
        {
            favChannels = new FavChannelCollection();
            if (!File.Exists(DataFolder))
            {
                InitFavChannel(userName);
            }
            else
            {
                string favChannelString;
                using (StreamReader reader = new StreamReader(DataFolder))
                {
                    favChannelString = reader.ReadToEnd();
                }
                if (string.IsNullOrEmpty(favChannelString))
                    return;
                favChannels = FavChannelCollection.Json2Object(favChannelString);
                if (favChannels == null)
                    return;
                else
                {
                    bool findUser = false;
                    foreach (FavChannelEntity entity in favChannels.AllFavChannels)
                    {
                        if (entity.UserName == userName)
                        {
                            findUser = true;
                            channelInfo = entity.FavChannels;
                            break;
                        }
                    }
                    if (!findUser && userName != "")
                        InitFavChannel(userName);
                }
            }
        }

        private void InitFavChannel(string userName)
        {
            if (string.IsNullOrEmpty(userName))
            {
                channelInfo = new GetChannelCollection();
                channelInfo.Channels = new List<ChannelEntity>();
                channelInfo.ChannelCount = 0;
                using (StreamWriter writer = new StreamWriter(DataFolder))
                {
                    writer.Write(string.Empty);
                }
            }
            else
            {
                ChannelEntity privateCh = new ChannelEntity() { Introduction = "私人兆赫", Name = "私人兆赫", SongsCount = "...", Cover = "Images/Private.png", Banner = "Images/Private.png", Creator = new ChannelCreator() { Name = "豆瓣FM" }, ChannelID = 0, HotSongs = new List<String>() };
                ChannelEntity redHeartCh = new ChannelEntity() { Introduction = "红心兆赫", Name = "红心兆赫", SongsCount = "...", Cover = "Images/RedHeart.jpg", Banner = "Images/RedHeart.png", Creator = new ChannelCreator() { Name = "豆瓣FM" }, ChannelID = -3, HotSongs = new List<String>() };
                //GetChannelCollection chs = new GetChannelCollection();
                List<ChannelEntity> chList = new List<ChannelEntity>();
                chList.Add(privateCh);
                chList.Add(redHeartCh);
                channelInfo = new GetChannelCollection();
                channelInfo.Channels = chList;
                channelInfo.ChannelCount = 2;
                FavChannelEntity entity = new FavChannelEntity() { UserName = userName, FavChannels = channelInfo };
                List<FavChannelEntity> list = new List<FavChannelEntity>();
                list.Add(entity);
                favChannels.AllFavChannels = list;
                using (StreamWriter writer = new StreamWriter(DataFolder))
                {
                    writer.Write(FavChannelCollection.Object2Json(favChannels));
                }
            }
        }
    }
}
