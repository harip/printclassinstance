using System;
using System.Collections.Generic;
using System.Linq;
using PrintClassInstanceLib.Extensions;
using PrintClassInstanceLib.Model;

namespace PrintClassInstanceLib.Format
{
    public static class VariableFormat
    {
        public static Dictionary<string,object> CreateOutputAsDictionary(PrintInfo printInfo, Type type)
        {
            var classDataList = new Dictionary<string, object>();
            foreach (var printInfoValue in printInfo.Values)
            {
                GetOutputData(printInfoValue, classDataList, printInfoValue.Name);
            }
            return classDataList;
        }

        public static List<string> CreateOutputAsVariableFormat(PrintInfo printInfo, Type type,OutputMode outputMode=OutputMode.Default)
        {
            var classDataList = new List<string>();
            var parentName = string.IsNullOrEmpty(type.Namespace)
                ? type.ToString()
                : type.ToString().Replace($"{type.Namespace}.", string.Empty);

            if (outputMode != OutputMode.Raw)
            {
                classDataList.Add($"{parentName}");
                classDataList.Add("{");
            }

            foreach (var printInfoValue in printInfo.Values)
            {
                GetOutputData(printInfoValue, classDataList, string.Empty, outputMode);
            }

            if (outputMode != OutputMode.Raw)
            {
                classDataList.Add("};");
            }

            var cleanData = classDataList.Select(s =>
            {
                s = s.ReplaceText();
                return s;
            }).ToList();
            return cleanData;
        }

        public static void GetOutputData(PrintInfo printInfo, Dictionary<string, object> outputData, string tab)
        {
            if (printInfo.Values == null || !printInfo.Values.Any())
            {
                object val;
                switch (printInfo.Type)
                {
                    case var c when c == "System.String" || c == "String":
                        val = $"{printInfo.Value}";
                        break;
                    default:
                        val = printInfo.Value ?? "null";
                        break;
                }
                outputData.Add(tab, val);
                return;
            }
            foreach (var printInfoValue in printInfo.Values)
            {
                var key1 = tab;
                if (printInfo.IsEnum)
                {
                    key1 = $"{tab}_{printInfo.Values.IndexOf(printInfoValue)}";
                }
                if (!string.IsNullOrEmpty(printInfoValue.Name))
                {
                    key1 = $"{key1}_{printInfoValue.Name}";
                }
                
                GetOutputData(printInfoValue, outputData, key1);
            }
        }
        
        public static void GetOutputData(PrintInfo printInfo, List<string> outputData, string tab, OutputMode outputMode = OutputMode.Default)
        {
            var currentTab = string.Empty;
            if (outputMode != OutputMode.Raw)
            {
                currentTab = tab + "\t";
            }

            if (printInfo.Values == null || !printInfo.Values.Any())
            {
                object val;
                switch (printInfo.Type)
                {
                    case var c when c == "System.String" || c == "String":
                        val = $"\"{printInfo.Value}\"";
                        break;
                    default:
                        val = printInfo.Value ?? "null";
                        break;
                }

                if (outputMode == OutputMode.Verbose)
                {
                    outputData.Add($"{currentTab}// {GetVerboseString(printInfo)}");
                }
                
                outputData.Add(string.IsNullOrEmpty(printInfo.Name)
                    ? $"{currentTab}{val},"
                    : $"{currentTab}{printInfo.Name} = {val},");

                if (outputMode == OutputMode.Verbose)
                {
                    outputData.Add(string.Empty);
                }
                return;
            }

            if (!string.IsNullOrEmpty(printInfo.Type) && !string.IsNullOrEmpty(printInfo.Namespace))
                printInfo.Type = printInfo.Type.Replace($"{printInfo.Namespace}.", string.Empty);

            if (!string.IsNullOrEmpty(printInfo.Type))
            {
                outputData.Add(string.IsNullOrEmpty(printInfo.Name)
                    ? $"{currentTab}new {printInfo.Type}"
                    : $"{currentTab}{printInfo.Name} = new {printInfo.Type}");
            }

            if (outputMode != OutputMode.Raw)
            {
                outputData.Add($"{currentTab}" + "{");
            }

            foreach (var printInfoValue in printInfo.Values)
            {
                GetOutputData(printInfoValue, outputData, currentTab,outputMode);
            }

            if (outputMode != OutputMode.Raw)
            {
                outputData.Add($"{currentTab}" + "},");
            }
        }

        private static string GetVerboseString(PrintInfo printInfo)
        {
            var verboseString = $"{printInfo.Type}";
            return verboseString;
        }
    }
}