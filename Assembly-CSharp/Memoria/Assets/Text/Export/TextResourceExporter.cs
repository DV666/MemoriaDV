using Assets.Scripts.Common;
using Memoria.Prime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Memoria.Assets
{
    public static class TextResourceExporter
    {
        public static void ExportSafe()
        {
            try
            {
                if (!Configuration.Export.Text)
                {
                    Log.Message("[TextResourceExporter] Pass through {Configuration.Export.Text = 0}.");
                    return;
                }

                CreditsExporter credits = new CreditsExporter();

                foreach (String symbol in Configuration.Export.Languages)
                {
                    EmbadedTextResources.CurrentSymbol = symbol;
                    ModTextResources.Export.CurrentSymbol = symbol;

                    credits.Export();

                    foreach (IExporter exporter in EnumerateExporters())
                        exporter.Export();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to export text resources.");
            }
            finally
            {
                EmbadedTextResources.CurrentSymbol = null;
                ModTextResources.Export.CurrentSymbol = null;
            }
        }

        public static IEnumerator ExportSafeEnumerator()
        {
            if (!Configuration.Export.Text)
            {
                Log.Message("[TextResourceExporter] Pass through {Configuration.Export.Text = 0}.");
                yield break;
            }

            try
            {
                CreditsExporter credits = new CreditsExporter();
                String[] languages = Configuration.Export.Languages;
                IExporter[] exporters = EnumerateExporters().ToArray();

                Int32 totalSteps = languages.Length * (exporters.Length + 1);
                Int32 currentStep = 0;

                foreach (String symbol in languages)
                {
                    EmbadedTextResources.CurrentSymbol = symbol;
                    ModTextResources.Export.CurrentSymbol = symbol;

                    currentStep++;
                    ResourceExporter.UpdateProgress($"[{symbol}] CreditsExporter", currentStep, totalSteps);
                    yield return null;

                    try
                    {
                        credits.Export();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[CreditsExporter] Failed to export resource.");
                    }

                    foreach (IExporter exporter in exporters)
                    {
                        currentStep++;
                        ResourceExporter.UpdateProgress($"[{symbol}] {exporter.GetType().Name}", currentStep, totalSteps);
                        yield return null;

                        if (exporter is SingleFileExporter singleFileExporter)
                        {
                            yield return PersistenSingleton<SceneDirector>.Instance.StartCoroutine(singleFileExporter.ExportCoroutine());
                        }
                        else
                        {
                            try
                            {
                                exporter.Export();
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, $"[{exporter.GetType().Name}] Failed to export resource.");
                            }
                        }
                    }
                }
            }
            finally
            {
                EmbadedTextResources.CurrentSymbol = null;
                ModTextResources.Export.CurrentSymbol = null;
            }
        }

        private static IEnumerable<IExporter> EnumerateExporters()
        {
            yield return new LocalizationExporter();

            foreach (EtcTextResource value in Enum.GetValues(typeof(EtcTextResource)))
                yield return new EtcExporter(value);

            yield return new CommandExporter();
            yield return new AbilityExporter();
            yield return new SkillExporter();
            yield return new ItemExporter();
            yield return new KeyItemExporter();
            yield return new BattleExporter();
            yield return new LocationNameExporter();
            yield return new FieldExporter();
            yield return new CharacterNamesExporter();
        }
    }

    public interface IExporter
    {
        void Export();
    }
}
