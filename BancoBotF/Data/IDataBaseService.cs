using BancoBotF.Common.Models.Cuenta;
using BancoBotF.Common.Models.Queja;
using BancoBotF.Common.Models.Reclamo;
using BancoBotF.Common.Models.Sugerencia;
using BancoBotF.Common.Models.Tarjeta;
using BancoBotF.Common.Models.Transaccion;
using BancoBotF.Common.Models.User;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Data
{
    //esta es la interfaz de la base de datos, es decir el esquema de lo que esta debe tener
    public interface IDataBaseService
    {
        DbSet<ClienteModelo> Cliente { get; set; }
        DbSet<TarjetaModelo> Tarjeta { get; set; }
        DbSet<CuentaModelo> Cuenta { get; set; }
        DbSet<TransaccionModelo> Transaccion { get; set; }     
        DbSet<ReclamoModelo> Reclamo { get; set; }
        DbSet<SugerenciaModelo> Sugerencia { get; set; }
        DbSet<QuejaModelo> Queja { get; set; }
        //metodo para guardar datos en la BD
        Task<bool> SaveAsync();
    }
}
