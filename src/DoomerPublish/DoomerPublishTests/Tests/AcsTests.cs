using DoomerPublish;
using DoomerPublish.Tools.Acs;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text;

namespace DoomerPublishTests;

public class AcsTests
{
	private const string BaseFilePath = "AcsParseFiles/basefile.acs";

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
	public void AcsFileCanBeReadAndParsed()
	{
		var acsParseService = this._services.GetRequiredService<IAcsParseService>();
		var filePath = Path.GetFullPath(BaseFilePath);

		Assert.DoesNotThrowAsync(async () =>
		{
			var token = CancellationToken.None;
			var acsFile = await AcsFile.FromPathAsync(filePath, token);
			await acsParseService.ParseFileAsync(acsFile, token);
		});
	}
}