using System;
using System.Collections.Generic;
using System.Linq;
using PrintClassInstanceLib.Extensions;
using PrintClassInstanceLib.Model;

namespace PrintClassInstanceLib.Format
{
    public static class VariableFormat
    {
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
                    case "System.String":
                    case "String":
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
            if (printInfo.IsPrivate)
            {
                verboseString = string.Concat(verboseString, ", IsPrivate");
            }
            if (printInfo.IsStatic)
            {
                verboseString = string.Concat(verboseString, ", IsStatic");
            }
            return verboseString;
        }
    }
}