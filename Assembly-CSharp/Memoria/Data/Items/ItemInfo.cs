using Memoria.Prime.CSV;
using System;
using System.Collections.Generic;

namespace Memoria.Data
{
    public class ItemInfo : ICsvEntry
    {
        public RegularItem Id;
        public UInt32 Price;
        public Int32 SellingPrice;
        public ItemCharacter CharacterMask;
        public Int32 GraphicsId;
        public Int32 ColorId;
        public Single Quality;
        public Int32 BonusId;
        public Int32[] AbilityIds;
        public ItemType TypeMask;
        public Single Order;
        public String UseCondition;
        public Int32 WeaponId;
        public Int32 ArmorId;
        public Int32 EffectId;

        public Boolean IsAppend;
        public List<String> AppendFields = new List<String>();

        public void ParseEntry(String[] raw, CsvMetaData metadata)
        {
            AppendFields.Clear();
            IsAppend = false;

            foreach (String line in metadata.GenerateLines())
            {
                String trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("AppendMode"))
                {
                    IsAppend = true;
                    AppendFields.AddRange(trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
                }
            }

            AppendFields.Remove("AppendMode");

            Int32 index = 0;
            Boolean hasAuxIds = metadata.HasOption("IncludeAuxiliaryIds");

            Id = metadata.HasOption($"Include{nameof(Id)}") ? (RegularItem)CsvParser.Int32(raw[index++]) : (RegularItem)(-1);

            if (!IsAppend)
            {
                WeaponId = hasAuxIds || metadata.HasOption($"Include{nameof(WeaponId)}") ? CsvParser.Int32(raw[index++]) : -1;
                ArmorId = hasAuxIds || metadata.HasOption($"Include{nameof(ArmorId)}") ? CsvParser.Int32(raw[index++]) : -1;
                EffectId = hasAuxIds || metadata.HasOption($"Include{nameof(EffectId)}") ? CsvParser.Int32(raw[index++]) : -1;

                Price = CsvParser.UInt32(raw[index++]);

                if (metadata.HasOption($"Include{nameof(SellingPrice)}"))
                    SellingPrice = CsvParser.Int32(raw[index++]);
                else
                    SellingPrice = (Int32)(Price / 2);

                GraphicsId = CsvParser.Int32(raw[index++]);
                ColorId = CsvParser.Int32(raw[index++]);
                Quality = CsvParser.Single(raw[index++]);
                BonusId = CsvParser.Int32(raw[index++]);
                AbilityIds = CsvParser.AnyAbilityArray(raw[index++]);

                Byte type = 0;
                for (Int32 i = 0; i < 8; i++)
                {
                    type <<= 1;
                    type |= CsvParser.Byte(raw[index++]);
                }
                TypeMask = (ItemType)type;

                Order = CsvParser.Single(raw[index++]);

                if (metadata.HasOption($"Include{nameof(UseCondition)}"))
                    UseCondition = CsvParser.String(raw[index++]);
                else
                    UseCondition = String.Empty;

                UInt64 equippable = 0;
                for (Int32 i = 0; i < 12; i++)
                {
                    equippable <<= 1;
                    equippable |= CsvParser.Byte(raw[index++]);
                }
                for (Int32 i = 12; index < raw.Length; i++)
                    if (CsvParser.Byte(raw[index++]) != 0)
                        equippable |= 1ul << i;

                CharacterMask = (ItemCharacter)equippable;
            }
            else
            {
                void TryParseField<T>(String fieldName, Func<String, T> parseFunc, Action<T> setAction, Boolean hasDefault = false, T defaultValue = default)
                {
                    if (AppendFields.Contains(fieldName))
                    {
                        String val = raw[index++];
                        if (String.IsNullOrEmpty(val))
                            AppendFields.Remove(fieldName);
                        else
                            setAction(parseFunc(val));
                    }
                    else if (hasDefault)
                    {
                        setAction(defaultValue);
                    }
                }

                TryParseField(nameof(WeaponId), CsvParser.Int32, v => WeaponId = v, true, -1);
                TryParseField(nameof(ArmorId), CsvParser.Int32, v => ArmorId = v, true, -1);
                TryParseField(nameof(EffectId), CsvParser.Int32, v => EffectId = v, true, -1);

                TryParseField(nameof(Price), CsvParser.UInt32, v => Price = v);
                TryParseField(nameof(SellingPrice), CsvParser.Int32, v => SellingPrice = v);

                TryParseField(nameof(GraphicsId), CsvParser.Int32, v => GraphicsId = v);
                TryParseField(nameof(ColorId), CsvParser.Int32, v => ColorId = v);
                TryParseField(nameof(Quality), CsvParser.Single, v => Quality = v);
                TryParseField(nameof(BonusId), CsvParser.Int32, v => BonusId = v);
                TryParseField(nameof(AbilityIds), CsvParser.AnyAbilityArray, v => AbilityIds = v);

                if (AppendFields.Contains(nameof(TypeMask)))
                {
                    if (String.IsNullOrEmpty(raw[index]))
                    {
                        AppendFields.Remove(nameof(TypeMask));
                        index += 8;
                    }
                    else
                    {
                        Byte type = 0;
                        for (Int32 i = 0; i < 8; i++) { type <<= 1; type |= CsvParser.Byte(raw[index++]); }
                        TypeMask = (ItemType)type;
                    }
                }

                TryParseField(nameof(Order), CsvParser.Single, v => Order = v);
                TryParseField(nameof(UseCondition), CsvParser.String, v => UseCondition = v);

                if (AppendFields.Contains(nameof(CharacterMask)))
                {
                    if (String.IsNullOrEmpty(raw[index]))
                    {
                        AppendFields.Remove(nameof(CharacterMask));
                        index += 12;
                        while (index < raw.Length)
                            index++;
                    }
                    else
                    {
                        UInt64 equippable = 0;
                        for (Int32 i = 0; i < 12; i++)
                        {
                            equippable <<= 1;
                            equippable |= CsvParser.Byte(raw[index++]);
                        }

                        for (Int32 i = 12; index < raw.Length; i++)
                            if (CsvParser.Byte(raw[index++]) != 0)
                                equippable |= 1ul << i;

                        CharacterMask = (ItemCharacter)equippable;
                    }
                }
            }
        }

