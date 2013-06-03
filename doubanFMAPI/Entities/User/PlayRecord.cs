using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.User
{
    [DataContract]
    public class PlayRecord
    {
        [DataMember(Name="banned")]
        public int Banned { get; internal set; }
        [DataMember(Name = "liked")]
        public int Liked { get; internal set; }
        [DataMember(Name = "played")]
        public int Played { get; internal set; }
    }
}
