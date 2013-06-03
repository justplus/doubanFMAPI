using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.MoreInfo
{
    [DataContract]
    internal class AlbumInfo
    {
        [DataMember(Name = "artists")]
        public string Artist { get; internal set; }
        [DataMember(Name = "date")]
        public string PublishDate { get; internal set; }
        [DataMember(Name = "img")]
        public string AlbumPicture { get; internal set; }
        [DataMember(Name = "media")]
        public string MediaType { get; internal set; }
        [DataMember(Name = "publisher")]
        public string Publisher { get; internal set; }
        [DataMember(Name = "rating")]
        public double Rating { get; internal set; }
        [DataMember(Name = "subject_id")]
        public string AlbumID { get; internal set; }
        [DataMember(Name = "title")]
        public string AlbunName { get; internal set; }
    }
}
