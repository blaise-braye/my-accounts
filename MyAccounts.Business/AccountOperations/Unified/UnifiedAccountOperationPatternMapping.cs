﻿using MyAccounts.Business.AccountOperations.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyAccounts.Business.AccountOperations.Unified
{
    public class UnifiedAccountOperationPatternMapping
    {
        private ObjectExpression _compiledExpression;
        private JObject _expression;

        public string Name { get; set; }

        public SourceKind SourceKind { get; set; }

        public JObject Expression
        {
            get => _expression;

            set
            {
                if (!Equals(_expression, value))
                {
                    _expression = value;
                    _compiledExpression = null;
                }
            }
        }

        [JsonIgnore]
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