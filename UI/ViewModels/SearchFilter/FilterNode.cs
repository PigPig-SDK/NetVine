
namespace UI.ViewModels.SearchFilter
{
    public class FilterNode(string fieldName, string fieldValue, string op) : IFilterNode
    {
        public string FieldName { get; } = fieldName;
        public string FieldValue { get; } = fieldValue;
        private readonly string _op = op;

        private bool EvaluateNumerical(float targetValue)
        {
            if (!float.TryParse(FieldValue, out float v)) return false;
            return _op switch
            {
                ">=" => targetValue >= v,
                ">" => targetValue > v,
                "<" => targetValue < v,
                "<=" => targetValue <= v,
                "=" => targetValue == v,
                _ => false
            };
        }


        bool IFilterNode.Evaluate(TableRow target)
        {
            var targetLive = target.LiveData;
            var targetHistorical = target.HistoricalData;

            if (targetLive == null) return false;


            return LiveViewModel.IsLive switch
            {
                true => FieldName.ToLower() switch
                {
                    "process" or "proc" =>
                        (targetLive.ProcessName == FieldValue && _op == "="),
                    "system" or "sys" =>
                        (targetLive.SystemName == FieldValue && _op == "="),
                    "cpu" =>
                        EvaluateNumerical(targetLive.CpuUsage),
                    "disk" =>
                        EvaluateNumerical(targetLive.DiskUsage),
                    "memory" or "mem" =>
                        EvaluateNumerical(targetLive.MemoryUsage),
                    "network" or "net" =>
                        EvaluateNumerical(targetLive.NetworkUsage),
                    _ => false
                },

                false => FieldName.ToLower() switch
                {
                    "process" or "proc" =>
                        (targetLive.ProcessName == FieldValue && _op == "="),
                    "system" or "sys" =>
                        (targetLive.SystemName == FieldValue && _op == "="),
                    "cpuavg" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.CpuUsageAvg),
                    "diskavg" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.DiskUsageAvg),
                    "memoryavg" or "memavg" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.MemoryUsageAvg),
                    "networkavg" or "netavg" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.NetworkUsageAvg),
                    "cpupeak" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.CpuUsagePeak),
                    "diskpeak" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.DiskUsagePeak),
                    "memorypeak" or "mempeak" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.MemoryUsagePeak),
                    "networkpeak" or "netpeak" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.NetworkUsagePeak),
                    _ => false
                }
            };
        }

     

    }
}
