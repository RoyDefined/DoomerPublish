using DoomerPublish;
using DoomerPublish.Tools.Acs;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text;

namespace DoomerPublishTests;

internal sealed class AcsTests
{
	private const string BaseFilePath = "AcsParseFiles/basefile.acs";

	private ServiceProvider _services;

	[OneTimeSetUp]
	public void Setup()
	{
		this._services = new ServiceCollection()
			.AddLogging()
			.AddDoomerPublish()
			.BuildServiceProvider();
	}

	[OneTimeTearDown]
	public void TearDown()
	{
		this._services.Dispose();
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