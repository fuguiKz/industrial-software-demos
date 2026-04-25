namespace WinFormsEapGateTool.Models;

public sealed class GateScenario
{
    public required string LotNumber { get; init; }
    public required string EquipmentCode { get; init; }
    public required string RecipeName { get; init; }
    public required string ProductName { get; init; }
    public required string OperatorName { get; init; }
    public required string Status { get; init; }
    public required string Recommendation { get; init; }
    public required IReadOnlyList<GateCheckItem> Checks { get; init; }
}

public sealed class GateCheckItem
{
    public required string CheckPoint { get; init; }
    public required string Result { get; init; }
    public required string Note { get; init; }
    public bool Mandatory { get; init; }
}
