using Assets.Scripts.Common;
using Memoria.Prime;
using System;
using System.Collections;
using System.IO;

namespace Memoria.Assets
{
    public abstract class SingleFileExporter : IExporter
    {
        protected abstract String TypeName { get; }
        protected abstract TextResourcePath ExportPath { get; }

        protected abstract TxtEntry[] PrepareEntries();

        protected virtual IEnumerator PrepareEntriesCoroutine(Action<TxtEntry[]> onCompleted)
        {
            TxtEntry[] entries = null;
            try
            {
                entries = PrepareEntries();
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{TypeName}] Failed to prepare entries.");
            }
            onCompleted?.Invoke(entries);
            yield break;
        }

        public void Export()
        {
            try
            {
                TextResourcePath exportPath = ExportPath;
                if (File.Exists(exportPath.Value))
                {
                    Log.Warning($"[{TypeName}] Export was skipped bacause a file already exists: [{exportPath}].");
                    return;
                }

                Log.Message($"[{TypeName}] Exporting...");

                TxtEntry[] entries = PrepareEntries();

                FileCommander.PrepareFileDirectory(exportPath.Value);
                exportPath.WriteAll(entries);

                Log.Message($"[{TypeName}] Exporting completed successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{TypeName}] Failed to export resource.");
            }
        }

        public IEnumerator ExportCoroutine()
        {
            TextResourcePath exportPath;
            try
            {
                exportPath = ExportPath;
                if (File.Exists(exportPath.Value))
                {
                    Log.Warning($"[{TypeName}] Export was skipped bacause a file already exists: [{exportPath}].");
                    yield break;
                }

                Log.Message($"[{TypeName}] Exporting...");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{TypeName}] Failed to initialize export.");
                yield break;
            }

            TxtEntry[] entries = null;
            yield return PersistenSingleton<SceneDirector>.Instance.StartCoroutine(PrepareEntriesCoroutine(result => entries = result));

            if (entries == null)
                yield break;

            try
            {
                FileCommander.PrepareFileDirectory(exportPath.Value);
                exportPath.WriteAll(entries);

                Log.Message($"[{TypeName}] Exporting completed successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{TypeName}] Failed to export resource.");
            }
        }
    }
}
