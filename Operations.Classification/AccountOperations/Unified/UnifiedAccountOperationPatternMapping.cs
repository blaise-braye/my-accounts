using Newtonsoft.Json.Linq;
using Operations.Classification.AccountOperations.Contracts;

namespace Operations.Classification.AccountOperations.Unified
{
    public class UnifiedAccountOperationPatternMapping
    {
        private ObjectExpression _compiledExpression;
        private JObject _expression;

        public string Name { get; set; }

        public SourceKind SourceKind { get; set; }

        public JObject Expression
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

        public ObjectExpression CompiledExpression
        {
            get
            {
                if (_compiledExpression == null)
                {
                    _compiledExpression = ObjectExpression.Parse(_expression);
                }

                return _compiledExpression;
            }
        }

        public JObject ExplicitMappings { get; set; }
    }
}