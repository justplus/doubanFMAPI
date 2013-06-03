using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doubanFMAPI.Utilities;
using doubanFMAPI.Entities.Song;
using doubanFMAPI.Entities.OpenChannel;

namespace doubanFMAPI.Interfaces
{
    
    internal class OpenChannelAPI
    {
        public Queue<SongEntity> playList = new Queue<SongEntity>();       //当前播放列表
        public SongEntity currentSong;                                     //当前歌曲
        private int loadCount = 20;                                         //最多加载搜索结果

        public bool isPlayListChanged = false;
        public bool isPlayListLoadFailed = false;
        public bool isCurrentSongChanged = false;
        public bool isPlayListEmpty = false;

        /// <summary>
        /// 获取新播放列表（仅歌手兆赫和专辑兆赫）
        /// </summary>
        /// <param name="type">兆赫类型：1-歌手兆赫 2-专辑兆赫 其他-搜索兆赫</param>
        /// <param name="singerName">歌手名</param>
        /// <param name="albumName">专辑名</param>
        /// <param name="keyword">关键字</param>
        /// <returns>SearchedSongResult对象</returns>
        private SearchedSongResult GetNewPlayList(int type, string singerName, string albumName, string keyword)
        {
            string getData = string.Empty;
            if (type == 1)
                getData = WebConnection.GetCommand(string.Format(@"http://douban.fm/j/open_channel/creation/search?keyword={0}&cate=singer&limit={1}", singerName, loadCount));
            else if(type == 2)
                getData = WebConnection.GetCommand(string.Format(@"http://douban.fm/j/open_channel/creation/search?keyword={0}&cate=album&limit={1}", albumName, loadCount));
            else
                getData = WebConnection.GetCommand(string.Format(@"http://douban.fm/j/open_channel/creation/search?keyword={0}&cate=misc&limit={1}", keyword, loadCount));
            if (string.IsNullOrEmpty(getData))
            {
                isPlayListLoadFailed = true;
                return null;
            }
            return SearchedSongResult.Json2Object(getData);
        }

        /// <summary>
        /// 获取歌手兆赫播放列表
        /// </summary>
        /// <param name="singerName">歌手名</param>
        public void GetSingerFMNewPlayList(string singerName)
        {
            SearchedSongResult result = GetNewPlayList(1, singerName, string.Empty, string.Empty);
            if (result == null || result.SearchStatus == false)
            {
                isPlayListLoadFailed = true;
            }
            else if (result.SearchResult.SongsCount == 0)
            {
                isPlayListEmpty = true;
            }
            else
            {
                playList.Clear();
                foreach (SearchedSongEntity searchedSong in result.SearchResult.Songs)
                {
                    if (searchedSong.IsDeleted == false)
                    {
                        SongEntity song = MoreInfoAPI.GetSongEntity(searchedSong);
                        if (song != null)
                            playList.Enqueue(song);
                    }
                }
                EmptyCheck();
            }
        }

        /// <summary>
        /// 获取专辑兆赫播放列表
        /// </summary>
        /// <param name="albumName">专辑名</param>
        public void GetAlbumFMNewPlayList(string albumName)
        {
            SearchedSongResult result = GetNewPlayList(2, string.Empty, albumName, string.Empty);
            if (result == null || result.SearchStatus == false)
            {
                isPlayListLoadFailed = true;
            }
            else if (result.SearchResult.SongsCount == 0)
            {
                isPlayListEmpty = true;
            }
            else
            {
                playList.Clear();
                foreach (SearchedSongEntity searchedSong in result.SearchResult.Songs)
                {
                    if (searchedSong.IsDeleted == false)
                    {
                        SongEntity song = MoreInfoAPI.GetSongEntity(searchedSong);
                        if (song != null)
                            playList.Enqueue(song);
                    }
                }
                EmptyCheck();
            }
        }
        /*
        /// <summary>
        /// 标记红心，（需要用户登陆）
        /// </summary>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="ck">用户CK码</param>
        /// <returns>返回是否成功标记红心</returns>
        public bool Like(string songID,string ck)
        { 
            Dictionary<string,object> parameters=new Dictionary<string,object>();
            parameters.Add("action","y");
            parameters.Add("ck",ck);
            string getData = WebConnection.PostCommand(string.Format(@"http://music.douban.com/j/song/{0}/interest", songID), parameters);
            return getData == "y" ? true : false;
        }

        /// <summary>
        /// 取消红心（需要用户登陆）
        /// </summary>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="ck">用户CK码</param>
        /// <returns>回是否成功取消红心</returns>
        public bool UnLike(string songID,string ck)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("action", "n");
            parameters.Add("ck", ck);
            string getData = WebConnection.PostCommand(string.Format(@"http://music.douban.com/j/song/{0}/interest", songID), parameters);
            return getData == "n" ? true : false;
        }
        */

