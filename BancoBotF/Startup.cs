// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EmptyBot v4.10.3

using BancoBotF.Data;
using BancoBotF.Dialogs;
using BancoBotF.Infrastructure.Luis;
using BancoBotF.Infrastructure.QnAMakerAI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BancoBotF
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //se pasan los parametros en appsettings para obtener sus valores y agregar el servicio, se usa blob para almacenar las imagenes del menu del bot
            var storage = new AzureBlobStorage(
                Configuration.GetSection("StorageConnectionString").Value,
                Configuration.GetSection("StorageContainer").Value
                );
            var userState = new UserState(storage);
            services.AddSingleton(userState);

            var conversationState = new ConversationState(storage);
            services.AddSingleton(conversationState);

            services.AddControllers().AddNewtonsoftJson();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            //agrega el servicio de base de datos
            services.AddDbContext<DataBaseService>(options =>
            {
                options.UseCosmos(
                    Configuration["CosmosEndPoint"],
                    Configuration["CosmosKey"],
                    Configuration["CosmosDatabase"]
                );
            });

            services.AddScoped<IDataBaseService, DataBaseService>();

            //Se agrega la referencia al servicio de Luis agregando la interfaz y la clase
            services.AddSingleton<ILuisService, LuisService>();

            //Se agrega la referencia al servicio QnAMaker
            services.AddSingleton<IQnAMakerAIService, QnAMakerAIService>();

            //Se agrega el servicio de los dialogos:
            services.AddTransient<RootDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, BancoBot<RootDialog>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
