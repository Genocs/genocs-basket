﻿namespace Genocs.MassTransit.Integrations.Worker;

public class ConsoleHostedService : IHostedService
{

    public ConsoleHostedService()
    {
        // _bus = bus;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        //  await _bus.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
        //  return _bus.StopAsync(cancellationToken);
    }
}
