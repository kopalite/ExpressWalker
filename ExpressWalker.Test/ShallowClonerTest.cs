using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressWalker.Test
{
    [TestClass]
    public class ShallowClonerTest
    {
        [TestMethod]
        public void ShallowCloner_Clone()
        {
            //Arrange

            var x = new X
            {
                Name = "Name1",
                Number = 1,
                Double = 2,
                Y = new Y()
            };

            //Act

            var cloner = new ShallowCloner<X>();
            var clone = cloner.Clone(x);

            //Assert

            Assert.AreEqual("Name1", clone.Name);
            Assert.AreEqual(1, clone.Number);
            Assert.AreEqual(2, clone.Double);
            Assert.IsNull(clone.Y);
        }
    }

    public class X
    {
        public string Name { get; set; }

        public int Number { get; set; }

        public double? Double { get; set; }

        public Y Y { get; set; }
    }

    public class Y
    {
        public string Name { get; set; }
    }
}
