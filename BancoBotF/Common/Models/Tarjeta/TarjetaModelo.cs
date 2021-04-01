using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Common.Models.Tarjeta
{
    // clase modelo de tarjetas de clientes para enlazar con la base de datos
    public class TarjetaModelo
    {
        public string id { get; set; }
        public string FVencimiento { get; set; }
        public string TipoTarjeta { get; set; }
        public string DNI { get; set; }
    }
}
