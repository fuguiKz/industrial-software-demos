using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;
using WinFormsEapGateTool.Models;
using WinFormsEapGateTool.Services;

namespace WinFormsEapGateTool;

public sealed class MainForm : Form
{
    private readonly ComboBox _lotSelector = new();
    private readonly Label _statusValue = new();
    private readonly Label _equipmentValue = new();
    private readonly Label _recipeValue = new();
    private readonly Label _productValue = new();
    private readonly Label _operatorValue = new();
    private readonly Label _recommendationValue = new();
    private readonly DataGridView _checksGrid = new();
    private readonly RichTextBox _payloadPreview = new();
    private readonly ListBox _eventLog = new();
    private readonly Button _validateButton = new();
    private readonly Button _releaseButton = new();
    private readonly Button _lockButton = new();
    private readonly Button _waferMapButton = new();
    private readonly IReadOnlyList<GateScenario> _scenarios;

    public MainForm()
    {
        _scenarios = new GateScenarioService().Load();
        InitializeComponent();
        BindScenarios();
    }

    private void InitializeComponent()
    {
        Text = "WinForms EAP Gate Tool";
        Width = 1420;
        Height = 900;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(242, 246, 252);

        var heroPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 104,
            BackColor = Color.FromArgb(20, 59, 132),
            Padding = new Padding(20)
        };

        var titleLabel = new Label
        {
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            Text = "EAP 站位卡控 / Lock-Release 工程工具"
        };

        var subtitleLabel = new Label
        {
            AutoSize = true,
            Top = 58,
            ForeColor = Color.FromArgb(216, 229, 255),
            Font = new Font("Segoe UI", 10),
            Text = "贴合简历里的上位机 / 工程工具场景：recipe 校验、Wafer Map 复核、Lock/Release 决策与日志追踪。"
        };

        heroPanel.Controls.Add(titleLabel);
        heroPanel.Controls.Add(subtitleLabel);

