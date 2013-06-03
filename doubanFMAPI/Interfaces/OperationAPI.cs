using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using doubanFMAPI.Entities.Song;
using doubanFMAPI.Utilities;

namespace doubanFMAPI.Interfaces
{
    public delegate void PlayListChanged(IEnumerable<Entity> playlist);
    public delegate void PlayListLoadFailed(string errorMessage);
    public delegate void CurrentSongChanged(Entity song);
    public abstract class OperationAPI
    {
        public enum FMType
        {
            NormalFM,                                           //普通兆赫
            SingerFM,                                           //歌手兆赫
            AlbumFM,                                            //专辑兆赫
        };

        public abstract void Like(int channelID, string songID, double songPT);
        public abstract void UnLike(int channelID, string songID, double songPT);
        public abstract void Ban(int channelID, string songID, double songPT);
        public abstract void Next(int channelID, string songID, double songPT);
        public abstract void NaturalEnd(int channelID, string songID, double songPT);
    }
}
