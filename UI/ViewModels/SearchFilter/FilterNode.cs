
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
                true => FieldName switch
                {
                    "Process" or "process" or "proc" =>
                        (targetLive.ProcessName == FieldValue && _op == "="),
                    "System" or "system" or "sys" or "Sys" =>
                        (targetLive.SystemName == FieldValue && _op == "="),
                    "Cpu" or "CPU" or "cpu" =>
                        EvaluateNumerical(targetLive.CpuUsage),
                    "Disk" or "disk" =>
                        EvaluateNumerical(targetLive.DiskUsage),
                    "Memory" or "memory" or "mem" or "Mem" =>
                        EvaluateNumerical(targetLive.MemoryUsage),
                    "Network" or "network" or "net" or "Net" =>
                        EvaluateNumerical(targetLive.NetworkUsage),
                    _ => false
                },

                false => FieldName switch
                {
                    "Process" or "process" or "proc" =>
                        (targetLive.ProcessName == FieldValue && _op == "="),
                    "System" or "system" or "sys" or "Sys" =>
                        (targetLive.SystemName == FieldValue && _op == "="),
                    "CpuAvg" or "CPUAvg" or "cpuAvg" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.CpuUsageAvg),
                    "DiskAvg" or "diskAvg" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.DiskUsageAvg),
                    "MemoryAvg" or "memoryAvg" or "memAvg" or "MemAvg" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.MemoryUsageAvg),
                    "NetworkAvg" or "networkAvg" or "netAvg" or "NetAvg" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.NetworkUsageAvg),
                    "CpuPeak" or "CPUPeak" or "cpuPeak" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.CpuUsagePeak),
                    "DiskPeak" or "diskPeak" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.DiskUsagePeak),
                    "MemoryPeak" or "memoryPeak" or "memPeak" or "MemPeak" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.MemoryUsagePeak),
                    "NetworkPeak" or "networkPeak" or "netPeak" or "NetPeak" =>
                        targetHistorical != null && EvaluateNumerical(targetHistorical.NetworkUsagePeak),
                    _ => false
                }
            };
        }

     

    }
}
