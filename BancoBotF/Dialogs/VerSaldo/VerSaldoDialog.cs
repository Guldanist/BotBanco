using BancoBotF.Common.Models.BotState;
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

namespace BancoBotF.Dialogs.VerSaldo
{
    //hacemos que herede de componentDialog para poder obtener diferentes metodos
    public class VerSaldoDialog: ComponentDialog
    {
        //variables creadas para la inyeccion de dependencia (BD, el state para ver si ya se logeo o no) y variable cliente para guardar datos del usuario y acceder a ellos
        private readonly IDataBaseService _dataBaseService;
        private readonly IStatePropertyAccessor<BotStateModel> _userState;
        public static ClienteModelo cliente1 = new ClienteModelo();

        public VerSaldoDialog(IDataBaseService dataBaseService, UserState userState)
        {
            //asignando valor por inyeccion de dependencia para la bd y el state
            _dataBaseService = dataBaseService;
            _userState = userState.CreateProperty<BotStateModel>(nameof(BotStateModel));

            //se crea esto para definir los pasos que se seguiran en este dialogo
            var waterfallStep = new WaterfallStep[]
            {
                setDNI,
                setContrasena,
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
            if(userStateModel.clienteDatos)
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
        private async Task<DialogTurnResult> confirmacion(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //primero obtiene el userState para ver si ya se logueo el usuario o no, si lo esta obvia este paso, 
            //sino guarda la contraseña en la variable global para usarse despues y pide al usuario que confirme su respuesta (boton o escrito)
            var userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());
            if (userStateModel.clienteDatos)
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else
            {
                var contrasenaUsuario = stepContext.Context.Activity.Text;
                cliente1.Contrasena = contrasenaUsuario;

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
            //si esta logueado entonces:
            if (userStateModel.clienteDatos)
            {
                //se obtiene las tarjetas
                var tarjetas = _dataBaseService.Tarjeta.Where(x => x.DNI == userStateModel.dni).ToList();

                //se busca en la lista de tarjetas la tarjeta que sea de debito
                foreach (var tarjeta in tarjetas)
                {
                    if(tarjeta.TipoTarjeta=="Debito")
                    {
                        //se buscan las cuentas de la tarjeta, por cada cuenta se muestra el tipo de cuenta, su saldo y la moneda en la que esta
                        var cuentas = _dataBaseService.Cuenta.Where(x => x.Tarjeta == tarjeta.id).ToList();
                        foreach (var cuenta in cuentas)
                        {
                            await stepContext.Context.SendActivityAsync($"En cuenta: {cuenta.TipoCuenta}", cancellationToken: cancellationToken);
                            await stepContext.Context.SendActivityAsync($"Su saldo es de: {cuenta.Saldo} {cuenta.Moneda}", cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync("Usted no cuenta con una tarjeta de ahorros", cancellationToken: cancellationToken);
                    }
                }
                
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }
            //si no esta logueado:
            else
            {
                var confirmacionCliente = stepContext.Context.Activity.Text;   
                //si respondio "si" a la confirmacion
                if (confirmacionCliente.ToLower().Equals("si"))
                {
                    //se obtiene el use state para guardar posteriormente los adtos del cliente
                    userStateModel = await _userState.GetAsync(stepContext.Context, () => new BotStateModel());
                    //se hace consulta si existe el cliente en la base de datos
                    var clien = _dataBaseService.Cliente.Where(x => x.id == cliente1.id && x.Contrasena == cliente1.Contrasena).ToList();
                    // si se encontro el cliente entonces:
                    if (clien.Count > 0)
                    {
                        //se almacenan los datos del cliente en el state
                        userStateModel.clienteDatos = true;
                        userStateModel.dni = cliente1.id;

                        //se obtiene las tarjetas
                        var tarjetas = _dataBaseService.Tarjeta.Where(x => x.DNI == cliente1.id).ToList();

                        //en cada tarjeta busca las tarjetas de debito ya que estas son las que tienen saldo
                        foreach (var tarjeta in tarjetas)
                        {
                            //si es de debito busca cada cuenta dentro de esa tarjeta y muesta el tipo de cuenta y el saldo
                            if (tarjeta.TipoTarjeta == "Debito")
                            {
                                var cuentas = _dataBaseService.Cuenta.Where(x => x.Tarjeta == tarjeta.id).ToList();
                                foreach (var cuenta in cuentas)
                                {
                                    await stepContext.Context.SendActivityAsync($"En cuenta: {cuenta.TipoCuenta}", cancellationToken: cancellationToken);
                                    await stepContext.Context.SendActivityAsync($"Su saldo es de: {cuenta.Saldo} {cuenta.Moneda}", cancellationToken: cancellationToken);
                                }
                            }
                            else
                            {
                                await stepContext.Context.SendActivityAsync("Usted no cuenta con una tarjeta de ahorros", cancellationToken: cancellationToken);
                            }
                        }
                    }
                    //si no se encontro el cliente muestra el siguiente mensaje:
                    else
                    {
                        await stepContext.Context.SendActivityAsync("DNI o contraseña equivocados", cancellationToken: cancellationToken);
                    }
                }
                //si puso otra respuesta en la confirmacion muestra el siguiente mensaje:
                else
                {
                    await stepContext.Context.SendActivityAsync("No hay problema, sera la proxima.", cancellationToken: cancellationToken);
                }
                //continua con la conversacion (espera respuesta del cliente)
                return await stepContext.ContinueDialogAsync(cancellationToken: cancellationToken);
            }         
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
    }
}
