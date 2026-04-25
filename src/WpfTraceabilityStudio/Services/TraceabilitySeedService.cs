using WpfTraceabilityStudio.Models;

namespace WpfTraceabilityStudio.Services;

public sealed class TraceabilitySeedService
{
    public IReadOnlyList<TraceabilityUnit> Load()
    {
        return new[]
        {
            new TraceabilityUnit
            {
                SerialNumber = "PKG-240426-00017",
                LotNumber = "LOT-KZ-240426-17",
                PackageName = "QFN-56",
                CurrentStation = "AOI Review",
                Grade = "MRB Hold",
                Customer = "Automotive PMIC",
                AiModelVersion = "ResNet-v2.3",
                AiDecision = "Review Required",
                AiRecommendation = "建议复核 pin 12-14 的 lead offset，并保留当前 strip 作为工程样本。",
                EngineerSummary = "该样本适合作为物料-影像关联分析的演示对象，能够展示站点追溯、影像复判和工程判定闭环。",
                RiskLevel = "Medium",
                Route = new[]
                {
                    new StationEvent
                    {
                        StationName = "Die Attach",
                        EquipmentCode = "Besi-03",
                        Status = "Completed",
                        Timestamp = "04-26 07:58",
                        Summary = "Die attach 完成，wafer map revision = R12。"
                    },
                    new StationEvent
                    {
                        StationName = "Wire Bond",
                        EquipmentCode = "ASM-WB-02",
                        Status = "Completed",
                        Timestamp = "04-26 08:21",
                        Summary = "Bond profile 正常，但后续 AOI 发现疑似 pin shift。"
                    },
                    new StationEvent
                    {
                        StationName = "Molding",
                        EquipmentCode = "MOLD-01",
                        Status = "Completed",
                        Timestamp = "04-26 08:44",
                        Summary = "模压参数在控，关联 carrier 已绑定。"
                    },
                    new StationEvent
                    {
                        StationName = "AOI",
                        EquipmentCode = "AOI-11",
                        Status = "Review Required",
                        Timestamp = "04-26 09:03",
                        Summary = "模型判断为 offset anomaly，触发工程复判队列。"
                    }
                },
                ReviewImages = new[]
                {
                    new ReviewImageCard
                    {
                        Title = "Front View",
                        Source = "AOI-11-CAM-A",
                        Verdict = "Review",
                        Finding = "Lead offset suspected around pin group 12-14.",
                        ConfidenceText = "94%"
                    },
                    new ReviewImageCard
                    {
                        Title = "45° View",
                        Source = "AOI-11-CAM-B",
                        Verdict = "Recheck",
                        Finding = "Possible overkill, package edge is still within warning band.",
                        ConfidenceText = "88%"
                    },
                    new ReviewImageCard
                    {
                        Title = "Golden Sample",
                        Source = "Reference",
                        Verdict = "Baseline",
                        Finding = "Used for engineer-side image alignment and feature comparison.",
                        ConfidenceText = "100%"
                    }
                }
            },
            new TraceabilityUnit
            {
                SerialNumber = "PKG-240426-00011",
                LotNumber = "LOT-KZ-240426-11",
                PackageName = "BGA-128",
                CurrentStation = "Packing",
                Grade = "Pass",
                Customer = "Consumer SoC",
                AiModelVersion = "ResNet-v2.3",
                AiDecision = "Pass",
                AiRecommendation = "结果可自动放行，无需工程介入。",
                EngineerSummary = "适合演示正常样本的快速回溯与多站点履历关联。",
                RiskLevel = "Low",
                Route = new[]
                {
                    new StationEvent
                    {
                        StationName = "Die Attach",
                        EquipmentCode = "Besi-04",
                        Status = "Completed",
                        Timestamp = "04-26 07:20",
                        Summary = "Die attach UPH 正常，未触发 hold。"
                    },
                    new StationEvent
                    {
                        StationName = "Wire Bond",
                        EquipmentCode = "ASM-WB-01",
                        Status = "Completed",
                        Timestamp = "04-26 07:48",
                        Summary = "Bond pull 抽检通过。"
                    },
                    new StationEvent
                    {
                        StationName = "Singulation",
                        EquipmentCode = "SAW-06",
                        Status = "Completed",
                        Timestamp = "04-26 08:13",
                        Summary = "切割完成，序列号写入 trace layer。"
                    },
                    new StationEvent
                    {
                        StationName = "Packing",
                        EquipmentCode = "PK-01",
                        Status = "Released",
                        Timestamp = "04-26 08:56",
                        Summary = "包装放行，生成最终 shipment trace。"
                    }
                },
                ReviewImages = new[]
                {
                    new ReviewImageCard
                    {
                        Title = "Top AOI",
                        Source = "AOI-07-CAM-A",
                        Verdict = "Pass",
                        Finding = "No visible anomaly, marking and edge alignment are stable.",
                        ConfidenceText = "97%"
                    },
                    new ReviewImageCard
                    {
                        Title = "OCR Validation",
                        Source = "OCR-02",
                        Verdict = "Pass",
                        Finding = "Marking recognized and lot linked successfully.",
                        ConfidenceText = "99%"
                    }
                }
            }
        };
    }
}
