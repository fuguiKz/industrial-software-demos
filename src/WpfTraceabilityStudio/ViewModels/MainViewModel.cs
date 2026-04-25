using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfTraceabilityStudio.Infrastructure;
using WpfTraceabilityStudio.Models;
using WpfTraceabilityStudio.Services;

namespace WpfTraceabilityStudio.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IReadOnlyList<TraceabilityUnit> _units;
    private readonly RelayCommand _searchCommand;
    private readonly RelayCommand _loadSampleCommand;
    private readonly RelayCommand _exportCommand;
    private string _searchKeyword = "PKG-240426-00017";
    private TraceabilityUnit? _selectedUnit;
    private string _statusHeadline = "已加载异常样本";
    private string _statusDetail = "当前演示强调物料-影像关联、AOI 复判与工程闭环。";

    public MainViewModel()
    {
        _units = new TraceabilitySeedService().Load();
        Route = new ObservableCollection<StationEvent>();
        ReviewImages = new ObservableCollection<ReviewImageCard>();
        ActivityLogs = new ObservableCollection<string>();

        _searchCommand = new RelayCommand(Search);
        _loadSampleCommand = new RelayCommand(parameter => LoadSample(parameter?.ToString()));
        _exportCommand = new RelayCommand(ExportSummary, () => SelectedUnit is not null);

        SelectUnit(_units[0]);
    }

    public string SearchKeyword
    {
        get => _searchKeyword;
        set => SetProperty(ref _searchKeyword, value);
    }

    public string StatusHeadline
    {
        get => _statusHeadline;
        private set => SetProperty(ref _statusHeadline, value);
    }

    public string StatusDetail
    {
        get => _statusDetail;
        private set => SetProperty(ref _statusDetail, value);
    }

    public string SelectedSerialNumber => SelectedUnit?.SerialNumber ?? "--";
    public string SelectedLotNumber => SelectedUnit?.LotNumber ?? "--";
    public string SelectedPackageName => SelectedUnit?.PackageName ?? "--";
    public string SelectedCurrentStation => SelectedUnit?.CurrentStation ?? "--";
    public string SelectedGrade => SelectedUnit?.Grade ?? "--";
    public string SelectedCustomer => SelectedUnit?.Customer ?? "--";
    public string AiModelVersion => SelectedUnit?.AiModelVersion ?? "--";
    public string AiDecision => SelectedUnit?.AiDecision ?? "--";
    public string AiRecommendation => SelectedUnit?.AiRecommendation ?? "--";
    public string EngineerSummary => SelectedUnit?.EngineerSummary ?? "--";
    public string RiskLevel => SelectedUnit?.RiskLevel ?? "--";

    public ObservableCollection<StationEvent> Route { get; }
    public ObservableCollection<ReviewImageCard> ReviewImages { get; }
    public ObservableCollection<string> ActivityLogs { get; }

    public ICommand SearchCommand => _searchCommand;
    public ICommand LoadSampleCommand => _loadSampleCommand;
    public ICommand ExportCommand => _exportCommand;

    private TraceabilityUnit? SelectedUnit
    {
        get => _selectedUnit;
        set
        {
            if (SetProperty(ref _selectedUnit, value))
            {
                RaiseSurfaceProperties();
                _exportCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private void Search()
    {
        var keyword = SearchKeyword.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            StatusHeadline = "请输入序列号或批次号";
            StatusDetail = "支持按 serial number 或 lot number 快速定位回溯对象。";
            return;
        }

        var unit = _units.FirstOrDefault(item =>
            item.SerialNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
            item.LotNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        if (unit is null)
        {
            StatusHeadline = "未找到匹配对象";
            StatusDetail = $"没有匹配 {keyword} 的样本，建议切换到示例对象查看演示流程。";
            AddLog($"Search miss: {keyword}");
            return;
        }

        SelectUnit(unit);
        StatusHeadline = "定位成功";
        StatusDetail = $"{unit.SerialNumber} 已载入分析台，可查看站点履历、影像与 AI 结论。";
        AddLog($"Search hit: {unit.SerialNumber}");
    }

    private void LoadSample(string? sampleKey)
    {
        if (string.IsNullOrWhiteSpace(sampleKey))
        {
            return;
        }

        SearchKeyword = sampleKey;
        Search();
    }

    private void ExportSummary()
    {
        if (SelectedUnit is null)
        {
            return;
        }

        AddLog($"Export preview: {SelectedUnit.SerialNumber} / {SelectedUnit.LotNumber} / {AiDecision}");
        StatusHeadline = "已生成导出预览";
        StatusDetail = "此处演示 MVVM 命令链路，实际项目中可扩展为 Excel / CSV 或工程报告导出。";
    }

    private void SelectUnit(TraceabilityUnit unit)
    {
        SelectedUnit = unit;
        ReplaceItems(Route, unit.Route);
        ReplaceItems(ReviewImages, unit.ReviewImages);
        AddLog($"Loaded unit: {unit.SerialNumber} ({unit.CurrentStation})");
    }

    private static void ReplaceItems<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private void RaiseSurfaceProperties()
    {
        RaisePropertyChanged(nameof(SelectedSerialNumber));
        RaisePropertyChanged(nameof(SelectedLotNumber));
        RaisePropertyChanged(nameof(SelectedPackageName));
        RaisePropertyChanged(nameof(SelectedCurrentStation));
        RaisePropertyChanged(nameof(SelectedGrade));
        RaisePropertyChanged(nameof(SelectedCustomer));
        RaisePropertyChanged(nameof(AiModelVersion));
        RaisePropertyChanged(nameof(AiDecision));
        RaisePropertyChanged(nameof(AiRecommendation));
        RaisePropertyChanged(nameof(EngineerSummary));
        RaisePropertyChanged(nameof(RiskLevel));
    }

    private void AddLog(string message)
    {
        ActivityLogs.Insert(0, $"{DateTime.Now:HH:mm:ss}  {message}");
        while (ActivityLogs.Count > 8)
        {
            ActivityLogs.RemoveAt(ActivityLogs.Count - 1);
        }
    }
}
