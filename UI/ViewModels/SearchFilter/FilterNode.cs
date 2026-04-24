using Core;
using Infrastructure;
using System;
using System.Linq.Expressions;

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
            
            //switch (FieldName)
            //{
            //    case "Process": case "process": case "proc":
            //        if (targetLive.ProcessName == FieldValue && _op == "=")
            //            return true;
            //        else return false;
            //    case "System": case "system": case "sys": case "Sys":
            //        if (targetLive.SystemName == FieldValue && _op == "=")
            //            return true;
            //        else return false;
            //    case "Cpu": case "CPU": case "cpu":
            //        return EvaluateNumerical(targetLive.CpuUsage);
            //    case "Disk": case "disk":
            //        return EvaluateNumerical(targetLive.DiskUsage);
            //    case "Memory": case "memory": case "mem": case "Mem":
            //        return EvaluateNumerical(targetLive.MemoryUsage);
            //    case "Network": case "network": case "net": case "Net":
            //        return EvaluateNumerical(targetLive.NetworkUsage);
            //}

            return FieldName switch
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
            };
        }

        //helper for switch statement in ToExpression, to avoid repeating the same code for each numerical field
        private Expression ToNumericalExpression(ParameterExpression param, string propertyName)
        {
            if (param == null) return Expression.Constant(false);
            if (propertyName == null) return Expression.Constant(false);

            if (!float.TryParse(FieldValue, out float v))
                return Expression.Constant(false);

            var prop = Expression.Property(param, propertyName);

            return _op switch
            {
                ">=" => Expression.GreaterThanOrEqual(prop, Expression.Constant(v)),
                ">" => Expression.GreaterThan(prop, Expression.Constant(v)),
                "<" => Expression.LessThan(prop, Expression.Constant(v)),
                "<=" => Expression.LessThanOrEqual(prop, Expression.Constant(v)),
                "=" => Expression.Equal(prop, Expression.Constant(v)),
                _ => Expression.Constant(false)
            };
        }


        //for historical data, only equality is supported for Process and System, and numerical comparisons for the rest
        Expression<Func<ProgramDataHistorical, bool>> IFilterNode.ToExpression(ParameterExpression param)
        {
            var procName = Expression.Property(param, nameof(ProgramDataHistorical.ProcessName));
            Expression? body = null;
            switch (FieldName)
            {
                case "Process":case "process":case "proc":
                    if (_op == "=")
                        body = Expression.Equal(
                        Expression.Property(param, nameof(ProgramDataHistorical.ProcessName)),
                        Expression.Constant(FieldValue));
                    break;
                case "System": case "system": case "sys": case "Sys":
                    if (_op == "=")
                        body = Expression.Equal(
                        Expression.Property(param, nameof(ProgramDataHistorical.SystemName)),
                        Expression.Constant(FieldValue)
                    );
                    break;
                case "CpuAvg":
                    body = ToNumericalExpression(param, nameof(ProgramDataHistorical.CpuUsageAvg));
                    break;
                case "DiskAvg":
                    body = ToNumericalExpression(param, nameof(ProgramDataHistorical.DiskUsageAvg));
                    break;
                case "MemoryAvg": case "MemAvg": case "memAvg":
                    body = ToNumericalExpression(param, nameof(ProgramDataHistorical.MemoryUsageAvg));
                    break;
                case "NetworkAvg": case "NetAvg": case "netAvg":
                    body = ToNumericalExpression(param, nameof(ProgramDataHistorical.NetworkUsageAvg));
                    break;
                case "CpuPeak":
                    body = ToNumericalExpression(param, nameof(ProgramDataHistorical.CpuUsagePeak));
                    break;
                case "DiskPeak":
                    body = ToNumericalExpression(param, nameof(ProgramDataHistorical.DiskUsagePeak));
                    break;
                case "MemoryPeak": case "MemPeak": case "memPeak":
                    body = ToNumericalExpression(param, nameof(ProgramDataHistorical.MemoryUsagePeak));
                    break;
                case "NetworkPeak": case "NetPeak": case "netPeak":
                    body = ToNumericalExpression(param, nameof(ProgramDataHistorical.NetworkUsagePeak));
                    break;
                case "NetworkTotal": case "NetTotal": case "netTotal":
                    body = ToNumericalExpression(param, nameof(ProgramDataHistorical.NetworkUsageTotal));
                    break;
            }

            if (body == null)
                return x => false;

            return Expression.Lambda<Func<ProgramDataHistorical, bool>>(body, param);

        }

    }
}
