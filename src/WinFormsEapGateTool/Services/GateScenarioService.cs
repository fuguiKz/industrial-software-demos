using WinFormsEapGateTool.Models;

namespace WinFormsEapGateTool.Services;

public sealed class GateScenarioService
{
    public IReadOnlyList<GateScenario> Load()
    {
        return new[]
        {
            new GateScenario
            {
                LotNumber = "LOT-KZ-240426-17",
                EquipmentCode = "Besi-03",
                RecipeName = "DA_QFN56_REV12",
                ProductName = "Automotive PMIC",
                OperatorName = "ENG_ZHENG",
                Status = "Review Required",
                Recommendation = "AOI 异常批次建议保持 Lock 状态，先完成 recipe / tooling / wafer map 三项复核。",
                Checks = new[]
                {
                    new GateCheckItem { CheckPoint = "Recipe version", Result = "Pass", Note = "设备 recipe = MES 下发版本 REV12。", Mandatory = true },
                    new GateCheckItem { CheckPoint = "Tooling readiness", Result = "Pass", Note = "吸嘴与 collet 状态正常。", Mandatory = true },
                    new GateCheckItem { CheckPoint = "Wafer map revision", Result = "Review", Note = "Host revision 与设备缓存 revision 不一致。", Mandatory = true },
                    new GateCheckItem { CheckPoint = "Carrier binding", Result = "Pass", Note = "Carrier / lot / strip 绑定成功。", Mandatory = true },
                    new GateCheckItem { CheckPoint = "AOI feedback", Result = "Review", Note = "上一工序复判队列未清空。", Mandatory = false }
                }
            },
            new GateScenario
            {
                LotNumber = "LOT-KZ-240426-11",
                EquipmentCode = "Besi-04",
                RecipeName = "DA_BGA128_REV08",
                ProductName = "Consumer SoC",
                OperatorName = "ENG_ZHENG",
                Status = "Ready To Release",
                Recommendation = "所有关键校验通过，可执行 release 并同步 dispatch board。",
                Checks = new[]
                {
                    new GateCheckItem { CheckPoint = "Recipe version", Result = "Pass", Note = "版本一致。", Mandatory = true },
                    new GateCheckItem { CheckPoint = "Tooling readiness", Result = "Pass", Note = "换线确认完成。", Mandatory = true },
                    new GateCheckItem { CheckPoint = "Wafer map revision", Result = "Pass", Note = "Map checksum 校验成功。", Mandatory = true },
                    new GateCheckItem { CheckPoint = "Carrier binding", Result = "Pass", Note = "全部 carrier 已关联。", Mandatory = true },
                    new GateCheckItem { CheckPoint = "Engineer hold flag", Result = "Pass", Note = "无 hold flag。", Mandatory = true }
                }
            }
        };
    }
}
