using System;
using System.Collections.Generic;

namespace LibraryTests.TestData
{
    public class SimpleObject
    {
        public int X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
    }

    public class SimpleObject1
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Z { get; set; }
        public string Z1;
    }


    public class SimpleObject2
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Z { get; set; } 
    }

    public class SimpleObjectWithList
    {
        public List<string> X { get; set; }
    }

    public class SimpleObject1WithList
    {
        public List<string> X { get; set; }
        public List<double> Y { get; set; }
    }

    public class SimpleObjectNullable
    {
        public List<string> X { get; set; }
        public int? Y { get; set; }
        private int? Z { get; set; }
    }

    public class SimpleObjectWithParent : Parent
    {
        public int Field1;
        public List<string> X { get; set; }
        private int Z = 10;

        public int GetCount()
        {
            return Z;
        }
    }

    public class Parent : Grandparent
    {
        public int ParentProperty { get; set; }

        private int GetParentCount()
        {
            return 10;
        }
    }

    public class Grandparent
    {
        public int GrandparentProperty { get; set; }
    }

    public class TestStruct
    {
        //public Tuple<int, int, string> Tuple { get; set; }
        //public (int intItem1, int intItem2, int intItem3, string strItem4) TupleC7 { get; set; }
        //public SimpleObject1 SimpleObject1;
        public Book Book { get; set; }
    }

    public struct Book
    {
        public string Title;
        public string Author { get; set; }
    }

    public class TestValueTuple
    {
        public (int intItem1, int intItem2, int intItem3, string strItem4) ValueTuple { get; set; }
    }

    public class TestTuple
    {
        public Tuple<int, int, string> Tuple { get; set; }
    }

    public class TestChildField
    {
        public SimpleObject1 SimpleObject1;
    }
}
