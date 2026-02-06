using Assets.Scripts.Common;
using Memoria.Prime;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Memoria.Assets
{
    public static class ResourceExporter
    {
        public static IEnumerator ExportSafe()
        {
            SceneDirector.IsExporting = true;

            try
            {
                if (!Configuration.Export.Enabled)
                {
                    Log.Message("[ResourceExporter] Pass through {Configuration.Export.Enabled = 0}.");
                    yield break;
                }

                yield return SceneDirector.Instance.StartCoroutine(BattleSceneExporter.ExportSafe());
                yield return SceneDirector.Instance.StartCoroutine(TextResourceExporter.ExportSafe());
                yield return SceneDirector.Instance.StartCoroutine(GraphicResourceExporter.ExportSafe());
                yield return SceneDirector.Instance.StartCoroutine(FieldSceneExporter.ExportSafe());
                yield return SceneDirector.Instance.StartCoroutine(TranslationExporter.ExportSafe());

                Log.Message("[ResourceExporter] Application will now quit.");
                SceneDirector.ExportStatus = "Done! Quitting...";
                SceneDirector.ExportProgress = 1.0f;
                yield return new WaitForSeconds(1f);

                UIManager.Input.ConfirmQuit();
            }
            finally
            {
                SceneDirector.IsExporting = false;
            }
        }
    }
}
