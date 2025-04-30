using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBrokerNotification.ModelChangeListener.ServiceBrokerListener;

public class ServiceBrokerListenerHostOptions
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string ConversationQueueName { get; set; }
    public string SchemaName { get; set; } = "dbo";
    public int CommandTimeout { get; set; } = 60000;
    public bool ClearQueueOnStartup { get; set; } = false;
}
