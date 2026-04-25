using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.WriteIndented = true;
});

var app = builder.Build();
var seed = TraceabilitySeed.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/summary", () => new
{
    unitCount = seed.Units.Count,
    abnormalCount = seed.Units.Count(unit => unit.AiReview.FinalDecision != "Pass"),
    latestLot = seed.Units.MaxBy(unit => unit.LastUpdated)?.LotNumber
});

app.MapGet("/api/units", (string? query) =>
{
    IEnumerable<TraceabilityUnit> results = seed.Units;

    if (!string.IsNullOrWhiteSpace(query))
    {
        var normalized = query.Trim();
        results = results.Where(unit =>
            unit.SerialNumber.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
            unit.LotNumber.Contains(normalized, StringComparison.OrdinalIgnoreCase));
    }

    return results
        .OrderByDescending(unit => unit.LastUpdated)
        .Select(unit => new
        {
            unit.SerialNumber,
            unit.LotNumber,
            unit.PackageName,
            unit.CurrentStation,
            unit.Grade,
            unit.LastUpdated
        });
});

app.MapGet("/api/units/{serialNumber}", (string serialNumber) =>
{
    var unit = seed.Units.FirstOrDefault(item =>
        item.SerialNumber.Equals(serialNumber, StringComparison.OrdinalIgnoreCase));

    return unit is null ? Results.NotFound() : Results.Ok(unit);
});

app.Run();

static class TraceabilitySeed
{
    public static TraceabilitySeedData Build()
    {
        return new TraceabilitySeedData(new[]
        {
            new TraceabilityUnit(
                "PKG-240426-00017",
                "LOT-KZ-240426-17",
                "QFN-56",
                "AOI Review",
                "MRB Hold",
                DateTimeOffset.Parse("2026-04-26T09:12:00+08:00"),
                new[]
                {
                    new StationEvent("Die Attach", "Completed", "Besi-03", DateTimeOffset.Parse("2026-04-26T07:58:00+08:00")),
                    new StationEvent("Wire Bond", "Completed", "ASM-WB-02", DateTimeOffset.Parse("2026-04-26T08:21:00+08:00")),
                    new StationEvent("Molding", "Completed", "MOLD-01", DateTimeOffset.Parse("2026-04-26T08:44:00+08:00")),
                    new StationEvent("AOI", "Review Required", "AOI-11", DateTimeOffset.Parse("2026-04-26T09:03:00+08:00"))
                },
                new[]
                {
                    new ImageReview("Front View", "AOI-11-CAM-A", "Lead offset suspected", "Review", 0.94),
                    new ImageReview("45° View", "AOI-11-CAM-B", "Potential overkill", "Recheck", 0.88),
                    new ImageReview("Reference Template", "Golden Sample", "Matched package edge and bond pad", "Reference", 1.0)
                },
                new AiReviewSummary("ResNet-v2.3", "Offset anomaly around pin group 12-14", "Review", "Yield risk medium, recommend engineer confirmation.")),
            new TraceabilityUnit(
                "PKG-240426-00011",
                "LOT-KZ-240426-11",
                "BGA-128",
                "Packing",
                "Pass",
                DateTimeOffset.Parse("2026-04-26T08:56:00+08:00"),
                new[]
                {
                    new StationEvent("Die Attach", "Completed", "Besi-04", DateTimeOffset.Parse("2026-04-26T07:20:00+08:00")),
                    new StationEvent("Wire Bond", "Completed", "ASM-WB-01", DateTimeOffset.Parse("2026-04-26T07:48:00+08:00")),
                    new StationEvent("Singulation", "Completed", "SAW-06", DateTimeOffset.Parse("2026-04-26T08:13:00+08:00")),
                    new StationEvent("Packing", "Released", "PK-01", DateTimeOffset.Parse("2026-04-26T08:56:00+08:00"))
                },
                new[]
                {
                    new ImageReview("Top AOI", "AOI-07-CAM-A", "No visible anomaly", "Pass", 0.97),
                    new ImageReview("Marking OCR", "OCR-02", "Marking recognized and verified", "Pass", 0.99)
                },
                new AiReviewSummary("ResNet-v2.3", "Package body and marking are aligned with template.", "Pass", "Auto release enabled.")),
            new TraceabilityUnit(
                "PKG-240426-00024",
                "LOT-KZ-240426-24",
                "DFN-8",
                "Wire Bond",
                "Monitoring",
                DateTimeOffset.Parse("2026-04-26T09:05:00+08:00"),
                new[]
                {
                    new StationEvent("Die Attach", "Completed", "Besi-02", DateTimeOffset.Parse("2026-04-26T08:04:00+08:00")),
                    new StationEvent("Wire Bond", "Sampling Hold", "ASM-WB-03", DateTimeOffset.Parse("2026-04-26T09:05:00+08:00"))
                },
                new[]
                {
                    new ImageReview("Bond View", "WB-Inspect-03", "Loop height variation within watch band", "Monitor", 0.81)
                },
                new AiReviewSummary("ResNet-v2.3", "Bond profile within alert threshold but not beyond control limit.", "Monitoring", "Keep sampling for next 3 strips.")),
        });
    }
}

record TraceabilitySeedData(IReadOnlyList<TraceabilityUnit> Units);

record TraceabilityUnit(
    string SerialNumber,
    string LotNumber,
    string PackageName,
    string CurrentStation,
    string Grade,
    DateTimeOffset LastUpdated,
    IReadOnlyList<StationEvent> Route,
    IReadOnlyList<ImageReview> ImageReviews,
    AiReviewSummary AiReview);

record StationEvent(string StationName, string Status, string EquipmentCode, DateTimeOffset Timestamp);

record ImageReview(string Title, string Source, string Summary, string Verdict, double Confidence);

record AiReviewSummary(string ModelVersion, string Finding, string FinalDecision, string Recommendation);
