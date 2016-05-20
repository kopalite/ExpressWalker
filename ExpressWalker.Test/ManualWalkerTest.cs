using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpressWalker;

namespace ExpressWalker.Test
{
    [TestClass]
    public class ManualWalkerTest
    {
        [TestMethod]
        public void WalkerLiquid_Visit()
        {
            var walker = GetWalker();
            var sample = GetSample();
            walker.Visit(sample);
        }

        private IElementVisitor<A1> GetWalker()
        {
            return ManualWalker.Create<A1>()
                                    .Property<A1, string>(a1 => a1.A1Date, va1 => va1 + "Test1")
                                    .Property<A1, int>(a1 => a1.A1Amount, va1 => va1 * 3)
                                    .Element<A1, B1>(a1 => a1.B1, b1 =>
                                            b1.Property<B1, string>(x => x.B1Name, vb1 => vb1 + "Test2")
                                              .Element<B1, C1>(b11 => b11.C1, c1 =>
                                                  c1.Property<C1, DateTime>(x => x.C1Date, vc1 => vc1.AddYears(10))))
                                    .Element<A1, B2>(a1 => a1.B2, b2 => b2
                                        .Property<B2, DateTime>(x => x.B2Date, vb2 => vb2.AddYears(10)));
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
