namespace MyAccounts.Tests.Gmc.Steps
{
    public class Wrapper<T>
    {
        public Wrapper(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public static implicit operator Wrapper<T>(T val)
        {
            return new Wrapper<T>(val);
        }

        public static implicit operator T(Wrapper<T> wrapper)
        {
            return wrapper.Value;
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
}