        /// <summary>
        /// 加红心,不更换新列表
        /// </summary>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">加红心时歌曲进度(单位：0.1秒)</param>
        public void Like(string songID, double songPT)
        {
            LikeOrNot(songID, songPT, true);
        }

        /// <summary>
        /// 取消红心，不更换列表
        /// </summary>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">取消红心时歌曲进度(单位：0.1秒)</param>
        public void UnLike(string songID, double songPT)
        {
            LikeOrNot(songID, songPT, false);
        }

        /// <summary>
        /// 禁止播放，在歌手或者专辑兆赫均为下一首
        /// </summary>
        public void Ban()
        {
            EmptyCheck();
        }

        /// <summary>
        /// 跳过该曲，在歌手或者专辑兆赫均为下一首
        /// </summary>
        public void Next()
        {
            EmptyCheck();
        }

        /// <summary>
        /// 自然结束，在歌手或者专辑兆赫均为下一首
        /// </summary>
        public void NaturalEnd()
        {
            EmptyCheck();
        }

        /// <summary>
        /// 判断播放列表是否为空
        /// </summary>
        private void EmptyCheck()
        {
            if (playList.Count == 0)
            {
                isPlayListEmpty = true;
            }
            else
            {
                currentSong = playList.Dequeue();
                isPlayListChanged = true;
                isCurrentSongChanged = true;
            }
        }


        /// <summary>
        /// 获取播放列表
        /// </summary>
        /// <param name="operationType">操作代码，字符串：n-新播放列表，p-加载更多，r-红心，u-取消红心，b-不再播放，s-跳过，e-自然结束</param>
        /// <param name="currentsongID">歌曲ID号</param>
        /// <param name="currentSongPT">加红心时歌曲进度(单位：0.1秒)</param>
        /// <returns>播放列表对象，自然结束时不返回字符串，因此为null</returns>
        private GetPlaylistResult GetNormalPlayList(string operationType = "n", string currentsongID = "", double currentSongPT = 0.0)
        {
            string getData = string.Empty;
            Random random = new Random();
            byte[] bytes = new byte[8];
            random.NextBytes(bytes);
            string r = (BitConverter.ToUInt64(bytes, 0) % 0xFFFFFFFFFF).ToString("x10");
            getData = WebConnection.GetCommand(string.Format("http://douban.fm/j/mine/playlist?type={0}&sid={1}&channel=0&pt={2}&pb=64&from=mainsite&r={3}", operationType, currentsongID, currentSongPT, r));
            if (string.IsNullOrEmpty(getData))
                return null;
            GetPlaylistResult result = GetPlaylistResult.Json2Object(getData);
            return result;
        }

        /// <summary>
        /// 加红心/取消红心操作
        /// </summary>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">结束时歌曲进度，为歌曲总长度(单位：0.1秒)</param>
        /// <param name="like">操作标志，true为加红心操作，false为取消红心操作</param>
        private void LikeOrNot(string songID, double songPT, bool like)
        {
            GetPlaylistResult result = GetNormalPlayList(like ? "r" : "u", songID, songPT);
            if (result == null || result.R != 0)
            {
                System.Diagnostics.Debug.WriteLine(like ? "加红心失败！" : "取消红心失败！");
                return;
            }
        }
    }
}
