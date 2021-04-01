using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Common.Models.Transaccion
{
    // clase modelo de transacciones en cuentas bancarias para enlazar con la base de datos
    public class TransaccionModelo
    {
        public string id { get; set; }
        public float Monto { get; set; }
        public string Origen { get; set; }
        public string TipoTransaccion { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string Cuenta { get; set; }
    }
}
