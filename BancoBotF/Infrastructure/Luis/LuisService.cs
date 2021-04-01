using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace BancoBotF.Infrastructure.Luis
{
    //heredando la interfaz del servicio
    public class LuisService: ILuisService
    {
        //esta variable contendra el intent(la intencion de la persona) y el score(cuanto de posibildad de que el intent sea el correcto, dado del 0 al 1)
        public LuisRecognizer _luisRecognizer {get; set;}


        //Definimos la configuracion del servicio Luis, en el que le pasamos las diferentes claves del servicio
        public LuisService(IConfiguration configuration)
        {
            var luisApplication = new LuisApplication(
                configuration["LuisAppId"],
                configuration["LuisApiKey"],
                configuration["LuisHostName"]
                );

            var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication)
            {
                PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions()
                {
                    IncludeInstanceData = true
                }
            };
            _luisRecognizer = new LuisRecognizer(recognizerOptions);
        }
    }
}
