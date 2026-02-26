using Assets.Sources.Scripts.UI.Common;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using NCalc;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Memoria
{
    public static class CardPatcher
    {
        public static Boolean IsUsed => _patches.Count > 0;

        private class Patch
        {
            public String Condition;
            public String TargetElement;
            public String AtlasName;
            public String SpriteName;
            public Expression CompiledExpression;

            public Byte? Atk;
            public Byte? PDef;
            public Byte? MDef;
            public Byte? CPoint;
            public QuadMistCard.Type? Type;
        }

        private static List<Patch> _patches = new List<Patch>();
        private static Dictionary<string, Sprite> _cachedInGameSprites = new Dictionary<string, Sprite>();

        public static void Load(String[] source)
        {
            _patches.Clear();
            foreach (var sprite in _cachedInGameSprites.Values)
            {
                if (sprite != null)
                    UnityEngine.Object.Destroy(sprite);
            }
            _cachedInGameSprites.Clear();

            Patch currentPatch = null;

            foreach (String line in source)
            {
                String trim = line.Trim();
                if (String.IsNullOrEmpty(trim) || trim.StartsWith("//")) continue;

                if (trim == ">TETRAMASTER") continue;

                if (trim == "BorderSprite" || trim == "ImageSprite" || trim == "BackgroundSprite" || trim == "Statistics")
                {
                    currentPatch = new Patch { TargetElement = trim };
                    _patches.Add(currentPatch);
                    continue;
                }

                if (currentPatch == null) continue;

                if (trim.StartsWith("[code=Condition]") && trim.EndsWith("[/code]"))
                {
                    currentPatch.Condition = trim.Substring(16, trim.Length - 23).Trim();
                    try
                    {
                        currentPatch.CompiledExpression = new Expression(currentPatch.Condition);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[CardPatcher] Error parsing condition: {currentPatch.Condition}. {ex.Message}");
                    }
                }
                else if (trim.StartsWith("Atlas:"))
                {
                    currentPatch.AtlasName = trim.Substring(6).Trim();
                }
                else if (trim.StartsWith("Sprite:"))
                {
                    currentPatch.SpriteName = trim.Substring(7).Trim();
                }
                else if (trim.StartsWith("Atk:"))
                {
                    if (Byte.TryParse(trim.Substring(4).Trim(), out Byte val)) currentPatch.Atk = val;
                }
                else if (trim.StartsWith("Pdef:"))
                {
                    if (Byte.TryParse(trim.Substring(5).Trim(), out Byte val)) currentPatch.PDef = val;
                }
                else if (trim.StartsWith("Mdef:"))
                {
                    if (Byte.TryParse(trim.Substring(5).Trim(), out Byte val)) currentPatch.MDef = val;
                }
                else if (trim.StartsWith("Cpoint:"))
                {
                    if (Byte.TryParse(trim.Substring(7).Trim(), out Byte val)) currentPatch.CPoint = val;
                }
                else if (trim.StartsWith("Type:"))
                {
                    String typeStr = trim.Substring(5).Trim();
                    try
                    {
                        currentPatch.Type = (QuadMistCard.Type)Enum.Parse(typeof(QuadMistCard.Type), typeStr, true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[CardPatcher] Parsing error for <Type> : {typeStr}. {ex.Message}");
                    }
                }
            }
            Log.Message($"[CardPatcher] Loaded {_patches.Count} patches.");
        }

        public static Boolean Apply(QuadMistCard card, UISprite sprite, String targetElement)
        {
            if (_patches.Count == 0 || card == null || sprite == null) return false;

            foreach (var patch in _patches)
            {
                if (patch.TargetElement != targetElement) continue;

                if (EvaluatePatch(patch, card))
                {
                    if (!String.IsNullOrEmpty(patch.AtlasName))
                    {
                        if (FF9UIDataTool.customAtlas.TryGetValue(patch.AtlasName, out UIAtlas customAtlas))
                            sprite.atlas = customAtlas;
                        else if (patch.AtlasName == "QuadMistImageAtlas")
                            sprite.atlas = FF9UIDataTool.QuadMistImageAtlas;
                        else
                            sprite.atlas = FF9UIDataTool.QuadMistCardAtlas;
                    }

                    if (!String.IsNullOrEmpty(patch.SpriteName))
                    {
                        sprite.spriteName = patch.SpriteName;
                    }
                    return true;
                }
            }
            return false;
        }

        public static void Apply(QuadMistCard card, CardDisplay display)
        {
            if (_patches.Count == 0 || card == null || display == null) return;

            foreach (var patch in _patches)
            {
                SpriteDisplay targetDisplay = null;

                switch (patch.TargetElement)
                {
                    case "ImageSprite": targetDisplay = display.character; break;
                    case "BackgroundSprite": targetDisplay = display.background; break;
                    case "BorderSprite": targetDisplay = display.frame; break;
                    default: continue;
                }

                if (targetDisplay == null) continue;
                Vector2 pivotToUse = new Vector2(0.5f, 0.5f);
                float targetWorldWidth = -1f;
                float targetWorldHeight = -1f;
                bool hasReference = false;

                SpriteRenderer currentRenderer = targetDisplay.GetComponent<SpriteRenderer>();

                if (currentRenderer != null && currentRenderer.sprite != null)
                {
                    float oldRectWidth = currentRenderer.sprite.rect.width;
                    float oldRectHeight = currentRenderer.sprite.rect.height;

                    if (oldRectWidth > 0 && oldRectHeight > 0)
                    {
                        pivotToUse = new Vector2(
                            currentRenderer.sprite.pivot.x / oldRectWidth,
                            currentRenderer.sprite.pivot.y / oldRectHeight
                        );
                    }

                    targetWorldWidth = currentRenderer.sprite.bounds.size.x;
                    targetWorldHeight = currentRenderer.sprite.bounds.size.y;

                    if (targetWorldWidth > 0 && targetWorldHeight > 0)
                    {
                        hasReference = true;
                    }
                }

                if (EvaluatePatch(patch, card))
                {
                    if (!String.IsNullOrEmpty(patch.SpriteName))
                    {
                        Sprite newSprite = null;
                        string cacheKey = patch.AtlasName + "_" + patch.SpriteName;

                        if (_cachedInGameSprites.TryGetValue(cacheKey, out newSprite) && newSprite != null)
                        {
                            if (currentRenderer != null && currentRenderer.sprite != newSprite)
                            {
                                currentRenderer.sprite = newSprite;
                            }
                            continue;
                        }

                        if (!String.IsNullOrEmpty(patch.AtlasName) && FF9UIDataTool.customAtlas.TryGetValue(patch.AtlasName, out UIAtlas customAtlas))
                        {
                            UISpriteData spriteData = customAtlas.GetSprite(patch.SpriteName);

                            if (spriteData != null && customAtlas.texture != null)
                            {
                                int texHeight = customAtlas.texture.height;
                                Rect rect = new Rect(spriteData.x, texHeight - spriteData.y - spriteData.height, spriteData.width, spriteData.height);

                                float finalPPU = 482f; // Default value is 482f

                                if (hasReference)
                                {
                                    float ppuWidth = rect.width / targetWorldWidth;
                                    float ppuHeight = rect.height / targetWorldHeight;
                                    finalPPU = Mathf.Max(ppuWidth, ppuHeight);
                                }

                                newSprite = Sprite.Create(customAtlas.texture as Texture2D, rect, pivotToUse, finalPPU);
                                newSprite.name = patch.SpriteName;

                                _cachedInGameSprites[cacheKey] = newSprite;
                            }
                        }

                        if (newSprite == null)
                            newSprite = QuadMistResourceManager.Instance.GetSprite(patch.SpriteName);

                        if (newSprite != null && currentRenderer != null)
                        {
                            if (currentRenderer.sprite != newSprite)
                            {
                                currentRenderer.sprite = newSprite;
                            }
                        }
                    }
                }
            }
        }

        private static bool EvaluatePatch(Patch patch, QuadMistCard card)
        {
            if (patch.CompiledExpression == null) return false;

            try
            {
                patch.CompiledExpression.Parameters["Id"] = (int)card.id;
                patch.CompiledExpression.Parameters["Side"] = (int)card.side;
                patch.CompiledExpression.Parameters["PlayerSide"] = (int)card.side == QuadMistCardUI.PLAYER_SIDE;
                patch.CompiledExpression.Parameters["EnemySide"] = (int)card.side == QuadMistCardUI.ENEMY_SIDE;
                patch.CompiledExpression.Parameters["Atk"] = (int)card.atk;
                patch.CompiledExpression.Parameters["PDef"] = (int)card.pdef;
                patch.CompiledExpression.Parameters["MDef"] = (int)card.mdef;
                patch.CompiledExpression.Parameters["CPoint"] = (int)card.cpoint;
                patch.CompiledExpression.Parameters["Arrow"] = (int)card.arrow;
                patch.CompiledExpression.Parameters["ArrowNumber"] = MathEx.BitCount(card.arrow);
                patch.CompiledExpression.Parameters["Type"] = (int)card.type;
                patch.CompiledExpression.Parameters["IsBlock"] = card.IsBlock;

                object result = patch.CompiledExpression.Evaluate();
                return result is bool success && success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void ApplyCardPatches(QuadMistCard card, CardDetailHUD cardHud)
        {
            if (!CardPatcher.Apply(card, cardHud.CardImageSprite, "ImageSprite"))
            {
                cardHud.CardImageSprite.atlas = FF9UIDataTool.QuadMistCardAtlas;
                cardHud.CardImageSprite.spriteName = "card_" + ((Int32)card.id).ToString("0#");
            }

            UISprite bgSprite = cardHud.Self.transform.GetChild(4).GetComponent<UISprite>();

            if (bgSprite != null)
            {
                if (!CardPatcher.Apply(card, bgSprite, "BackgroundSprite"))
                {
                    bgSprite.atlas = FF9UIDataTool.QuadMistImageAtlas;
                    bgSprite.spriteName = "card_player_bg";
                }
            }

            if (!CardPatcher.Apply(card, cardHud.CardBorderSprite, "BorderSprite"))
            {
                cardHud.CardBorderSprite.atlas = FF9UIDataTool.QuadMistImageAtlas;
                cardHud.CardBorderSprite.spriteName = "card_player_frame";
            }
        }

        private static Dictionary<QuadMistCard, QuadMistCard> _originalCards = new Dictionary<QuadMistCard, QuadMistCard>();
        private static Dictionary<QuadMistCard, QuadMistCard> _patchedBaselines = new Dictionary<QuadMistCard, QuadMistCard>();

        public static void ApplyStatPatches(QuadMistCard card)
        {
            if (_patches.Count == 0 || card == null) return;

            QuadMistCard tempCard = new QuadMistCard(card);
            bool isPatched = false;

            foreach (var patch in _patches)
            {
                if (patch.TargetElement != "Statistics") continue;

                if (EvaluatePatch(patch, tempCard))
                {
                    if (patch.Atk.HasValue) tempCard.atk = patch.Atk.Value;
                    if (patch.PDef.HasValue) tempCard.pdef = patch.PDef.Value;
                    if (patch.MDef.HasValue) tempCard.mdef = patch.MDef.Value;
                    if (patch.CPoint.HasValue) tempCard.cpoint = patch.CPoint.Value;
                    if (patch.Type.HasValue) tempCard.type = patch.Type.Value;
                    isPatched = true;
                }
            }

            if (isPatched)
            {
                if (card.OriginalCard == null)
                    card.OriginalCard = new QuadMistCard(card);

                card.atk = tempCard.atk;
                card.pdef = tempCard.pdef;
                card.mdef = tempCard.mdef;
                card.cpoint = tempCard.cpoint;
                card.type = tempCard.type;
            }
        }

        public static void RestoreOriginalStats(IEnumerable<QuadMistCard> cards)
        {
            if (cards == null) return;
            foreach (QuadMistCard card in cards)
            {
                if (card.OriginalCard != null)
                {
                    card.atk = card.OriginalCard.atk;
                    card.pdef = card.OriginalCard.pdef;
                    card.mdef = card.OriginalCard.mdef;
                    card.type = card.OriginalCard.type;
                    card.OriginalCard = null;
                }
            }
        }
    }
}
