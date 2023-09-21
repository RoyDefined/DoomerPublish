namespace DoomerPublish;

internal interface IPublishTask
{
	Task RunAsync(PublishContext context, CancellationToken stoppingToken);
}
