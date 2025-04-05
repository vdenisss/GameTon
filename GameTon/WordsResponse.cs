using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GameTon
{
    internal class WordsResponse
    {
        [JsonPropertyName("done")]
        public bool Done { get; set; }

        [JsonPropertyName("words")]
        public List<WordInfo> Words { get; set; } = new List<WordInfo>();
    }
}
