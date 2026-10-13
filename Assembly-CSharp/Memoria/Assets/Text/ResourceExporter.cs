using Assets.Scripts.Common;
using Memoria.Prime;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Memoria.Assets
{
    public static class ResourceExporter
    {
        public static Boolean IsExporting { get; private set; }
        public static String CurrentResourceText { get; private set; } = String.Empty;
        public static Int32 CurrentValue { get; private set; }
        public static Int32 MaxValue { get; private set; } = 1;

        private static readonly Stopwatch _frameTimer = new Stopwatch();

        public static void UpdateProgress(String resourceText, Int32 currentValue, Int32 maxValue = 100)
        {
            CurrentResourceText = resourceText ?? String.Empty;
            CurrentValue = currentValue;
            MaxValue = Math.Max(1, maxValue);
        }

        public static Boolean ShouldYield()
        {
            if (_frameTimer.ElapsedMilliseconds >= 33)
            {
                _frameTimer.Reset();
                _frameTimer.Start();
                return true;
            }
            return false;
        }

        public static void ExportSafe()
        {
            PersistenSingleton<SceneDirector>.Instance.StartCoroutine(ExportSafeCoroutine());
        }

        private static IEnumerator ExportSafeCoroutine()
        {
            if (!Configuration.Export.Enabled)
            {
                Log.Message("[ResourceExporter] Pass through {Configuration.Export.Enabled = 0}.");
                yield break;
            }

            IsExporting = true;
            _frameTimer.Reset();
            _frameTimer.Start();

            try
            {
                UpdateProgress("Initialisation de l'export...", 0, 1);
                yield return null;

                yield return PersistenSingleton<SceneDirector>.Instance.StartCoroutine(TextResourceExporter.ExportSafeEnumerator());
                yield return PersistenSingleton<SceneDirector>.Instance.StartCoroutine(GraphicResourceExporter.ExportSafeEnumerator());
                yield return PersistenSingleton<SceneDirector>.Instance.StartCoroutine(FieldSceneExporter.ExportSafeEnumerator());
                yield return PersistenSingleton<SceneDirector>.Instance.StartCoroutine(BattleSceneExporter.ExportSafeEnumerator());
                yield return PersistenSingleton<SceneDirector>.Instance.StartCoroutine(TranslationExporter.ExportSafeEnumerator());

                UpdateProgress("Exportation terminée !", 1, 1);
                yield return null;

                Log.Message("[ResourceExporter] Application will now quit. Please disable Configuration.Export.Enabled and restart the game.");
                UIManager.Input.ConfirmQuit();
            }
            finally
            {
                _frameTimer.Stop();
                IsExporting = false;
            }
        }

        private static void RunSafe(Action exportAction)
        {
            try
            {
                exportAction();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ResourceExporter] Failed to export resources.");
            }
        }
    }
}
