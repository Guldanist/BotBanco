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
    // esta clase se usara para conectar con la base de datos, para lo cual heredaremos dbcontext que representa una sesion con una BD
    public class DataBaseService: DbContext, IDataBaseService
    {
        // los siguientes son 2 constructores:
        //en este constructor se recibe el contexto (configuracion de la base de datos) configurado en el startup, si no encuentra nada crea la BD segun el esquema de la interfaz
        public DataBaseService(DbContextOptions options): base(options)
        {
            Database.EnsureCreatedAsync();
        }
        //al no recibir el contexto crea la BD segun el esquema de la interfaz
        public DataBaseService()
        {
            Database.EnsureCreatedAsync();
        }
        //las siguientes son las tablas que debe tener la BD, lo que esta entre <> es el tipo de dato (objeto/entidad) de la variable, 
        //en el caso de estos al ser tablas el objeto contiene sus atributos
        public DbSet<ClienteModelo> Cliente { get; set; }
        public DbSet<TarjetaModelo> Tarjeta { get; set; }
        public DbSet<CuentaModelo> Cuenta { get; set; }
        public DbSet<TransaccionModelo> Transaccion { get; set; }
        public DbSet<ReclamoModelo> Reclamo { get; set; }
        public DbSet<SugerenciaModelo> Sugerencia { get; set; }
        public DbSet<QuejaModelo> Queja { get; set; }
        //metodo para grabar en la base de datos
        public async Task<bool> SaveAsync()
        {
            return (await SaveChangesAsync() > 0);
        }
        //aqui se contruyen los modelos de las diferentes tablas de la BD, definiendo la clave de particion y la clave primaria (id)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClienteModelo>().ToContainer("Cliente").HasPartitionKey("Ciudad").HasNoDiscriminator().HasKey("id");
            modelBuilder.Entity<TarjetaModelo>().ToContainer("Tarjeta").HasPartitionKey("TipoTarjeta").HasNoDiscriminator().HasKey("id");
            modelBuilder.Entity<CuentaModelo>().ToContainer("Cuenta").HasPartitionKey("TipoCuenta").HasNoDiscriminator().HasKey("id");
            modelBuilder.Entity<TransaccionModelo>().ToContainer("Transaccion").HasPartitionKey("TipoTransaccion").HasNoDiscriminator().HasKey("id");
            modelBuilder.Entity<ReclamoModelo>().ToContainer("Reclamo").HasPartitionKey("TipoReclamo").HasNoDiscriminator().HasKey("id");
            modelBuilder.Entity<SugerenciaModelo>().ToContainer("Sugerencia").HasPartitionKey("TipoSugerencia").HasNoDiscriminator().HasKey("id");
            modelBuilder.Entity<QuejaModelo>().ToContainer("Queja").HasPartitionKey("TipoQueja").HasNoDiscriminator().HasKey("id");
        }
    }
}
