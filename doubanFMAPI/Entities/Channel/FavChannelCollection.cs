using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.Channel
{
    [DataContract]
    public class FavChannelCollection : EntityBase<FavChannelCollection>
    {
        [DataMember(Name = "allfavchannels")]
        public List<FavChannelEntity> AllFavChannels { get; internal set; }
    }
}
