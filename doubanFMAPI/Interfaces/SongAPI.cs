using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doubanFMAPI.Entities.Song;
using doubanFMAPI.Utilities;

namespace doubanFMAPI.Interfaces
{
    internal class SongAPI
    {
        public Queue<SongEntity> playList = new Queue<SongEntity>();    //当前播放列表
        public SongEntity currentSong;                                  //当前歌曲         
        public int kbps = 64;
        public bool isPlayListChanged = false;
        public bool isPlayListLoadFailed = false;
        public bool isCurrentSongChanged = false;
        /*public delegate void PlayListChanged(IEnumerable<SongEntity> playlist);
        public delegate void PlayListLoadFailed(string errorMessage);
        public delegate void CurrentSongChanged(SongEntity song);
        public event PlayListChanged playListChangedEvent;
        public event PlayListLoadFailed playListLoadFailedEvent;
        public event CurrentSongChanged currentSongChangedEvent;*/
       /* public enum FMType
        {
            NormalFM,                                           //普通兆赫
            SingerFM,                                           //歌手兆赫
            AlbumFM,                                            //专辑兆赫
        };*/

        /// <summary>
        /// 获取播放列表
        /// </summary>
        /// <param name="currentChannelID">兆赫ID号</param>
        /// <param name="operationType">操作代码，字符串：n-新播放列表，p-加载更多，r-红心，u-取消红心，b-不再播放，s-跳过，e-自然结束</param>
        /// <param name="currentsongID">歌曲ID号</param>
        /// <param name="currentSongPT">加红心时歌曲进度(单位：0.1秒)</param>
        /// <returns>播放列表对象，自然结束时不返回字符串，因此为null</returns>
        private GetPlaylistResult GetNormalPlayList(int currentChannelID = 1, string operationType = "n", string currentsongID = "", double currentSongPT = 0.0)
        {
            string getData = string.Empty;
            Random random = new Random();
		    byte[] bytes = new byte[8];
            random.NextBytes(bytes);
            string r = (BitConverter.ToUInt64(bytes, 0) % 0xFFFFFFFFFF).ToString("x10");
            getData = WebConnection.GetCommand(string.Format("http://douban.fm/j/mine/playlist?type={0}&sid={1}&channel={2}&pt={3}&pb={4}&from=mainsite&r={5}", operationType, currentsongID, currentChannelID, currentSongPT, kbps, r));
            if (string.IsNullOrEmpty(getData))
                return null;
            GetPlaylistResult result = GetPlaylistResult.Json2Object(getData);
            return result;
        }

        /// <summary>
        /// 加载新的播放列表，适用于刚启动或者切换兆赫
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="SongPT">加红心时歌曲进度(单位：0.1秒)</param>
        public void GetNewPlayList(int channelID, string songID = "", double SongPT = 0.0)
        {
            GetPlaylistResult result = GetNormalPlayList(channelID, "n", songID, SongPT);
            if (result == null || result.R != 0 || result.Songs.Count == 0)
            {
                isPlayListLoadFailed = true;
                return;
            }
            playList.Clear();
            foreach (SongEntity song in result.Songs)
            {
                if(song.SubType != "T")
                    playList.Enqueue(song);
            }
            currentSong = playList.Dequeue();
            LoadMore(channelID, string.Empty, 0.0);
            isPlayListChanged = true;
            isCurrentSongChanged = true;
        }

        /// <summary>
        /// 加红心,可能会更换新列表
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">加红心时歌曲进度(单位：0.1秒)</param>
        public void Like(int channelID, string songID, double songPT)
        {
            LikeOrNot(channelID, songID, songPT, true);
        }

        /// <summary>
        /// 取消红心，登陆用户会更换新列表，为登陆用户不更换
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">取消红心时歌曲进度(单位：0.1秒)</param>
        public void UnLike(int channelID, string songID, double songPT)
        {
            LikeOrNot(channelID, songID, songPT, false);
        }

