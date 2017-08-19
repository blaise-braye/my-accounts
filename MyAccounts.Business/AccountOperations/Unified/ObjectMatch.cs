using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MyAccounts.Business.AccountOperations.Unified
{
    public class ObjectMatch
    {
        public ObjectMatch()
        {
            Groups = new Dictionary<string, Group>();
        }

        public bool Success { get; set; }

        public IDictionary<string, Group> Groups { get; }
    }
}