using ExpressWalker.Visitors;
using FizzWare.NBuilder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ExpressWalker.Test
{
    [TestClass]
    public class StressTest
    {
        [TestMethod]
        public void TypeWalker_StressTest_ComplexCircularReference_Build()
        {
            //Arrange

            var watch = new Stopwatch();
            
            //Act

            watch.Start();
            var visitor1 = TypeWalker<Document>.Create().ForProperty<DateTime>((x, m) => DateTime.Now).Build(10, new PropertyGuard(), false);
            watch.Stop();

            //Assert
            Assert.IsTrue(watch.ElapsedMilliseconds <= 1000);
        }

        [TestMethod]
        public void TypeWalker_StressTest_ComplexCircularReference_Visit()
        {
            //Arrange

            var watch = new Stopwatch();
            var visitor = TypeWalker<Document>.Create().ForProperty<DateTime>((x, m) => DateTime.Now.AddYears(10)).Build(10, new PropertyGuard(), false);
            var values = new HashSet<PropertyValue>();
            var document = GetComplexSample();

            //Act

            watch.Start();
            visitor.Visit(document, depth:10, guard:new InstanceGuard(), values:values);
            watch.Stop();

            //Assert
            Assert.IsTrue(values.Count == 90 && values.All(x => ((DateTime)x.NewValue).Year == DateTime.Now.Year + 10));
        }

        [TestMethod]
        public void TypeWalker_StressTest_AllowedHierarchy_Visit()
        {
            //Arrange

            var visitor = TypeWalker<AllowedHierarchy>.Create().ForProperty<DateTime>((x, m) => DateTime.Now.AddYears(10)).Build(10, new PropertyGuard());
            var hierarchy = Builder<AllowedHierarchy>.CreateListOfSize(10).BuildHierarchy(new HierarchySpec<AllowedHierarchy>
            {
                Depth = 5,
                MaximumChildren = 1,
                MinimumChildren = 1,
                NumberOfRoots = 1,
                AddMethod = (x1, x2) => x1.Child = x2
            }).First();
            var values = new HashSet<PropertyValue>();

            //Act

            visitor.Visit(hierarchy, depth: 10, guard: new InstanceGuard(), values: values);

            //Assert

            Assert.AreEqual(6, values.Count);
        }

        [TestMethod]
        public void TypeWalker_StressTest_SuppressedHierarchy_Visit()
        {
            //Arrange

            var visitor = TypeWalker<SuppressedHierarchy>.Create().ForProperty<DateTime>((x, m) => DateTime.Now.AddYears(10)).Build(10, new PropertyGuard());
            var hierarchy = Builder<SuppressedHierarchy>.CreateListOfSize(10).BuildHierarchy(new HierarchySpec<SuppressedHierarchy>
            {
                Depth = 5,
                MaximumChildren = 1,
                MinimumChildren = 1,
                NumberOfRoots = 1,
                AddMethod = (x1, x2) => x1.Child = x2
            }).First();
            var values = new HashSet<PropertyValue>();

            //Act

            visitor.Visit(hierarchy, depth: 10, guard: new InstanceGuard(), values: values);

            //Assert

            Assert.AreEqual(3, values.Count);
        }

        private Document GetComplexSample()
        {
            var document = Builder<Document>.CreateNew()


                               .With(x => x.DocumentDefaultUser1, Builder<User>.CreateNew()
                                           .With(x => x.User_UserToRole, Builder<UserToRole>.CreateNew()
                                                                            .With(y => y.UserToRole_User, Builder<User>.CreateNew().Build())
                                                                            .With(x => x.UserToRole_Role, Builder<Role>.CreateNew().Build())
                                           .Build())
                               .Build())

                               .With(x => x.DocumentDefaultRole1, Builder<Role>.CreateNew()
                                           .With(x => x.Role_UserToRoleList, Builder<UserToRole>.CreateListOfSize(2).All()
                                                                                                        .With(x => x.UserToRole_User, Builder<User>.CreateNew()
                                                                                                        .Build())
                                           .Build())

                               .With(x => x.Role_Profile, Builder<Profile>.CreateNew()
                                                                .With(x => x.Profile_OperationsList, Builder<OperationToProfile>.CreateListOfSize(5).All()
                                                                                                          .With(x => x.OperationToProfile_Operation, Builder<Operation>.CreateNew().Build())
                                                                                                          .Build())
                                                                .Build())
                               .Build())

                               .With(x => x.DocumentDefaultUnit1, Builder<Unit>.CreateNew()
                                                                       .With(x => x.Unit_Roles, Builder<Role>.CreateListOfSize(2).All()
                                                                                                   .With(x => x.Role_UserToRoleList, Builder<UserToRole>.CreateListOfSize(1).All()
                                                                                                                                       .With(x => x.UserToRole_User, Builder<User>.CreateNew()
                                                                                                                                       .Build())
                                                                                                   .Build())
                                                                       .Build())
                               .Build())

                               .With(x => x.DocumentDefaultUnit2, Builder<Unit>.CreateListOfSize(10).BuildHierarchy(new HierarchySpec<Unit>
                               {
                                   Depth = 5,
                                   MaximumChildren = 1,
                                   MinimumChildren = 1,
                                   NumberOfRoots = 1,
                                   AddMethod = (x1, x2) => x1.Unit_ParentUnit = x2
                               }).First())

                           .Build();

            return document;
        }
    }

    

    public class Document
    {
        public User DocumentDefaultUser1 { get; set; }
        public Role DocumentDefaultRole1 { get; set; }
        public Unit DocumentDefaultUnit1 { get; set; }
        public User DocumentDefaultUser2 { get; set; }
        public Role DocumentDefaultRole2 { get; set; }
        public Unit DocumentDefaultUnit2 { get; set; }
        public DateTime TestDocumentDateTime1 { get; set; }
        public DateTime TestDocumentDateTime2 { get; set; }
        public DateTime TestDocumentDateTime3 { get; set; }
        public DateTime TestDocumentDateTime4 { get; set; }
        public DateTime TestDocumentDateTime5 { get; set; }
    }

    

    public class User
    {
        public string TestUserString1 { get; set; }
        public string TestUserString2 { get; set; }
        public string TestUserString3 { get; set; }
        public string TestUserString4 { get; set; }
        public string TestUserString5 { get; set; }

        public DateTime TestUserDateTime1 { get; set; }
        public DateTime TestUserDateTime2 { get; set; }
        public DateTime TestUserDateTime3 { get; set; }
        public DateTime TestUserDateTime4 { get; set; }
        public DateTime TestUserDateTime5 { get; set; }

        public int TestUserInt1 { get; set; }
        public int TestUserInt2 { get; set; }
        public int TestUserInt3 { get; set; }
        public int TestUserInt4 { get; set; }
        public int TestUserInt5 { get; set; }

        public UserToRole User_UserToRole { get; set; }
    }

    public class Role
    {
        public string TestRoleString1 { get; set; }
        public string TestRoleString2 { get; set; }
        public string TestRoleString3 { get; set; }
        public string TestRoleString4 { get; set; }
        public string TestRoleString5 { get; set; }

        public DateTime TestRoleDateTime1 { get; set; }
        public DateTime TestRoleDateTime2 { get; set; }
        public DateTime TestRoleDateTime3 { get; set; }
        public DateTime TestRoleDateTime4 { get; set; }
        public DateTime TestRoleDateTime5 { get; set; }

        public int TestRoleInt1 { get; set; }
        public int TestRoleInt2 { get; set; }
        public int TestRoleInt3 { get; set; }
        public int TestRoleInt4 { get; set; }
        public int TestRoleInt5 { get; set; }

        public IList<UserToRole> Role_UserToRoleList { get; set; }

        public Profile Role_Profile { get; set; }
    }

    public class Unit
    {
        public string TestUnitString1 { get; set; }
        public string TestUnitString2 { get; set; }
        public string TestUnitString3 { get; set; }
        public string TestUnitString4 { get; set; }
        public string TestUnitString5 { get; set; }

        public DateTime TestUnitDateTime1 { get; set; }
        public DateTime TestUnitDateTime2 { get; set; }
        public DateTime TestUnitDateTime3 { get; set; }
        public DateTime TestUnitDateTime4 { get; set; }
        public DateTime TestUnitDateTime5 { get; set; }

        public int TestUnitInt1 { get; set; }
        public int TestUnitInt2 { get; set; }
        public int TestUnitInt3 { get; set; }
        public int TestUnitInt4 { get; set; }
        public int TestUnitInt5 { get; set; }

        [VisitorHierarchy]
        public Unit Unit_ParentUnit { get; set; }

        public IList<Role> Unit_Roles { get; set; }
    }

    public class Profile
    {
        public string TestProfileString1 { get; set; }
        public string TestProfileString2 { get; set; }
        public string TestProfileString3 { get; set; }
        public string TestProfileString4 { get; set; }
        public string TestProfileString5 { get; set; }

        public DateTime TestProfileDateTime1 { get; set; }
        public DateTime TestProfileDateTime2 { get; set; }
        public DateTime TestProfileDateTime3 { get; set; }
        public DateTime TestProfileDateTime4 { get; set; }
        public DateTime TestProfileDateTime5 { get; set; }

        public int TestProfileInt1 { get; set; }
        public int TestProfileInt2 { get; set; }
        public int TestProfileInt3 { get; set; }
        public int TestProfileInt4 { get; set; }
        public int TestProfileInt5 { get; set; }

        public IList<OperationToProfile> Profile_OperationsList { get; set; }
    }

    public class UserToRole
    {
        public User UserToRole_User { get; set; }
        public Role UserToRole_Role { get; set; }
    }

    public class RoleToProfile
    {
        public Role UserToRole_Role { get; set; }
        public Profile UserToRole_Profile { get; set; }
    }

    public class Operation
    {
        public string TestOperationString1 { get; set; }
        public string TestOperationString2 { get; set; }
        public string TestOperationString3 { get; set; }
        public string TestOperationString4 { get; set; }
        public string TestOperationString5 { get; set; }

        public DateTime TestOperationDateTime1 { get; set; }
        public DateTime TestOperationDateTime2 { get; set; }
        public DateTime TestOperationDateTime3 { get; set; }
        public DateTime TestOperationDateTime4 { get; set; }
        public DateTime TestOperationDateTime5 { get; set; }

        public int TestOperationInt1 { get; set; }
        public int TestOperationInt2 { get; set; }
        public int TestOperationInt3 { get; set; }
        public int TestOperationInt4 { get; set; }
        public int TestOperationInt5 { get; set; }
    }

    public class OperationToProfile
    {
        public Operation OperationToProfile_Operation { get; set; }
        public Profile OperationToProfile_Profile { get; set; }
    }

    public class AllowedHierarchy
    {
        public DateTime DateTime1 { get; set; }

        [VisitorHierarchy]
        public AllowedHierarchy Child { get; set; }
    }

    public class SuppressedHierarchy
    {
        public DateTime DateTime1 { get; set; }

        //[VisitorHierarchy]
        public SuppressedHierarchy Child { get; set; }
    }
}
