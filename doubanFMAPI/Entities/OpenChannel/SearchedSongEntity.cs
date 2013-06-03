using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.OpenChannel
{
    [DataContract]
    public class SearchedSongEntity
    {
        [DataMember(Name = "artist")]
        public string SingerName { get; internal set; }
        [DataMember(Name = "id")]
        public Int32 SongID { get; internal set; }
        [DataMember(Name = "is_deleted")]
        public bool IsDeleted { get; internal set; }
        [DataMember(Name = "name")]
        public string SongName { get; internal set; }
        [DataMember(Name = "source")]
        public AlbumSimpleInfo Album { get; internal set; }
        [DataMember(Name = "url")]
        public string SongUrl { get; internal set; }
    }
}
