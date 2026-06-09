using Memoria.Data;
using System.Collections.Generic;
using UnityEngine;
using static Memoria.Scripts.TranceSeek.TranceSeekRegularItem;

namespace Memoria.Scripts.TranceSeek
{
    public static class TranceSeekVisualAccessoryDB
    {
        public class AccessoryConfig : Dictionary<CharacterId, CharacterTransform>
        {
            public string ModelName { get; }
            public string AnimIdle { get; }

            public AccessoryConfig(string modelName, string animIdle = null)
            {
                ModelName = modelName;
                AnimIdle = animIdle;
            }
        }

        public struct CharacterTransform
        {
            public int BoneIndex;
            public Vector3 PositionOffset;
            public Vector3 RotationOffset;
            public Vector3 ScaleOffset;
            public int[] BonesToHide;

            public CharacterTransform(int boneIndex, Vector3 posOffset, Vector3 rotOffset, Vector3 scaleOffset, int[] bonesToHide = null)
            {
                BoneIndex = boneIndex;
                PositionOffset = posOffset;
                RotationOffset = rotOffset;
                ScaleOffset = scaleOffset;
                BonesToHide = bonesToHide;
            }

            public CharacterTransform(int boneIndex, Vector3 posOffset, Vector3 rotOffset, float uniformScale = 1f, int[] bonesToHide = null)
            {
                BoneIndex = boneIndex;
                PositionOffset = posOffset;
                RotationOffset = rotOffset;
                ScaleOffset = Vector3.one * uniformScale;
                BonesToHide = bonesToHide;
            }
        }

