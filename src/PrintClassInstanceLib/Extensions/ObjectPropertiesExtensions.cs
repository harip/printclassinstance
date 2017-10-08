using System;
using System.Linq;
using PrintClassInstanceLib.Model;

namespace PrintClassInstanceLib.Extensions
{
    public static class ObjectPropertiesExtensions
    {
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
                return new PrintInfo
                {
                    Value = ex.Message,
                    Name = "Error"
                };
            }
        }
    }
}
