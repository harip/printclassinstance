using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
            try
            {
                GetReflectionInfo(baseType, data, printInfo, bindingFlags);
            }
            catch (Exception ex)
            {
                //Need to add logging
            }
        }

        public static void GetFieldOrPropertyKeyValue(object data,
            PrintInfo printInfo,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |  BindingFlags.Static)
        {
            var type = data?.GetType();
            if (type == null)
                return;

            try
            {
                GetReflectionInfo(type, data, printInfo, bindingFlags);
            }
            catch (Exception ex)
            {
                //Need to add logging
            }
        }

        private static void GetReflectionInfo(Type type,
            object data, 
            PrintInfo printInfo, 
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        {
            var members = type.GetMembers(bindingFlags)
                .Where(s => s.MemberType == MemberTypes.Property || s.MemberType == MemberTypes.Field)
                .ToList();
            string SetVal<T>(T t, object fParent, object newVal)
            {
                try
                {
                    switch (t)
                    {
                        case PropertyInfo p:
                            p.SetValue(fParent, newVal, null);
                            break;
                        case FieldInfo f:
                            f.SetValue(fParent, newVal);
                            break;
                    }
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    return $"Set value failed - {ex.Message}";
                }
            }

            foreach (var memberInfo in members)
            {
                if (IsCompilerGeneratedItem(memberInfo.GetCustomAttributes().ToList()))
                {
                    continue;
                }

                object val = null;
                Type mType = null;
                var mTypeNamespace = string.Empty;
                
                switch (memberInfo.MemberType)
                {
                    case MemberTypes.Property:
                        var propertyInfo = type.GetProperty(memberInfo.Name, bindingFlags);
                        val = propertyInfo.GetValue(data, null);
                        mType = propertyInfo.PropertyType;
                        mTypeNamespace = propertyInfo.PropertyType.Namespace;
                        break;
                    case MemberTypes.Field:
                        var fieldInfo = type.GetField(memberInfo.Name, bindingFlags);
                        val = fieldInfo.GetValue(data);
                        mType = fieldInfo.FieldType;
                        mTypeNamespace = fieldInfo.FieldType.Namespace;
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
                    Type = mType!=null ? mType.ToString():string.Empty,
                    Namespace = mTypeNamespace,
                    Id = stackedObj.Id,
                    ParentId = stackedObj.ParentId,
                    RawMemberValue = val,
                    RawMemberType = mType,
                    SetVal= SetVal,
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
                    if (queue.Count > 1000 || pList.Count> 1000)
                    {
                        pList=new Queue<PrintInfo>();
                        pInfo.Value = "Error: Member too big to evaluate or has infinite reference";
                        pList.Enqueue(pInfo);
                        break;
                    }
                    var popObject = queue.Dequeue();
                    reflectMember = popObject.Obj.HasMembersOrGetValue();
                    if (reflectMember.Item1)
                    {
                        continue;
                    }

                    if (popObject.Obj.IsEnumerable() || popObject.Obj.IsArray())
                    {
                        EnumerableObjectHandler(popObject, queue, pList);
                        var mem = pList.SingleOrDefault(s => s.Id == popObject.Id);
                        if (mem != null)
                        {
                            mem.IsEnum = true;
                        }
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
                        parent?.Values.Add(info);
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
                .Where(s => s.MemberType == MemberTypes.Property || s.MemberType == MemberTypes.Field )
                .ToList();
            foreach (var nestedMember in nestedMembers)
            {
                string objType;
                string nameSpace;
                object objVal;

                if (nestedMember.MemberType == MemberTypes.Field)
                {
                    var fld = currentObject.GetType().GetField(nestedMember.Name);
                    objVal = fld.GetValue(currentObject);
                    objType = fld.FieldType.ToString();
                    nameSpace = fld.FieldType.Namespace;
                }
                else
                {
                    var prop = currentObject.GetType().GetProperty(nestedMember.Name);
                    objVal = prop.GetValue(currentObject, null);
                    objType = prop.PropertyType.ToString();
                    nameSpace = prop.PropertyType.Namespace;
                }

                var stackedObject = new StackedObject
                {
                    Id = Guid.NewGuid(),
                    ParentId = popObject.Id,
                    Obj = objVal
                };
                queue.Enqueue(stackedObject);
                
                var cinfo = new PrintInfo
                {
                    Name = nestedMember.Name,
                    Type = objType,
                    Namespace = nameSpace,
                    Id = stackedObject.Id,
                    ParentId = stackedObject.ParentId
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
            return customAttributes.Any(s => s is CompilerGeneratedAttribute);
        }

        public static Type GetBaseType(Type type)
        {
            return type.GetTypeInfo().BaseType;
        }
    }
}