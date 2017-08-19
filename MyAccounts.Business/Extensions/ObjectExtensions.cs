using System;
using System.Collections.Generic;
using System.Linq;
using FastMember;

namespace MyAccounts.Business.Extensions
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, string> ToRawMembersDictionary(this object obj)
        {
            var accessor = TypeAccessor.Create(obj.GetType());
            var members = accessor.GetMembers();
            var keyvalues = members.ToDictionary(m => m.Name, m => Convert.ToString(accessor[obj, m.Name]));
            return keyvalues;
        }
    }
}