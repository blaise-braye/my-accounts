using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using FastMember;

using Newtonsoft.Json.Linq;

namespace Operations.Classification.AccountOperations.Unified
{
    public class ObjectExpression
    {
        private readonly IList<CompositeRegexEntry> _entries;

        private readonly string[] _groupNames;

        private ObjectExpression(IList<CompositeRegexEntry> entries)
        {
            _entries = entries;
            _groupNames = _entries.SelectMany(e => e.Expression.GetGroupNames())
                .Where(e => !e.Equals("0"))
                .Distinct()
                .ToArray();
        }

        public static ObjectExpression Parse(JObject compositeExp)
        {
            List<CompositeRegexEntry> entries = new List<CompositeRegexEntry>();
            
            foreach (var property in compositeExp.Properties())
            {
                var entry = new CompositeRegexEntry
                {
                    SourceProperty = property.Name,
                    Expression = new Regex(property.Value.ToString(), RegexOptions.Compiled)
                };
                
                entries.Add(entry);
            }

            return new ObjectExpression(entries);
        }

        public string[] GetGroupNames()
        {
            return _groupNames;
        }

        public ObjectMatch Match(object @object)
        {
            var typeAccessor = TypeAccessor.Create(@object.GetType());

            var result = new ObjectMatch();

            result.Success = true;
            foreach (var entry in _entries)
            {
                var input = typeAccessor[@object, entry.SourceProperty]?.ToString() ?? string.Empty;
                var match = entry.Expression.Match(input);

                result.Success = result.Success && match.Success;

                foreach (var groupName in entry.Expression.GetGroupNames())
                {
                    if (groupName.Equals("0"))
                    {
                        continue;
                    }

                    result.Groups.Add(groupName, match.Groups[groupName]);
                }
            }

            return result;
        }

        private class CompositeRegexEntry
        {
            public string SourceProperty { get; set; }
            
            public Regex Expression { get; set; }
        }
    }
}