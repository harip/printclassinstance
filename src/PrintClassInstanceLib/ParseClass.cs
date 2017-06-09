using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PrintClassInstanceLib.Extensions;
using PrintClassInstanceLib.Model;

namespace PrintClassInstanceLib
{
    public static class ParseClass
    {
        public static void GetBaseTypeInfo(Type baseType,
            object data, 
            PrintInfo printInfo, 
            BindingFlags bindingFlags = 
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            GetReflectionInfo(baseType, data, printInfo, bindingFlags);
        }

        public static void GetFieldOrPropertyKeyValue(object data,
            PrintInfo printInfo,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |  BindingFlags.Static)
        {
            var type = data?.GetType();
            if (type == null)
                return;
            GetReflectionInfo(type, data, printInfo, bindingFlags);
        }

        private static void GetReflectionInfo(Type type,
            object data, 
            PrintInfo printInfo, 
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            var isPrivate = false;
            var isStatic = false;
            var members = type.GetMembers(bindingFlags)
                .Where(s => s.MemberType == MemberTypes.Property || s.MemberType == MemberTypes.Field)
                .ToList();

            foreach (var memberInfo in members)
            {
                object val = null;
                Func<MemberInfo, object, object,string> setVal = null;
                Type mType = null;
                var mTypeNamespace = string.Empty;
                
                if (IsCompilerGeneratedItem(memberInfo.GetCustomAttributes().ToList()))
                {
                    continue;
                }

                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Property:
                        var propertyInfo = type.GetProperty(memberInfo.Name, bindingFlags);
                        val = propertyInfo.GetValue(data, null);
                        mType = propertyInfo.PropertyType;
                        mTypeNamespace = propertyInfo.PropertyType.Namespace;
                        setVal = (f,fParent,newVal) =>
                        {
                            try
                            {
                                var propInfo = (PropertyInfo)f;
                                propInfo.SetValue(fParent, newVal,null);
                                return string.Empty;
                            }
                            catch (Exception ex)
                            {
                                return $"Set value failed - {ex.Message}";
                            }
                        };
                        var getGetMethod = propertyInfo.GetGetMethod(true);
                        if (getGetMethod != null)
                        {
                            isPrivate = getGetMethod.IsPrivate;
                            isStatic = getGetMethod.IsStatic;
                        }
                        break;
                    case MemberTypes.Field:
                        var fieldInfo = type.GetField(memberInfo.Name, bindingFlags);
                        val = fieldInfo.GetValue(data);
                        mType = fieldInfo.FieldType;
                        mTypeNamespace = fieldInfo.FieldType.Namespace;
                        setVal = (f, fParent, newVal) =>
                        {
                            try
                            {
                                var fInfo = (FieldInfo)f;
                                fInfo.SetValue(fParent, newVal);
                                return string.Empty;
                            }
                            catch (Exception ex)
                            {
                                return $"Set value failed - {ex.Message}";
                            }
                        };
                        isPrivate = fieldInfo.IsPrivate;
                        isStatic = fieldInfo.IsStatic;
                        break;
                }

                var stackedObj = new StackedObject
                {
                    Id = Guid.NewGuid(),
                    ParentId = Guid.NewGuid(),
                    Obj = val
                };
                var queue = new Queue<StackedObject>();
                queue.Enqueue(stackedObj);

                var pInfo = new PrintInfo
                {
                    Name = memberInfo.Name,
                    Type = mType.ToString(),
                    Namespace = mTypeNamespace,
                    Id = stackedObj.Id,
                    ParentId = stackedObj.ParentId,
                    IsPrivate=isPrivate,
                    IsStatic=isStatic,
                    RawMemberValue = val,
                    RawMemberType = mType,
                    SetVal=setVal,
                    Member=memberInfo
                };
                var pList = new Queue<PrintInfo>();
                pList.Enqueue(pInfo);

                var reflectMember = val.HasMembersOrGetValue();
                if (reflectMember.Item1)
                {
                    pInfo.Value = reflectMember.Item2;
                    queue.Clear();
                }

                while (queue.Count > 0)
                {
                    var popObject = queue.Dequeue();
                    reflectMember = popObject.Obj.HasMembersOrGetValue();
                    if (reflectMember.Item1)
                    {
                        continue;
                    }

                    if (popObject.Obj.IsEnumerable() || popObject.Obj.IsArray())
                    {
                        EnumerableObjectHandler(popObject, queue, pList);
                    }
                    else
                    {
                        ObjectHandler(popObject.Obj, popObject, queue, pList);
                    }
                }
                var relationList = GetMemberHierarchy(pList);
                printInfo.Values.AddRange(relationList);
            }
        }
        
