namespace DoomerPublish;

internal interface IPublisherService
{
	Task<PublisherResult> DoPublishAsync(PublisherConfiguration configuration, CancellationToken cancellationToken);
}