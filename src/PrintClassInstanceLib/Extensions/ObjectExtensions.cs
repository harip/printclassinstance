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
                var type = classInstance.GetType();
                var printInfo = classInstance.GetObjectProperties();
                var cleanData = VariableFormat.CreateOutputAsVariableFormat(printInfo, type, outputMode);
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
            var propNames = new List<ObjectPropertyCompareInfo>();
            obj1Prop.Values.ForEach(s =>
            {
                propNames.Add(new ObjectPropertyCompareInfo
                {
                    PropertyName = s.Name,
                    PropertyType=s.Type
                });
            } );
            obj2Prop.Values.ForEach(s =>
            {
                propNames.Add(new ObjectPropertyCompareInfo
                {
                    PropertyName = s.Name,
                    PropertyType = s.Type
                });
            });

            //Get names 
            propNames = propNames.DistinctBy(s => new {Name = s.PropertyName, Type = s.PropertyType}).ToList();
            foreach (var propName in propNames)
            {
                var inObject1 =obj1Prop.Values.Any(s => s.Name == propName.PropertyName && s.Type == propName.PropertyType);
                var inObject2 = obj2Prop.Values.Any(s => s.Name == propName.PropertyName && s.Type == propName.PropertyType);
                
                //If prop name and type is not in both objects capture that
                if (!inObject1 || !inObject2)
                {
                    var parentName = inObject1 ? object1Name : object2Name;
                    compareResult.NoMatchList.Add( new ObjectPropertyCompareInfo
                    {
                        PropertyName = propName.PropertyName,
                        PropertyType = propName.PropertyType,
                        Description = $"Property only exists in {parentName}"
                    });
                    continue;
                }

                //The name type exist in both objects, check if their values are same
                var obj1OutputData = new List<string>();
                var obj1Val = obj1Prop.Values.First(s => s.Name == propName.PropertyName && s.Type == propName.PropertyType);
                VariableFormat.GetOutputData(obj1Val, obj1OutputData,string.Empty,OutputMode.Raw);

                var obj2OutputData = new List<string>();
                var obj2Val = obj2Prop.Values.First(s => s.Name == propName.PropertyName && s.Type == propName.PropertyType);
                VariableFormat.GetOutputData(obj2Val, obj2OutputData, string.Empty, OutputMode.Raw);

                //If the count is not same
                if (obj1OutputData.Count != obj2OutputData.Count)
                {
                    compareResult.NoMatchList.Add(new ObjectPropertyCompareInfo
                    {
                        PropertyName = propName.PropertyName,
                        PropertyType = propName.PropertyType,
                        Description = "Count does not match"
                    });
                }

                var intersectElements = obj1OutputData.Select(s=>s.ToLower()).Except(obj2OutputData.Select(s=>s.ToLower())).ToList();
                if (!intersectElements.Any())
                {
                    //All the values match
                    continue;
                }

                compareResult.NoMatchList.Add(new ObjectPropertyCompareInfo
                {
                    PropertyName = propName.PropertyName,
                    PropertyType = propName.PropertyType,
                    Description = "Property value does not match"
                });
            }

            return compareResult;
        }

        public static T DeepClone<T>(this object classInstance)
        {
            var type = classInstance.GetType();
            var newObject = Activator.CreateInstance(type);

            var obj1Prop = classInstance.GetObjectProperties();
            var memberNames = obj1Prop.Values.Select(s => s.Name).ToList();

            foreach (var memberName in memberNames)
            {
                var memberValue = obj1Prop.Values.SingleOrDefault(s => s.Name == memberName);
                if (memberValue == null)
                {
                    continue;
                }
                newObject.SetMemberValue(memberName, memberValue.RawMemberValue);
            }

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

        public static async Task<OperationMessage> SaveToS3(this object classInstance, S3UploadMessage uploadMessage)
        {
            uploadMessage.ByteArray = classInstance.GetByteArray();
            var task = S3Operations.UploadToS3(uploadMessage);
            return await task;
        }
    }
}