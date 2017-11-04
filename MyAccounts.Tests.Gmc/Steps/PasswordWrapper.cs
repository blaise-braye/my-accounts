namespace MyAccounts.Tests.Gmc.Steps
{
    public class PasswordWrapper : Wrapper<string>
    {
        public PasswordWrapper(string value)
            : base(value)
        {
        }

        public static implicit operator PasswordWrapper(string val)
        {
            return new PasswordWrapper(val);
        }

        public static implicit operator string(PasswordWrapper wrapper)
        {
            return wrapper.Value;
        }

        public override string ToString()
        {
            return "*********";
        }
    }
}