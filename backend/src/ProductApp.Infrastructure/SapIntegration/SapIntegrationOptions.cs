namespace ProductApp.Infrastructure.SapIntegration;

public sealed class SapIntegrationOptions
{
    public const string SectionName = "SapIntegration";

    public SapProvider Provider { get; init; } = SapProvider.Mock;
}

public enum SapProvider
{
    Mock,
    OData
}
