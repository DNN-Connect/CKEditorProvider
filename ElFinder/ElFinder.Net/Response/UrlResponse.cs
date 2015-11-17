using System.Drawing;
using System.Runtime.Serialization;

namespace ElFinder.Response
{
    [DataContract]
    internal class UrlResponse
    {
        [DataMember(Name="url")]
        public string Url { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}