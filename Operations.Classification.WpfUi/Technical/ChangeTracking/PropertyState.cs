using System;
using System.Diagnostics;

namespace Operations.Classification.WpfUi.Technical.ChangeTracking
{
    [DebuggerDisplay("[IsDirty = {" + nameof(IsDirty) + "}] [IsMixed = {" + nameof(IsMixed) + "}] {" + nameof(Value) + "}", Name = "{Name}")]
    public class PropertyState
    {
        public PropertyState(string name, object originalValue, Type type)
        {
            Name = name;
            OriginalValue = originalValue;
            Type = type;
            Value = originalValue;
        }

        public string Name { get; }

        public Type Type { get; }

        public object Value { get; set; }

        public object OriginalValue { get; private set; }

        public bool IsDirty => !IsMixed && !Equals(OriginalValue, Value);

        public bool IsMixed { get; set; }

        public void CommitChanges()
        {
            OriginalValue = Value;
        }
    }
}
