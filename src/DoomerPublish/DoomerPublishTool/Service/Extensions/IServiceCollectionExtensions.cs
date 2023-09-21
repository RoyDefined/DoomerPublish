using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;

namespace DoomerPublish;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddPublisher(
        this IServiceCollection serviceCollection)
    {
        if (serviceCollection == null) {
            throw new ArgumentNullException(nameof(serviceCollection));
        }

		_ = serviceCollection.AddSingleton<IPublisherService, DefaultPublisherService>();

		return serviceCollection;
    }
}