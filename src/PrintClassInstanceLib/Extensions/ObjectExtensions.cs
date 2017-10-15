using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrintClassInstanceLib.Format;
using PrintClassInstanceLib.Model;
using MoreLinq;
using Newtonsoft.Json;
using NLog.LayoutRenderers.Wrappers;
using PrintClassInstanceLib.Messages;
using PrintClassInstanceLib.Upload;

namespace PrintClassInstanceLib.Extensions
{
    public static class ObjectExtensions
    {
        public static object MemberValue(this object classInstance,string name)
        {
            var member = classInstance.GetObjectProperties().Values.SingleOrDefault(s => s.Name == name);
            return member?.RawMemberValue;
        }

        public static string SetMemberValue(this object classInstance, string name,object newVal)
        {
            var member = classInstance.GetObjectProperties().Values.SingleOrDefault(s => s.Name == name);
            return member != null 
                ? member.SetVal(member.Member, classInstance, newVal) 
                : "Set value failed - Member not found";
        }

        public static IEnumerable<string> NullMembers(this object classInstance)
        {
            return classInstance.GetObjectProperties().Values.Where(s => s.RawMemberValue == null).Select(s => s.Name);
        }

        public static void SaveToFile(this object classInstance, string fileName="objectGraph.txt",OutputMode outputMode=OutputMode.Default)
        {
            try
            { 
                var printInfo = classInstance.GetObjectProperties();
                var cleanData = VariableFormat.CreateOutputAsVariableFormat(printInfo, classInstance.GetType(), outputMode);
                File.WriteAllLines(fileName, cleanData);
            }
            catch (Exception ex)
            {
                File.WriteAllText(fileName, ex.Message);
            }
        }

        //Todo: Figure out the names instead of accepting them as parameters
        public static ObjectCompareInfo CompareObjects(this object object1, object object2,string object1Name, string object2Name)
        {
            var compareResult = new ObjectCompareInfo(); 
            var obj1Prop = object1.GetObjectProperties();
            var obj2Prop = object2.GetObjectProperties();

            //Get common properties, check name and data type
            var propNames = GetPropNames(obj1Prop, obj2Prop);

            //Get names 
            propNames = propNames.DistinctBy(s => new {Name = s.PropertyName, Type = s.PropertyType}).ToList();
            Parallel.ForEach(propNames, propName =>
            {
                var inObject1 =obj1Prop.Values.Any(s => s.Name == propName.PropertyName && s.Type == propName.PropertyType);
                var inObject2 =obj2Prop.Values.Any(s => s.Name == propName.PropertyName && s.Type == propName.PropertyType);

                //If prop name and type is not in both objects capture that
                if (!inObject1 || !inObject2)
                {
                    var parentName = inObject1 ? object1Name : object2Name;
                    compareResult.NoMatchList.Add(new ObjectPropertyCompareInfo
                    {
                        PropertyName = propName.PropertyName,
                        PropertyType = propName.PropertyType,
                        Description = $"Property only exists in {parentName}"
                    });
                    return;
                }

                //The name type exist in both objects, check if their values are same
                var obj1OutputData = GetObjectOutputData(obj1Prop, propName);
                var obj2OutputData = GetObjectOutputData(obj2Prop, propName);
                if (obj1OutputData.Count != obj2OutputData.Count)
                {
                    compareResult.NoMatchList.Add(new ObjectPropertyCompareInfo
                    {
                        PropertyName = propName.PropertyName,
                        PropertyType = propName.PropertyType,
                        Description = "Count does not match"
                    });
                }

                var intersectElements = obj1OutputData.Select(s => s.ToLower()).Except(obj2OutputData.Select(s => s.ToLower())).ToList();
                if (!intersectElements.Any())
                {
                    //All the values match
                    return;
                }
                compareResult.NoMatchList.Add(new ObjectPropertyCompareInfo
                {
                    PropertyName = propName.PropertyName,
                    PropertyType = propName.PropertyType,
                    Description = "Property value does not match"
                });
            });
            
            return compareResult;

            List<string> GetObjectOutputData(PrintInfo objVal, ObjectPropertyCompareInfo compareInfo)
            {
                var returnData = new List<string>();
                var obj1Val = objVal.Values.First(s => s.Name == compareInfo.PropertyName && s.Type == compareInfo.PropertyType);
                VariableFormat.GetOutputData(obj1Val, returnData, string.Empty, OutputMode.Raw);
                return returnData;
            }
            List<ObjectPropertyCompareInfo> GetPropNames(params PrintInfo[] pInfo)
            {
                return pInfo.SelectMany(s => s.Values.Select(r => new ObjectPropertyCompareInfo
                {
                    PropertyName = r.Name,
                    PropertyType = r.Type
                })).ToList();
            }
        }

