using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.Song
{
    [DataContract]
    public class GetPlaylistResult : EntityBase<GetPlaylistResult>
    {
        [DataMember(Name = "r")]
        public int R { get; internal set; }
        [DataMember(Name = "song")]
        public IList<SongEntity> Songs { get; internal set; }
    }
}
