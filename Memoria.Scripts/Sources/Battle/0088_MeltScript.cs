using FF9;
using System;
using Memoria.Data;
using static Memoria.Scripts.Battle.TranceSeekAPI;

namespace Memoria.Scripts.Battle
{
    /// <summary>
    /// Melt, Blowup
    /// </summary>
    [BattleScript(Id)]
    public sealed class MeltScript : IBattleScript
    {
        public const Int32 Id = 0088;

        private readonly BattleCalculator _v;

        public MeltScript(BattleCalculator v)
        {
            _v = v;
        }

        public void Perform()
        {
            if (_v.Command.AbilityStatus == BattleStatus.AutoLife)
            {
                _v.Target.RemoveStatus(BattleStatus.AutoLife);
                TranceSeekAPI.SpecialSAEffect[_v.Target.Data][1] = 0;
            }
            _v.Target.Flags |= CalcFlag.HpAlteration;
            if (_v.Command.Power == 99 & _v.Command.HitRate == 99) // Explosion to make a game over.
            {
                _v.Target.RemoveStatus(BattleStatus.AutoLife | TranceSeekStatus.MechanicalArmor);
                _v.Target.HpDamage = 9999;
            }
            else
            {
                
                if (_v.Command.Power == 1 & _v.Command.HitRate == 1) // King Of Pop (Superboss CD3)
                {
                    uint HPZombDance1 = 0;
                    uint HPZombDance2 = 0;
                    _v.Target.Flags |= CalcFlag.HpRecovery;
                    foreach (BattleUnit monster in BattleState.EnumerateUnits())
                    {
                        if (!monster.IsPlayer)
                        {
                            if (monster.Data.btl_id == 16)
                            {
                                HPZombDance1 = monster.CurrentHp - 10000 == 0 ? 1 : monster.CurrentHp - 10000;
                            }
                            else if (monster.Data.btl_id == 64)
                            {
                                HPZombDance2 = monster.CurrentHp - 10000 == 0 ? 1 : monster.CurrentHp - 10000;
                            }
                        }
                    }
                    foreach (BattleUnit monster in BattleState.EnumerateUnits())
                    {
                        if (!monster.IsPlayer)
                        {
                            if (monster.Data.btl_id != 128)
                            {
                                if (monster.CurrentHp != 0)
                                {
                                    monster.AddDelayedModifier(
                                        null,
                                        monster =>
                                        {
                                            if (!BattleState.IsBattleStateEnabled || monster.CurrentHp == 0)
                                                return;
                                            monster.Kill();
                                        }
                                    );
                                }
                            } 
                            else
                            {
                                monster.Data.bi.target = 1;
                                btl_mot.ShowMesh(monster.Data, 65535, false);
                                _v.Target.HpDamage = (int)(HPZombDance1 + HPZombDance2) / 10;
                                monster.Strength = (byte)(monster.Strength + (HPZombDance1 + HPZombDance2) / 1000);
                                monster.Magic = (byte)(monster.Magic + (HPZombDance1 + HPZombDance2) / 1000);
                                monster.Will = (byte)(monster.Will + (HPZombDance1 + HPZombDance2) / 1000);
                                monster.PhysicalDefence = (byte)(monster.PhysicalDefence + (HPZombDance1 + HPZombDance2) / 1000);
                                monster.PhysicalEvade = (byte)(monster.PhysicalEvade + (HPZombDance1 + HPZombDance2) / 1000);
                                monster.MagicDefence = (byte)(monster.MagicDefence + (HPZombDance1 + HPZombDance2) / 1000);
                                monster.MagicEvade = (byte)(monster.MagicEvade + (HPZombDance1 + HPZombDance2) / 1000);                                
                            }
                        }
                    }
                    return;
                }
                _v.Target.HpDamage = (Int32)_v.Caster.CurrentHp;
            }

            if (_v.Caster.CurrentHp != 0)
            {
                _v.Caster.AddDelayedModifier(
                    null,
                    caster =>
                    {
                        if (!BattleState.IsBattleStateEnabled || caster.CurrentHp == 0)
                            return;
                        caster.Kill();
                    }
                );
            }
        }
    }
}
