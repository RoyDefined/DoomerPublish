using DoomerPublish.Tools.Acs;
using DoomerPublish.Tools.Decorate;
using DoomerPublish;
using Microsoft.Extensions.DependencyInjection;

namespace DoomerPublish;

public static class IServiceCollectionExtensions
{
	/// <summary>
	/// Adds the library tools into the service collection.
	/// </summary>
	/// <param name="serviceCollection"> The service collection to add the library tools into.</param>
	/// <returns>The service collection.</returns>
	public static IServiceCollection AddDoomerPublish(
		this IServiceCollection serviceCollection)
	{
		_ = serviceCollection.AddSingleton<IPublisherService, PublisherService>();

		_ = serviceCollection.AddSingleton<IAcsService, AcsService>();
		_ = serviceCollection.AddSingleton<IAcsParseService, AcsParseService>();

		_ = serviceCollection.AddSingleton<IDecorateService, DecorateService>();
		_ = serviceCollection.AddSingleton<IDecorateParseService, DecorateParseService>();

		return serviceCollection;
	}
}