        var topPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 160,
            ColumnCount = 2,
            Padding = new Padding(20, 16, 20, 0)
        };
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));

        var selectorCard = BuildSelectorCard();
        var statusCard = BuildStatusCard();
        topPanel.Controls.Add(selectorCard, 0, 0);
        topPanel.Controls.Add(statusCard, 1, 0);

        var contentPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20, 16, 20, 20),
            ColumnCount = 2
        };
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));

        var leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

        var checksCard = BuildChecksCard();
        var logCard = BuildEventLogCard();
        leftPanel.Controls.Add(checksCard, 0, 0);
        leftPanel.Controls.Add(logCard, 0, 1);

        var rightPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2
        };
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 55));

        var recommendationCard = BuildRecommendationCard();
        var payloadCard = BuildPayloadCard();
        rightPanel.Controls.Add(recommendationCard, 0, 0);
        rightPanel.Controls.Add(payloadCard, 0, 1);

        contentPanel.Controls.Add(leftPanel, 0, 0);
        contentPanel.Controls.Add(rightPanel, 1, 0);

        Controls.Add(contentPanel);
        Controls.Add(topPanel);
        Controls.Add(heroPanel);
    }

    private Control BuildSelectorCard()
    {
        var card = BuildCard();
        var title = BuildSectionTitle("Lot / Recipe 上控对象");

        _lotSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        _lotSelector.Font = new Font("Segoe UI", 11);
        _lotSelector.Width = 280;
        _lotSelector.SelectedIndexChanged += (_, _) => RefreshScenario();

        _validateButton.Text = "Validate";
        _releaseButton.Text = "Release";
        _lockButton.Text = "Force Lock";
        _waferMapButton.Text = "Wafer Map Check";

        foreach (var button in new[] { _validateButton, _releaseButton, _lockButton, _waferMapButton })
        {
            button.AutoSize = true;
            button.Padding = new Padding(14, 8, 14, 8);
            button.BackColor = Color.FromArgb(32, 74, 162);
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
        }

        _validateButton.Click += (_, _) => ValidateScenario();
        _releaseButton.Click += (_, _) => ReleaseScenario();
        _lockButton.Click += (_, _) => ForceLock();
        _waferMapButton.Click += (_, _) => PreviewWaferMapCheck();

        var row1 = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 14, 0, 0)
        };
        row1.Controls.Add(new Label
        {
            AutoSize = true,
            Text = "Lot:",
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Padding = new Padding(0, 10, 8, 0)
        });
        row1.Controls.Add(_lotSelector);

        var row2 = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Padding = new Padding(0, 14, 0, 0)
        };
        row2.Controls.Add(_validateButton);
        row2.Controls.Add(_releaseButton);
        row2.Controls.Add(_lockButton);
        row2.Controls.Add(_waferMapButton);

        card.Controls.Add(row2);
        card.Controls.Add(row1);
        card.Controls.Add(title);
        return card;
    }

    private Control BuildStatusCard()
    {
        var card = BuildCard();
        var title = BuildSectionTitle("设备 / recipe / 当前结论");
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true,
            Padding = new Padding(0, 14, 0, 0)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddInfoRow(grid, "Status", _statusValue, 0, true);
        AddInfoRow(grid, "Equipment", _equipmentValue, 1, false);
        AddInfoRow(grid, "Recipe", _recipeValue, 2, false);
        AddInfoRow(grid, "Product", _productValue, 3, false);
        AddInfoRow(grid, "Operator", _operatorValue, 4, false);

        card.Controls.Add(grid);
        card.Controls.Add(title);
        return card;
    }

    private Control BuildChecksCard()
    {
        var card = BuildCard();
        var title = BuildSectionTitle("站位卡控检查项");

        _checksGrid.Dock = DockStyle.Fill;
        _checksGrid.BackgroundColor = Color.White;
        _checksGrid.BorderStyle = BorderStyle.None;
        _checksGrid.AllowUserToAddRows = false;
        _checksGrid.AllowUserToDeleteRows = false;
        _checksGrid.AllowUserToResizeRows = false;
        _checksGrid.ReadOnly = true;
        _checksGrid.RowHeadersVisible = false;
        _checksGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _checksGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _checksGrid.ColumnCount = 4;
        _checksGrid.Columns[0].Name = "CheckPoint";
        _checksGrid.Columns[1].Name = "Mandatory";
        _checksGrid.Columns[2].Name = "Result";
        _checksGrid.Columns[3].Name = "Note";

        var host = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 14, 0, 0)
        };
        host.Controls.Add(_checksGrid);

        card.Controls.Add(host);
        card.Controls.Add(title);
        return card;
    }

    private Control BuildRecommendationCard()
    {
        var card = BuildCard();
        var title = BuildSectionTitle("工程建议");

        _recommendationValue.Dock = DockStyle.Fill;
        _recommendationValue.Padding = new Padding(0, 14, 0, 0);
        _recommendationValue.Font = new Font("Segoe UI", 11);
        _recommendationValue.ForeColor = Color.FromArgb(68, 84, 120);
        _recommendationValue.AutoSize = false;

        card.Controls.Add(_recommendationValue);
        card.Controls.Add(title);
        return card;
    }

    private Control BuildPayloadCard()
    {
        var card = BuildCard();
        var title = BuildSectionTitle("远程命令 / Wafer Map 校验预览");

        _payloadPreview.Dock = DockStyle.Fill;
        _payloadPreview.ReadOnly = true;
        _payloadPreview.Font = new Font("Consolas", 10.5f);
        _payloadPreview.BorderStyle = BorderStyle.None;
        _payloadPreview.BackColor = Color.White;

        var host = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 14, 0, 0)
        };
        host.Controls.Add(_payloadPreview);

        card.Controls.Add(host);
        card.Controls.Add(title);
        return card;
    }

    private Control BuildEventLogCard()
    {
        var card = BuildCard();
        var title = BuildSectionTitle("操作日志");

        _eventLog.Dock = DockStyle.Fill;
        _eventLog.BorderStyle = BorderStyle.None;
        _eventLog.Font = new Font("Consolas", 10.5f);

        var host = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 14, 0, 0)
        };
        host.Controls.Add(_eventLog);

        card.Controls.Add(host);
        card.Controls.Add(title);
        return card;
    }

    private static Panel BuildCard()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(18),
            Margin = new Padding(0, 0, 18, 18)
        };
    }

    private static Label BuildSectionTitle(string text)
    {
        return new Label
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            Font = new Font("Segoe UI", 13, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 38, 77),
            Text = text
        };
    }

    private static void AddInfoRow(TableLayoutPanel grid, string key, Label valueLabel, int rowIndex, bool accent)
    {
        if (grid.RowStyles.Count <= rowIndex)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        grid.Controls.Add(new Label
        {
            AutoSize = true,
            Text = key,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Color.FromArgb(95, 112, 149),
            Padding = new Padding(0, 10, 0, 0)
        }, 0, rowIndex);

        valueLabel.AutoSize = true;
        valueLabel.Text = "--";
        valueLabel.Font = accent ? new Font("Segoe UI", 12, FontStyle.Bold) : new Font("Segoe UI", 11);
        valueLabel.ForeColor = accent ? Color.FromArgb(20, 59, 132) : Color.FromArgb(17, 38, 77);
        valueLabel.Padding = new Padding(0, 10, 0, 0);
        grid.Controls.Add(valueLabel, 1, rowIndex);
    }

    private void BindScenarios()
    {
        _lotSelector.DataSource = _scenarios.ToList();
        _lotSelector.DisplayMember = nameof(GateScenario.LotNumber);
        RefreshScenario();
        AddLog("Initialized EAP gate tool demo.");
    }

    private void RefreshScenario()
    {
        var scenario = CurrentScenario;
        if (scenario is null)
        {
            return;
        }

        _statusValue.Text = scenario.Status;
        _equipmentValue.Text = scenario.EquipmentCode;
        _recipeValue.Text = scenario.RecipeName;
        _productValue.Text = scenario.ProductName;
        _operatorValue.Text = scenario.OperatorName;
        _recommendationValue.Text = scenario.Recommendation;

        _checksGrid.Rows.Clear();
        foreach (var item in scenario.Checks)
        {
            _checksGrid.Rows.Add(item.CheckPoint, item.Mandatory ? "Yes" : "No", item.Result, item.Note);
        }

        _payloadPreview.Text = BuildCommandPreview(scenario, "ValidateOnly");
        AddLog($"Loaded scenario: {scenario.LotNumber}");
    }

    private void ValidateScenario()
    {
        var scenario = CurrentScenario;
        if (scenario is null)
        {
            return;
        }

        var hasMandatoryReview = scenario.Checks.Any(item => item.Mandatory && item.Result != "Pass");
        _statusValue.Text = hasMandatoryReview ? "Hold / Review" : "Validated";
        _payloadPreview.Text = BuildCommandPreview(scenario, "Validate");
        AddLog($"Validated {scenario.LotNumber}: {(hasMandatoryReview ? "needs review" : "ready")}");
    }

    private void ReleaseScenario()
    {
        var scenario = CurrentScenario;
        if (scenario is null)
        {
            return;
        }

        var canRelease = scenario.Checks.All(item => !item.Mandatory || item.Result == "Pass");
        _statusValue.Text = canRelease ? "Released" : "Blocked";
        _payloadPreview.Text = BuildCommandPreview(scenario, canRelease ? "LotRelease" : "ReleaseBlocked");
        AddLog(canRelease
            ? $"Release command preview generated for {scenario.LotNumber}"
            : $"Release blocked for {scenario.LotNumber} because mandatory checks failed");
    }

    private void ForceLock()
    {
        var scenario = CurrentScenario;
        if (scenario is null)
        {
            return;
        }

        _statusValue.Text = "Locked";
        _payloadPreview.Text = BuildCommandPreview(scenario, "ForceLock");
        AddLog($"Force lock issued for {scenario.LotNumber}");
    }

    private void PreviewWaferMapCheck()
    {
        var scenario = CurrentScenario;
        if (scenario is null)
        {
            return;
        }

        var payload = new
        {
            scenario.LotNumber,
            scenario.EquipmentCode,
            Check = "WaferMapRevision",
            ExpectedRevision = scenario.RecipeName.Contains("REV12", StringComparison.OrdinalIgnoreCase) ? "R12" : "R08",
            DeviceCache = scenario.Checks.FirstOrDefault(item => item.CheckPoint == "Wafer map revision")?.Result,
            HostDecision = scenario.Status
        };

        _payloadPreview.Text = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        AddLog($"Wafer map check preview refreshed for {scenario.LotNumber}");
    }

    private string BuildCommandPreview(GateScenario scenario, string command)
    {
        var payload = new
        {
            Command = command,
            scenario.LotNumber,
            scenario.EquipmentCode,
            scenario.RecipeName,
            scenario.ProductName,
            scenario.OperatorName,
            Checks = scenario.Checks.Select(item => new
            {
                item.CheckPoint,
                item.Result,
                item.Mandatory
            }),
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }

    private GateScenario? CurrentScenario => _lotSelector.SelectedItem as GateScenario;

    private void AddLog(string message)
    {
        _eventLog.Items.Insert(0, $"{DateTime.Now:HH:mm:ss}  {message}");
        while (_eventLog.Items.Count > 12)
        {
            _eventLog.Items.RemoveAt(_eventLog.Items.Count - 1);
        }
    }
}
