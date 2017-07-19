namespace Operations.Classification.Gmc.IntegrationTests.Steps
{
    public class Wrapper<T>
    {
        public T Value { get; }

        public Wrapper(T value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }

        public static implicit operator Wrapper<T>(T val)
        {
            return new Wrapper<T>(val);
        }

        public static implicit operator T(Wrapper<T> wrapper)
        {
            return wrapper.Value;
        }
    }
}