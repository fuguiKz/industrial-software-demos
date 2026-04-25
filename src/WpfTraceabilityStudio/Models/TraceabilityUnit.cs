namespace WpfTraceabilityStudio.Models;

public sealed class TraceabilityUnit
{
    public required string SerialNumber { get; init; }
    public required string LotNumber { get; init; }
    public required string PackageName { get; init; }
    public required string CurrentStation { get; init; }
    public required string Grade { get; init; }
    public required string Customer { get; init; }
    public required string AiModelVersion { get; init; }
    public required string AiDecision { get; init; }
    public required string AiRecommendation { get; init; }
    public required string EngineerSummary { get; init; }
    public required string RiskLevel { get; init; }
    public IReadOnlyList<StationEvent> Route { get; init; } = Array.Empty<StationEvent>();
    public IReadOnlyList<ReviewImageCard> ReviewImages { get; init; } = Array.Empty<ReviewImageCard>();
}

public sealed class StationEvent
{
    public required string StationName { get; init; }
    public required string EquipmentCode { get; init; }
    public required string Status { get; init; }
    public required string Timestamp { get; init; }
    public required string Summary { get; init; }
}

public sealed class ReviewImageCard
{
    public required string Title { get; init; }
    public required string Source { get; init; }
    public required string Verdict { get; init; }
    public required string Finding { get; init; }
    public required string ConfidenceText { get; init; }
}
