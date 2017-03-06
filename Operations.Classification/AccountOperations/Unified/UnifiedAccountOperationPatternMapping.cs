using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using Operations.Classification.AccountOperations.Contracts;

namespace Operations.Classification.AccountOperations.Unified
{
    public class UnifiedAccountOperationPatternMapping
    {
        private string _expression;

        private Regex _compiledExpression;

        public string Name { get; set; }

        public SourceKind SourceKind { get; set; }

        public string Expression
        {
            get
            {
                return _expression;
            }

            set
            {
                if (!Equals(_expression, value))
                {
                    _expression = value;
                    _compiledExpression = null;
                }
            }
        }

        public Regex CompiledExpression
        {
            get
            {
                if (_compiledExpression == null)
                {
                    _compiledExpression = new Regex(_expression ?? string.Empty, RegexOptions.Compiled);
                }
                
                return _compiledExpression;
            }
        }

        public JObject StaticValues { get; set; }
    }
}