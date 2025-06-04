using System;
using System.Collections.Generic;
using Memoria.Data;
using Memoria.Prime;
using Object = System.Object;

namespace Memoria.DefaultScripts
{
    [StatusScript(BattleStatusId.CustomStatus21)] // Special
    public class SpecialStatusScript : StatusScriptBase
    {
        public Int32 Secretingredient;
        public Int32 MasterofAlchemy;
        public Int32 SoakedBlade;
        public Int32 CursedBlood;
        public Int32 LifeorDeath;
        public Int32 Propagation;
        public Int32 Flexible;
        public Int32 Duelist;
        public Int32 CanCover;
        

        public override UInt32 Apply(BattleUnit target, BattleUnit inflicter, params Object[] parameters)
        {
            base.Apply(target, inflicter, parameters);
            if (parameters.Length > 0)
            {
                String Parameter = parameters[0] as String;
                if (Parameter == "Secretingredient++")
                {
                    Secretingredient++;
                    Dictionary<String, String> localizedStatusProtect = new Dictionary<String, String>
                    {
                        { "US", $"Secret ingredient x {Secretingredient} !" },
                        { "UK", $"Secret ingredient x {Secretingredient}" },
                        { "JP", $"Secret ingredient x {Secretingredient}" },
                        { "ES", $"Secret ingredient x {Secretingredient}" },
                        { "FR", $"Ingrédient secret x {Secretingredient}" },
                        { "GR", $"Secret ingredient x {Secretingredient}" },
                        { "IT", $"Secret ingredient x {Secretingredient}" },
                    };
                    btl2d.Btl2dReqSymbolMessage(target.Data, "[25BB00]", localizedStatusProtect, HUDMessage.MessageStyle.DAMAGE, 5);
                }
                else if (Parameter == "Secretingredient--")
                {
                    if (Secretingredient > 0)
                    Secretingredient--;
                }
                else if (Parameter == "CursedBlood")
                    CursedBlood = 1;
                else if (Parameter == "SoakedBlade" && parameters[1] != null)
                    SoakedBlade = (Int32)parameters[1];
                else if (Parameter == "MasterofAlchemy")
                    MasterofAlchemy = 1;
                else if (Parameter == "LifeorDeath++")
                    LifeorDeath = 1;
                else if (Parameter == "LifeorDeath--")
                    LifeorDeath = 0;
                else if (Parameter == "Propagation2")
                    Propagation = 2;
                else if (Parameter == "Propagation1")
                    Propagation = 1;
                else if (Parameter == "Propagation--")
                    Propagation = 0;
                else if (Parameter == "Flexible2")
                    Flexible = 2;
                else if (Parameter == "Flexible1")
                    Flexible = 1;
                else if (Parameter == "Flexible0")
                    Flexible = 0;
                else if (Parameter == "Duelist++")
                    Duelist++;
                else if (Parameter == "Duelist--")
                    Duelist = 0;
                else if (Parameter == "Duelist--")
                    Duelist = 0;
                else if (Parameter == "CanCover0")
                    CanCover = 0;
                else if (Parameter == "CanCover1")
                    CanCover = 1;
            }
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }
    }
}
