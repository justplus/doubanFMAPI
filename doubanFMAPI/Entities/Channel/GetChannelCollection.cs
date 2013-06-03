using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.Channel
{
    [DataContract]
    public class GetChannelCollection
    {
        [DataMember(Name = "channels")]
        public List<ChannelEntity> Channels { get; internal set; }
        [DataMember(Name = "total")]
        public int ChannelCount { get; internal set; }
    }
}
