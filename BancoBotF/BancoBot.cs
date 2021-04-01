// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using BancoBotF.Common.Cards;
using BancoBotF.Data;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace BancoBotF
{
    public class BancoBot<T> : ActivityHandler where T: Dialog
    {
        private readonly BotState _userState;
        private readonly BotState _conversationState;
        private readonly Dialog _dialog;
        private readonly IDataBaseService _databaseService;

        public BancoBot(UserState userState, ConversationState conversationState, T dialog, IDataBaseService dataBaseService)
        {
            _userState = userState;
            _conversationState = conversationState;
            _dialog = dialog;
            _databaseService = dataBaseService;
        }





        //funcion que se ejecuta al unirse un nuevo miembro (usuarios)
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    //al identificar que alguien se conecta manda el siguiente mensaje y ademas muestra el menu de opciones
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Bienvenido, que quieres hacer?"), cancellationToken);
                    await turnContext.SendActivityAsync(activity: MainOptionCard.CreateCarousel(), cancellationToken);
                }
            }
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await _dialog.RunAsync(
                turnContext,
                _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
                cancellationToken
                );

        }

    }
}
