using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BancoBotF.Common.Cards
{
    public class MainOptionCard
    {
        //metodo con el que se llamara al carrusel de opciones
        public static async Task toShow(DialogContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(activity: CreateCarousel(), cancellationToken);
        }

        public static Activity CreateCarousel()
        {
            //muestra la parte de menu que tiene las consultas que puede hacer el cliente
            var cardConsultas = new HeroCard
            {
                Title = "Consultas",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://bancobotstorage.blob.core.windows.net/images/menu1.png") },
                Buttons = new List<CardAction>()
                { 
                    new CardAction()
                    {
                        Title = "Consultar Saldo", Value = "Consultar Saldo", Type = ActionTypes.ImBack
                    },

                    new CardAction()
                    {
                        Title = "Ver Transacciones", Value = "Ver transacciones", Type = ActionTypes.ImBack
                    }
                }
            };

            //muestra la parte de menu que tiene la opcion de hacer reclamos
            var cardReclamos = new HeroCard
            {
                Title = "Reclamos",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://bancobotstorage.blob.core.windows.net/images/menu2.png") },
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Title = "Hacer reclamo", Value = "Hacer reclamo", Type = ActionTypes.ImBack
                    },
                    new CardAction()
                    {
                        Title = "Hacer sugerencia", Value = "Hacer sugerencia", Type = ActionTypes.ImBack
                    },
                    new CardAction()
                    {
                        Title = "Hacer queja", Value = "Hacer queja", Type = ActionTypes.ImBack
                    }
                }
            };

            //muestra la parte de menu que tiene la informacion de la entidad
            var cardInformacionContacto = new HeroCard
            {
                Title = "Informacion de contacto",
                Subtitle = "Opciones",
                Images = new List<CardImage> { new CardImage("https://bancobotstorage.blob.core.windows.net/images/menu3.jpg") },
                Buttons = new List<CardAction>()
                {
                    new CardAction()
                    {
                        Title = "Centro de contacto", Value = "Centro de contacto", Type = ActionTypes.ImBack
                    },

                    new CardAction()
                    {
                        Title = "Sitio Web", Value = "https://www.scotiabank.com.pe/Personas/Default", Type = ActionTypes.OpenUrl
                    }
                }
            };

            //var cardCalificacion = new HeroCard
            //{
            //    Title = "Calificacion",
            //    Subtitle = "Opciones",
            //    Images = new List<CardImage> { new CardImage("https://bancobotstorage.blob.core.windows.net/images/menu4.png") },
            //    Buttons = new List<CardAction>()
            //    {
            //        new CardAction()
            //        {
            //            Title = "Calificar bot", Value = "Calificar bot", Type = ActionTypes.ImBack
            //        }
            //    }
            //};

            //se arma una lista de todas las partes del menu
            var optionAttachments = new List<Attachment>()
            {
                cardConsultas.ToAttachment(),
                cardReclamos.ToAttachment(),
                cardInformacionContacto.ToAttachment(),
                //cardCalificacion.ToAttachment()
            };

            var reply = MessageFactory.Attachment(optionAttachments);
            // se despliega el menu
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            return reply as Activity;
        }
    }
}
