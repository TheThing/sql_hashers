namespace SafeArgon2
{
    public class Argon2id : Argon2
    {
        public Argon2id(byte[] password) : base(password) {}

        internal override Argon2Core BuildCore(int bc)
        {
            return new Argon2idCore(bc);
        }
    }
}
