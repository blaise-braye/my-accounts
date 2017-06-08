﻿using System;
using System.Collections.Generic;
using System.Linq;
using FastMember;

namespace Operations.Classification.Extensions
{
    internal static class ObjectExtensions
    {
        public static IDictionary<string, object> ToMembersDictionary(this object obj)
        {
            var accessor = TypeAccessor.Create(obj.GetType());
            var members = accessor.GetMembers();
            var keyvalues = members.ToDictionary(m => m.Name, m => accessor[obj, m.Name]);
            return keyvalues;
        }

        public static IDictionary<string, string> ToRawMembersDictionary(this object obj)
        {
            var accessor = TypeAccessor.Create(obj.GetType());
            var members = accessor.GetMembers();
            var keyvalues = members.ToDictionary(m => m.Name, m => Convert.ToString(accessor[obj, m.Name]));
            return keyvalues;
        }
    }
}