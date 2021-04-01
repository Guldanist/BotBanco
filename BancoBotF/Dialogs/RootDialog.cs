using BancoBotF.Common.Cards;
using BancoBotF.Common.Models.BotState;
using BancoBotF.Data;
using BancoBotF.Dialogs.HacerQueja;
using BancoBotF.Dialogs.HacerReclamo;
using BancoBotF.Dialogs.HacerSugerencia;
using BancoBotF.Dialogs.VerSaldo;
using BancoBotF.Dialogs.VerTransacciones;
using BancoBotF.Infrastructure.Luis;
using BancoBotF.Infrastructure.QnAMakerAI;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BancoBotF.Dialogs
{
    //hacemos que herede de componentDialog para poder obtener diferentes metodos
    public class RootDialog: ComponentDialog
    {
        //variables creadas para la inyeccion de dependencia (BD, el state para ver si ya se logeo o no y mandarlo a otros dialogos
        //los servicios de qna y luis).
        private readonly ILuisService _luisService;
        private readonly IDataBaseService _dataBaseService;
        private readonly IQnAMakerAIService _qnaMakerAIService;
        private readonly IStatePropertyAccessor<BotStateModel> _userState;

        public RootDialog(ILuisService luisService, IDataBaseService dataBaseService, UserState userState, IQnAMakerAIService qnaMakerAIService)
        {
            //asignando valor por inyeccion de dependencia para la bd, servicios luis y qna
            _dataBaseService = dataBaseService;
            _luisService = luisService;
            _qnaMakerAIService = qnaMakerAIService;
            _userState = userState.CreateProperty<BotStateModel>(nameof(BotStateModel));

            //se definen los metodos de inicio y fin de proceso
            var waterfallSteps = new WaterfallStep[]
            {
                InitialProcess,
                FinalProcess
            };
            // se agregan los diagolos (ver saldo, ver transacciones y hacer reclamo) y componentes que se usaran(textPrompt y waterfallDialog)
            AddDialog(new VerSaldoDialog(_dataBaseService, userState));
            AddDialog(new VerTransaccionesDialog(_dataBaseService, userState));
            AddDialog(new HacerReclamoDialog(_dataBaseService, userState));
            AddDialog(new HacerQuejaDialog(_dataBaseService, userState, luisService));
            AddDialog(new HacerSugerenciaDialog(_dataBaseService, userState));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            InitialDialogId = nameof(WaterfallDialog);
        }

        // este metodo (proceso inicial) tiene una variable que contiene el resultado del dialogo enviado (el intent que interpreto de este dialogo) y el score de este (posibilidad de que sea correcto)
        private async Task<DialogTurnResult> InitialProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var luisResult = await _luisService._luisRecognizer.RecognizeAsync(stepContext.Context, cancellationToken);
            return await ManageIntentions(stepContext, luisResult, cancellationToken);
        }

        private async Task<DialogTurnResult> ManageIntentions(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            // esta variable almacena el mayor score que obtiene el servicio luis
            var topIntent = luisResult.GetTopScoringIntent();

            //se comprueba si el score es mayor a 0.5 , si lo es selecciona el intent y llama al metodo de ese intent para ser ejecutado, sino llama al intent None
            if(topIntent.score >= 0.5)
            {
                switch (topIntent.intent)
                {
                    case "Saludar":
                        await IntentSaludar(stepContext, luisResult, cancellationToken);
                        break;
                    case "Agradecer":
                        await IntentAgradecer(stepContext, luisResult, cancellationToken);
                        break;
                    case "Despedir":
                        await IntentDespedir(stepContext, luisResult, cancellationToken);
                        break;
                    case "None":
                        await IntentNone(stepContext, luisResult, cancellationToken);
                        break;
                    case "VerOpciones":
                        await IntentVerOpciones(stepContext, luisResult, cancellationToken);
                        break;
                    case "VerCentroContacto":
                        await IntentVerCentroContacto(stepContext, luisResult, cancellationToken);
                        break;
                    case "VerSaldo":
                        return await IntentVerSaldo(stepContext, luisResult, cancellationToken);
                    case "VerTransacciones":
                        return await IntentVerTransacciones(stepContext, luisResult, cancellationToken);
                    case "HacerReclamo":
                        return await IntentHacerReclamo(stepContext, luisResult, cancellationToken);
                    case "HacerQueja":
                        return await IntentHacerQueja(stepContext, luisResult, cancellationToken);
                    case "HacerSugerencia":
                        return await IntentHacerSugerencia(stepContext, luisResult, cancellationToken);
                    default:
                        break;
                }
            }
            else
            {
                await IntentNone(stepContext, luisResult, cancellationToken);
            }
            //va al siguiente paso
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        #region IntentLuis
        //llama al dialogo hacer sugerencias
        private async Task<DialogTurnResult> IntentHacerSugerencia(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(HacerSugerenciaDialog), cancellationToken: cancellationToken);
        }
        //llama al dialogo hacer quejas
        private async Task<DialogTurnResult> IntentHacerQueja(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(HacerQuejaDialog), cancellationToken: cancellationToken);
        }
        //llama al dialogo hacer reclamo
        private async Task<DialogTurnResult> IntentHacerReclamo(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(HacerReclamoDialog), cancellationToken: cancellationToken);
        }
        //llama al dialogo ver transacciones
        private async Task<DialogTurnResult> IntentVerTransacciones(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(VerTransaccionesDialog), cancellationToken: cancellationToken);
        }
        //llama al dialogo ver saldo
        private async Task<DialogTurnResult> IntentVerSaldo(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(VerSaldoDialog),cancellationToken: cancellationToken);
        }
        //llama al menu de opciones
        private async Task IntentVerOpciones(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Aqui tengo mis opciones", cancellationToken: cancellationToken);
            await MainOptionCard.toShow(stepContext, cancellationToken);
        }
        //muestra la informacion del contacto de la entidad financiera (nro de telefono y ubicacion)
        private async Task IntentVerCentroContacto(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            string phoneDetail = $"Nuestros numeros de atencion son los siguientes:{Environment.NewLine}" +
                $"📞 +999999999  {Environment.NewLine} 📞 +999999999";

            string addressDetail = $"🏢 Estamos ubicados en {Environment.NewLine} Calle ABC 123, Arequipa";

            await stepContext.Context.SendActivityAsync(phoneDetail, cancellationToken: cancellationToken);
            await Task.Delay(1000);
            await stepContext.Context.SendActivityAsync(addressDetail, cancellationToken: cancellationToken);
            await Task.Delay(1000);
            await stepContext.Context.SendActivityAsync("¿En que mas te puedo ayudar?", cancellationToken: cancellationToken);
        }
        //responde al saludo del cliente mandando un mensaje
        private async Task IntentSaludar(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Hola que gusto verte. Como puedo ayudarte?", cancellationToken: cancellationToken);
        }
        //responde al agradecimiento del usuario
        private async Task IntentAgradecer(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("No hay por que. Puedo hacer algo mas por ti?", cancellationToken: cancellationToken);
        }
        //responde al despedirse el usuario
        private async Task IntentDespedir(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            //se obtiene el state para deslogearse
            await stepContext.Context.SendActivityAsync("Hasta luego.", cancellationToken: cancellationToken);
            await _userState.DeleteAsync(stepContext.Context, cancellationToken: cancellationToken);

        }
        //esta es la respuesta en caso de no identificar la entrada del usuario
        private async Task IntentNone(WaterfallStepContext stepContext, RecognizerResult luisResult, CancellationToken cancellationToken)
        {
            //como al entrar aqui no se sabe la intencion primero verifica las preguntas frecuentes
            //se crea el objeto qnaMaker para poder ver su score y segun la precision de esta respuesta para ver si encaja con alguna pregunta frecuente
            //si sigue sin identificar la pregunta pasa a mostrar el mensaje "No entiendo lo que me dices"
            var resultQnA = await _qnaMakerAIService._qnaMakerResult.GetAnswersAsync(stepContext.Context);

            var score = resultQnA.FirstOrDefault()?.Score;
            string response = resultQnA.FirstOrDefault()?.Answer;

            if (score >= 0.5)
            {
                await stepContext.Context.SendActivityAsync(response, cancellationToken: cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("No entiendo lo que me dices.", cancellationToken: cancellationToken);
                await Task.Delay(1000);

                await IntentVerOpciones(stepContext, luisResult, cancellationToken);
            }           
        }
        #endregion
        //metodo que marca el fin del dialogo
        private async Task<DialogTurnResult> FinalProcess(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

    }
}
