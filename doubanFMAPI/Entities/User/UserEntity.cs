using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.User
{
    [DataContract]
    public class UserEntity
    {
        [DataMember(Name = "ck")]
        public string CK { get; internal set; }
        [DataMember(Name = "id")]
        public int ID { get; internal set; }
        [DataMember(Name = "name")]
        public string Name { get; internal set; }
        [DataMember(Name = "play_record")]
        public PlayRecord Record { get; internal set; }
        [DataMember(Name = "url")]
        public string Url { get; internal set; }
        [DataMember(Name = "is_dj")]
        public bool IsDJ { get; internal set; }
        [DataMember(Name = "is_pro")]
        public bool IsPro { get; internal set; }
        [DataMember(Name = "uid")]
        public string UID { get; internal set; }
    }
}
