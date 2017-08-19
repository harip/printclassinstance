using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.S3;
using PrintClassInstanceLib.Extensions;
using PrintClassInstanceLib.Messages;

namespace DemoCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Dump the object graph to a file
            var data = TestDataGenerator.GenerateTestData1();
            data.SaveToFile(@"C:\tmp\test.txt");

            //Save to S3
            var contentData = new Dictionary<string, string>
            {
                { "AccessKeyId","your access key"},
                {"SecretAccessKey","your secret access key"}
            };
            var uploadMessage = new S3UploadMessage
            {
                S3Client = new AmazonS3Client(contentData["AccessKeyId"], contentData["SecretAccessKey"],
                        Amazon.RegionEndpoint.USEast1),
                BucketName = "your bucket name",
                Key = "objectgraph.txt"
            };
            var result = data.SaveToS3(uploadMessage).Result;

            //Get member value
            var memberNames = data.MemberNames();
            foreach (var memberName in memberNames)
            {
                var memberVal = data.MemberValue(memberName);
                Console.WriteLine($"Value:{memberVal}-Type:{memberVal.GetType()}");
            }

            //Get null members
            var nullMembers = data.NullMembers();
            foreach (var nullMember in nullMembers)
            {
                Console.WriteLine($"{nullMember}");
            }

            var obj1 = TestDataGenerator.GenerateTestData1();
            var obj2 = TestDataGenerator.GenerateTestData2();

            //Compare two objects 
            var diff1 = obj1.CompareObjects(obj2, "obj1", "obj2");
            Console.WriteLine(diff1.NoMatchList.Any() ? "The objects differ" : "The objects are same");
            diff1.SaveToFile(@"c:\tmp\compare1.txt");


            //Compare two objects 
            //Todo: Figure out the names instead of sending them as parameters
            var diff2 = obj1.CompareObjects(obj1, "obj1", "obj1");
            Console.WriteLine(diff2.NoMatchList.Any() ? "The objects differ" : "The objects are same");
            diff2.SaveToFile(@"c:\tmp\compare2.txt");


            //Compare two objects
            var simpleObj1 = new Object1 { X = 1, Y = "A", Z = "Z" };
            var simpleObj2 = new Object2 { X = "1", Y = "B", Z = "Z" };
            var diff3 = simpleObj1.CompareObjects(simpleObj2, "simpleObj1", "simpleObj2");
            Console.WriteLine(diff3.NoMatchList.Any() ? "The objects differ" : "The objects are same");
            diff3.SaveToFile(@"c:\tmp\compare3.txt");

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }

    public class Object1
    {
        public int X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
    }

    public class Object2
    {
        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
    }
}
