using Microsoft.AspNetCore.SignalR;
using ServiceBrokerNotification.API.ModelChangeNotifications;
using ServiceBrokerNotification.ModelChangeListener;

namespace ServiceBrokerNotification.API.HostedServices
{
    public class WeatherNotificationHostedService : BackgroundService
    {
        private readonly IModelChangeListenerHost<WeatherChangeNotification> _listenerHost;

        public WeatherNotificationHostedService(IModelChangeListenerHost<WeatherChangeNotification> listenerHost)
        {
            _listenerHost = listenerHost;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _listenerHost.RunListener(stoppingToken);
        }
    }
}
