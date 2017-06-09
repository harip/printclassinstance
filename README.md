# ObjectExtensions

Nuget Package: https://www.nuget.org/packages/PrintClassInstance

ObjectExtensions (supports .NET Core, .NET 4.6.) can be used to:
~~~
1. Deep compare two objects and save or view the results. 
2. Get list of members names
3. Get member value
4. Set member value including Private members. Supports setting values on Class Hierarchies. 
5. Get list of member names that are null
6. Deep Clone
7. Save or view object graph of an object's instance
~~~

## CompareObjects
Deep compare any two objects - Supports classes with multilevel Hierarchies

Example:
```cs
public class Program
{
	public static void Main(string[] args)
	{
		var simpleObj1 = new Object1 {X = 1, Y="A",Z="Z"};
		var simpleObj2 = new Object2 { X = "1", Y = "B",Z="Z" };
		var diff3 = simpleObj1.CompareObjects(simpleObj2, "simpleObj1", "simpleObj2");
		Console.WriteLine(diff3.NoMatchList.Any() ? "The objects differ" : "The objects are same");
		diff3.SaveToFile(@"c:\tmp\compare3.txt");
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
```
The text file looks like this:
```cs
ObjectCompareInfo
{
	NoMatchList = new List<PrintClassInstanceLib.Model.ObjectPropertyCompareInfo>
	{
		new ObjectPropertyCompareInfo
		{
			PropertyName = "X",
			PropertyType = "System.Int32",
			Description = "Property only exists in simpleObj1",
		},
		new ObjectPropertyCompareInfo
		{
			PropertyName = "Y",
			PropertyType = "String",
			Description = "Property value does not match",
		},
		new ObjectPropertyCompareInfo
		{
			PropertyName = "X",
			PropertyType = "String",
			Description = "Property only exists in simpleObj2",
		},
	},
};
```
## MemberNames
Get list of members (Fields,Properties) declared in the class, along with all members declared in all classes in its inheritance hierarchy and includes Private and Static members.

Example:
```cs
public class SimpleObjectNullable
{
	public List<string> X { get; set; }
	public int? Y { get; set; }
	private int? Z { get; set; }
}

var memberList = new SimpleObjectNullable().MemberNames().ToList();
```

## MemberValue
Get value of a member

Example:
```cs
var memberValue = new SimpleObjectNullable().MemberValue("X");
```

## NullMembers
Get all member names that are Null

Example:
```cs
var nullMembers = new SimpleObjectNullable().NullMembers().ToList();
```
## DeepClone
Create a deep clone (including private and values on Class Hierarchies)

Example:
```cs
var so1 = new SimpleObjectWithParent
{
	X = new List<string> { "1", "2", "3" },
	Field1 = 5,
	ParentProperty = 5,
	GrandparentProperty = 5
};
var so1DeepClone = so1.DeepClone<SimpleObjectWithParent>();
```
## SetMemberValue
Set member value including Private members. Supports setting values on Class Hierarchies. Returns a string with error message if the operation failed.

Example:
```cs
public class SimpleObjectWithParent : Parent
{
	public int Field1;
	public List<string> X { get; set; }
	private int Z=10;
}
public class Parent:Grandparent
{
	public int ParentProperty { get; set; }
}
public class Grandparent 
{
	public int GrandparentProperty { get; set; }
}

var so1 = new SimpleObjectWithParent
{
	X = new List<string> { "1", "2", "3" },
	Field1 = 5,
	ParentProperty = 5,
	GrandparentProperty = 5
};
so1.SetMemberValue("X", new List<string> { "4","5" });
so1.SetMemberValue("Field1", 100);
so1.SetMemberValue("ParentProperty", 100);
so1.SetMemberValue("GrandparentProperty", 100);
so1.SetMemberValue("Z", 100);
```

## SaveToFile
View or Save object graph

<details>
  <summary>Example: class PrintMe</summary>
  <p>
