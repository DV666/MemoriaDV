﻿using System;
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
                {
                    CursedBlood = 1;
                }
                else if (Parameter == "SoakedBlade" && parameters[1] != null)
                {
                    SoakedBlade = (Int32)parameters[1];
                }
                else if (Parameter == "MasterofAlchemy")
                {
                    MasterofAlchemy = 1;
                }
            }
            return btl_stat.ALTER_SUCCESS;
        }

        public override Boolean Remove()
        {
            return true;
        }
    }
}
