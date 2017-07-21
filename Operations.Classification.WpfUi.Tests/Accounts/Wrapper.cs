namespace Operations.Classification.WpfUi.Tests.Accounts
{
    /// <summary>
    /// this wrapper is used as a trick to fool the test explorer.
    /// Without this mecanism, some tests could be written with the autofixture style
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
        
        public static implicit operator T(Wrapper<T> wrapper)
        {
            return wrapper.Value;
        }
    }
}