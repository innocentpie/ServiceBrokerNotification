using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBrokerNotification.ModelChangeListener;

public interface IModelChangeListener<T>
{
    event Action<T[]>? OnRecieveEvent;
}
