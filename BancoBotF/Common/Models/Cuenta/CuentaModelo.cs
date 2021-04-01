using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Common.Models.Cuenta
{
    // clase modelo de la cuenta de clientes para enlazar con la base de datos
    public class CuentaModelo
    {
        public string id { get; set; }
        public float Saldo { get; set; }
        public string Moneda { get; set; }
        public string TipoCuenta { get; set; }
        public string Tarjeta { get; set; }
    }
}
