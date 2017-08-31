using System.Diagnostics;

namespace Operations.Classification.WpfUi.Technical.ChangeTracking
{
    [DebuggerDisplay("[IsDirty = {" + nameof(IsDirty) + "}] {" + nameof(Value) + "}", Name = "{Property}")]
    public class PropertyState
    {
        public PropertyState(string property, object originalValue)
        {
            Property = property;
            OriginalValue = originalValue;
            Value = originalValue;
        }

        public string Property { get; }

        public object Value { get; set; }

        public object OriginalValue { get; private set; }

        public bool IsDirty => !Equals(OriginalValue, Value);

        public void CommitChanges()
        {
            OriginalValue = Value;
        }
    }
}
