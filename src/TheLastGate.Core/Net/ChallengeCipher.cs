using System;
using System.Buffers.Binary;

namespace TheLastGate.Net;

public static class ChallengeCipher
{
    private const uint ResultMask = 0x5A7CE52E;

    private static ReadOnlySpan<byte> Secret =>
        "Ifhjf64hH8sa,-#39ddj843tvxcv0434dvsdc40G#34Trefc349534Y5#34trecerr943"u8
        + "5#erZt#eA534#5erFtw#Trwec,9345mwrxm gerte-534lMIZDN(/dn8sfn8&DBDB/D&s"u8
        + "8efnsd897)DDzD'D'D''Dofs,t0943-rg-gdfg-gdf.t,e95.34u.5retfrh.wretv.56"u8
        + "9v4#asf.59m(D)/ND/DDLD;gd+dsa,fw9r,x  OD(98snfsfa"u8;

    public static uint Encrypt(uint challenge)
    {
        Span<byte> bytes = stackalloc byte[sizeof(uint)];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, challenge);
        foreach (ref var value in bytes)
        {
            value = Secret[value];
        }

        return BinaryPrimitives.ReadUInt32LittleEndian(bytes) ^ ResultMask;
    }
}
