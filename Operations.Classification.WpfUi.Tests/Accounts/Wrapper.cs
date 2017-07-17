namespace Operations.Classification.WpfUi.Tests.Accounts
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
            return typeof(T).ToString();
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