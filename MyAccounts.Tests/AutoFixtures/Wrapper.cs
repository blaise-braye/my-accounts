namespace MyAccounts.Tests.AutoFixtures
{
    /// <summary>
    /// this wrapper is used as a trick to fool the test explorer.
    /// Without this mecanism, some tests could be written with the autofixture style
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    public class Wrapper<T>
    {
        public Wrapper(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public static implicit operator T(Wrapper<T> wrapper)
        {
            return wrapper.Value;
        }

        public override string ToString()
        {
            return typeof(T).ToString();
        }
    }
}