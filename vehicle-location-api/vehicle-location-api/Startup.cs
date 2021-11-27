using System;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using vehicle_location_api.protos.V1;

namespace vehicle_location_api
{
    public class Startup
    {
        private const string MQTT_EVENT_TYPE = "gps";
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                
                x.UsingRabbitMq((context,cfg) =>
                {
                    // not required though for defaults
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    
                    cfg.SetMessageSerializer(() => new RawProtobufSerializer());
                    EndpointConvention.Map<VehicleLocationEvent>(new Uri($"exchange:amq.topic?type=topic"));
                    cfg.Send<VehicleLocationEvent>(configurator =>
                    {
                        configurator.UseRoutingKeyFormatter(context =>
                            $"events.{MQTT_EVENT_TYPE}.routes.{context.Message.routeId}"
                        );
                    });
                    
                    cfg.ConfigureEndpoints(context); // automatically create queues using selected case
                });
            });

            services.AddHostedService<VehiclePositionWorker>(); // background job

            services.AddMassTransitHostedService(); // to automatically stop/start the bust when host is started
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
                {
                    Predicate = (check) => check.Tags.Contains("ready"),
                });

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions());
            });
        }
    }
}