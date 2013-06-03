using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace doubanFMAPI.Entities.MoreInfo
{
    [DataContract]
    internal class MoreInfoEntity : EntityBase<MoreInfoEntity>
    {
        [DataMember(Name = "album_intro")]
        public string AlbumIntroduction { get; internal set; }
        [DataMember(Name = "album_rate")]
        public string AlbumRating { get; internal set; }
        [DataMember(Name = "album_starts")]
        public int AlbumStarta { get; internal set; }
        [DataMember(Name = "albums")]
        public IList<AlbumInfo> Albums { get; internal set; }
        [DataMember(Name = "artist_birth")]
        public string ArtistBirthday { get; internal set; }
        [DataMember(Name = "artist_genre")]
        public string ArtistType { get; internal set; }
        [DataMember(Name = "artist_id")]
        public string ArtistID { get; internal set; }
        [DataMember(Name = "artist_intro")]
        public string ArtistIntroduction { get; internal set; }
        [DataMember(Name = "artist_name")]
        public string ArtistName { get; internal set; }
        [DataMember(Name = "artist_region")]
        public string ArtistRegion { get; internal set; }
        [DataMember(Name = "lyric")]
        public string Lyric { get; internal set; }
        [DataMember(Name = "photos")]
        public IList<PhotoInfo> Photos { get; internal set; }
        [DataMember(Name = "subject_id")]
        public string AlbumID { get; internal set; }
    }
}
