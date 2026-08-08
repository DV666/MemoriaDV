using Memoria.Data;
using System;
using System.Collections.Generic;

namespace Memoria.Scripts.TranceSeek
{
    public static class TranceSeekMessages
    {
        public static readonly Dictionary<BattleStatus, Dictionary<string, string>> ProtectMessages = new Dictionary<BattleStatus, Dictionary<string, string>>();

        public static void InitProtectMessages()
        {
            foreach (BattleStatus status in Enum.GetValues(typeof(BattleStatus))) // Don't work with customstatus... maybe later, with a Memoria update ? (or made it here)
            {
                string message = $"-{status}";
                ProtectMessages[status] = new Dictionary<string, string>
                {
                    { "US", message }, { "UK", message }, { "JP", message },
                    { "ES", message }, { "FR", message }, { "GR", message }, { "IT", message }
                };
            }
        }

        public static readonly Dictionary<String, String> MessageNope = new Dictionary<String, String>
        {
            { "US", "It's a NO!" }, { "UK", "It's a NO!" }, { "JP", "ダメだ！" },
            { "ES", "¡Es un NO!" }, { "FR", "C'est non !" }, { "DE", "Das ist ein NEIN!" }, { "IT", "È un NO!" }
        };

        public static readonly Dictionary<String, String> MessageFerocity = new Dictionary<String, String>
        {
            { "US", "Ferocity!" }, { "UK", "Ferocity!" }, { "JP", "凶暴！" },
            { "ES", "¡Ferocidad!" }, { "FR", "Férocité !" }, { "DE", "Ferozität!" }, { "IT", "Ferocia!" }
        };

        public static readonly Dictionary<String, String> MessagePeuh = new Dictionary<String, String>
        {
            { "US", "--Hmph!" }, { "UK", "--Hmph!" }, { "JP", "--ふん！" },
            { "ES", "--¡Bah!" }, { "FR", "--Peuh !" }, { "DE", "--Pah!" }, { "IT", "--Pah!" }
        };

        public static readonly Dictionary<String, String> MessageEmergencyPlan = new Dictionary<String, String>
        {
            { "US", "Emergency Plan!" }, { "UK", "Emergency Plan!" }, { "JP", "緊急時対策!" },
            { "ES", "¡Plan de emergencia!" }, { "FR", "Plan d'urgence !" }, { "GR", "Notfallplan!" }, { "IT", "Piano di emergenza!" }
        };

        public static readonly Dictionary<String, String> MessagePreserved = new Dictionary<String, String>
        {
            { "US", "Preserved!" }, { "UK", "Preserved!" }, { "JP", "プリザーブド！" },
            { "ES", "¡Conservado!" }, { "FR", "Conservée !" }, { "GR", "Erhalten!" }, { "IT", "Conservata!" }
        };

        public static readonly Dictionary<String, String> MessageZidaneCritical = new Dictionary<String, String>
        {
            { "US", "↑ Critical ↑" }, { "UK", "↑ Critical ↑" }, { "JP", "↑ Critical ↑" },
            { "ES", "↑ Letal ↑" }, { "FR", "↑ Critique ↑" }, { "GR", "↑ KRITISCH ↑" }, { "IT", "↑ Letale ↑" }
        };

        public static readonly Dictionary<String, String> MessageInstinct = new Dictionary<String, String>
        {
            { "US", "Instinct!" }, { "UK", "Instinct!" }, { "JP", "直感！" },
            { "ES", "¡Instinto!" }, { "FR", "Instinct !" }, { "GR", "Instinkt!" }, { "IT", "Istinto!" }
        };

        public static readonly Dictionary<String, String> MessageZidaneDodge = new Dictionary<String, String>
        {
            { "US", "↑ Dodge ↑" }, { "UK", "↑ Dodge ↑" }, { "JP", "↑ かいひりつ ↑" },
            { "ES", "↑ DST fisica ↑" }, { "FR", "↑ Esquive ↑" }, { "GR", "↑ Evasión F ↑" }, { "IT", "↑ Reflex ↑" }
        };

