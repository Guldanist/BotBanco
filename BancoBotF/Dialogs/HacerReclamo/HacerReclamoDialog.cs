using BancoBotF.Common.Models.BotState;
using BancoBotF.Common.Models.Reclamo;
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

namespace BancoBotF.Dialogs.HacerReclamo
{
    //hacemos que herede de componentDialog para poder obtener diferentes metodos
    public class HacerReclamoDialog : ComponentDialog
    {
        //variables creadas para la inyeccion de dependencia (BD, el state para ver si ya se logeo o no) y variable cliente para guardar datos del usuario y acceder a ellos, al igual que el modelo reclamo para poder guardarlo en la bd
        private readonly IDataBaseService _dataBaseService;
        private readonly IStatePropertyAccessor<BotStateModel> _userState;
        public static ClienteModelo cliente1 = new ClienteModelo();
        public static ReclamoModelo reclamo1 = new ReclamoModelo();
        public HacerReclamoDialog(IDataBaseService dataBaseService, UserState userState)
        {
            //asignando valor por inyeccion de dependencia para la bd y el state
            _dataBaseService = dataBaseService;
            _userState = userState.CreateProperty<BotStateModel>(nameof(BotStateModel));
            //se crea esto para definir los pasos que se seguiran en este dialogo
            var waterfallStep = new WaterfallStep[]
            {
                setDNI,
                setContrasena,
                setTipoReclamo,
                setReclamo,
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
        private async Task<DialogTurnResult> setTipoReclamo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //primero obtiene el userState para ver si ya se logueo el usuario o no, si lo esta obvia este paso, 
            //sino guarda la contraseña en la variable global para usarse despues y pide al usuario que ingrese el tipo de reclamo que tiene (dando 5 opciones)
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());
            if (userStateModel.clienteDatos)
            {
                return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = CreateTipoReclamoButton() }, cancellationToken
               );
            }
            else
            {
                var contrasenaUsuario = stepContext.Context.Activity.Text;
                cliente1.Contrasena = contrasenaUsuario;

                return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = CreateTipoReclamoButton() }, cancellationToken
               );
            }
        }
        private async Task<DialogTurnResult> setReclamo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //guarda el tipo de reclamo en la variable global para usarse despues y pide al usuario que ingrese el reclamo que tiene
            var tipoReclamoCliente = stepContext.Context.Activity.Text;
            reclamo1.TipoReclamo = tipoReclamoCliente.ToLower();

            return await stepContext.PromptAsync(
                nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text("Por favor ingresa tu Reclamo") }, cancellationToken
               );
        }
        private async Task<DialogTurnResult> confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //guarda el reclamo en la variable global para usarse despues y pide al usuario que ingrese la confirmacion
            var reclamoCliente = stepContext.Context.Activity.Text;
            reclamo1.Reclamo = reclamoCliente;
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
                    //comprueba si se escribieron alguna de las siguientes opciones como tipo de reclamo y si es correcta entonces:
                    if (reclamo1.TipoReclamo.Equals("depositos") || reclamo1.TipoReclamo.Equals("transferencias") || reclamo1.TipoReclamo.Equals("prestamos") || reclamo1.TipoReclamo.Equals("asesoria") || reclamo1.TipoReclamo.Equals("otros"))
                    {
                        //crea el id con el que se guardara el reclamo
                        reclamo1.id = Guid.NewGuid().ToString();
                        //asigna el dni que se guardara porsteriormente
                        reclamo1.DNI = userStateModel.dni;
                        //el estado que tendra el reclamo
                        reclamo1.EstadoReclamo = "activo";

                        //se graba el reclamo en la base de datos y muestra un mensaje de guardado exitoso
                        await _dataBaseService.Reclamo.AddAsync(reclamo1);
                        await _dataBaseService.SaveAsync();
                        await stepContext.Context.SendActivityAsync("Su reclamo se guardo exitosamente y lo tomaremos en cuenta.", cancellationToken: cancellationToken);
                    }
                    else
                    //si no escribe bien el tipo de reclamo muestra el siguiente mensaje
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
                        //comprueba si se escribieron alguna de las siguientes opciones como tipo de reclamo y si es correcta entonces:
                        if (reclamo1.TipoReclamo.Equals("depositos") || reclamo1.TipoReclamo.Equals("transferencias") || reclamo1.TipoReclamo.Equals("prestamos") || reclamo1.TipoReclamo.Equals("asesoria") || reclamo1.TipoReclamo.Equals("otros"))
                        {
                            //crea el id con el que se guardara el reclamo
                            reclamo1.id = Guid.NewGuid().ToString();
                            //asigna el dni que se guardara porsteriormente
                            reclamo1.DNI = cliente1.id;
                            //el estado que tendra el reclamo
                            reclamo1.EstadoReclamo = "activo";

                            //se graba el reclamo en la base de datos y muestra un mensaje de guardado exitoso
                            await _dataBaseService.Reclamo.AddAsync(reclamo1);
                            await _dataBaseService.SaveAsync();
                            await stepContext.Context.SendActivityAsync("Su reclamo se guardo exitosamente y lo tomaremos en cuenta.", cancellationToken: cancellationToken);
                        }
                        //si no escribe bien el tipo de reclamo muestra el siguiente mensaje
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
        //metodo para la creacion de botones de tipo de reclamo
        private Activity CreateTipoReclamoButton()
        {
            //el texto que le aparecera al usuario
            var reply = MessageFactory.Text("¿Con que area/servicio esta relacionado su reclamo?");
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
