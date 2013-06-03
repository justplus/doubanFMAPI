using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.OpenChannel
{
    [DataContract]
    public class SearchedSongCollection
    {
        [DataMember(Name = "songs")]
        public IList<SearchedSongEntity> Songs { get; internal set; }
        [DataMember(Name = "total")]
        public int SongsCount { get; internal set; }
    }
}