        private static List<PrintInfo> GetMemberHierarchy(Queue<PrintInfo> pList)
        {
            var relationList = pList.ToList();
            var childsLeftBehind = true;
            while (childsLeftBehind)
            {
                var toRemove = new List<PrintInfo>();
                if (relationList.Count == 1)
                {
                    break;
                }
                //find child that has no childs
                foreach (var info in relationList)
                {
                    var isThisAParent = relationList.Count(s => s.ParentId == info.Id && s.Id != info.Id);
                    if (isThisAParent < 1)
                    {
                        //Get the info's parentId
                        var parent = relationList.SingleOrDefault(s => s.Id == info.ParentId);
                        parent.Values.Add(info);
                        //parent.Values.Add(info);
                        toRemove.Add(info);
                    }
                }
                foreach (var info in toRemove)
                {
                    relationList.Remove(info);
                }
                if (!toRemove.Any() || relationList.Count <= 1)
                {
                    childsLeftBehind = false;
                }
            }
            return relationList;
        }

        private static void ObjectHandler(object currentObject, StackedObject popObject, Queue<StackedObject> queue, Queue<PrintInfo> pList)
        {
            var nestedMembers = currentObject.GetType()
                .GetMembers()
                .Where(s => s.MemberType == MemberTypes.Property)
                .ToList();
            foreach (var nestedMember in nestedMembers)
            {
                var pInfo = currentObject.GetType().GetProperty(nestedMember.Name);

                var stackedObject = new StackedObject
                {
                    Id = Guid.NewGuid(),
                    ParentId = popObject.Id,
                    Obj = pInfo.GetValue(currentObject, null)
                };
                queue.Enqueue(stackedObject);
                
                var cinfo = new PrintInfo
                {
                    Name = nestedMember.Name,
                    Type = pInfo.PropertyType.ToString(),
                    Namespace = pInfo.PropertyType.Namespace,
                    Id = stackedObject.Id,
                    ParentId = stackedObject.ParentId,
                    IsPrivate = pInfo.GetGetMethod(true)!=null && pInfo.GetGetMethod(true).IsPrivate,
                    IsStatic = pInfo.GetGetMethod(true) != null && pInfo.GetGetMethod(true).IsStatic,
                };

                if (currentObject.GetType().GetObjTypeCategory() == ObjType.IsKeyValPair)
                {
                    cinfo.Name = string.Empty;
                }

                var reflectMember = stackedObject.Obj.HasMembersOrGetValue();
                if (reflectMember.Item1)
                {
                    cinfo.Value = reflectMember.Item2;
                }

                pList.Enqueue(cinfo);
            }
        }

        private static void EnumerableObjectHandler(StackedObject popObject, Queue<StackedObject> queue, Queue<PrintInfo> pList)
        {
            var iterator = ((IEnumerable)popObject.Obj).GetEnumerator();
            while (iterator.MoveNext())
            {
                var currentVal = iterator.Current;
                var currentValType = currentVal.GetType();
                var childPInfo = new PrintInfo {Namespace = currentValType.Namespace};
                switch (currentVal.GetObjTypeCategory())
                {
                    case ObjType.IsKeyValPair:
                        childPInfo.Type = string.Empty;
                        break;
                    case ObjType.IsEnumerable:
                        childPInfo.Type = currentValType.ToString();
                        break;
                    default:
                        childPInfo.Type = currentValType.Name;
                        childPInfo.Value = currentVal;
                        break;
                }

                var stackedObject = new StackedObject
                {
                    Id = Guid.NewGuid(),
                    ParentId = popObject.Id,
                    Obj = currentVal
                };
                queue.Enqueue(stackedObject);

                childPInfo.ParentId = stackedObject.ParentId;
                childPInfo.Id = stackedObject.Id;
                pList.Enqueue(childPInfo);
            }
        }

        public static bool IsCompilerGeneratedItem(List<Attribute> customAttributes)
        {
            if (customAttributes == null || !customAttributes.Any())
                return false;

            foreach (var item in customAttributes)
            {
                var z = item.GetType() == typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute);
            }    

            return customAttributes.Any(s => s.GetType() == typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute));
        }

        public static Type GetBaseType(Type type)
        {
#if (NET45 || NET451 || NET452 || NET46 || NET461 || NET462)
            return type.BaseType;
#else
            return type.GetTypeInfo().BaseType;
#endif
        }
    }
}