        public void ApplyTo(FF9ITEM_DATA item)
        {
            if (AppendFields.Contains(nameof(Price)))
                item.price = Price;
            if (AppendFields.Contains(nameof(SellingPrice)))
                item.selling_price = SellingPrice;
            if (AppendFields.Contains(nameof(GraphicsId)))
                item.shape = (UInt16)GraphicsId;
            if (AppendFields.Contains(nameof(ColorId)))
                item.color = (Byte)ColorId;
            if (AppendFields.Contains(nameof(Quality)))
                item.eq_lv = (Byte)Quality;
            if (AppendFields.Contains(nameof(BonusId)))
                item.bonus = (Byte)BonusId;
            if (AppendFields.Contains(nameof(AbilityIds)))
                item.ability = AbilityIds;
            if (AppendFields.Contains(nameof(TypeMask)))
                item.type = TypeMask;
            if (AppendFields.Contains(nameof(Order)))
                item.sort = (UInt16)Order;
            if (AppendFields.Contains(nameof(UseCondition)))
                item.use_condition = UseCondition;
            if (AppendFields.Contains(nameof(CharacterMask)))
                item.equip = (UInt64)CharacterMask;

            if (AppendFields.Contains(nameof(WeaponId)) && WeaponId != -1)
                item.weapon_id = WeaponId;
            if (AppendFields.Contains(nameof(ArmorId)) && ArmorId != -1)
                item.armor_id = ArmorId;
            if (AppendFields.Contains(nameof(EffectId)) && EffectId != -1)
                item.effect_id = EffectId;
        }

