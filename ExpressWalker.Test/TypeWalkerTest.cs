using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ExpressWalker.Test
{
    [TestClass]
    public class TypeWalkerTest
    {
        [TestMethod]
        public void TypeWalker_Build()
        {
            //Arrange

            var sample = GetSample();

            //Act

            var walker = GetWalker();
            var visitor = walker.Build();
            visitor.Visit(sample);

            //Assert

            Assert.IsTrue(IsCorrect(sample));
        }

        public Parent GetSample()
        {
            var retVal = new Parent
            {
                TestString = "aaa",
                TestInt = 10,
                TestDate = DateTime.Now,
                Child = new Child
                {
                    TestString1 = "aaa1",
                    TestInt1 = 10,
                    TestDate1 = DateTime.Now,
                    Child1 = new Child()
                }
            };

            retVal.Child.Parent = retVal;

            return retVal;
        }

        public TypeWalker<Parent> GetWalker()
        {
            return TypeWalker<Parent>.Create(depth: 10)
                                     .ForProperty<Parent, int>(p => p.TestInt, x => x * x)
                                     .ForProperty<Parent, string>(p => p.TestString, x => x + x)
                                     .ForElement<Child>()
                                     .ForProperty<Child, DateTime>(p => p.TestDate1, x => x.AddYears(10))
                                     .ForProperty<Child, Parent>(p => p.Parent, x => null)
                                     .ForProperty<Child, Child>(p => p.Child1, x => null);
        }

        private bool IsCorrect(Parent parent)
        {
            return parent.TestInt == 100 && 
                   parent.TestString == "aaaaaa" && 
                   parent.Child.TestDate1.Year == DateTime.Now.Year + 10;
        }
    }

    public class Parent
    {
        public string TestString { get; set; }

        public int TestInt { get; set; }

        public DateTime TestDate { get; set; }

        public virtual Child Child { get; set; }
    }

    public class Child
    {
        public string TestString1 { get; set; }

        public int TestInt1 { get; set; }

        public DateTime TestDate1 { get; set; }

        //For testing circular references.

        public virtual Parent Parent { get; set; }

        public Child Child1 { get; set; }
    }
}
