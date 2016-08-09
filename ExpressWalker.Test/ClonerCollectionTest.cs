using ExpressWalker.Cloners;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ExpressWalker.Test
{
    [TestClass]
    public class ClonerCollectionTest
    {
        [TestMethod]
        public void Cloner_Collection_List()
        {
            //Arrange

            var test2List = new List<Test2>
            {
                new Test2 { Name = "Name11" },
                new Test2 { Name = "Name12"} 
            };
            

            //Act

            var cloner = ClonerBase.Create(test2List.GetType());
            var clone = (List<Test2>)cloner.Clone(test2List);

            //Assert

            Assert.IsTrue(clone != null && 
                          clone != test2List &&
                          clone.GetType() == test2List.GetType() &&
                          clone.Count == 2 && 
                          clone[0].Name == "Name11" &&
                          clone[1].Name == "Name12");
        }

        [TestMethod]
        public void Cloner_Collection_Collection()
        {
            //Arrange

            var test2List = new Collection<Test2>
            {
                new Test2 { Name = "Name11" },
                new Test2 { Name = "Name12"}
            };


            //Act

            var cloner = ClonerBase.Create(test2List.GetType());
            var clone = (Collection<Test2>)cloner.Clone(test2List);

            //Assert

            Assert.IsTrue(clone != null &&
                          clone != test2List &&
                          clone.GetType() == test2List.GetType() &&
                          clone.Count == 2 &&
                          clone[0].Name == "Name11" &&
                          clone[1].Name == "Name12");
        }

        [TestMethod]
        public void Cloner_Collection_HashSet()
        {
            //Arrange

            var test2List = new HashSet<Test2>
            {
                new Test2 { Name = "Name11" },
                new Test2 { Name = "Name12"}
            };


            //Act

            var cloner = ClonerBase.Create(test2List.GetType());
            var clone = (HashSet<Test2>)cloner.Clone(test2List);

            //Assert

            Assert.IsTrue(clone != null &&
                          clone != test2List &&
                          clone.GetType() == test2List.GetType() &&
                          clone.Count == 2 &&
                          clone.Any(x => x.Name == "Name11") &&
                          clone.Any(x => x.Name == "Name12"));
        }

        [TestMethod]
        public void Cloner_Collection_Array()
        {
            //Arrange

            var test2List = new[]
            {
                new Test2 { Name = "Name11" },
                new Test2 { Name = "Name12"}
            };


            //Act

            var cloner = ClonerBase.Create(test2List.GetType());
            var clone = (Test2[])cloner.Clone(test2List);

            //Assert

            Assert.IsTrue(clone != null &&
                          clone != test2List &&
                          clone.GetType() == test2List.GetType() &&
                          clone.Length == 2 &&
                          clone[0].Name == "Name11" &&
                          clone[1].Name == "Name12");
        }
    }

    public class Test2
    {
        public string Name { get; set; }
    }
}
