using System;
using System.Reflection;

namespace Memoria.Scripts.TranceSeek
{
    public static class TranceSeekCanaryFix
    {
        private static readonly FieldInfo SlotNoField = typeof(BTL_INFO).GetField("slot_no");

        public static int GetSlotNo(this BTL_DATA btl)
        {
            if (btl == null || btl.bi == null || SlotNoField == null)
                return 0;

            return Convert.ToInt32(SlotNoField.GetValue(btl.bi));
        }

        public static int GetSlotNo(this BattleUnit unit)
        {
            return unit?.Data != null ? unit.Data.GetSlotNo() : 0;
        }
    }
}
