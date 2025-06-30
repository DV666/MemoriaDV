using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Assets.Sources.Scripts.UI.Common;
using Memoria.Prime.Text;
using Memoria.Prime;
using NCalc;
using UnityEngine;

namespace Memoria
{
    public class DescriptionPatcher
    {
        private delegate void ExpressionInitializer(ref Expression expr);

        private static List<DescriptionPatcher> AADescriptionPatchers = new List<DescriptionPatcher>();
        private static List<DescriptionPatcher> CMDDescriptionPatchers = new List<DescriptionPatcher>();
        private static List<DescriptionPatcher> ItemDescriptionPatchers = new List<DescriptionPatcher>();

        public static void PatchDescription(String[] patchCode) // [DV] To do... one day.
        {
            DescriptionPatcher patcher = null;
            FindAndReplacer finder = null;
            Appender appender = null;
            foreach (String line in patchCode)
            {
                if (line.StartsWith("//"))
                    continue;
                List<DescriptionPatcher> list = IsPatcherDeclaration(line);
                if (list != null)
                {
                    patcher = new DescriptionPatcher();
                    list.Add(patcher);
                    finder = null;
                    appender = null;
                    continue;
                }
                if (patcher == null)
                    continue;
                if (line.StartsWith("FindAndReplace"))
                {
                    finder = new FindAndReplacer();
                    appender = null;
                    patcher.Modifiers.Add(finder);
                }
                else if (line.StartsWith("Append"))
                {
                    finder = null;
                    appender = new Appender();
                    appender.IsAppend = true;
                    patcher.Modifiers.Add(appender);
                }
                else if (line.StartsWith("Prepend"))
                {
                    finder = null;
                    appender = new Appender();
                    appender.IsAppend = false;
                    patcher.Modifiers.Add(appender);
                }
                else if (line.StartsWith("[code=Condition]") && line.EndsWith("[/code]"))
                {
                    String condition = line.Substring("[code=Condition]".Length, line.Length - "[code=Condition][/code]".Length).Trim();
                    if (finder != null)
                        finder.Condition = condition;
                    else if (appender != null)
                        appender.Condition = condition;
                    else
                        patcher.Condition = condition;
                }
            }
        }

        private static List<DescriptionPatcher> IsPatcherDeclaration(String line)
        {
            if (line.StartsWith(">AA"))
                return AADescriptionPatchers;
            if (line.StartsWith(">CMD"))
                return CMDDescriptionPatchers;
            if (line.StartsWith(">ITEM"))
                return ItemDescriptionPatchers;
            return null;
        }

        public static String PatchDialogString(String str, Dialog dialog)
        {
            try
            {
                if (DialogPatchers.Count == 0)
                    return str;
                Int32 lineCount = str.OccurenceCount("\n") + 1;
                ExpressionInitializer ncalcInit = delegate (ref Expression expr)
                {
                    expr.Parameters["RawText"] = str;
                    expr.Parameters["FieldZoneId"] = FF9TextTool.FieldZoneId;
                    expr.Parameters["TextId"] = dialog.TextId;
                    expr.Parameters["DialogFlags"] = ETb.StylesToFlag(dialog.Style, dialog.CapType);
                    expr.Parameters["SpeakerModelId"] = dialog.Po?.model ?? -1;
                    expr.Parameters["SpeakerAnimationId"] = dialog.Po?.anim ?? -1;
                    expr.Parameters["SpeakerIdleAnimationId"] = (dialog.Po as Actor)?.idle ?? -1;
                    expr.Parameters["LineCount"] = lineCount;
                    expr.EvaluateFunction += delegate (String name, FunctionArgs args)
                    {
                        if (name == "SearchCount" && args.Parameters.Length >= 1)
                        {
                            String search = NCalcUtility.EvaluateNCalcString(args.Parameters[0].Evaluate());
                            String from = args.Parameters.Length >= 2 ? NCalcUtility.EvaluateNCalcString(args.Parameters[1].Evaluate()) : str;
                            args.Result = String.IsNullOrEmpty(search) ? 0 : from.OccurenceCount(search);
                        }
                        else if (name == "SpriteExists" && args.Parameters.Length == 2)
                        {
                            String atlasName = NCalcUtility.EvaluateNCalcString(args.Parameters[0].Evaluate());
                            String spriteName = NCalcUtility.EvaluateNCalcString(args.Parameters[1].Evaluate());
                            args.Result = FF9UIDataTool.GetSpriteSize(atlasName, spriteName) != Vector2.zero;
                        }
                        else if (name == "GetTextVariable" && args.Parameters.Length == 1)
                        {
                            Int32 scriptId = (Int32)NCalcUtility.ConvertNCalcResult(args.Parameters[0].Evaluate(), -1);
                            args.Result = scriptId >= 0 && scriptId < ETb.gMesValue.Length ? ETb.gMesValue[scriptId] : 0;
                        }
                    };
                };
                foreach (TextPatcher patcher in DialogPatchers)
                    if (patcher.ApplyPatch(ref str, ncalcInit))
                        lineCount = str.OccurenceCount("\n") + 1;
            }
            catch (Exception err)
            {
                Log.Error(err);
            }
            return str;
        }
    }
}
