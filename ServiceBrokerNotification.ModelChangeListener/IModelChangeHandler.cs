using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBrokerNotification.ModelChangeListener;

public interface IModelChangeHandler<T>
{
    public void OnRecieveEvent(T[] models);
}
