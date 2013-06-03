using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.Song
{
    [DataContract]
    public class SongEntity
    {
        [DataMember(Name = "aid")]
        public string AlbumID { get; internal set; }
        [DataMember(Name = "album")]
        public string AlbumPath { get; internal set; }
        [DataMember(Name = "albumtitle")]
        public string AlbumName { get; internal set; }
        [DataMember(Name = "artist")]
        public string SingerName { get; internal set; }
        [DataMember(Name = "company")]
        public string Company { get; internal set; }
        [DataMember(Name = "kbps")]
        public string Kbps { get; internal set; }
        [DataMember(Name = "length")]
        public string SongLength { get; internal set; }
        [DataMember(Name = "like")]
        public string IsLiked { get; internal set; }
        [DataMember(Name = "picture")]
        public string AlbumPicture { get; internal set; }
        [DataMember(Name = "public_time")]
        public string PublishTime { get; internal set; }
        [DataMember(Name = "rating_avg")]
        public double RatingScore { get; internal set; }
        [DataMember(Name = "sid")]
        public string SongID { get; internal set; }
        [DataMember(Name = "ssid")]
        public string Ssid { get; internal set; }
        [DataMember(Name = "subtype")]
        public string SubType { get; internal set; }
        [DataMember(Name = "title")]
        public string SongName { get; internal set; }
        [DataMember(Name = "url")]
        public string SongUrl { get; internal set; }

        public IEnumerable<KeyValuePair<TimeSpan, string>> Lyrics { get; internal set; }
    }
}
