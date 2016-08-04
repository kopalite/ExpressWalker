using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
            visitor.Visit(sample, blueprint, 10, new InstanceGuard());

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
                CommonType1 = new CommonType { CommonString = "brlj" },
                Child = new Child
                {
                    TestString1 = "aaa1",
                    TestInt1 = 10,
                    TestDate1 = DateTime.Now,
                    CommonType1 = new CommonType { CommonString = "njanja" }
                },
            };

            retVal.Child.Parent = retVal;

            return retVal;
        }

        public TypeWalker<Parent> GetWalker()
        {
            return TypeWalker<Parent>.Create()
                                     .ForProperty<Parent, int>(p => p.TestInt, null, (x, m) => x * x)
                                     .ForProperty<Parent, string>(p => p.TestString, (x, m) => Foo(x, m), (x, m) => x + x + m)
                                     .ForProperty<Child, DateTime>(p => p.TestDate1, (x, m) => Foo(x, m), (x, m) => x.AddYears(10))
                                     .ForProperty<CommonType>((x, m) => Foo(x, m), (x, m) => new CommonType { CommonString = "..." });
        }

        private int _counter;

        private void Foo(DateTime input, object metadata)
        {
            _counter++;
        }

        private void Foo(int input, object metadata)
        {
            _counter++;
        }

        private void Foo(string input, object metadata)
        {
            if (metadata == null)
            {
                throw new Exception("Metadata object should be properly passed to 'getOldValue' function!");
            }

            _counter++;
        }

        private void Foo(CommonType input, object metadata)
        {
            _counter++;
        }

        private bool IsCorrect(Parent parent)
        {
            return parent.TestInt == 100 &&
                   parent.TestString == "aaaaaametadata" &&
                   parent.Child.TestDate1.Year == DateTime.Now.Year + 10 &&
                   parent.CommonType1.CommonString == "..." &&
                   parent.Child.CommonType1.CommonString == "..." &&
                   _counter == 4;

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
    }

    public class CommonType
    {
        public string CommonString { get; set; }

    }
}
