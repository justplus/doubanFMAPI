using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.User
{
    [DataContract]
    public class LoginResult : EntityBase<LoginResult>
    {
        [DataMember(Name = "r")]
        public int R { get; internal set; }
        [DataMember(Name = "err_no")]
        public int ErrorNo { get; internal set; }
        [DataMember(Name = "err_msg")]
        public string ErrorMessage { get; internal set; }
        [DataMember(Name = "user_info")]
        public UserEntity User { get; internal set; }
    }
}
