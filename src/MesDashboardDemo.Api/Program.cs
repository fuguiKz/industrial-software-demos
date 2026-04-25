using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();
var seed = MesSeed.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/overview", () => seed.Overview);
app.MapGet("/api/lines", () => seed.Lines);
app.MapGet("/api/trends/hourly", (string? line) =>
{
    var selectedLine = string.IsNullOrWhiteSpace(line) ? seed.Overview.LineName : line.Trim();
    return seed.HourlyTrend.Where(item => item.LineName.Equals(selectedLine, StringComparison.OrdinalIgnoreCase));
});
app.MapGet("/api/defects/distribution", () => seed.DefectDistribution);
app.MapGet("/api/alerts", (int? take) => seed.Alerts
    .OrderByDescending(item => item.OccurredAt)
    .Take(Math.Clamp(take ?? 6, 1, 20)));
app.MapGet("/api/equipment", () => seed.Equipment);
app.MapGet("/api/dispatch/recommendation", () => seed.DispatchRecommendation);

app.Run();

static class MesSeed
{
    public static MesSeedData Build()
    {
        var hourlyTrend = new[]
        {
            new HourlyMetric("TS-BESI-03", "08:00", 74.3, 98.5, 1.91, 1402),
            new HourlyMetric("TS-BESI-03", "09:00", 76.8, 98.7, 1.87, 1455),
            new HourlyMetric("TS-BESI-03", "10:00", 79.6, 98.8, 1.82, 1512),
            new HourlyMetric("TS-BESI-03", "11:00", 81.1, 99.0, 1.77, 1568),
            new HourlyMetric("TS-BESI-03", "12:00", 80.4, 98.9, 1.79, 1540),
            new HourlyMetric("TS-BESI-03", "13:00", 82.9, 99.1, 1.74, 1606),
            new HourlyMetric("TS-BESI-03", "14:00", 84.2, 99.2, 1.71, 1641),
            new HourlyMetric("TS-BESI-03", "15:00", 83.5, 99.0, 1.73, 1614)
        };

        var overview = new DashboardOverview(
            "TS-BESI-03",
            "白班",
            83.5,
            99.0,
            1.73,
            11854,
            "BGA-AOI / Die Attach 联线看板",
            4,
            1,
            "AOI 复判队列低于阈值，可维持当前节拍");

        return new MesSeedData(
            overview,
            new[]
            {
                new LineCard("TS-BESI-03", "封装一线", "Auto", 83.5, 99.0, 1.73),
                new LineCard("ASM-WB-02", "打线二线", "Attention", 79.8, 98.6, 1.94),
                new LineCard("MOLD-01", "塑封线", "Stable", 86.2, 99.3, 1.61)
            },
            hourlyTrend,
            new[]
            {
                new DefectSlice("虚焊", 23),
                new DefectSlice("引脚偏移", 17),
                new DefectSlice("漏检复判", 11),
                new DefectSlice("封装崩角", 8),
                new DefectSlice("其他", 6)
            },
            new[]
            {
                new AlertItem("ALM-240426-031", "TS-BESI-03", "AOI 复判排队持续超过 4 分钟", AlertSeverity.Medium, DateTimeOffset.Parse("2026-04-26T08:41:00+08:00")),
                new AlertItem("ALM-240426-029", "ASM-WB-02", "引线偏移异常，建议触发 recipe 复核", AlertSeverity.High, DateTimeOffset.Parse("2026-04-26T08:35:00+08:00")),
                new AlertItem("ALM-240426-021", "MOLD-01", "模压温区恢复稳定，告警已解除", AlertSeverity.Low, DateTimeOffset.Parse("2026-04-26T08:18:00+08:00"))
            },
            new[]
            {
                new EquipmentCard("TS-BESI-03", "Besi Die Attach", "Running", 91.2, 2, "UPH 稳定，可继续承接高优先级批次"),
                new EquipmentCard("TS-BESI-04", "Besi Die Attach", "Idle", 0, 0, "等待工单切换，建议插单补齐空档"),
                new EquipmentCard("AOI-11", "AOI Review Station", "Running", 74.5, 12, "AI 复判模型已切换到 ResNet-v2"),
                new EquipmentCard("WB-02", "ASM Wire Bonder", "Alarm", 32.8, 1, "建议先完成 capillary 校准再放行")
            },
            new DispatchRecommendation(
                "保持 TS-BESI-03 当前 recipe，不建议切换机种。",
                "AOI 复判积压已回落，优先将批次 KZ240426-17 切入 TS-BESI-04，避免 WB-02 成为瓶颈。"));
    }
}

record MesSeedData(
    DashboardOverview Overview,
    IReadOnlyList<LineCard> Lines,
    IReadOnlyList<HourlyMetric> HourlyTrend,
    IReadOnlyList<DefectSlice> DefectDistribution,
    IReadOnlyList<AlertItem> Alerts,
    IReadOnlyList<EquipmentCard> Equipment,
    DispatchRecommendation DispatchRecommendation);

record DashboardOverview(
    string LineName,
    string ShiftName,
    double Oee,
    double Yield,
    double CycleTimeSeconds,
    int Output,
    string Focus,
    int RunningEquipment,
    int AlarmEquipment,
    string Recommendation);

record LineCard(string Code, string Name, string Status, double Oee, double Yield, double CycleTimeSeconds);

record HourlyMetric(string LineName, string Hour, double Oee, double Yield, double CycleTimeSeconds, int Throughput);

record DefectSlice(string Name, int Count);

record AlertItem(string Code, string EquipmentCode, string Message, AlertSeverity Severity, DateTimeOffset OccurredAt);

record EquipmentCard(string Code, string Model, string Status, double Utilization, int QueueCount, string Note);

record DispatchRecommendation(string Decision, string Reason);

enum AlertSeverity
{
    Low,
    Medium,
    High
}
