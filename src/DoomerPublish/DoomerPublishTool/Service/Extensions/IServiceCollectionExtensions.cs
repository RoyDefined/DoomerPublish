using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;

namespace DoomerPublish;

public static class IServiceCollectionExtensions
{
	/// <summary>
	/// Adds the base publisher to the service collection.
	/// </summary>
	/// <param name="serviceCollection"> The service collection to add the library tools into.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddPublisher(
        this IServiceCollection serviceCollection)
    {
        if (serviceCollection == null) {
            throw new ArgumentNullException(nameof(serviceCollection));
        }

		_ = serviceCollection.AddSingleton<IPublisherService, DefaultPublisherService>();
		_ = serviceCollection.AddDoomerPublishTools();
		return serviceCollection;
    }
}