        public static readonly Dictionary<RegularItem, AccessoryConfig> VisualAccessoriesDict = new()
        {
            {
                MuTail, new("GEO_WEP_Tail_Mu_Suit")
                {
                    // CharacterId             | BONE | POSITION OFFSET       | ROTATION OFFSET                        | SCALE | HIDDEN BONES
                    { CharacterId.Zidane,      new(1,  new(0f, 0f, 0f),        new(343.0201f, 178.6945f, 181.0454f),   1f,     new[] { 24 }) },
                    { CharacterId.Vivi,        new(1,  new(5f, 37.5f, 0f),     new(303f, 180f, 180f)) },
                    { CharacterId.Garnet,      new(1,  new(0f, 0f, 0f),        new(74.80124f, 350.618f, 352.3549f)) },
                    { CharacterId.Steiner,     new(1,  new(0f, 0f, 0f),        new(63.00001f, 0f, 0f)) },
                    { CharacterId.Freya,       new(1,  new(0.5f, 34f, 0f),     new(273.9999f, 180f, 180f)) },
                    { CharacterId.Quina,       new(1,  new(0f, 119f, 0f),      new(304f, 180f, 180f)) },
                    { CharacterId.Eiko,        new(1,  new(0.5f, -2f, 28f),    new(82.00002f, 180f, 180f)) },
                    { CharacterId.Amarant,     new(1,  new(0.5f, -2f, 28f),    new(72.00003f, 0f, 0f)) },
                    { CharacterId.Cinna,       new(1,  new(0f, 55.5f, 0f),     new(283.3859f, 15.79045f, 169.5151f)) },
                    { CharacterId.Marcus,      new(1,  new(0f, 28.5f, -15f),   new(301f, 180f, 180f)) },
                    { CharacterId.Blank,       new(1,  new(0f, 0f, 0f),        new(279f, 180f, 180f)) },
                    { CharacterId.Beatrix,     new(1,  new(0f, -33.5f, 59f),   new(83.00005f, 0f, 0f)) }
                }
            },

            {
                HaloGhost, new("GEO_WEP_Halo_Ghost_Suit")
                {
                    { CharacterId.Zidane,      new(8,  new(0f, -6f, 83f),      new(270f, 0f, 0f)) },
                    { CharacterId.Vivi,        new(8,  new(0f, -0.5f, 17f),    new(291f, 0f, 0f)) },
                    { CharacterId.Garnet,      new(20, new(0f, 0f, 0f),        new(53.00001f, 0f, 0f)) },
                    { CharacterId.Steiner,     new(20, new(0f, 1f, 72.5f),     new(270f, 0f, 0f)) },
                    { CharacterId.Freya,       new(8,  new(0f, 6f, 72f),       new(273.2788f, 128.0834f, 225.9914f)) },
                    { CharacterId.Quina,       new(7,  new(0f, -10.5f, -1f),   new(294f, 0f, 0f)) },
                    { CharacterId.Eiko,        new(19, new(0f, 2.5f, 63f),     new(272.9999f, 0f, 0f)) },
                    { CharacterId.Amarant,     new(18, new(0f, 1.5f, 95f),     new(273.605f, 33.66046f, 326.2872f)) },
                    { CharacterId.Cinna,       new(13, new(0f, -68.5f, -6f),   new(3f, 0f, 0f)) },
                    { CharacterId.Marcus,      new(9,  new(0f, 25f, 19f),      new(41.00002f, 0f, 0f)) },
                    { CharacterId.Blank,       new(4,  new(0f, 0f, 0f),        new(337.0333f, 1.272633f, 356.7415f)) },
                    { CharacterId.Beatrix,     new(19, new(0f, 0f, 0f),        new(60.85868f, 354.6063f, 353.8303f)) }
                }
            },

            {
                LadybugAntenna, new("GEO_WEP_Antenna_Ladybug_Suit")
                {
                    { CharacterId.Zidane,      new(8,  new(-9.5f, 3f, 100f),   new(307.2082f, 155.2831f, 197.5205f)) },
                    { CharacterId.Vivi,        new(8,  new(-5.5f, -15.5f, -2.5f), new(320.1827f, 4.18273f, 353.4846f)) },
                    { CharacterId.Garnet,      new(20, new(0f, 2.5f, -19.5f),  new(73.87874f, 353.0607f, 352.7796f)) },
                    { CharacterId.Steiner,     new(20, new(0f, 0f, 72f),       new(297f, 180f, 180f)) },
                    { CharacterId.Freya,       new(8,  new(0f, -18f, 58f),     new(282f, 180f, 180f)) },
                    { CharacterId.Quina,       new(7,  new(0f, 26.5f, 12.5f),  new(71.00002f, 180f, 180f)) },
                    { CharacterId.Eiko,        new(19, new(0f, -21.5f, 82f),   new(279f, 180f, 180f)) },
                    { CharacterId.Amarant,     new(18, new(-1f, -97.5f, 57f),  new(274.1229f, 165.9864f, 194.0486f)) },
                    { CharacterId.Cinna,       new(12, new(0f, -47.5f, 105.5f), new(276.0825f, 189.4279f, 170.5197f)) },
                    { CharacterId.Marcus,      new(9,  new(0f, 38.5f, -13f),   new(0f, 0f, 0f)) },
                    { CharacterId.Blank,       new(4,  new(0.5f, -41f, -28.5f), new(317f, 0f, 0f)) },
                    { CharacterId.Beatrix,     new(18, new(0f, -46.5f, 48.5f), new(281f, 0f, 0f)) }
                }
            },

            {
                YetiMouth, new("GEO_WEP_Mouth_Yeti_Suit")
                {
                    { CharacterId.Zidane,      new(8,  new(0f, -45.5f, 0f),    new(290f, 0f, 0f),       0.35f) },
                    { CharacterId.Vivi,        new(7,  new(0f, -39.5f, 0f),    new(298f, 0f, 0f),       0.32f) },
                    { CharacterId.Garnet,      new(19, new(0.5f, -44f, -6.5f), new(288f, 0f, 0f),       0.28f) },
                    { CharacterId.Steiner,     new(20, new(0f, -46f, 0f),      new(287f, 0f, 0f),       0.5174f) },
                    { CharacterId.Freya,       new(8,  new(0f, -46.5f, 13f),   new(276f, 0f, 0f),       0.5174344f) },
                    { CharacterId.Quina,       new(8,  new(0f, -0.5f, 34f),    new(47f, 180f, 180f),    1f) },
                    { CharacterId.Eiko,        new(19, new(0f, -40f, -3f),     new(307f, 0f, 0f),       0.321286f) },
                    { CharacterId.Amarant,     new(18, new(0f, -98.5f, 58f),   new(292.0511f, 182.8635f, 174.5446f), 1f) },
                    { CharacterId.Cinna,       new(13, new(0f, -4f, -56.5f),   new(0f, 0f, 0f),         1f) },
                    { CharacterId.Marcus,      new(8,  new(0f, -67f, -10f),    new(290f, 0f, 0f),       0.6830134f) },
                    { CharacterId.Blank,       new(3,  new(0f, -40f, 7.5f),    new(294f, 0f, 0f),       0.4743148f) },
                    { CharacterId.Beatrix,     new(18, new(0f, -45.5f, -9.5f), new(300f, 0f, 0f),       0.2920782f) }
                }
            },

            {
                NymphFlower, new("GEO_WEP_Flower_Nymph_Suit")
                {
                    { CharacterId.Zidane,      new(8,  new(0f, -6f, 78f),      new(270f, 0f, 0f)) },
                    { CharacterId.Vivi,        new(9,  new(0f, 0f, 0f),        new(0f, 0f, 0f)) },
                    { CharacterId.Garnet,      new(20, new(0f, 0f, 0f),        new(68.00002f, 0f, 0f)) },
                    { CharacterId.Steiner,     new(20, new(0f, 1f, 83f),       new(270f, 0f, 0f)) },
                    { CharacterId.Freya,       new(8,  new(0f, 36f, 132.5f),   new(283f, 180f, 180f)) },
                    { CharacterId.Quina,       new(9,  new(0f, 0f, 0f),        new(279f, 180f, 180f)) },
                    { CharacterId.Eiko,        new(19, new(0f, -4.5f, 65f),     new(314f, 0f, 0f)) },
                    { CharacterId.Amarant,     new(18, new(0f, 0f, 77.5f),     new(280.0001f, 0f, 0f)) },
                    { CharacterId.Cinna,       new(12, new(0f, 5.5f, 79.5f),   new(274f, 180f, 180f)) },
                    { CharacterId.Marcus,      new(9,  new(0f, 19f, 21.5f),    new(45f, 0f, 0f)) },
                    { CharacterId.Blank,       new(4,  new(0f, 0f, 0f),        new(274f, 180f, 180f)) },
                    { CharacterId.Beatrix,     new(21, new(0f, 0f, 0f),        new(84.00002f, 0f, 0f)) }
                }
            },

            {
                JabberworkCrest, new("GEO_WEP_Crest_Jabberwock_Suit")
                {
                    { CharacterId.Zidane,      new(8,  new(0f, 19f, 56.5f),    new(278f, 0f, 0f),       0.3887561f) },
                    { CharacterId.Vivi,        new(8,  new(9f, -6f, 55.5f),    new(11f, 0f, 0f),        0.4703948f) },
                    { CharacterId.Garnet,      new(20, new(0f, 1f, 17.5f),     new(80.99998f, 0f, 0f),  0.3855432f) },
                    { CharacterId.Steiner,     new(20, new(3.5f, 22.5f, 55.5f),new(287f, 0f, 0f),       0.3563597f) },
                    { CharacterId.Freya,       new(8,  new(0f, 0f, 97.5f),     new(11f, 180f, 180f),    0.4311953f) },
                    { CharacterId.Quina,       new(7,  new(0f, -1f, -53.5f),   new(50f, 180f, 180f),    0.5089171f) },
                    { CharacterId.Eiko,        new(19, new(0f, 29f, 67f),      new(277f, 0f, 0f),       0.3919957f) },
                    { CharacterId.Amarant,     new(18, new(0f, 17.5f, 39.5f),  new(284f, 0f, 0f),       0.6887051f) },
                    { CharacterId.Cinna,       new(12, new(0f, 4.5f, 64f),     new(288.9753f, 180f, 180f), 0.4276317f) },
                    { CharacterId.Marcus,      new(9,  new(0f, 31f, 35.5f),    new(82f, 0f, 0f),        0.4240976f) },
                    { CharacterId.Blank,       new(3,  new(0f, 0f, 11.5f),     new(0f, 0f, 0f),         0.3855432f) },
                    { CharacterId.Beatrix,     new(18, new(0f, -3f, 22f),      new(86.8549f, 14.02577f, 19.92445f), 0.4240976f) }
                }
            },

            {
                Mini_FriendlyFeatherCircle, new("GEO_MON_B3_162", "ANH_MON_B3_162_000")
                {
                    { CharacterId.Zidane,      new(0,  new(98.5f, 133f, 0f),   new(0f, 180f, 180f),     0.5f) },
                    { CharacterId.Vivi,        new(0,  new(3f, -129.5f, -99f), new(0f, 84f, 0f),        0.5f) },
                    { CharacterId.Garnet,      new(0,  new(6.5f, -90f, -80f),  new(0f, 97f, 0f),        0.5f) },
                    { CharacterId.Steiner,     new(0,  new(79f, -147f, -147f), new(0f, 75f, 0f),        0.5f) },
                    { CharacterId.Freya,       new(0,  new(-75.5f, -175f, -103.5f), new(0f, 122f, 0f),  0.5f) },
                    { CharacterId.Quina,       new(0,  new(63f, -147.5f, -129.5f),  new(0f, 90f, 0f),   0.5f) },
                    { CharacterId.Eiko,        new(0,  new(96.5f, 46f, 0f),    new(0f, 180f, 180f),     0.5f) },
                    { CharacterId.Amarant,     new(0,  new(-7.5f, -150.5f, -147.5f), new(0f, 94f, 0f),  0.5f) },
                    { CharacterId.Cinna,       new(0,  new(-132.5f, 2.5f, -118f), new(291.3287f, 64.06817f, 115.4517f), 0.5f) },
                    { CharacterId.Marcus,      new(0,  new(102f, 0f, 0f),      new(286f, 180f, 180f),   0.5f) },
                    { CharacterId.Blank,       new(4,  new(104f, 0f, 0f),      new(322f, 0f, 0f),       0.5f) },
                    { CharacterId.Beatrix,     new(18, new(90f, -13f, 0f),     new(294f, 180f, 180f),   0.5f) }
                }
            },

            {
                GarudaWing, new("GEO_WEP_GarudaWing_Suit")
                {
                    { CharacterId.Zidane,      new(0,  new(98.5f, 133f, 0f),   new(0f, 180f, 180f)) },
                    { CharacterId.Vivi,        new(0,  new(3f, -129.5f, -99f), new(0f, 84f, 0f)) },
                    { CharacterId.Garnet,      new(0,  new(6.5f, -90f, -80f),  new(0f, 97f, 0f)) },
                    { CharacterId.Steiner,     new(0,  new(79f, -147f, -147f), new(0f, 75f, 0f)) },
                    { CharacterId.Freya,       new(0,  new(-75.5f, -175f, -103.5f), new(0f, 122f, 0f)) },
                    { CharacterId.Quina,       new(0,  new(63f, -147.5f, -129.5f),  new(0f, 90f, 0f)) },
                    { CharacterId.Eiko,        new(0,  new(96.5f, 46f, 0f),    new(0f, 180f, 180f)) },
                    { CharacterId.Amarant,     new(0,  new(-7.5f, -150.5f, -147.5f), new(0f, 94f, 0f)) },
                    { CharacterId.Cinna,       new(0,  new(-132.5f, 2.5f, -118f), new(291.3287f, 64.06817f, 115.4517f)) },
                    { CharacterId.Marcus,      new(0,  new(102f, 0f, 0f),      new(286f, 180f, 180f)) },
                    { CharacterId.Blank,       new(4,  new(104f, 0f, 0f),      new(322f, 0f, 0f)) },
                    { CharacterId.Beatrix,     new(18, new(90f, -13f, 0f),     new(294f, 180f, 180f)) }
                }
            }
        };
    }
}
