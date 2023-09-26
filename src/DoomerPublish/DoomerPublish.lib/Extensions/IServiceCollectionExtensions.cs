using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace DoomerPublish;

public static class IServiceCollectionExtensions
{
	public static IServiceCollection AddDoomerPublishTools(
		this IServiceCollection serviceCollection)
	{
		return serviceCollection;
	}
}