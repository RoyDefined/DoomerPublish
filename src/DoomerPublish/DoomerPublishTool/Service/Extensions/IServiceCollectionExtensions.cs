using Microsoft.Extensions.DependencyInjection;
using DoomerPublish;

namespace DoomerPublishConsole;

internal static class IServiceCollectionExtensions
{
	/// <summary>
	/// Adds the base publisher to the service collection.
	/// </summary>
	/// <param name="serviceCollection"> The service collection to add the library tools into.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddPublisher(
		this IServiceCollection serviceCollection)
	{
		ArgumentNullException.ThrowIfNull(serviceCollection);

		_ = serviceCollection.AddSingleton<IPublisherService, DefaultPublisherService>();
		_ = serviceCollection.AddDoomerPublishTools();
		return serviceCollection;
	}
}