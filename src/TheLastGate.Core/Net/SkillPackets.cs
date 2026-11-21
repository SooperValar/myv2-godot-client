namespace TheLastGate.Net;

public static class SkillPackets
{
    public static byte[] Use(int skillNumber, int targetCharacter) =>
        ClientPackets.ThreeValues(ClientCommand.Skill, skillNumber, targetCharacter, 0);
}
