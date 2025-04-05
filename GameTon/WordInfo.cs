using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GameTon
{
    internal class WordInfo
    {
        [JsonPropertyName("dir")]
        public int Direction { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("pos")]
        public List<int> Position { get; set; } = new List<int>();
    }
}
