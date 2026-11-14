using TheLastGate.Net;

namespace TheLastGate.Tests;

public class ChallengeCipherTests
{
    [Theory]
    [InlineData(0x00000000u, 0x1335AC67u)]
    [InlineData(0x03020100u, 0x30148367u)]
    [InlineData(0xFFFFFFFFu, 0x3B1D844Fu)]
    [InlineData(0x12345678u, 0x3E4F9106u)]
    [InlineData(0xDEADBEEFu, 0x1418CB17u)]
    public void Encrypt_GivesTheAnswerTheServerExpects(uint challenge, uint expected)
    {
        Assert.Equal(expected, ChallengeCipher.Encrypt(challenge));
    }
}
