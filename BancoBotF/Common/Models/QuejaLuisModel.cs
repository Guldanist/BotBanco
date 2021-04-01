using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Common.Models
{
    public class QuejaLuisModel
    {
        public List<QuejaEntity> Queja { get; set; }
    }
    public class QuejaEntity
    {
        [JsonProperty("TipoQueja" ) ]
        public List<string> TipoQueja { get; set; }
        [JsonProperty("$instance")]
        public Instance Instance { get; set; }
    }
    public class Instance
    {
        public List<TipoQueja> tipoQueja { get; set; }
    }
    public class TipoQueja
    {
        public string Type { get; set; }
        public string Text { get; set; }
        public string ModelType { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public List<string> RecognitionSources { get; set; }
    }
}
