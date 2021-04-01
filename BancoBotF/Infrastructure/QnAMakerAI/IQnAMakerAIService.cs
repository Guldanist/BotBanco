using Microsoft.Bot.Builder.AI.QnA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Infrastructure.QnAMakerAI
{
    //Interfaz de QnAMaker, en su contenido solo almacenara el resultado de cuando se llame a este servicio
    public interface IQnAMakerAIService
    {
        QnAMaker _qnaMakerResult { get; set; }
    }
}
