using DoomerPublish.Tools.Acs;
using DoomerPublish.Tools.Decorate;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace DoomerPublish;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddDoomerPublishTools(
		this IServiceCollection serviceCollection)
	{
		_ = serviceCollection.AddSingleton<IAcsService, DefaultAcsService>();
		_ = serviceCollection.AddSingleton<IAcsParseService, DefaultAcsParseService>();

		_ = serviceCollection.AddSingleton<IDecorateService, DefaultDecorateService>();
		_ = serviceCollection.AddSingleton<IDecorateParseService, DefaultDecorateParseService>();

		return serviceCollection;
	}
}