        public static T DeepClone<T>(this object classInstance)
        {
            var type = typeof(T);
            var newObject = Activator.CreateInstance(type);
            var obj1Prop = classInstance.GetObjectProperties();
            var memberNames = obj1Prop.Values.Select(s => s.Name).ToList();
            void Body(string memberName)
            {
                var memberValue = obj1Prop.Values.SingleOrDefault(s => s.Name == memberName);
                if (memberValue != null)
                {
                    newObject.SetMemberValue(memberName, memberValue.RawMemberValue);
                }
            }
            Parallel.ForEach(memberNames, Body);
            return (T)newObject;
        }
        
        public static object InvokeMethod(this object classInstance,string methodName,object[] methodParams)
        {
            var type = classInstance.GetType();
            var method = type.GetAllMethods().FirstOrDefault(s => s.Name == methodName);
            if (method == null)
            {
                return $"Method {methodName} not found";
            }
            try
            {
                return method.Invoke(classInstance, methodParams);
            }
            catch (Exception ex)
            {
                return $"Failed to invoke {methodName} - {ex.Message}";
            }
        }

        public static byte[] GetByteArray(this object classInstance)
        {
            var type = classInstance.GetType();
            var printInfo = classInstance.GetObjectProperties();
            var cleanData = VariableFormat.CreateOutputAsVariableFormat(printInfo, type);
            Encoding enc = new UTF8Encoding(true, true);
            var byteArray = cleanData.SelectMany(s =>
            {
                var al = new ArrayList();
                al.AddRange(enc.GetBytes(s));
                al.AddRange(enc.GetBytes(Environment.NewLine));
                return al.ToArray().OfType<byte>().ToArray();
            }).ToArray();

            return byteArray;
        }

        public static async ValueTask<OperationMessage> SaveToS3(this object classInstance, S3UploadMessage uploadMessage)
        {
            uploadMessage.ByteArray = classInstance.GetByteArray();
            var task = await S3Operations.UploadToS3(uploadMessage);
            return task;
        }

        public static Task<Dictionary<string, object>> Flatten(this object classInstance)
        { 
            var printInfo = classInstance.GetObjectProperties();
            var cleanData = VariableFormat.CreateOutputAsDictionary(printInfo, classInstance.GetType());
            return Task.FromResult(cleanData);
        }

        public static async ValueTask<string> FlattenedJson(this object classInstance)
        {
            var data = await Flatten(classInstance);
            var jsonData = JsonConvert.SerializeObject(data);
            return jsonData;
        }

        public static Task<Dictionary<string, object>> CombineAndFlatten(this object classInstance, params object[] classInstance1)
        {
            var objects = new List<object> { classInstance};
            objects.AddRange(classInstance1);
            var dict=new Dictionary<string,object>();
            classInstance1.Append(classInstance).ForEach(async o =>
            {
                var oDict = await o.Flatten();
                oDict.ForEach(c =>
                {
                    var key = $"obj{objects.IndexOf(o)}_{c.Key}";
                    if (!dict.ContainsKey(key))
                    {
                        dict.Add(key,c.Value);
                    }
                });
            });
            return Task.FromResult(dict);
        }

        public static async ValueTask<string> CombineAndFlattenedJson(this object classInstance, params object[] classInstance1)
        {
            var dict = await CombineAndFlatten(classInstance, classInstance1);
            return JsonConvert.SerializeObject(dict);
        }
    }
}