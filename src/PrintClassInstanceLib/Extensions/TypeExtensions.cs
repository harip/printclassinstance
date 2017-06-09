﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PrintClassInstanceLib.Model;
using System.Reflection.Emit;
using MoreLinq;

namespace PrintClassInstanceLib.Extensions
{
    public static class TypeExtensions
    {
        public static string GetNamespace(this Type type)
        {
            return type.Namespace;
        }

        public static List<string> GetBaseClassesNames(this Type type)
        {
            var baseClassNames = new List<string>();
            //Get all base types
            var baseType = ParseClass.GetBaseType(type);
            while (baseType != null && baseType.ToString() != "System.Object")
            {
                baseClassNames.Add(baseType.Name);
                baseType = ParseClass.GetBaseType(baseType);
            }
            return baseClassNames;
        }

        public static List<Type> GetBaseClassesTypes(this Type type)
        {
            var baseClassType = new List<Type>();
            //Get all base types
            var baseType = ParseClass.GetBaseType(type);
            while (baseType != null && baseType.ToString() != "System.Object")
            {
                baseClassType.Add(baseType);
                baseType = ParseClass.GetBaseType(baseType);
            }
            return baseClassType;
        }

        public static List<string> GetMemberNames(this Type type, BindingFlags bindingFlags)
        {
            var memberNames=new List<string>();
            type.GetMembers(bindingFlags)
                .Where(s => s.MemberType == MemberTypes.Property || s.MemberType == MemberTypes.Field)
                .ToList().ForEach(s =>
                {
                    if (!ParseClass.IsCompilerGeneratedItem(s.GetCustomAttributes().ToList()))
                    {
                        memberNames.Add(s.Name);
                    }
                });
            return memberNames;
        }

        public static List<string> GetAllMemberNames(this Type type)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                        BindingFlags.Static;
            var members = type.GetMemberNames(bindingFlags);

            var baseClassTypes = type.GetBaseClassesTypes();
            foreach (var baseClassType in baseClassTypes)
            {
                var baseMembers =
                    baseClassType.GetMemberNames(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                members.AddRange(baseMembers);
            }

            return members;
        }

        public static List<MethodInfo> GetAllMethods(this Type type,bool includeComplierGeneratedAttribute=false)
        {
            var methodInfo = new List<MethodInfo>();
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                        BindingFlags.Static;

            var types= type.GetBaseClassesTypes();
            types.Add(type);
            
            types.ForEach(t =>
            {
                var methods = t.GetMembers(bindingFlags)
                    .Where(s => s.MemberType == MemberTypes.Method)
                    .Where(s =>
                        s.CustomAttributes.Any(
                            a => a.AttributeType == typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)) ==
                        includeComplierGeneratedAttribute)
                    .Where(s => s.DeclaringType != typeof(object))
                    .Select(s => (MethodInfo) s)
                    .ToList()
                    .Where(s => s != null).ToList();

                methodInfo.AddRange(methods);
            });

            methodInfo = methodInfo.DistinctBy(m => new {
                m.DeclaringType.Namespace,m.DeclaringType.Name, Signature = m.ToString()
            }).ToList();

            return methodInfo.ToList();
        }

        public static List<MethodMeta> GetAllMethodsMetaData(this Type type)
        {
            var methods = GetAllMethods(type);
            return methods.Select(m => new MethodMeta
            {
                NameSpace = m.DeclaringType?.Namespace,
                DeclaringType = m.DeclaringType?.Name,
                Name=m.Name,
                Signature = m.ToString()
            }).ToList();
        }
    }
}
