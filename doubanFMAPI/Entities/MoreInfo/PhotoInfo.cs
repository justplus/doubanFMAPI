using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.MoreInfo
{
    [DataContract]
    internal class PhotoInfo
    {
        [DataMember(Name = "url")]
        public string Url { get; internal set; }
    }
}