        /// <summary>
        /// 不再播放，登陆用户会更换新列表，为登陆用户不更换
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">不再播放时歌曲进度(单位：0.1秒)</param>
        public void Ban(int channelID, string songID, double songPT)
        {
            BanOrSkip(channelID, songID, songPT, true);
        }

        /// <summary>
        /// 跳过该曲，登陆用户会更换新列表，为登陆用户不更换
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">跳过时歌曲进度(单位：0.1秒)</param>
        public void Next(int channelID, string songID, double songPT)
        {
            BanOrSkip(channelID, songID, songPT, false);
        }

        /// <summary>
        /// 自然结束
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">结束时歌曲进度，为歌曲总长度(单位：0.1秒)</param>
        public void NaturalEnd(int channelID, string songID, double songPT)
        {
            GetPlaylistResult result = GetNormalPlayList(channelID, "e", songID, songPT);
            currentSong = playList.Dequeue();
            LoadMore(channelID, songID, songPT);
            isPlayListChanged = true;
            isCurrentSongChanged = true;
        }

        /// <summary>
        /// 加载更多播放列表，列表中剩余最后一首歌曲时会继续加载新的列表
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">结束时歌曲进度，为歌曲总长度(单位：0.1秒)</param>
        private void LoadMore(int channelID, string songID, double songPT)
        {
            if (playList.Count > 1)
                return;
            GetPlaylistResult result = GetNormalPlayList(channelID, "p", songID, songPT);
            if (result == null || result.R != 0 || result.Songs.Count == 0)
            {
                isPlayListLoadFailed = true;
                return;
            }
            foreach (SongEntity song in result.Songs)
            {
                if (song.SubType != "T")
                    playList.Enqueue(song);
            }
        }

        /// <summary>
        /// 加红心/取消红心操作
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">结束时歌曲进度，为歌曲总长度(单位：0.1秒)</param>
        /// <param name="like">操作标志，true为加红心操作，false为取消红心操作</param>
        private void LikeOrNot(int channelID, string songID, double songPT, bool like)
        {
            GetPlaylistResult result = GetNormalPlayList(channelID, like ? "r" : "u", songID, songPT);
            if (result == null || result.R != 0)
            {
                System.Diagnostics.Debug.WriteLine(like ? "加红心失败！" : "取消红心失败！");
                return;
            }
            else if (result.Songs.Count == 0)
            {
                return;
            }
            playList.Clear();
            foreach (SongEntity song in result.Songs)
            {
                if (song.SubType != "T")
                    playList.Enqueue(song);
            }
            isPlayListChanged = true;
        }

        /// <summary>
        /// 不再播放/跳过该曲操作
        /// </summary>
        /// <param name="channelID">兆赫ID号</param>
        /// <param name="songID">歌曲ID号</param>
        /// <param name="songPT">结束时歌曲进度，为歌曲总长度(单位：0.1秒)</param>
        /// <param name="ban">操作标志，true为不再播放操作，false为跳过该曲操作</param>
        private void BanOrSkip(int channelID, string songID, double songPT, bool ban)
        {
            GetPlaylistResult result = GetNormalPlayList(channelID, ban ? "b" : "s", songID, songPT);
            if (result == null || result.R != 0)
            {
                System.Diagnostics.Debug.WriteLine(ban ? "标记不再播放失败！" : "跳过该曲失败！");
                return;
            }
            else if (result.Songs.Count == 0)
            {
                currentSong = playList.Dequeue();
                LoadMore(channelID, songID, songPT);
            }
            else
            {
                playList.Clear();
                foreach (SongEntity song in result.Songs)
                {
                    if (song.SubType != "T")
                        playList.Enqueue(song);
                }
                currentSong = playList.Dequeue();
                LoadMore(channelID, songID, songPT);
            }
            isPlayListChanged = true;
            isCurrentSongChanged = true;
        }
    }
}
