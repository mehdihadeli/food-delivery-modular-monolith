namespace BuildingBlocks.Integration.Hangfire;

public class HangfireOptions
{
    public string ConnectionString { get; set; } = null!;
    public bool UseInMemory { get; set; }
}
