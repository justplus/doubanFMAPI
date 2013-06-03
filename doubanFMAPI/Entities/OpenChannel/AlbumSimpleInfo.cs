using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.OpenChannel
{
    [DataContract]
    public class AlbumSimpleInfo
    {
        [DataMember(Name = "name")]
        public string AlbumName { get; internal set; }
        [DataMember(Name = "url")]
        public string AlbumPath { get; internal set; }
    }
}
