using Core;

namespace UI.ViewModels.SearchFilter
{
    public class FilterNode : IFilterNode
    {
        public string FieldName { get; }
        public string FieldValue { get; }
        private string _op;

        public FilterNode(string fieldName, string fieldValue, string op)
        {
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


        bool IFilterNode.Evaluate(TableRow target)
        {
            var targetLive = target.LiveData;
            var targetHistorical = target.HistoricalData;

            if (targetLive == null) return false;
            
            switch (FieldName)
            {
                case "Process": case "process": case "proc":
                    if (targetLive.ProcessName == FieldValue && _op == "=")
                        return true;
                    else return false;

                case "System": case "system": case "sys": case "Sys":
                    if (targetLive.SystemName == FieldValue && _op == "=")
                        return true;
                    else return false;
            }

            if (float.TryParse(FieldValue, out float v)) 
            {
                switch(FieldName)
                {
                    case "Cpu": case "CPU": case "cpu":
                        return EvaluateField(targetLive.CpuUsage, v);
                    case "Disk": case "disk":
                        return EvaluateField(targetLive.DiskUsage, v);
                    case "Memory": case "memory": case "mem": case "Mem":
                        return EvaluateField(targetLive.MemoryUsage, v);
                    case "Network": case "network": case "net": case "Net":
                        return EvaluateField(targetLive.NetworkUsage, v);
                }
            }

            return false;
        }
    }
}