```cs
namespace PrintClassInstance
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
	}

	public class PrintMe: PrintMeParent
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
		public Tuple<int, int, int, string> TestTuple { get; set; }

		public PrintMe()
		{
			PrivateStringPropertyTest = "PrivateStringPropertyTest";
			F = new List<List<List<PrintMeChild>>>();
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
```
</p>
</details>

To print the object graph to a file use the following syntax:

```cs
namespace PrintClassInstance
{
    public class Program
    {
            static void Main(string[] args)
            {
                PrintMe data = GenerateTestDataForPrintMe();

                data.SaveToFile(@"c:\tmp\instance.txt");  
				              
                Console.WriteLine("Finished");
                Console.ReadLine();
            }
    }
}
```

<details>
  <summary>output:</summary>  
  <p>
```cs
PrintMe
{
	PrivateStringPropertyTest = "PrivateStringPropertyTest",
	DateTimeTest = DateTime.Parse("9/16/2016 12:00:00 AM"),
	ListOfStringTest = new List<String>
	{
		"A",
		"B",
		"C",
	},
	PrintMeChildPropertyTest = new PrintMeChild
	{
		ResponseTitle = "Child2Title1",
		ResponseValue = "",
	},
	F = new List<List<List<PrintClassInstance.PrintMeChild>>>
	{
		new List<List<PrintClassInstance.PrintMeChild>>
		{
			new List<PrintClassInstance.PrintMeChild>
			{
				new PrintMeChild
				{
					ResponseTitle = "0-0-0",
					ResponseValue = "",
				},
				new PrintMeChild
				{
					ResponseTitle = "0-0-1",
					ResponseValue = "",
				},
			},
			new List<PrintClassInstance.PrintMeChild>
			{
				new PrintMeChild
				{
					ResponseTitle = "0-1-0",
					ResponseValue = "",
				},
				new PrintMeChild
				{
					ResponseTitle = "0-1-1",
					ResponseValue = "",
				},
			},
		},
		new List<List<PrintClassInstance.PrintMeChild>>
		{
			new List<PrintClassInstance.PrintMeChild>
			{
				new PrintMeChild
				{
					ResponseTitle = "1-0-0",
					ResponseValue = "",
				},
				new PrintMeChild
				{
					ResponseTitle = "1-0-1",
					ResponseValue = "",
				},
			},
			new List<PrintClassInstance.PrintMeChild>
			{
				new PrintMeChild
				{
					ResponseTitle = "1-1-0",
					ResponseValue = "",
				},
				new PrintMeChild
				{
					ResponseTitle = "1-1-1",
					ResponseValue = "",
				},
			},
		},
	},
	PrintMeEnum = PrintClassInstance.PrintMeEnum.PrintMeEnum2,
	DictionaryWithListTest = new Dictionary<String,List<PrintClassInstance.PrintMeChild>>
	{
		{
			Key = "key1",
			Value = new List<PrintClassInstance.PrintMeChild>
			{
				new PrintMeChild
				{
					ResponseTitle = "aa",
					ResponseValue = "bb",
				},
				new PrintMeChild
				{
					ResponseTitle = "11",
					ResponseValue = "22",
				},
			},
		},
		{
			Key = "key2",
			Value = new List<PrintClassInstance.PrintMeChild>
			{
				new PrintMeChild
				{
					ResponseTitle = "zz",
					ResponseValue = "xx",
				},
				new PrintMeChild
				{
					ResponseTitle = "88",
					ResponseValue = "99",
				},
			},
		},
	},
	TestTuple = new Tuple`4<Int32,Int32,Int32,String>
	{
		Item1 = 1,
		Item2 = 1,
		Item3 = 1,
		Item4 = "1",
	},
	PrivateStringFieldTest = "PrivateStringFieldTest",
	StaticIntTest = 10,
	ReadOnlyStaticIntTest = 10,
	ParentStringTest = "ParentStringTestSetInParent",
	GrandParentStringTest = "GrandParentStringTest",
	ParentIntPrivatePropertyTest = 1000,
	GrandParentIntPrivatePropertyTest = 1000,
};
```
</p>
</details>