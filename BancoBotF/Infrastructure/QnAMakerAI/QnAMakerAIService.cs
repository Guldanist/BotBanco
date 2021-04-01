using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Infrastructure.QnAMakerAI
{
    //la clase del servicio hereda de la interfaz
    public class QnAMakerAIService : IQnAMakerAIService
    {
        //esta variable contiene el resultado del servicio
        public QnAMaker _qnaMakerResult { get; set; }
        //se carga la configuracion del servicio, definido en el appsettings.json
        public QnAMakerAIService(IConfiguration configuration)
        {
            _qnaMakerResult = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["QnAMakerBaseId"],
                EndpointKey = configuration["QnAMakerKey"],
                Host = configuration["QnAMakerHostName"]
            });
        }
    }
}
