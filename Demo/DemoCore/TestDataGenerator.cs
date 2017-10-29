using System;
using System.Collections.Generic;

namespace DemoCore
{
    public static class TestDataGenerator
    {
        public static PrintMe GenerateTestData1()
        {
            var data = new PrintMe
            {
                F = new List<List<List<PrintMeChild>>>
                {

                    new List<List<PrintMeChild>>
                    {
                        new List<PrintMeChild>
                        {
                            new PrintMeChild
                            {
                                ResponseTitle = "0-0-0",
                            },
                            new PrintMeChild
                            {
                                ResponseTitle = "0-0-1",
                            }
                        },
                        new List<PrintMeChild>
                        {
                            new PrintMeChild
                            {
                                ResponseTitle = "0-1-0",
                            },
                            new PrintMeChild
                            {
                                ResponseTitle = "0-1-1",
                            }
                        }
                    },

                    new List<List<PrintMeChild>>
                    {
                        new List<PrintMeChild>
                        {
                            new PrintMeChild
                            {
                                ResponseTitle = "1-0-0",
                            },
                            new PrintMeChild
                            {
                                ResponseTitle = "1-0-1",
                            }
                        },
                        new List<PrintMeChild>
                        {
                            new PrintMeChild
                            {
                                ResponseTitle = "1-1-0",
                            },
                            new PrintMeChild
                            {
                                ResponseTitle = "1-1-1",
                            }
                        }
                    }

                },

                PrintMeChildPropertyTest = new PrintMeChild
                {
                    ResponseTitle = "Child2Title1",
                },
                PrintMeEnum = PrintMeEnum.PrintMeEnum2,

                DictionaryWithListTest = new Dictionary<string, List<PrintMeChild>>
                {
                    {
                        "key1",
                        new List<PrintMeChild>
                        {
                            new PrintMeChild
                            {
                                ResponseTitle = "aa",
                                ResponseValue = "bb"
                            },
                            new PrintMeChild
                            {
                                ResponseTitle = "11",
                                ResponseValue = "22"
                            }
                        }
                    },
                    {
                        "key2",
                        new List<PrintMeChild>
                        {
                            new PrintMeChild
                            {
                                ResponseTitle = "zz",
                                ResponseValue = "xx"
                            },
                            new PrintMeChild
                            {
                                ResponseTitle = "88",
                                ResponseValue = "99"
                            }
                        }
                    }
                },

                TestValueTuple = (1,1,1,"1"),
                DateTimeTest = DateTime.Parse("09/16/2016"),
                ListOfStringTest = new List<string> { "A", "B", "C" },
            };
            return data;
        }

        public static PrintMeV2 GenerateTestData2()
        {
            var data = new PrintMeV2("GenerateTestData2")
            {
                PrintMeChildPropertyTest = new PrintMeChild
                {
                    ResponseTitle = "Child2Title1",
                },
                PrintMeEnum = PrintMeEnum.PrintMeEnum2,

                DictionaryWithListTest = new Dictionary<string, List<PrintMeChild>>
                {
                    {
                        "key1",
                        new List<PrintMeChild>
                        {
                            new PrintMeChild
                            {
                                ResponseTitle = "aa",
                                ResponseValue = "bb"
                            },
                            new PrintMeChild
                            {
                                ResponseTitle = "11",
                                ResponseValue = "22"
                            }
                        }
                    },
                    {
                        "key2",
                        new List<PrintMeChild>
                        {
                            new PrintMeChild
                            {
                                ResponseTitle = "zz",
                                ResponseValue = "xx"
                            },
                            new PrintMeChild
                            {
                                ResponseTitle = "88",
                                ResponseValue = "99"
                            }
                        }
                    }
                },
                TestTuple = Tuple.Create(1, 1, 1, "2"),
                DateTimeTest = DateTime.Parse("09/16/2016"),
                ListOfStringTest = new List<string> { "A", "B", "C" ,"D" },
            };
            return data;
        }

        public static object GenerateTestData3()
        {
            return new 
            {
                Anonymous = "true"
            };
        }
    }
}
