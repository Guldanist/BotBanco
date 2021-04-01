using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Common.Models.Reclamo
{
    // clase modelo de reclamos para enlazar con la base de datos
    public class ReclamoModelo
    {
        public string id { get; set; }
        public string Reclamo { get; set; }
        public string TipoReclamo { get; set; }
        public string EstadoReclamo { get; set; }
        public string DNI { get; set; }
    }
}
