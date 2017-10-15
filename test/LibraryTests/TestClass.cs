using System;
using System.Collections.Generic;
using System.Linq;
using LibraryTests.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PrintClassInstanceLib.Extensions;
using PrintClassInstanceLib.Format;

namespace LibraryTests
{
    [TestClass]
    public class TestClass
    {
        [TestMethod]
        public void CompareSimpleObjectWithItself()
        {
            var simpleObject = new SimpleObject
            {
                X = 1,
                Y = "Y",
                Z = "Z"
            };
            var diff = simpleObject.CompareObjects(simpleObject, "obj1", "obj2");
            Assert.AreEqual(0, diff.NoMatchList.Count,
                "The objects are similar however the library returned that objects are different");
        }

        [TestMethod]
        public void CompareSimpleObjectsWithDifferentValue()
        {
            var simpleObject1 = new SimpleObject
            {
                X = 1,
                Y = "Y",
                Z = "Z"
            };
            var simpleObject2 = new SimpleObject
            {
                X = 1,
                Y = "Y",
                Z = "A"
            };
            var diff = simpleObject1.CompareObjects(simpleObject2, "obj1", "obj2");
            Assert.AreEqual(1, diff.NoMatchList.Count);
            Assert.AreEqual("Z", diff.NoMatchList.Single().PropertyName);
        }

        [TestMethod]
        public void CompareDifferentObjects()
        {
            var simpleObject = new SimpleObject
            {
                X = 1,
                Y = "Y",
                Z = "Z"
            };
            var simpleObject1 = new SimpleObject2
            {
                X = 1,
                Y = 1,
                Z = "A"
            };
            var diff = simpleObject.CompareObjects(simpleObject1, "obj1", "obj2");
            Assert.AreEqual(3, diff.NoMatchList.Count);

            //Property Y share name but their data types are different (int and string)
            Assert.AreEqual(2, diff.NoMatchList.Count(s => s.PropertyName == "Y"),
                "Property Y share name but their data types are different (int and string)");

            //Property Z share name and their data types are same (string) but their values are different
            Assert.AreEqual(1, diff.NoMatchList.Count(s => s.PropertyName == "Z"),
                "Property Z share name and their data types are same (string) but their values are different");
        }

        [TestMethod]
        public void CompareObjectWithLists()
        {
            var simpleObject1 = new SimpleObjectWithList
            {
                X = new List<string> {"String1", "string2", "string3"}
            };
            var simpleObject2 = new SimpleObjectWithList
            {
                X = new List<string> {"STRING1", "STRING2", "STRING3"}
            };
            var diff = simpleObject1.CompareObjects(simpleObject2, "obj1", "obj2");
            Assert.AreEqual(0, diff.NoMatchList.Count);
        }

        [TestMethod]
        public void CompareObjectWithMultipleLists()
        {
            var so1 = new SimpleObject1WithList
            {
                X = new List<string> {"String1", "string2", "string3"},
                Y = new List<double> {1.1, 2.2, 3.3}
            };
            var so2 = new SimpleObject1WithList
            {
                X = new List<string> {"STRING1", "STRING2", "STRING3"},
                Y = new List<double> {1.1, 2.2, 3.03}
            };
            var diff = so1.CompareObjects(so2, "obj1", "obj2");
            Assert.AreEqual(1, diff.NoMatchList.Count);
            Assert.AreEqual("Y", diff.NoMatchList.Single().PropertyName);
        }

        [TestMethod]
        public void CompareObjectWithNullable()
        {
            var so1 = new SimpleObjectNullable
            {
                X = new List<string> {"String1", "string2", "string3"},
                Y = null
            };
            var so2 = new SimpleObjectNullable
            {
                X = new List<string> {"STRING1", "STRING2", "STRING3"},
                Y = 2
            };
            var diff = so1.CompareObjects(so2, "obj1", "obj2");
            Assert.AreEqual(1, diff.NoMatchList.Count);
            Assert.AreEqual("Y", diff.NoMatchList.Single().PropertyName);
        }

        [TestMethod]
        public void TestObjectPropertyName()
        {
            var so1 = new SimpleObjectWithParent();
            var prop = so1.MemberNames().ToList();
            Assert.IsTrue(prop.Contains("Field1"));
            Assert.IsTrue(prop.Contains("X"));
            Assert.IsTrue(prop.Contains("Z"));
            Assert.IsTrue(prop.Contains("X"));
            Assert.IsTrue(prop.Contains("ParentProperty"));
            Assert.IsTrue(prop.Contains("GrandparentProperty"));
            Assert.AreEqual(5, prop.Count);
        }

