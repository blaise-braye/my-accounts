using System.Collections.Generic;
using System.Linq;
using FastMember;
using Newtonsoft.Json;

namespace MyAccounts.Business.Extensions
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, string> ToRawMembersDictionary(this object obj)
        {
            var accessor = TypeAccessor.Create(obj.GetType());
            var members = accessor.GetMembers();
            var keyvalues = members.ToDictionary(m => m.Name, m => JsonConvert.ToString(accessor[obj, m.Name]));
            return keyvalues;
        }

        public static void SetFromRawMembersDictionary(this object obj, IDictionary<string, string> keyvalues)
        {
            var typeAccessor = TypeAccessor.Create(obj.GetType());

            foreach (var member in typeAccessor.GetMembers())
            {
                if (keyvalues.ContainsKey(member.Name))
                {
                    var @override = keyvalues[member.Name];

                    typeAccessor[obj, member.Name] = JsonConvert.DeserializeObject(@override, member.Type);
                }
            }
        }
    }
}