using System;
using System.Linq;
using PrintClassInstanceLib.Extensions;

namespace Demo462
{
    class Program
    {
        static void Main(string[] args)
        {
            var data = TestDataGenerator.GenerateTestData();
            data.SaveToFile(@"c:\tmp\instance_462.txt");

            //Compare two objects
            var simpleObj1 = new Object1 { X = 1, Y = "A", Z = "Z" };
            var simpleObj2 = new Object2 { X = "1", Y = "B", Z = "Z" };
            var diff3 = simpleObj1.CompareObjects(simpleObj2, "simpleObj1", "simpleObj2");
            Console.WriteLine(diff3.NoMatchList.Any() ? "The objects differ" : "The objects are same");
            diff3.SaveToFile(@"c:\tmp\462.txt");

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
