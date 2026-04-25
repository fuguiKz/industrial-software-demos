using System.Text.Json;

var scenarios = ScenarioCatalog.Build();
var command = args.FirstOrDefault()?.Trim().ToLowerInvariant() ?? "overview";
var asJson = args.Any(arg => arg.Equals("--json", StringComparison.OrdinalIgnoreCase));
var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true
};

switch (command)
{
    case "overview":
        PrintOverview(scenarios, asJson);
        break;
    case "alarm":
        PrintScenario(scenarios["alarm"], asJson);
        break;
    case "remote-command":
        PrintScenario(scenarios["remote-command"], asJson);
        break;
    case "wafer-map":
        PrintScenario(scenarios["wafer-map"], asJson);
        break;
    case "all":
        foreach (var scenario in scenarios.Values)
        {
            PrintScenario(scenario, asJson);
            if (!asJson)
            {
                Console.WriteLine();
            }
        }
        break;
    default:
        Console.Error.WriteLine("Usage: dotnet run --project src/SecsGemWorkbench -- [overview|alarm|remote-command|wafer-map|all] [--json]");
        Environment.ExitCode = 1;
        break;
}

void PrintOverview(IReadOnlyDictionary<string, DemoScenario> scenarios, bool asJson)
{
    if (asJson)
    {
        Console.WriteLine(JsonSerializer.Serialize(scenarios.Values, jsonOptions));
        return;
    }

    Console.WriteLine("SECS/GEM Workbench Demo");
    Console.WriteLine("=======================");
    Console.WriteLine("This demo focuses on interface scenarios rather than low-level packet memorization.");
    Console.WriteLine();

    foreach (var scenario in scenarios.Values)
    {
        Console.WriteLine($"- {scenario.Command}");
        Console.WriteLine($"  {scenario.Title}");
        Console.WriteLine($"  {scenario.Description}");
    }
}

void PrintScenario(DemoScenario scenario, bool asJson)
{
    if (asJson)
    {
        Console.WriteLine(JsonSerializer.Serialize(scenario, jsonOptions));
        return;
    }

    Console.WriteLine($"{scenario.Title}");
    Console.WriteLine(new string('=', scenario.Title.Length));
    Console.WriteLine(scenario.Description);
    Console.WriteLine($"Goal: {scenario.BusinessGoal}");
    Console.WriteLine();
    Console.WriteLine("Key fields");
    Console.WriteLine("----------");

    foreach (var field in scenario.KeyFields)
    {
        Console.WriteLine($"- {field.Name}: {field.Value}");
    }

    Console.WriteLine();
    Console.WriteLine("Exchange flow");
    Console.WriteLine("-------------");

    foreach (var step in scenario.ExchangeFlow)
    {
        Console.WriteLine($"{step.Direction} {step.Topic}");
        Console.WriteLine($"  {step.Summary}");
    }
}

static class ScenarioCatalog
{
    public static IReadOnlyDictionary<string, DemoScenario> Build()
    {
        return new Dictionary<string, DemoScenario>
        {
            ["alarm"] = new(
                "alarm",
                "Alarm / Event Collection Scenario",
                "Models how the host consumes equipment alarms and converts them into actionable manufacturing events.",
                "Enable rapid response for AOI overkill and station hold conditions.",
                new[]
                {
                    new FieldItem("Equipment", "AOI-11"),
                    new FieldItem("Trigger", "Lead offset suspected on package lane 4"),
                    new FieldItem("Host Action", "Create review task and hold affected strip"),
                    new FieldItem("Expected Output", "Alarm dashboard item + engineer assignment")
                },
                new[]
                {
                    new ExchangeStep("EQP -> HOST", "Alarm report", "Equipment pushes alarm/event payload with equipment state, lot, carrier and alarm context."),
                    new ExchangeStep("HOST -> MES", "Review task creation", "Host correlates carrier data and opens a review task for the abnormal unit set."),
                    new ExchangeStep("HOST -> Engineer UI", "Actionable event", "UI renders severity, impacted lot and recommended mitigation path.")
                }),
            ["remote-command"] = new(
                "remote-command",
                "Remote Command Dispatch Scenario",
                "Demonstrates how recipe switch and lot release commands are validated before execution.",
                "Avoid blind command dispatch and keep equipment state aligned with process intent.",
                new[]
                {
                    new FieldItem("Equipment", "Besi-03"),
                    new FieldItem("Command", "LotRelease"),
                    new FieldItem("Pre-check", "Recipe version / tooling / material readiness"),
                    new FieldItem("Completion", "Result captured into MES and dispatch board")
                },
                new[]
                {
                    new ExchangeStep("HOST -> EQP", "Remote command request", "Host sends command with lot, recipe and operator context."),
                    new ExchangeStep("EQP -> HOST", "Command ack", "Equipment acknowledges receipt and reports acceptance or rejection reason."),
                    new ExchangeStep("EQP -> HOST", "Execution completion", "Completion event updates lot status and releases downstream station constraints.")
                }),
            ["wafer-map"] = new(
                "wafer-map",
                "Wafer Map Data Exchange Scenario",
                "Captures the business meaning of wafer map transfer for die attach / sort alignment rather than listing base stream-function trivia.",
                "Ensure wafer-level bin and coordinate data stay aligned between host, equipment and traceability layer.",
                new[]
                {
                    new FieldItem("Material", "WAFER-LOT-240426-08"),
                    new FieldItem("Map Purpose", "Known good die map + bad die exclusion"),
                    new FieldItem("Downstream Use", "Die pick path planning and traceability binding"),
                    new FieldItem("Validation", "Coordinate count, bin distribution, checksum")
                },
                new[]
                {
                    new ExchangeStep("HOST -> EQP", "Wafer map transfer", "Host loads wafer map payload with bin definitions, die coordinates and map revision."),
                    new ExchangeStep("EQP -> HOST", "Validation result", "Equipment validates revision/checksum and confirms the map can be consumed."),
                    new ExchangeStep("EQP -> TRACE", "Execution trace", "Consumed die positions are written back to traceability records for downstream recall.")
                })
        };
    }
}

record DemoScenario(
    string Command,
    string Title,
    string Description,
    string BusinessGoal,
    IReadOnlyList<FieldItem> KeyFields,
    IReadOnlyList<ExchangeStep> ExchangeFlow);

record FieldItem(string Name, string Value);

record ExchangeStep(string Direction, string Topic, string Summary);
