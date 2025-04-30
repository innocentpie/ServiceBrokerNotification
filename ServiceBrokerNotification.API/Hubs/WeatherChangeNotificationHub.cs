using Microsoft.AspNetCore.SignalR;
using ServiceBrokerNotification.API.ModelChangeNotifications;
using ServiceBrokerNotification.ModelChangeListener;

namespace ServiceBrokerNotification.API.Hubs
{
    public sealed class WeatherChangeHub : Hub;

    public interface IWeatherChangeListenerHubHandler : IModelChangeHandler<WeatherChangeNotification>;
    public class WeatherChangeHubHandler : IWeatherChangeListenerHubHandler
    {
        private readonly IHubContext<WeatherChangeHub> _hubContext;
        private readonly IModelChangeListener<WeatherChangeNotification> _listener;

        public WeatherChangeHubHandler(IHubContext<WeatherChangeHub> hubContext, IModelChangeListener<WeatherChangeNotification> listener)
        {
            _hubContext = hubContext;
            _listener = listener;
            _listener.OnRecieveEvent += OnRecieveEvent;
        }

        public async void OnRecieveEvent(WeatherChangeNotification[] coll)
        {
            foreach (var item in coll)
                await _hubContext.Clients.All.SendAsync("RecieveWeatherChangeNotification", item.Id);
        }
    }
}
