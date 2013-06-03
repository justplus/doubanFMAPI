using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.OpenChannel
{
    [DataContract]
    public class SearchedSongResult : EntityBase<SearchedSongResult>
    {
        [DataMember(Name = "status")]
        public bool SearchStatus { get; internal set; }
        [DataMember(Name = "data")]
        public SearchedSongCollection SearchResult { get; internal set; }
    }
}
