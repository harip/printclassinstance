using System;
using System.Collections.Generic;

namespace DemoCore
{
    public class PrintMeGrandParent
    {
        public PrintMeGrandParent()
        {
            GrandParentIntPrivatePropertyTest = 1000;
        }
        private int GrandParentIntPrivatePropertyTest { get; set; }
        public string GrandParentStringTest = "GrandParentStringTest";
    }

    public class PrintMeParent : PrintMeGrandParent
    {
        public PrintMeParent()
        {
            ParentIntPrivatePropertyTest = 1000;
        }
        private int ParentIntPrivatePropertyTest { get; set; }
        public string ParentStringTest = "ParentStringTestSetInParent";

        public void ParentSetCount(int z)
        {
        }

        private  void ParentGetCount(int z)
        {
        }
    }

    public class PrintMe : PrintMeParent
    {
        // Fields
        private string PrivateStringFieldTest = "PrivateStringFieldTest";
        public static int StaticIntTest = 10;
        public static readonly int ReadOnlyStaticIntTest = 10;

        // Properties
        private string PrivateStringPropertyTest { get; set; }
        public DateTime DateTimeTest { get; set; }
        public List<string> ListOfStringTest { get; set; }
        public PrintMeChild PrintMeChildPropertyTest { get; set; }

        public List<List<List<PrintMeChild>>> F { get; set; }
        public PrintMeEnum PrintMeEnum { get; set; }
        public Dictionary<string, List<PrintMeChild>> DictionaryWithListTest { get; set; }

        //public Tuple<int, int, int, string> TestTuple { get; set; }
        public (int intItem1,int intItem2,int intItem3,string strItem4) TestTuple { get; set; }
        
        public PrintMe()
        {
            PrivateStringPropertyTest = "PrivateStringPropertyTest";
            F = new List<List<List<PrintMeChild>>>();
            DictionaryWithListTest = new Dictionary<string, List<PrintMeChild>>();
            ListOfStringTest = new List<string>();
        }

        public PrintMe(string privateStringPropertyTest)
        {
            PrivateStringPropertyTest = privateStringPropertyTest;
            F = new List<List<List<PrintMeChild>>>();
            DictionaryWithListTest = new Dictionary<string, List<PrintMeChild>>();
            ListOfStringTest = new List<string>();
        }

        public void SetCount(int z)
        {
        }

        public int GetCount(PrintMe printme,List<string> test)
        {
            var y = 1 * 10;
            return y;
        }
    }

    public class PrintMeV2 : PrintMeParent
    {
        // Fields
        private string PrivateStringFieldTest = "PrivateStringFieldTest";
        public static int StaticIntTest = 10;
        public static readonly int ReadOnlyStaticIntTest = 10;

        // Properties
        private string PrivateStringPropertyTest { get; set; }
        public DateTime DateTimeTest { get; set; }
        public List<string> ListOfStringTest { get; set; }
        public PrintMeChild PrintMeChildPropertyTest { get; set; }

        public PrintMeEnum PrintMeEnum { get; set; }
        public Dictionary<string, List<PrintMeChild>> DictionaryWithListTest { get; set; }
        public Tuple<int, int, int, string> TestTuple { get; set; }

        public PrintMeV2()
        {
            PrivateStringPropertyTest = "PrivateStringPropertyTest";
            DictionaryWithListTest = new Dictionary<string, List<PrintMeChild>>();
            ListOfStringTest = new List<string>();
        }

        public PrintMeV2(string privateStringPropertyTest)
        {
            PrivateStringPropertyTest = privateStringPropertyTest;
            DictionaryWithListTest = new Dictionary<string, List<PrintMeChild>>();
            ListOfStringTest = new List<string>();
        }
    }

    public class PrintMeChild
    {
        public string ResponseTitle { get; set; }
        public string ResponseValue { get; set; }
    }

    public enum PrintMeEnum
    {
        PrintMeEnum0 = 0,
        PrintMeEnum1 = 1,
        PrintMeEnum2 = 2,
        PrintMeEnum3 = 3
    }
}