        public static readonly Dictionary<String, String> MessageImmune = new Dictionary<String, String>
        {
            { "US", "Immune!" }, { "UK", "Immune!" }, { "JP", "免疫！" },
            { "ES", "¡Inmune!" }, { "FR", "Immunisé !" }, { "GR", "Immun!" }, { "IT", "Immunità!" }
        };

        public static readonly Dictionary<String, String> MessageLastStand = new Dictionary<String, String>
        {
            { "US", "Last Stand!" }, { "UK", "Last Stand!" }, { "JP", "背水の陣！" },
            { "ES", "¡Resistencia final!" }, { "FR", "Échappée belle !" }, { "GR", "Letzter Widerstand!" }, { "IT", "Ultima resistenza!" }
        };

        public static readonly Dictionary<String, String> MessageBodyguard = new Dictionary<String, String>
        {
            { "US", "Bodyguard!" }, { "UK", "Bodyguard!" }, { "JP", "用心棒！" },
            { "ES", "¡Guardaespaldas!" }, { "FR", "Garde du corps !" }, { "GR", "Leibwächter!" }, { "IT", "Guardia del corpo!" }
        };

        public static readonly Dictionary<String, String> MessageAutoLife = new Dictionary<String, String>
        {
            { "US", "Auto-Life!" }, { "UK", "Auto-Life!" }, { "JP", "リレイズ!" },
            { "ES", "¡AutoLázaro!" }, { "FR", "Auréole !" }, { "GR", "Reinkarnat!" }, { "IT", "Risveglio!" }
        };

        public static readonly Dictionary<String, String> MessageFocusViviPlus = new Dictionary<String, String>
        {
            { "US", "Focus +{0}%!" }, { "UK", "Focus +{0}%!" }, { "JP", "フォーカス +{0}%!" },
            { "ES", "¡Focus +{0}%!" }, { "FR", "Focus +{0}% !" }, { "GR", "Focus +{0}%!" }, { "IT", "Focus +{0}%!" }
        };

        public static readonly Dictionary<String, String> MessageFocusViviLost = new Dictionary<String, String>
        {
            { "US", "- Focus!" }, { "UK", "- Focus!" }, { "JP", "- フォーカス!" },
            { "ES", "¡- Focus!" }, { "FR", "- Focus !" }, { "GR", "- Focus!" }, { "IT", "- Focus!" }
        };

        public static readonly Dictionary<String, String> MessagePlutoStack = new Dictionary<String, String>
        {
            { "US", "[SPRT=IconAtlas,item200_00] Pluto!" }, { "UK", "[SPRT=IconAtlas,item200_00] Pluto!" }, { "JP", "[SPRT=IconAtlas,item200_00] プルート！" },
            { "ES", "[SPRT=IconAtlas,item200_00] ¡Pluto!" }, { "FR", "[SPRT=IconAtlas,item200_00] Brutos !" }, { "GR", "[SPRT=IconAtlas,item200_00] Pluto!" }, { "IT", "[SPRT=IconAtlas,item200_00] Plutò!" }
        };

        public static readonly Dictionary<String, String> MessageRedemptionStack = new Dictionary<String, String>
        {
            { "US", "[SPRT=IconAtlas,item200_01] Redemption!" }, { "UK", "[SPRT=IconAtlas,item200_01] Redemption!" }, { "JP", "[SPRT=IconAtlas,item200_01] 贖罪！" },
            { "ES", "[SPRT=IconAtlas,item200_01] ¡Redención!" }, { "FR", "[SPRT=IconAtlas,item200_01] Rédemption !" }, { "GR", "[SPRT=IconAtlas,item200_01] Erlösung!" }, { "IT", "[SPRT=IconAtlas,item200_01] Redenzione!" }
        };
    }
}
