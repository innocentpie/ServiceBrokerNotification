using ServiceBrokerNotification.API.HostedServices;
using ServiceBrokerNotification.API.Hubs;
using ServiceBrokerNotification.API.ModelChangeNotifications;
using ServiceBrokerNotification.ModelChangeListener;

namespace ServiceBrokerNotification.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddModelChangeListenerHost<WeatherChangeNotification>(options =>
        {
            options.ConnectionString = "connectionString";
            options.SchemaName = "dbo";
            options.DatabaseName = "DatabaseName";
            options.ConversationQueueName = "ListenerQueue_b67e1033_057a_48a1_a40f_89438359f115_Name";
        });
        builder.Services.AddSignalR();
        builder.Services.AddHostedService<WeatherNotificationHostedService>();
        builder.Services.AddActivatedSingleton<IWeatherChangeListenerHubHandler, WeatherChangeHubHandler>();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapHub<WeatherChangeHub>("/api/hub/weather");

        app.Run();
    }
}
