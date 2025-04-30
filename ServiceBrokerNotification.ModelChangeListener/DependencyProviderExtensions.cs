using Microsoft.Extensions.DependencyInjection;
using ServiceBrokerNotification.ModelChangeListener.ServiceBrokerListener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBrokerNotification.ModelChangeListener
{
    public static class DependencyProviderExtensions
    {
        public static void AddModelChangeListenerHost<T>(this IServiceCollection services, Action<ServiceBrokerListenerHostOptions> configureOptions)
        {
            ServiceBrokerListenerHostOptions options = new();
            configureOptions.Invoke(options);
            services.AddSingleton<IModelChangeListenerHost<T>, ServiceBrokerListenerHost<T>>(serviceProvider => new(options));
            services.AddSingleton<IModelChangeListener<T>>(serviceProvider => serviceProvider.GetRequiredService<IModelChangeListenerHost<T>>());
        }
    }
}
