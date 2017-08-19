using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MyAccounts.Business.AccountOperations.Unified
{
    public class UnifiedAccountOperationMapping
    {
        private Regex _compiledOperationIdPattern;
        private string _operationIdPattern;

        public string OperationIdPattern
        {
            get => _operationIdPattern;
            set
            {
                if (_operationIdPattern != value)
                {
                    _operationIdPattern = value;
                    _compiledOperationIdPattern = null;
                }
            }
        }

        public List<UnifiedAccountOperationPatternMapping> PatternMappings { get; set; }

        [JsonIgnore]
        public Regex CompiledOperationIdPattern
        {
            get
            {
                if (_compiledOperationIdPattern == null)
                {
                    var pattern = string.IsNullOrEmpty(OperationIdPattern) ? ".*" : OperationIdPattern;
                    _compiledOperationIdPattern = new Regex(pattern, RegexOptions.Compiled);
                }

                return _compiledOperationIdPattern;
            }
        }
    }
}