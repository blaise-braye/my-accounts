namespace Operations.Classification.Gmc.Tests.Steps
{
    public class PasswordWrapper : Wrapper<string>
    {
        public PasswordWrapper(string value) : base(value)
        {
        }

        public override string ToString()
        {
            return "*********";
        }

        public static implicit operator PasswordWrapper(string val)
        {
            return new PasswordWrapper(val);
        }

        public static implicit operator string(PasswordWrapper wrapper)
        {
            return wrapper.Value;
        }
    }
}