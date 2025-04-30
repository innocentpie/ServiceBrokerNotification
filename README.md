# ServiceBrokerNotification
Create or use an existing service broker queue to get custom notifications.
Included example of an ASP.NET Core Web API project that handles notifications using SignalR.

# How to use
1. Customize the [ServiceBrokerNotificationTemplate.sql](https://github.com/innocentpie/ServiceBrokerNotification/blob/master/ServiceBrokerNotificationTemplate.sql) template file and execute its contents to set up the listener. Make sure the service broker is enabled on the database.

3. Reference the project [ServiceBrokerNotification.ModelChangeListener](https://github.com/innocentpie/ServiceBrokerNotification/tree/master/ServiceBrokerNotification.ModelChangeListener)

4. Create the notification model class that represents the structure of the recieved message.
    
    For example if the message is defined as:
   
    ```sql
    SET @message = (
      SELECT Id
      FROM inserted
      FOR JSON PATH
    )
    ```
    
    Then the model should have the corresponding properties:
   
    ```csharp
    public record WeatherNotification(int Id);
    ```

5. Register the listener host in the DI container with the specified database configuration.
   
    ```csharp
    services.AddModelChangeListenerHost<WeatherChangeNotification>(options =>
    {
        options.ConnectionString = "connectionString";
        options.SchemaName = "dbo";
        options.DatabaseName = "DatabaseName";
        options.ConversationQueueName = "ListenerQueue_b67e1033_057a_48a1_a40f_89438359f115_Name";
    });
    ```

6. Inject the host into a background service and run it.
   
    ```csharp
    public WeatherNotificationHostedService(IModelChangeListenerHost<WeatherChangeNotification> listenerHost)
    {
        _listenerHost = listenerHost;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _listenerHost.RunListener(stoppingToken);
    }
    ```
	
7. Handle the notifications using a SignalR Hub and IHubContext.
   
    ```csharp
    public WeatherChangeListenerHubHandler(IHubContext<WeatherChangeHub> hubContext, IModelChangeListener<WeatherChangeNotification> listener)
    {
        _hubContext = hubContext;
        _listener = listener;
        _listener.OnRecieveEvent += OnRecieveEvent;
    }
    
    private async void OnRecieveEvent(WeatherChangeNotification[] coll)
    {
        foreach (var item in coll)
            await _hubContext.Clients.All.SendAsync("RecieveWeatherChangeNotification", item.Id);
    }
    ```

# Credits
Some parts based on [SqlDependecyEx](https://github.com/dyatchenko/ServiceBrokerListener/blob/master/ServiceBrokerListener/ServiceBrokerListener.Domain/SqlDependencyEx.cs)

# Licence
[MIT](LICENSE)
