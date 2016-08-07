﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressWalker;
using ExpressWalker.Visitors;
using System.Collections.Generic;

namespace ExpressWalker.Test
{
    [TestClass]
    public class ManualWalkerTest
    {
        [TestMethod]
        public void ManualWalker_Visit()
        {
            //Arrange

            var sample = GetSample();

            //Act

            var walker = GetWalker();
            var blueprint = new A1();
            var values = new HashSet<PropertyValue>();
            walker.Visit(sample, blueprint, 10, new InstanceGuard(), values);

            //Assert

            Assert.IsTrue(IsCorrect(sample, blueprint, values));
        }

        private A1 GetSample()
        {
            return new A1
            {
                A1Date = DateTime.Now,
                A1Amount = 34,
                B1 = new B1
                {
                    B1Name = "TestB1",

                    C1 = new C1
                    {
                        C1Date = DateTime.Now
                    }
                },
                B2 = new B2
                {
                    B2Date = DateTime.Now
                }
            };
        }

        private IVisitor GetWalker()
        {
            return ManualWalker.Create<A1>()
                                    .Property<A1, DateTime>(a1 => a1.A1Date, (va1, m) => va1.AddYears(10))
                                    .Property<A1, int>(a1 => a1.A1Amount, (va1, m) => va1 * 3)
                                    .Element<A1, B1>(a1 => a1.B1, b1 =>
                                            b1.Property<B1, string>(x => x.B1Name, (vb1, m) => vb1 + "Test2")
                                              .Element<B1, C1>(b11 => b11.C1, c1 =>
                                                  c1.Property<C1, DateTime>(x => x.C1Date, (vc1, m) => vc1.AddYears(10))))
                                    .Element<A1, B2>(a1 => a1.B2, b2 => b2
                                        .Property<B2, DateTime>(x => x.B2Date, (vb2, m) => vb2.AddYears(10)))
                                .Build();
        }

        
        private bool IsCorrect(A1 sample, A1 blueprint, HashSet<PropertyValue> values)
        {
            var tenYearsAfter = DateTime.Now.Year + 10;
            return sample.A1Date.Year == tenYearsAfter &&
                   sample.A1Amount == 102 &&
                   sample.B1.B1Name == "TestB1Test2" &&
                   sample.B1.C1.C1Date.Year == tenYearsAfter &&
                   sample.B2.B2Date.Year == tenYearsAfter;
        }

    }

    public class A1
    {
        public string A1Name { get; set; }

        public int A1Amount { get; set; }

        public DateTime A1Date { get; set; }

        public B1 B1 { get; set; }

        public B2 B2 { get; set; }
    }

    public class B1
    {
        public string B1Name { get; set; }

        public int B1Amount { get; set; }

        public DateTime B1Date { get; set; }

        public C1 C1 { get; set; }

        public C2 C2 { get; set; }
    }

    public class B2
    {
        public string B2Name { get; set; }

        public int B2Amount { get; set; }

        public DateTime B2Date { get; set; }

        public C3 C3 { get; set; }
    }

    public class C1
    {
        public string C1Name { get; set; }

        public int C1Amount { get; set; }

        public DateTime C1Date { get; set; }
    }

    public class C2
    {
        public string C2Name { get; set; }

        public int C2Amount { get; set; }

        public DateTime C2Date { get; set; }
    }

    public class C3
    {
        public string C3Name { get; set; }

        public int C3Amount { get; set; }

        public DateTime C3Date { get; set; }
    }
}
