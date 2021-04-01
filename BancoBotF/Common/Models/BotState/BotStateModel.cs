using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Common.Models.BotState
{
    // la clase BotStateModel almacenara el dni del cliente al momento de loggearse y el bool clienteDatos confirmara si es que hay datos del cliente o no almacenados
    public class BotStateModel
    {
        public bool clienteDatos { get; set; }
        public string dni { get; set; }
    }
}
