using System;

namespace Memoria.EchoS
{
    [Flags]
    public enum LineEntryFlag : uint
    {
        None = 0,
        PlayerSolo = 1,
        PlayerTeam = 2,
        EnemySolo = 4U,
        EnemyTeam = 8U,
        Self = 16U,
        Enemy = 32U,
        Ally = 64U,
        Single = 128U,
        Multi = 256U,
        Hit = 512U,
        Miss = 1024U,
        Dodge = 2048U,
        Crit = 4096U,
        Hp = 8192U,
        Mp = 16384U,
        FrontAttack = 32768U,
        Preemptive = 65536U,
        BackAttack = 131072U,
        FriendlyBattle = 262144U,
        NonFriendlyBattle = 524288U,
        Serious = 1048576U,
        Boss = 2097152U
    }
}
