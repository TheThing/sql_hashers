namespace SafeArgon2
{
    internal class Argon2idCore : Argon2iCore
    {
        private const uint ARGON2_SYNC_POINTS = 4;

        public override int Type => 2;
        
        public Argon2idCore(int hashSize) : base(hashSize) {}

        internal override IArgon2PseudoRands GenerateState(Argon2Lane[] lanes, int segmentLength, int pass, int lane, int slice)
        {
            if ((pass == 0) && (slice < (ARGON2_SYNC_POINTS / 2)))
            {
                return base.GenerateState(lanes, segmentLength, pass, lane, slice);
            }

            return new Argon2dCore.PseudoRands(lanes);
        }
    }
}
