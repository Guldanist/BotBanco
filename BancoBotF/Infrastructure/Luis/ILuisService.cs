using Microsoft.Bot.Builder.AI.Luis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Infrastructure.Luis
{
    //interfaz (esquema) del servicio de luis, contiene la variable que reconocera el contexto enviado
    public interface ILuisService
    {
        LuisRecognizer _luisRecognizer { get; set; }
    }
}
