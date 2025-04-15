namespace SafeArgon2
{
    public interface IArgon2PseudoRands
    {
        ulong PseudoRand(int segment, int prevLane, int prevOffset);
    }
}
