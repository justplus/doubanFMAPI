using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.Channel
{
    [DataContract]
    public class FavChannelEntity
    {
        [DataMember(Name = "username")]
        public string UserName { get; internal set; }
        [DataMember(Name = "favchannels")]
        public GetChannelCollection FavChannels { get; internal set; }
    }
}
