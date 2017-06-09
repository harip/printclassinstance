using System;
using System.Linq;
//Don't remove this
using System.Reflection;
using PrintClassInstanceLib.Model;

namespace PrintClassInstanceLib.Extensions
{
    internal static class ObjectPropertiesExtensions
    {
        public static PrintInfo GetObjectProperties(this object classInstance)
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
    }
}
