using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace dotnet_lab2_cli
{

    internal class RecentTracksRoot
    {
        public RecentTracks recenttracks { get; set; }

    }

    internal class RecentTracks
    {
        public List<Track> track { get; set; }
        public Attr attr { get; set; }
    }
    internal class Track
    {
       
        public Artist artist { get; set; }
        public string streamable { get; set; }
        public List<Image> image { get; set; }
        public string mbid { get; set; }
        public Album album { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public Date date { get; set; }

        //public string Timestamp { get; set; }

        public override string ToString()
        {
            return $"{artist.text} - {name} ({album.text}) @ {date.text}";
        }

        }

    internal class Artist
    {
        public string mbid { get; set; }
        [JsonPropertyName("#text")]
        public string text { get; set; }
        public override string ToString()
        {
            return text;
        }
    }

    internal class Image
    {
        public string size { get; set; }
        [JsonPropertyName("#text")]
        public string text { get; set; }
        public override string ToString()
        {
            return text;
        }
    }

    internal class Album
    {
        public string mbid { get; set; }
        [JsonPropertyName("#text")]
        public string text { get; set; }
        public override string ToString()
        {
            return text;
        }
    }

    internal class Date
    {
        public string tts { get; set; }
        [JsonPropertyName("#text")]
        public string text { get; set; }
        public override string ToString()
        {
            return text;
        }
    }

    public class Attr
    {
        public string user { get; set; }
        public string totalPages { get; set; }
        public string page { get; set; }
        public string perPage { get; set; }
        public string total { get; set; }
    }
}
