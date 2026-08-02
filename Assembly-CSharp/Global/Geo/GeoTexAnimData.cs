using Memoria.Prime.CSV;
using System;
using UnityEngine;

namespace FF9
{
    public class GeoTexAnimData : ICsvEntry
    {
        public String ModelName;
        public Int32 Id;
        public Byte Flags;
        public Int16 Rate;
        public UInt16 RandMin;
        public UInt16 RandRange;
        public Single BaseTexW;
        public Single BaseTexH;
        public Rect Target;
        public Byte NumFrames;
        public Vector2[] Coords;

        public void ParseEntry(String[] raw, CsvMetaData metadata)
        {
            ModelName = CsvParser.String(raw[0]);
            Id = CsvParser.Int32(raw[1]);
            Flags = CsvParser.Byte(raw[2]);
            Rate = CsvParser.Int16(raw[3]);
            RandMin = CsvParser.UInt16(raw[4]);
            RandRange = CsvParser.UInt16(raw[5]);

            BaseTexW = CsvParser.Single(raw[6]);
            BaseTexH = CsvParser.Single(raw[7]);

            Single tx = CsvParser.Single(raw[8]);
            Single ty = CsvParser.Single(raw[9]);
            Single tw = CsvParser.Single(raw[10]);
            Single th = CsvParser.Single(raw[11]);
            Target = new Rect(tx, ty, tw, th);

            NumFrames = CsvParser.Byte(raw[12]);
            Coords = new Vector2[NumFrames];

            for (Int32 i = 0; i < NumFrames; i++)
            {
                if (13 + (i * 2) + 1 < raw.Length)
                {
                    Single cx = CsvParser.Single(raw[13 + (i * 2)]);
                    Single cy = CsvParser.Single(raw[14 + (i * 2)]);
                    Coords[i] = new Vector2(cx, cy);
                }
            }
        }

        public void WriteEntry(CsvWriter sw, CsvMetaData metadata)
        {
            sw.String(ModelName);
            sw.Int32(Id);
            sw.Byte(Flags);
            sw.Int16(Rate);
            sw.UInt16(RandMin);
            sw.UInt16(RandRange);
            sw.Single(BaseTexW);
            sw.Single(BaseTexH);
            sw.Single(Target.x);
            sw.Single(Target.y);
            sw.Single(Target.width);
            sw.Single(Target.height);
            sw.Byte(NumFrames);
            for (Int32 i = 0; i < NumFrames; i++)
            {
                sw.Single(Coords[i].x);
                sw.Single(Coords[i].y);
            }
        }
    }
}
