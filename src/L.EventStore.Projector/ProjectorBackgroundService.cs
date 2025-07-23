using L.EventStore.Projector.Services;
using Microsoft.Extensions.Hosting;

namespace L.EventStore.Projector;

internal sealed class ProjectorBackgroundService<TStreamIdentifier>(
    IEventEntriesService<TStreamIdentifier> eventEntriesService,
    IProjectorService<TStreamIdentifier> projectorService) : BackgroundService
    where TStreamIdentifier : IEquatable<TStreamIdentifier>, IComparable<TStreamIdentifier>
{
    private readonly IEventEntriesService<TStreamIdentifier> _eventEntriesService = 
        eventEntriesService ?? throw new ArgumentNullException(nameof(eventEntriesService));

    private readonly IProjectorService<TStreamIdentifier> _projectorService = projectorService
        ?? throw new ArgumentNullException(nameof(projectorService));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var eventEntries = await _eventEntriesService.GetUnprocessedEventEntriesAsync();

            await _projectorService.ProjectAsync(eventEntries);
        }
    }
}
