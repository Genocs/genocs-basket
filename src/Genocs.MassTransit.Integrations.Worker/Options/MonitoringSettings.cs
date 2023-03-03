namespace Genocs.MassTransit.Integrations.Worker.Options;

public class MonitoringSettings
{
    public static string Position = "Monitoring";

    public string Jaeger { get; set; } = "localhost";
}
