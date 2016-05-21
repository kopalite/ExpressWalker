using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ExpressWalker.Test
{
    [TestClass]
    public class ProxyWalkerTest
    {
        [TestMethod]
        public void ProxyWalker_Unproxy()
        {
            //Arrange

            var sample = GetSample();

            //Act

            var walker = GetWalker();
            var unproxy = walker.Unproxy(sample);
            
            //Assert

            Assert.IsTrue(IsCorrect(unproxy));
        }

        public Parent GetSample()
        {
            var retVal = new ParentProxy
            {
                TestString = "aaa",
                TestInt = 10,
                TestDate = DateTime.Now,
                Child = new ChildProxy
                {
                    TestString1 = "aaa1",
                    TestInt1 = 10,
                    TestDate1 = DateTime.Now,
                }
            };

            return retVal;
        }

        public ProxyWalker<Parent> GetWalker()
        {
            var typeWalker = TypeWalker<Parent>.Create(depth: 2)
                                     .ForProperty<Parent, int>(p => p.TestInt, x => x * x)
                                     .ForProperty<Parent, string>(p => p.TestString, x => x + x)
                                     .ForElement<Child>()
                                     .ForProperty<Child, DateTime>(p => p.TestDate1, x => x.AddYears(10));

            var visitor = typeWalker.Build();

            var proxyWalker = new ProxyWalker<Parent>(visitor, depth:2);

            return proxyWalker;
        }

        private bool IsCorrect(Parent parent)
        {
            return parent is Parent &&
                   parent.TestInt == 100 && 
                   parent.TestString == "aaaaaa" && 
                   parent.Child.TestDate1.Year == DateTime.Now.Year + 10;
        }
    }

    public class ParentProxy : Parent
    {
        public override Child Child { get; set; }
    }

    public class ChildProxy : Child
    {
        public override Parent Parent { get; set; }
    }
}
