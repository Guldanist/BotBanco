using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Common.Models.User
{
    // clase modelo de clientes para enlazar con la base de datos
    public class ClienteModelo
    {
        public string id { get; set; }
        public string Nombres { get; set; }
        public string ApP { get; set; }
        public string ApM { get; set; }
        public string Fnac { get; set; }
        public string Ciudad { get; set; }
        public string Contrasena { get; set; }
    }
}