        [TestMethod]
        public void TestObjectPropertyValue()
        {
            var so1 = new SimpleObjectNullable()
            {
                X = new List<string> {"1", "2", "3"},
                Y = null
            };
            var prop = so1.NullMembers().ToList();
            Assert.IsTrue(prop.Contains("Y"));
            Assert.IsTrue(prop.Contains("Z"));
        }

        [TestMethod]
        public void TestObjectPropertySetValue()
        {
            var so1 = new SimpleObjectWithParent
            {
                X = new List<string> {"1", "2", "3"},
                Field1 = 5,
                ParentProperty = 5,
                GrandparentProperty = 5
            };
            so1.SetMemberValue("X", new List<string> {"4", "5"});
            so1.SetMemberValue("Field1", 100);
            so1.SetMemberValue("ParentProperty", 100);
            so1.SetMemberValue("GrandparentProperty", 100);
            so1.SetMemberValue("Z", 100);

            Assert.AreEqual(so1.X.Except(new List<string> {"4", "5"}).Count(), 0);
            Assert.AreEqual(so1.Field1, 100);
            Assert.AreEqual(so1.ParentProperty, 100);
            Assert.AreEqual(so1.GrandparentProperty, 100);
        }

        [TestMethod]
        public void TestObjectDeepClone()
        {
            var so1 = new SimpleObjectWithParent
            {
                X = new List<string> { "1", "2", "3" },
                Field1 = 5,
                ParentProperty = 5,
                GrandparentProperty = 5
            };
            
            var so1DeepClone = so1.DeepClone<SimpleObjectWithParent>();
            Assert.IsTrue(so1.X.SequenceEqual(so1DeepClone.X));
            Assert.AreEqual(so1.Field1, so1DeepClone.Field1);
            Assert.AreEqual(so1.ParentProperty, so1DeepClone.ParentProperty);
            Assert.AreEqual(so1.GrandparentProperty, so1DeepClone.GrandparentProperty);
            
            //Change the parent
            so1.Field1 = 6;
            Assert.AreEqual(so1.Field1,6);
            Assert.AreEqual(so1DeepClone.Field1, 5);
        }

        [TestMethod]
        public void TestObjectCopy()
        {
            var so1 = new SimpleObjectWithParent
            {
                X = new List<string> { "1", "2", "3" },
                Field1 = 5,
                ParentProperty = 5,
                GrandparentProperty = 5
            };

            var newObj = so1.DeepClone<SimpleObject1WithList>();
            Assert.IsTrue(newObj.X.SequenceEqual(so1.X));
            Assert.IsNull(newObj.Y);
        }

        [TestMethod]
        public void TestNamespace()
        {
            var so1 = new SimpleObjectWithParent
            {
                X = new List<string> { "1", "2", "3" },
                Field1 = 5,
                ParentProperty = 5,
                GrandparentProperty = 5
            };

            var ns = so1.GetNamespace();
            Assert.AreEqual("LibraryTests.TestData", ns);
        }

        [TestMethod]
        public void TestBaseClassNames()
        {
            var so1 = new SimpleObjectWithParent
            {
                X = new List<string> { "1", "2", "3" },
                Field1 = 5,
                ParentProperty = 5,
                GrandparentProperty = 5
            };

            var baseClassNames=so1.GetBaseClassesNames();
            Assert.AreEqual(2,baseClassNames.Count);
            Assert.IsTrue(baseClassNames.Contains("Parent"));
            Assert.IsTrue(baseClassNames.Contains("Grandparent"));
        }

        [TestMethod]
        public void TestObjectPropertyNameUsingTypeExtensions()
        {
            var prop = typeof(SimpleObjectWithParent).GetAllMemberNames();
            Assert.IsTrue(prop.Contains("Field1"));
            Assert.IsTrue(prop.Contains("X"));
            Assert.IsTrue(prop.Contains("Z"));
            Assert.IsTrue(prop.Contains("X"));
            Assert.IsTrue(prop.Contains("ParentProperty"));
            Assert.IsTrue(prop.Contains("GrandparentProperty"));
            Assert.AreEqual(5, prop.Count);
        }

