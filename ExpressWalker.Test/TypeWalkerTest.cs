using ExpressWalker.Visitors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ExpressWalker.Test
{
    [TestClass]
    public class TypeWalkerTest
    {
        [TestMethod]
        public void TypeWalker_Visit()
        {
            //Arrange

            var sample = GetSample();

            //Act

            var walker = GetWalker();
            var visitor = walker.Build();
            var blueprint = new Parent();
            var values = new HashSet<PropertyValue>();
            visitor.Visit(sample, blueprint, 10, new InstanceGuard(), values);

            //Assert

            Assert.IsTrue(IsCorrect(sample, blueprint, values));
        }

        public Parent GetSample()
        {
            var retVal = new Parent
            {
                TestString = "aaa",
                TestInt = 10,
                TestDate = DateTime.Now,
                CommonType1 = new CommonType { CommonString = "brlj" },
                Child = new Child
                {
                    TestString1 = "aaa1",
                    TestInt1 = 10,
                    TestDate1 = DateTime.Now,
                    CommonType1 = new CommonType { CommonString = "njanja" },
                    Items = new[]
                    {
                        new CollectionItem {  TestItemString = "njonjo" },
                        new CollectionItem {  TestItemString = "njinji" }
                    }
                },
            };

            retVal.Child.Parent = retVal;

            return retVal;
        }

        public TypeWalker<Parent> GetWalker()
        {
            return TypeWalker<Parent>.Create()
                                     .ForProperty<Parent, int>(p => p.TestInt, (x, m) => x * x)
                                     .ForProperty<Parent, string>(p => p.TestString, (x, m) => x + x + m)
                                     .ForProperty<Child, DateTime>(p => p.TestDate1, (x, m) => x.AddYears(10))
                                     .ForProperty<CommonType>((x, m) => new CommonType { CommonString = "..." })
                                     .ForProperty<CollectionItem, string>(p => p.TestItemString, (x, m) => "visited");
        }

        
        private bool IsCorrect(Parent parent, Parent blueprint, HashSet<PropertyValue> values)
        {
            Func<Parent, bool> isCorrect = p => p.TestInt == 100 &&
                   p.TestString == "aaaaaametadata" &&
                   p.Child.TestDate1.Year == DateTime.Now.Year + 10 &&
                   p.CommonType1.CommonString == "..." &&
                   p.Child.CommonType1.CommonString == "..." &&
                   p.Child.Items.Length == 2 &&
                   p.Child.Items[0].TestItemString == "visited" &&
                   p.Child.Items[1].TestItemString == "visited";

            return isCorrect(parent) && isCorrect(blueprint) && values.Count == 7;
        }
    }

    public class Parent
    {
        [VisitorMetadata("metadata")]
        public string TestString { get; set; }

        public int TestInt { get; set; }

        public DateTime TestDate { get; set; }

        public virtual Child Child { get; set; }

        public CommonType CommonType1 { get; set; }
    }

    public class Child
    {
        public string TestString1 { get; set; }

        public int TestInt1 { get; set; }

        public DateTime TestDate1 { get; set; }

        //For testing circular references.

        public virtual Parent Parent { get; set; }

        public CommonType CommonType1 { get; set; }

        public CollectionItem[] Items { get; set; }
    }

    public class CommonType
    {
        public string CommonString { get; set; }

    }

    public class CollectionItem
    {
        public string TestItemString { get; set; }
    }
}
