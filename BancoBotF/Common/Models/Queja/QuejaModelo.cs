using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Common.Models.Queja
{
    public class QuejaModelo
    {
        public string id { get; set; }
        public string Queja { get; set; }
        public string TipoQueja { get; set; }
        public string EstadoQueja { get; set; }
        public string DNI { get; set; }
    }
}
