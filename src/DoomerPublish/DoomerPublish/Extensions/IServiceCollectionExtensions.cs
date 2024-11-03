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
		_ = serviceCollection.AddSingleton<IPublisherService, DefaultPublisherService>();

		_ = serviceCollection.AddSingleton<IAcsService, DefaultAcsService>();
		_ = serviceCollection.AddSingleton<IAcsParseService, DefaultAcsParseService>();

		_ = serviceCollection.AddSingleton<IDecorateService, DefaultDecorateService>();
		_ = serviceCollection.AddSingleton<IDecorateParseService, DefaultDecorateParseService>();

		return serviceCollection;
	}
}