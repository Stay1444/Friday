using Friday.Common;
using LiveChartsCore;
using LiveChartsCore.Geo;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Serilog;

namespace Friday.Modules.Statistics;

public class StatisticsModuleBase : ModuleBase
{
    public Task OnLoad()
    {
        Log.Information("[Statistics] Module loaded");
        return Task.CompletedTask;
    }
}