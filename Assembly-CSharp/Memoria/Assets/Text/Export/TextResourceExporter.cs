using Assets.Scripts.Common;
using Memoria.Prime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Memoria.Assets
{
    public static class TextResourceExporter
    {
        public static IEnumerator ExportSafe()
        {
            if (!Configuration.Export.Text)
            {
                Log.Message("[TextResourceExporter] Pass through {Configuration.Export.Text = 0}.");
                yield break;
            }

            CreditsExporter credits = new CreditsExporter();
            string[] languages = Configuration.Export.Languages;

            var exporters = EnumerateExporters().ToList();
            int totalSteps = languages.Length * (1 + exporters.Count);
            int currentStep = 0;

            SceneDirector.ExportStatus = "Initializing Text Export...";
            yield return new WaitForEndOfFrame();

            try
            {
                foreach (String symbol in languages)
                {
                    EmbadedTextResources.CurrentSymbol = symbol;
                    ModTextResources.Export.CurrentSymbol = symbol;

                    // UI Update
                    SceneDirector.ExportStatus = "Exporting Text (" + symbol + "): Credits";
                    SceneDirector.ExportProgress = (float)currentStep / totalSteps;
                    yield return new WaitForEndOfFrame();

                    try { credits.Export(); }
                    catch (Exception ex) { Log.Error(ex, "Credits export failed"); }

                    currentStep++;

                    foreach (IExporter exporter in exporters)
                    {
                        // UI Update
                        SceneDirector.ExportStatus = "Exporting Text (" + symbol + "): " + exporter.GetType().Name;
                        SceneDirector.ExportProgress = (float)currentStep / totalSteps;
                        yield return new WaitForEndOfFrame();

                        try { exporter.Export(); }
                        catch (Exception ex) { Log.Error(ex, "Exporter failed: " + exporter.GetType().Name); }

                        currentStep++;
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