        public void WriteEntry(CsvWriter writer, CsvMetaData metadata)
        {
            Boolean hasAuxIds = metadata.HasOption($"IncludeAuxiliaryIds");
            if (metadata.HasOption($"Include{nameof(Id)}"))
                writer.Int32((Int32)Id);
            if (hasAuxIds || metadata.HasOption($"Include{nameof(WeaponId)}"))
                writer.Int32(WeaponId);
            if (hasAuxIds || metadata.HasOption($"Include{nameof(ArmorId)}"))
                writer.Int32(ArmorId);
            if (hasAuxIds || metadata.HasOption($"Include{nameof(EffectId)}"))
                writer.Int32(EffectId);

            writer.UInt32(Price);
            if (metadata.HasOption($"Include{nameof(SellingPrice)}"))
                writer.Int32(SellingPrice);
            writer.Int32(GraphicsId);
            writer.Int32(ColorId);
            writer.Single(Quality);
            writer.Int32(BonusId);
            writer.AnyAbilityArray(AbilityIds);

            writer.Boolean(Weapon);
            writer.Boolean(Armlet);
            writer.Boolean(Helmet);
            writer.Boolean(Armor);
            writer.Boolean(Accessory);
            writer.Boolean(Item);
            writer.Boolean(Gem);
            writer.Boolean(Usable);

            writer.Single(Order);

            if (metadata.HasOption($"Include{nameof(UseCondition)}"))
                writer.String(UseCondition);

            writer.Boolean(Zidane);
            writer.Boolean(Vivi);
            writer.Boolean(Garnet);
            writer.Boolean(Steiner);
            writer.Boolean(Freya);
            writer.Boolean(Quina);
            writer.Boolean(Eiko);
            writer.Boolean(Amarant);
            writer.Boolean(Cinna);
            writer.Boolean(Marcus);
            writer.Boolean(Blank);
            writer.Boolean(Beatrix);
        }

        public FF9ITEM_DATA ToItemData()
        {
            return new FF9ITEM_DATA(Price, SellingPrice, (UInt64)CharacterMask, GraphicsId, ColorId, Quality, BonusId, AbilityIds, TypeMask, Order, UseCondition, WeaponId, ArmorId, EffectId);
        }

        public Boolean Weapon => (TypeMask & ItemType.Weapon) == ItemType.Weapon;
        public Boolean Armlet => (TypeMask & ItemType.Armlet) == ItemType.Armlet;
        public Boolean Helmet => (TypeMask & ItemType.Helmet) == ItemType.Helmet;
        public Boolean Armor => (TypeMask & ItemType.Armor) == ItemType.Armor;
        public Boolean Accessory => (TypeMask & ItemType.Accessory) == ItemType.Accessory;
        public Boolean Item => (TypeMask & ItemType.Item) == ItemType.Item;
        public Boolean Gem => (TypeMask & ItemType.Gem) == ItemType.Gem;
        public Boolean Usable => (TypeMask & ItemType.Usable) == ItemType.Usable;

        public Boolean Zidane => (CharacterMask & ItemCharacter.Zidane) == ItemCharacter.Zidane;
        public Boolean Vivi => (CharacterMask & ItemCharacter.Vivi) == ItemCharacter.Vivi;
        public Boolean Garnet => (CharacterMask & ItemCharacter.Garnet) == ItemCharacter.Garnet;
        public Boolean Steiner => (CharacterMask & ItemCharacter.Steiner) == ItemCharacter.Steiner;
        public Boolean Freya => (CharacterMask & ItemCharacter.Freya) == ItemCharacter.Freya;
        public Boolean Quina => (CharacterMask & ItemCharacter.Quina) == ItemCharacter.Quina;
        public Boolean Eiko => (CharacterMask & ItemCharacter.Eiko) == ItemCharacter.Eiko;
        public Boolean Amarant => (CharacterMask & ItemCharacter.Amarant) == ItemCharacter.Amarant;
        public Boolean Cinna => (CharacterMask & ItemCharacter.Cinna) == ItemCharacter.Cinna;
        public Boolean Marcus => (CharacterMask & ItemCharacter.Marcus) == ItemCharacter.Marcus;
        public Boolean Blank => (CharacterMask & ItemCharacter.Blank) == ItemCharacter.Blank;
        public Boolean Beatrix => (CharacterMask & ItemCharacter.Beatrix) == ItemCharacter.Beatrix;
    }

    [Flags]
    public enum ItemType : byte
    {
        Weapon = 128,
        Armlet = 64,
        Helmet = 32,
        Armor = 16,
        Accessory = 8,

        Item = 4,
        Gem = 2,
        Usable = 1,

        AnyEquipment = Weapon | Armlet | Helmet | Armor | Accessory,
        AnyItem = Item | Gem | Usable
    }

    [Flags]
    public enum ItemCharacter : UInt64
    {
        Zidane = 2048,
        Vivi = 1024,
        Garnet = 512,
        Steiner = 256,
        Freya = 128,
        Quina = 64,
        Eiko = 32,
        Amarant = 16,
        Cinna = 8,
        Marcus = 4,
        Blank = 2,
        Beatrix = 1
    }
}
