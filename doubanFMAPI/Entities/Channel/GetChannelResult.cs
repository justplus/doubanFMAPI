using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.Channel
{
    [DataContract]
    public class GetChannelResult : EntityBase<GetChannelResult>
    {
        [DataMember(Name = "status")]
        public bool Status { get; internal set; }
        [DataMember(Name = "data")]
        public GetChannelCollection Data { get; internal set; }
    }
}
