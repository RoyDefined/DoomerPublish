using DoomerPublish;
using DoomerPublish.Tools.Acs;
using DoomerPublish.Tools.Decorate;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text;

namespace DoomerPublishTests;

public class DecorateTests
{
	private const string BaseFilePath = "DecorateParseFiles/basefile.dec";

	private ServiceProvider _services;

	[OneTimeSetUp]
	public void Setup()
	{
		this._services = new ServiceCollection()
			.AddLogging()
			.AddDoomerPublishTools()
			.BuildServiceProvider();
	}

	[Test]
	public void DecorateFileFileCanBeReadAndParsed()
	{
		var acsParseService = this._services.GetRequiredService<IDecorateParseService>();
		var filePath = Path.GetFullPath(BaseFilePath);

		Assert.DoesNotThrowAsync(async () =>
		{
			var token = CancellationToken.None;
			var decorateFile = await DecorateFile.FromPathAsync(filePath, token);
			await acsParseService.ParseFileAsync(decorateFile, token);
		});
	}
}