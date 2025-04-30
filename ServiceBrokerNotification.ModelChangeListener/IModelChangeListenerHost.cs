using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBrokerNotification.ModelChangeListener;

public interface IModelChangeListenerHost<T> : IModelChangeListener<T>
{
    event Action? OnListenerStarted;
    event Action<Exception>? OnListenerStopped;
    Task RunListener(CancellationToken cancellationToken);
}
