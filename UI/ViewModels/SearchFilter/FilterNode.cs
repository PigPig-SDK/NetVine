using Core;

namespace UI.ViewModels.SearchFilter
{
    public class FilterNode : IFilterNode
    {
        public string FieldName { get; }
        public string FieldValue { get; }
        private string _op;
        private IProgramData? _targetLive;
        private IProgramDataHistorical? _targetHistorical;

        public FilterNode(string fieldName, string fieldValue, string op, TableRow target)
        {
            _targetHistorical = target.HistoricalData;
            _targetLive = target.LiveData;
            _op = op;
            FieldValue = fieldValue;
            FieldName = fieldName;
        }

        public FilterNode(string fieldName, string fieldValue, string op
            , IProgramData targetLive
            , IProgramDataHistorical targetHistorical)
        {
            _targetHistorical = targetHistorical;
            _targetLive = targetLive;
            _op = op;
            FieldValue = fieldValue;
            FieldName = fieldName;
        }

        bool EvaluateField(float expected, float actual)
        {
            return _op switch
            {
                ">=" => expected >= actual,
                ">" => actual < expected,
                "<" => expected < actual,
                "<=" => expected <= actual,
                "=" => actual == expected,
                _ => false
            };
        }


        bool IFilterNode.Evaluate()
        {
            if (_targetLive == null) return false;
            switch (FieldName)
            {
                case "Process": case "process": case "proc":
                    if (_targetLive.ProcessName == FieldValue && _op == "=")
                        return true;
                    else return false;

                case "System": case "system": case "sys": case "Sys":
                    if (_targetLive.SystemName == FieldValue && _op == "=")
                        return true;
                    else return false;
            }

            if (float.TryParse(FieldValue, out float v)) 
            {
                switch(FieldName)
                {
                    case "Cpu": case "CPU": case "cpu":
                        return EvaluateField(_targetLive.CpuUsage, v);
                    case "Disk": case "disk":
                        return EvaluateField(_targetLive.DiskUsage, v);
                    case "Memory": case "memory": case "mem": case "Mem":
                        return EvaluateField(_targetLive.MemoryUsage, v);
                    case "Network": case "network": case "net": case "Net":
                        return EvaluateField(_targetLive.NetworkUsage, v);
                }
            }

            return false;
        }
    }
}
