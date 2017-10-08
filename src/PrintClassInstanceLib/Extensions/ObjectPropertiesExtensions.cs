using System;
using System.Linq;
using NLog;
using PrintClassInstanceLib.Model;

namespace PrintClassInstanceLib.Extensions
{
    public static class ObjectPropertiesExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static PrintInfo GetObjectProperties(this object classInstance)
        {
            try
            {
                var type = classInstance.GetType();

                var printInfo = new PrintInfo();
                ParseClass.GetFieldOrPropertyKeyValue(classInstance, printInfo);

                //Get all base types
                var baseType = ParseClass.GetBaseType(type);
                while (baseType != null && baseType.ToString() != "System.Object")
                {
                    var baseTypePrintInfo = new PrintInfo();
                    ParseClass.GetBaseTypeInfo(baseType, classInstance, baseTypePrintInfo);
                    if (baseTypePrintInfo.Values.Any())
                    {
                        printInfo.Values.AddRange(baseTypePrintInfo.Values);
                    }
                    baseType = ParseClass.GetBaseType(baseType);
                }

                return printInfo;
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error,ex, ex.Message);
                return new PrintInfo
                {
                    Value = ex.Message,
                    Name = "Error"
                };
            }
        }
    }
}
