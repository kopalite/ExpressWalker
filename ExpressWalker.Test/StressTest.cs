using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ExpressWalker.Test
{
    [TestClass]
    public class StressTest
    {
        [TestMethod]
        public void TypeWalker_Stress_Test()
        {
            //Act

            var watch = new Stopwatch();
            watch.Start();
            var visitor = TypeWalker<Document>.Create().ForProperty<DateTime>((x, m) => DateTime.Now).Build(20, new PropertyGuard());
            visitor.Visit(new Document());
            watch.Stop();
            var time = watch.ElapsedMilliseconds;
        }
    }

    //15, 8300, 1800, allowed 3 cycles

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

        public Unit Unit_ParentUnit { get; set; }

        public IList<Role> Unit_Roles { get; set; }
    }

    public class Profile
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
}
