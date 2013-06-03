using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using doubanFMAPI.Entities.MoreInfo;
using doubanFMAPI.Utilities;
using doubanFMAPI.Entities.Song;
using doubanFMAPI.Entities.OpenChannel;

namespace doubanFMAPI.Interfaces
{
    internal class MoreInfoAPI
    {
        /// <summary>
        /// 获取歌曲歌词(适合NormalFM来获取歌词)
        /// </summary>
        /// <param name="song">歌曲对象</param>
        public void GetLyric(ref SongEntity song)
        {
            MoreInfoEntity moreInfo = GetMoreInfo(song.SongID);
            if (moreInfo == null)
                return;
            string lyricString = moreInfo.Lyric;
            if (string.IsNullOrEmpty(lyricString))
                return;
            song.Lyrics = LyricTrans(lyricString);
        }

        /// <summary>
        /// 获取歌曲对象（适合歌手兆赫或者专辑兆赫）
        /// </summary>
        /// <param name="searchedSong">搜索到的歌曲</param>
        /// <returns>歌曲对象</returns>
        public static SongEntity GetSongEntity(SearchedSongEntity searchedSong)
        {
            MoreInfoEntity moreInfo = GetMoreInfo(searchedSong.SongID.ToString());
            if (moreInfo == null || moreInfo.Albums.Count == 0)
                return null;
            SongEntity song = new SongEntity();
            string albumPath = searchedSong.Album.AlbumPath;
            song.AlbumID = albumPath.Substring(32, albumPath.Length - 33);
            song.AlbumName = searchedSong.Album.AlbumName;
            song.AlbumPath = albumPath;
            song.AlbumPicture = moreInfo.Albums[0].AlbumPicture.Replace("spic","lpic");
            song.Company = moreInfo.Albums[0].Publisher;
            song.IsLiked = "0";
            song.PublishTime = moreInfo.Albums[0].PublishDate.Substring(0,4);
            song.RatingScore = double.Parse(moreInfo.AlbumRating);
            song.SingerName = searchedSong.SingerName;
            song.SongID = searchedSong.SongID.ToString();
            song.Kbps = "64";
            song.SongName = searchedSong.SongName;
            song.SongUrl = searchedSong.SongUrl;
            song.Ssid = string.Empty;
            song.SubType = string.Empty;
            song.Lyrics = LyricTrans(moreInfo.Lyric);
            return song;
        }

        /// <summary>
        /// 获取歌曲歌手的更多信息
        /// </summary>
        /// <param name="songID">歌曲的ID号</param>
        /// <returns>MoreInfo对象</returns>
        private static MoreInfoEntity GetMoreInfo(string songID)
        {
            try
            {
                string getData = WebConnection.GetCommand(string.Format(@"http://music.douban.com/api/song/info?song_id={0}", songID));
                MoreInfoEntity moreInfo = MoreInfoEntity.Json2Object(getData);
                if (moreInfo == null)
                    return null;
                return moreInfo;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将获取到的歌词字符串转换为储存歌词的键值对
        /// </summary>
        /// <param name="lyricString">歌词字符串</param>
        /// <returns>储存歌词结果的键值对</returns>
        private static Dictionary<TimeSpan, string> LyricTrans(string lyricString)
        {
            if (string.IsNullOrEmpty(lyricString))
                return null;
            Dictionary<TimeSpan, string> lyrics = new Dictionary<TimeSpan, string>();
            string[] lines = lyricString.Replace(@"\'", "'").Split(new char[2] { '\r', '\n' });
            for (int i = 0; i < lines.Length; i++)
            {
                string iLine = lines[i];
                if (!string.IsNullOrEmpty(iLine))
                {
                    MatchCollection iTimeMC = Regex.Matches(iLine, @"\[(\d+):(\d+(\.\d+)?)\]");
                    //Match iLyricMC = Regex.Match(iLine, @"(?<=(\[(\d+):(\d+(\.\d+)?)\])+)(\s\w+)");
                    //string iLyric = iLyricMC.Groups[0].Value;
                    foreach (Match mc in iTimeMC)
                    {
                        iLine = iLine.Replace(mc.Groups[0].Value,string.Empty);
                    }
                    foreach (Match mc in iTimeMC)
                    {
                        int iMin = Int32.Parse(mc.Groups[1].Value);
                        double iSec = double.Parse(mc.Groups[2].Value);
                        TimeSpan iTimeSpan = new TimeSpan(0, 0, iMin, (int)Math.Floor(iSec), (int)((iSec - Math.Floor(iSec)) * 1000));
                        lyrics[iTimeSpan] = iLine;
                    }
                }
            }
            return lyrics;
        }
    }
}
