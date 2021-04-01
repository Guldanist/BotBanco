using BancoBotF.Common.Models;
using BancoBotF.Common.Models.BotState;
using BancoBotF.Common.Models.Queja;
using BancoBotF.Common.Models.User;
using BancoBotF.Data;
using BancoBotF.Infrastructure.Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BancoBotF.Dialogs.HacerQueja
{
    public class HacerQuejaDialog : ComponentDialog
    {
        //variables creadas para la inyeccion de dependencia (BD, el state para ver si ya se logeo o no) y variable cliente para guardar datos del usuario y acceder a ellos, al igual que el modelo reclamo para poder guardarlo en la bd
        private readonly IDataBaseService _dataBaseService;
        private readonly IStatePropertyAccessor<BotStateModel> _userState;
        static string userText = null;
        static string tipoQueja = null;
        static QuejaLuisModel Entity;
        private readonly ILuisService _luisService;

        public static ClienteModelo cliente1 = new ClienteModelo();
        public static QuejaModelo queja1 = new QuejaModelo();
        public HacerQuejaDialog(IDataBaseService dataBaseService, UserState userState, ILuisService luisService)
        {
            //asignando valor por inyeccion de dependencia para la bd y el state
            _dataBaseService = dataBaseService;
            _userState = userState.CreateProperty<BotStateModel>(nameof(BotStateModel));
            _luisService = luisService;
            //se crea esto para definir los pasos que se seguiran en este dialogo
            var waterfallStep = new WaterfallStep[]
            {
                setDNI,
                setContrasena,
                setTipoQueja,
                setQueja,
                confirmacion,
                procesoFinal
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }
        private async Task<DialogTurnResult> setDNI(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            userText = stepContext.Context.Activity.Text;
            //primero obtiene el userState para ver si ya se logueo el usuario o no, si lo esta obvia este paso, sino pide al usuario ingresar su dni
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());
            if (userStateModel.clienteDatos)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa tu DNI") }, cancellationToken
               );
            }
        }
        private async Task<DialogTurnResult> setContrasena(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //primero obtiene el userState para ver si ya se logueo el usuario o no, si lo esta obvia este paso, 
            //sino guarda el dni en la variable global para usarse despues y pide al usuario ingresar su contraseña
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());
            if (userStateModel.clienteDatos)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var dniCliente = stepContext.Context.Activity.Text;
                cliente1.id = dniCliente;

                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa tu Contraseña") }, cancellationToken
                   );
            }
        }
        private async Task<DialogTurnResult> setTipoQueja(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //primero obtiene el userState para ver si ya se logueo el usuario o no, si lo esta obvia este paso, 
            //sino guarda la contraseña en la variable global para usarse despues y pide al usuario que ingrese el tipo de queja que tiene (dando 5 opciones)
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());

            var newStepContext = stepContext;
            newStepContext.Context.Activity.Text = userText;
            var luisResult = await _luisService._luisRecognizer.RecognizeAsync(newStepContext.Context, cancellationToken);

            Entity = luisResult.Entities.ToObject<QuejaLuisModel>();

            if (userStateModel.clienteDatos)
            {
                if (Entity.Queja.First().TipoQueja != null)
                {
                    tipoQueja = Entity.Queja.First().Instance.tipoQueja.First().Text.ToString();
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = CreateTipoQuejaButton() }, cancellationToken
                    );
                }
            }
            else
            {
                var contrasenaUsuario = stepContext.Context.Activity.Text;
                cliente1.Contrasena = contrasenaUsuario;

                if (Entity.Queja.First().TipoQueja != null)
                {
                    tipoQueja = Entity.Queja.First().Instance.tipoQueja.First().Text.ToString();
                    //AQUI ESTA EL ERROR POR ALGUN MOTIVO
                    await stepContext.Context.SendActivityAsync("llega aqui", cancellationToken: cancellationToken);
                    return await stepContext.NextAsync(cancellationToken: cancellationToken) ;
                }
                else
                {
                    return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = CreateTipoQuejaButton() }, cancellationToken
                   );
                }
            }
        }
        private async Task<DialogTurnResult> setQueja(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (tipoQueja != null)
            {
                if (tipoQueja == "deposito" || tipoQueja == "depositos")
                {
                    queja1.TipoQueja = "depositos";
                }
                if (tipoQueja == "transferencia" || tipoQueja == "transferencias" || tipoQueja == "transaccion" || tipoQueja == "transacciones")
                {
                    queja1.TipoQueja = "transferencias";
                }
                if (tipoQueja == "prestamo" || tipoQueja == "prestamos")
                {
                    queja1.TipoQueja = "prestamos";
                }
                if (tipoQueja == "asesoria")
                {
                    queja1.TipoQueja = "asesoria";
                }
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                if (Entity.Queja != null)
                {
                    queja1.Queja = userText;
                    return await stepContext.NextAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    var tipoQuejaCliente = stepContext.Context.Activity.Text;
                    queja1.TipoQueja = tipoQuejaCliente.ToLower();

                    return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa tu Queja") }, cancellationToken
                   );
                }
            }
        }
        private async Task<DialogTurnResult> confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Entity.Queja != null)
            {
                return await stepContext.NextAsync(cancellationToken);
            }
            else
            {
                //guarda la queja en la variable global para usarse despues y pide al usuario que ingrese la confirmacion
                var quejaCliente = stepContext.Context.Activity.Text;
                queja1.Queja = quejaCliente;
                return await stepContext.PromptAsync(
                    nameof(TextPrompt),
                    new PromptOptions { Prompt = CreateConfirmationButton() }, cancellationToken
                   );
            }
        }
        private async Task<DialogTurnResult> procesoFinal(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //primero obtiene el userState para ver si ya se logeo el usuario o no
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());
            //se obtiene la confirmacion del cliente
            var confirmacionCliente = stepContext.Context.Activity.Text;
            //si esta logueado entonces:
            if (userStateModel.clienteDatos)
            {
                //si en la confimacion escribio "si":
                if (confirmacionCliente.ToLower().Equals("si"))
                {
                    //comprueba si se escribieron alguna de las siguientes opciones como tipo de queja y si es correcta entonces:
                    if (queja1.TipoQueja.Equals("depositos") || queja1.TipoQueja.Equals("transferencias") || queja1.TipoQueja.Equals("prestamos") || queja1.TipoQueja.Equals("asesoria") || queja1.TipoQueja.Equals("otros"))
                    {
                        //crea el id con el que se guardara la queja
                        queja1.id = Guid.NewGuid().ToString();
                        //asigna el dni que se guardara porsteriormente
                        queja1.DNI = userStateModel.dni;
                        //el estado que tendra la queja
                        queja1.EstadoQueja = "activo";

                        //se graba la queja en la base de datos y muestra un mensaje de guardado exitoso
                        await _dataBaseService.Queja.AddAsync(queja1);
                        await _dataBaseService.SaveAsync();
                        await stepContext.Context.SendActivityAsync("Su queja se guardo exitosamente y la tomaremos en cuenta.", cancellationToken: cancellationToken);
                    }
                    else
                    //si no escribe bien el tipo de queja muestra el siguiente mensaje
                    {
                        await stepContext.Context.SendActivityAsync("El area/servicio ingresado no es valido", cancellationToken: cancellationToken);
                    }
                }
                //si no confirmo
                else
                {
                    await stepContext.Context.SendActivityAsync("No hay problema, sera la proxima.", cancellationToken: cancellationToken);
                }
            }
            else
            {
                //si en la confimacion escribio "si":
                if (confirmacionCliente.ToLower().Equals("si"))
                {
                    //se obtiene el cliente
                    var clien = _dataBaseService.Cliente.Where(x => x.id == cliente1.id).ToList();
                    //si encuentra al cliente en la BD
                    if (clien.Count > 0)
                    {
                        //comprueba si se escribieron alguna de las siguientes opciones como tipo de queja y si es correcta entonces:
                        if (queja1.TipoQueja.Equals("depositos") || queja1.TipoQueja.Equals("transferencias") || queja1.TipoQueja.Equals("prestamos") || queja1.TipoQueja.Equals("asesoria") || queja1.TipoQueja.Equals("otros"))
                        {
                            //crea el id con el que se guardara la queja
                            queja1.id = Guid.NewGuid().ToString();
                            //asigna el dni que se guardara porsteriormente
                            queja1.DNI = cliente1.id;
                            //el estado que tendra la queja
                            queja1.EstadoQueja = "activo";

                            //se graba la queja en la base de datos y muestra un mensaje de guardado exitoso
                            await _dataBaseService.Queja.AddAsync(queja1);
                            await _dataBaseService.SaveAsync();
                            await stepContext.Context.SendActivityAsync("Su queja se guardo exitosamente y la tomaremos en cuenta.", cancellationToken: cancellationToken);
                        }
                        //si no escribe bien el tipo de queja muestra el siguiente mensaje
                        else
                        {
                            await stepContext.Context.SendActivityAsync("El area/servicio ingresado no es valido", cancellationToken: cancellationToken);
                        }
                    }
                    //si encuentra el cliente en la BD
                    else
                    {
                        await stepContext.Context.SendActivityAsync("DNI o contraseña equivocados", cancellationToken: cancellationToken);
                    }
                }
                //si no confirmo
                else
                {
                    await stepContext.Context.SendActivityAsync("No hay problema, sera la proxima.", cancellationToken: cancellationToken);
                }
            }
            return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
        }
        //metodo para la creacion del boton de confirmacion
        private Activity CreateConfirmationButton()
        {
            //el texto que le aparecera al usuario
            var reply = MessageFactory.Text("Confirmas que los datos ingresados son correctos?");
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    // se le asigna el valor a las opciones(botones) que tendra
                    new CardAction(){Title = "Si", Value = "Si", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "No", Value = "No", Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }
        //metodo para la creacion de botones de tipo de queja
        private Activity CreateTipoQuejaButton()
        {
            //el texto que le aparecera al usuario
            var reply = MessageFactory.Text("¿Con que area/servicio esta relacionado su queja?");
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    // se le asigna el valor a las opciones(botones) que tendra
                    new CardAction(){Title = "Depositos", Value = "Depositos", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "Transferencias", Value = "Transferencias", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "Prestamos", Value = "Prestamos", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "Asesoria", Value = "Asesoria", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "Otros", Value = "Otros", Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }
    }
}