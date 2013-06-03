using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.Channel
{
    [DataContract]
    public class ChannelEntity
    {
        [DataMember(Name = "intro")]
        public string Introduction { get; internal set; }
        [DataMember(Name = "name")]
        public string Name { get; internal set; }
        [DataMember(Name = "song_num")]
        public string SongsCount { get; internal set; }
        [DataMember(Name = "creator")]
        public ChannelCreator Creator { get; internal set; }
        [DataMember(Name = "banner")]
        public string Banner { get; internal set; }
        [DataMember(Name = "cover")]
        public string Cover { get; internal set; }
        [DataMember(Name = "id")]
        public int ChannelID { get; internal set; }
        [DataMember(Name = "hot_songs")]
        public IEnumerable<string> HotSongs { get; internal set; }
    }
}
