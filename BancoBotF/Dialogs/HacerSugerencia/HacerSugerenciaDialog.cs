using BancoBotF.Common.Models.BotState;
using BancoBotF.Common.Models.Sugerencia;
using BancoBotF.Common.Models.User;
using BancoBotF.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BancoBotF.Dialogs.HacerSugerencia
{
    public class HacerSugerenciaDialog : ComponentDialog
    {
        //variables creadas para la inyeccion de dependencia (BD, el state para ver si ya se logeo o no) y variable cliente para guardar datos del usuario y acceder a ellos, al igual que el modelo reclamo para poder guardarlo en la bd
        private readonly IDataBaseService _dataBaseService;
        private readonly IStatePropertyAccessor<BotStateModel> _userState;
        public static ClienteModelo cliente1 = new ClienteModelo();
        public static SugerenciaModelo sugerencia1 = new SugerenciaModelo();
        public HacerSugerenciaDialog(IDataBaseService dataBaseService, UserState userState)
        {
            //asignando valor por inyeccion de dependencia para la bd
            _dataBaseService = dataBaseService;
            _userState = userState.CreateProperty<BotStateModel>(nameof(BotStateModel));
            //se crea esto para definir los pasos que se seguiran en este dialogo
            var waterfallStep = new WaterfallStep[]
            {
                setDNI,
                setContrasena,
                setTipoSugerencia,
                setSugerencia,
                confirmacion,
                procesoFinal
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallStep));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }
        private async Task<DialogTurnResult> setDNI(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
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
        private async Task<DialogTurnResult> setTipoSugerencia(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //primero obtiene el userState para ver si ya se logueo el usuario o no, si lo esta obvia este paso, 
            //sino guarda la contraseña en la variable global para usarse despues y pide al usuario que ingrese el tipo de queja que tiene (dando 5 opciones)
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());
            if (userStateModel.clienteDatos)
            {
                return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = CreateTipoSugerenciaButton() }, cancellationToken
               );
            }
            else
            {
                var contrasenaUsuario = stepContext.Context.Activity.Text;
                cliente1.Contrasena = contrasenaUsuario;

                return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = CreateTipoSugerenciaButton() }, cancellationToken
               );
            }
        }
        private async Task<DialogTurnResult> setSugerencia(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //sino guarda el tipo de sugerencia en la variable global para usarse despues y pide al usuario que ingrese el reclamo que tiene
            var tipoSugerenciaCliente = stepContext.Context.Activity.Text;
            sugerencia1.TipoSugerencia = tipoSugerenciaCliente.ToLower();

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa tu Sugerencia") }, cancellationToken
               );
        }
        private async Task<DialogTurnResult> confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //sino guarda el tipo de sugerencia en la variable global para usarse despues y pide al usuario que ingrese la confirmacion
            var sugerenciaUsuario = stepContext.Context.Activity.Text;
            sugerencia1.Sugerencia = sugerenciaUsuario;
            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = CreateConfirmationButton() }, cancellationToken
               );
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
                    //comprueba si se escribieron alguna de las siguientes opciones como tipo de sugerencia y si es correcta entonces:
                    if (sugerencia1.TipoSugerencia.Equals("depositos") || sugerencia1.TipoSugerencia.Equals("transferencias") || sugerencia1.TipoSugerencia.Equals("prestamos") || sugerencia1.TipoSugerencia.Equals("otros"))
                    {
                        //crea el id con el que se guardara la sugerencia
                        sugerencia1.id = Guid.NewGuid().ToString();
                        //asigna el dni que se guardara porsteriormente
                        sugerencia1.DNI = userStateModel.dni;

                        //se graba la sugerencia en la base de datos y muestra un mensaje de guardado exitoso
                        await _dataBaseService.Sugerencia.AddAsync(sugerencia1);
                        await _dataBaseService.SaveAsync();
                        await stepContext.Context.SendActivityAsync("Su queja se guardo exitosamente y la tomaremos en cuenta.", cancellationToken: cancellationToken);
                    }
                    else
                    //si no escribe bien el tipo de sugerencia muestra el siguiente mensaje
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
                        //comprueba si se escribieron alguna de las siguientes opciones como tipo de sugerencia y si es correcta entonces:
                        if (sugerencia1.TipoSugerencia.Equals("depositos") || sugerencia1.TipoSugerencia.Equals("transferencias") || sugerencia1.TipoSugerencia.Equals("prestamos") || sugerencia1.TipoSugerencia.Equals("otros"))
                        {
                            //crea el id con el que se guardara la queja
                            sugerencia1.id = Guid.NewGuid().ToString();
                            //asigna el dni que se guardara porsteriormente
                            sugerencia1.DNI = cliente1.id;

                            //se graba la sugerencia en la base de datos y muestra un mensaje de guardado exitoso
                            await _dataBaseService.Sugerencia.AddAsync(sugerencia1);
                            await _dataBaseService.SaveAsync();
                            await stepContext.Context.SendActivityAsync("Su sugerencia se guardo exitosamente y la tomaremos en cuenta.", cancellationToken: cancellationToken);
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
        //metodo para la creacion de botones de tipo de sugerencia
        private Activity CreateTipoSugerenciaButton()
        {
            //el texto que le aparecera al usuario
            var reply = MessageFactory.Text("¿Con que area/servicio esta relacionado su sugerencia?");
            reply.SuggestedActions = new SuggestedActions()
            {
                Actions = new List<CardAction>()
                {
                    // se le asigna el valor a las opciones(botones) que tendra
                    new CardAction(){Title = "Depositos", Value = "Depositos", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "Transferencias", Value = "Transferencias", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "Prestamos", Value = "Prestamos", Type = ActionTypes.ImBack},
                    new CardAction(){Title = "Otros", Value = "Otros", Type = ActionTypes.ImBack}
                }
            };
            return reply as Activity;
        }
    }
}
