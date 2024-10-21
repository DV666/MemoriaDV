using System;
using Memoria.Data;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus16)] // Vieillissement
    public class VieillissementStatusScript : StatusScriptBase
    {
        public Int32 BasicLevel;
        public Int32 BasicStrength;
        public Int32 BasicMagic;
        public Int32 BasicDexterity;
        public Int32 BasicWill;
        public Int32 BasicPhysicalDefence;
        public Int32 BasicPhysicalEvade;
        public Int32 BasicMagicDefence;
        public Int32 BasicMagicEvade;
        public Boolean Init;

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            if (target.IsUnderAnyStatus(BattleStatus.EasyKill))
                return btl_stat.ALTER_INVALID;

            base.Apply(target, inflicter, parameters);
            if (!Init)
            {
                BasicLevel = Target.Level;
                BasicStrength = Target.Strength;
                BasicMagic = Target.Magic;
                BasicDexterity = Target.Dexterity;
                BasicWill = Target.Will;
                BasicPhysicalDefence = Target.PhysicalDefence;
                BasicPhysicalEvade = Target.PhysicalEvade;
                BasicMagicDefence = Target.MagicDefence;
                BasicMagicEvade = Target.MagicEvade;
                Init = true;
            }    

            BasicStrength = Target.Strength;
            target.Level = (byte)Math.Max(1, BasicLevel - (BasicLevel * 9) / 10);
            target.Strength = (byte)Math.Max(1, BasicStrength - (BasicStrength * 9) / 10);
            target.Magic = (byte)Math.Max(1, BasicMagic - (BasicMagic * 9) / 10);
            target.Dexterity = (byte)Math.Max(1, BasicDexterity - (BasicDexterity * 9) / 10);
            target.Will = (byte)Math.Max(1, BasicWill - (BasicWill * 9) / 10);
            target.PhysicalDefence = (byte)Math.Max(1, BasicPhysicalDefence - (BasicPhysicalDefence * 9) / 10);
            target.PhysicalEvade = (byte)Math.Max(1, BasicPhysicalEvade - (BasicPhysicalEvade * 9) / 10);
            target.MagicDefence = (byte)Math.Max(1, BasicMagicDefence - (BasicMagicDefence * 9) / 10);
            target.MagicEvade = (byte)Math.Max(1, BasicMagicEvade - (BasicMagicEvade * 9) / 10);
            return btl_stat.ALTER_SUCCESS;            
        }

        public override Boolean Remove()
        {
            Target.Level = (byte)BasicLevel;
            Target.Strength = (byte)BasicStrength;
            Target.Magic = (byte)BasicMagic;
            Target.Dexterity = (byte)BasicDexterity;
            Target.Will = (byte)BasicWill;
            Target.PhysicalDefence = (byte)BasicPhysicalDefence;
            Target.PhysicalEvade = (byte)BasicPhysicalEvade;
            Target.MagicDefence = (byte)BasicMagicDefence;
            Target.MagicEvade = (byte)BasicMagicEvade;
            Init = false;
            return true;
        }
    }
}
