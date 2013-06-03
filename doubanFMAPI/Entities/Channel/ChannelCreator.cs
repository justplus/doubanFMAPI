using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.Channel
{
    [DataContract]
    public class ChannelCreator
    {
        [DataMember(Name = "url")]
        public string Url { get; internal set; }
        [DataMember(Name = "name")]
        public string Name { get; internal set; }
        [DataMember(Name = "id")]
        public int ID { get; internal set; }
    }
}