        [TestMethod]
        public void TestBaseClassNamesUsingTypeExtensions()
        {
            var baseClassNames = typeof(SimpleObjectWithParent).GetBaseClassesNames();
            Assert.AreEqual(2, baseClassNames.Count);
            Assert.IsTrue(baseClassNames.Contains("Parent"));
            Assert.IsTrue(baseClassNames.Contains("Grandparent"));
        }

        [TestMethod]
        public void TestNamespaceUsingTypeExtensions()
        {
            var ns = typeof(SimpleObjectWithParent).GetNamespace();
            Assert.AreEqual("LibraryTests.TestData", ns);
        }

        [TestMethod]
        public void GetMethodInfo()
        {
            var ns = typeof(SimpleObjectWithParent).GetAllMethods();
            Assert.AreEqual(ns.Count,2);
        }

        [TestMethod]
        public void GetMethodInfoMetaData()
        {
            var ns = typeof(SimpleObjectWithParent).GetAllMethodsMetaData();
            Assert.AreEqual(ns.Count, 2);
            Assert.IsTrue(ns.Any(n=>n.Signature== "Int32 GetCount()"));
            Assert.IsTrue(ns.Any(n => n.Signature == "Int32 GetParentCount()"));
        }

        [TestMethod]
        public void InvokeMethod()
        {
            var obj = new SimpleObjectWithParent();
            var val=obj.InvokeMethod("GetParentCount", null);
            Assert.AreEqual(val, 10);
        }

        [TestMethod]
        public void TestStruct()
        {
            var obj = new TestStruct
            {
                Book = new Book { Title = "harrypotter", Author = "rowling" }
            };

            var type = obj.GetType();
            var printInfo = obj.GetObjectProperties();
            var cleanData = VariableFormat.CreateOutputAsVariableFormat(printInfo, type );

            Assert.IsTrue(cleanData.Any(s=>s.Contains("Title")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("Author")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("harrypotter")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("rowling")));
        }

        [TestMethod]
        public void TestValueTuple()
        {
            var obj = new TestValueTuple
            {
                ValueTuple = (1,2,3,"TestValueTuple")
            };

            var type = obj.GetType();
            var printInfo = obj.GetObjectProperties();
            var cleanData = VariableFormat.CreateOutputAsVariableFormat(printInfo, type);

            Assert.IsTrue(cleanData.Any(s => s.Contains("1")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("2")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("3")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("TestValueTuple")));
        }

        [TestMethod]
        public void TestTuple()
        {
            var obj = new TestTuple
            {
                Tuple = Tuple.Create(1,2,"TupleTest")
            };

            var type = obj.GetType();
            var printInfo = obj.GetObjectProperties();
            var cleanData = VariableFormat.CreateOutputAsVariableFormat(printInfo, type);

            Assert.IsTrue(cleanData.Any(s => s.Contains("1")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("2")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("TupleTest")));
        }

        [TestMethod]
        public void TestChildField()
        {
            var obj = new TestChildField
            {
                SimpleObject1 = new SimpleObject1
                {
                    Z1 = "IAmAFeild",
                    Y = 1,
                    Z = "IAmAProperty",
                    X = 2
                }
            };

            var type = obj.GetType();
            var printInfo = obj.GetObjectProperties();
            var cleanData = VariableFormat.CreateOutputAsVariableFormat(printInfo, type);

            Assert.IsTrue(cleanData.Any(s => s.Contains("1")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("2")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("IAmAFeild")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("IAmAProperty")));
        }

        [TestMethod]
        public void TestInfiniteReference()
        {
            var infiniteLoop = new TestInfiniteLoop
            {
                IntVal = 10
            };
            infiniteLoop.InfiniteLoop = infiniteLoop;
             
            var printInfo = infiniteLoop.GetObjectProperties();
            var cleanData = VariableFormat.CreateOutputAsVariableFormat(printInfo, infiniteLoop.GetType());
            Assert.IsTrue(cleanData.Any(s => s.Contains("InfiniteLoop = Error: Member too big to evaluate or has infinite reference")));
        }

        [TestMethod]
        public void TestListOfStrings()
        {
            var data = new TestListOfString
            {
                StringListField = new List<string> { "A","B","C" },
                StringListProperty = new List<string> { "X","Y","Z"}
            };

            var printInfo = data.GetObjectProperties();
            var cleanData = VariableFormat.CreateOutputAsVariableFormat(printInfo, data.GetType());

            Assert.IsTrue(cleanData.Any(s => s.Contains("A")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("B")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("C")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("X")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("Y")));
            Assert.IsTrue(cleanData.Any(s => s.Contains("Z")));
        }
    }
}
