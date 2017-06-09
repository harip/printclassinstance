using System;
using System.Collections;
using System.Collections.Generic;
//Don't remove this
using System.Reflection;
using PrintClassInstanceLib.Model;

namespace PrintClassInstanceLib.Extensions
{
    internal static class ObjectTypeExtensions
    {
        public static ObjType GetObjTypeCategory(this object obj)
        {
            if (obj == null)
            {
                return ObjType.BaseType;
            }
            var type = obj.GetType();

            var isValueType = ModifiedIsValueType(type);
            var isGenericType = ModifiedIsGenericType(type);
            var isAssignableFrom = typeof(IEnumerable).IsAssignableFrom(type);
            
            if (isGenericType)
            {
                if (isAssignableFrom &&
                    !type.FullName.Equals("System.String", StringComparison.OrdinalIgnoreCase))
                    return ObjType.IsEnumerable;

                var baseType = type.GetGenericTypeDefinition();
                if ((baseType == typeof(KeyValuePair<,>)) &&
                    !type.FullName.Equals("System.String", StringComparison.OrdinalIgnoreCase))
                    return ObjType.IsKeyValPair;
            }

            if (isValueType && type == typeof(DateTime))
            {
                return ObjType.DateTime;
            }

            if (ModifiedIsArrayType(type))
            {
                return ObjType.IsArray;
            } 

            return ObjType.BaseType;
        }

        public static bool IsEnumerable(this object obj)
        {
            var objType = obj.GetObjTypeCategory();
            return objType == ObjType.IsEnumerable;
        }

        public static bool IsArray(this object obj)
        {
            var objType = obj.GetObjTypeCategory();
            return objType == ObjType.IsArray;
        }

        public static Tuple<bool, object> HasMembersOrGetValue(this object data)
        {
            var type = data?.GetType();
            if (type == null)
                return Tuple.Create<bool, object>(true, null);

            var isValueType = ModifiedIsValueType(type);
            var isPrimitive = ModifiedIsPrimitiveType(type);
            var isGeneric = ModifiedIsGenericType(type);
            var isEnum = ModifiedIsEnum(type);

            if ((type.ToString() == "System.String") || isPrimitive)
            {
                return Tuple.Create<bool, object>(true, data);
            }

            if (isValueType && type == typeof(Guid))
            {
                return Tuple.Create<bool, object>(true, $"Guid.Parse(\"{data}\")");
            }

            if (isValueType && isGeneric)
            {
                return Tuple.Create<bool, object>(false, null);
            }

            if (isValueType && isEnum)
            {
                return Tuple.Create<bool, object>(true, $"{type}.{data}");
            }

            if (isValueType && type == typeof(DateTime))
            {
                return Tuple.Create<bool, object>(true, $"DateTime.Parse(\"{data}\")");
            }

            if (isValueType)
            {
                return Tuple.Create<bool, object>(true, data);
            }

            return Tuple.Create<bool, object>(false, null);
        }

        public static bool ModifiedIsArrayType(this Type type)
        {
#if (NET45 || NET451 || NET452 || NET46 || NET461 || NET462)
            return type.IsArray;
#else
            return type.GetTypeInfo().IsArray;
#endif
        }

        public static bool ModifiedIsValueType(this Type type)
        {
#if (NET45 || NET451 || NET452 || NET46 || NET461 || NET462)
            return type.IsValueType;
#else
            return type.GetTypeInfo().IsValueType;
#endif
        }

        public static bool ModifiedIsPrimitiveType(this Type type)
        {
#if (NET45 || NET451 || NET452 || NET46 || NET461 || NET462)
            return type.IsPrimitive;
#else
            return type.GetTypeInfo().IsPrimitive;
#endif
        }

        public static bool ModifiedIsGenericType(this Type type)
        {
#if (NET45 || NET451 || NET452 || NET46 || NET461 || NET462)
            return type.IsGenericType;
#else
            return type.GetTypeInfo().IsGenericType;
#endif
        }

        public static bool ModifiedIsEnum(this Type type)
        {
#if (NET45 || NET451 || NET452 || NET46 || NET461 || NET462)
            return type.IsEnum;
#else
            return type.GetTypeInfo().IsEnum;
#endif
        }
    }
}
