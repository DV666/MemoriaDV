using Assets.Sources.Scripts.UI.Common;
using Memoria.Prime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Memoria.EchoS
{
    public class BattleSubtitles : PersistenSingleton<BattleSubtitles>
    {
        private readonly Dictionary<ushort, HUDMessageChild> activeSubtitles = new Dictionary<ushort, HUDMessageChild>();
        private readonly Dictionary<BattleUnit, string> createQueue = new Dictionary<BattleUnit, string>();
        private readonly HashSet<HUDMessageChild> deleteQueue = new HashSet<HUDMessageChild>();

        public bool Enabled;

        public void Update()
        {
            foreach (var kvp in createQueue)
            {
                try
                {
                    if (activeSubtitles.TryGetValue(kvp.Key.Id, out HUDMessageChild existingMsg))
                    {
                        Hide(kvp.Key.Id, existingMsg.Label);
                    }

                    btl2d.GetIconPosition(kvp.Key.Data, 5, out Transform targetTransform, out Vector3 offset);

                    HUDMessageChild newMsg = Singleton<HUDMessage>.Instance.Show(targetTransform, kvp.Value, 0, offset, 0);

                    newMsg.GetComponent<UIWidget>().color = FF9TextTool.White;
                    newMsg.GetComponent<TweenPosition>().enabled = false;
                    newMsg.GetComponent<TweenAlpha>().enabled = false;

                    activeSubtitles[kvp.Key.Id] = newMsg;
                }
                catch
                {
                    // Ignorer les erreurs si l'unité n'est plus valide
                }
            }
            createQueue.Clear();

            foreach (HUDMessageChild msgToDelete in deleteQueue)
            {
                try
                {
                    Singleton<HUDMessage>.Instance.ReleaseObject(msgToDelete);
                }
                catch
                {
                }
            }
            deleteQueue.Clear();
        }

        public void Show(BattleUnit speaker, string text)
        {
            if (!Enabled || speaker == null || text.Length < 3 || text.StartsWith("“$"))
            {
                return;
            }
            createQueue[speaker] = text;
        }

        public void Hide(ushort speakerID, string text)
        {
            if (!Enabled)
            {
                return;
            }

            if (activeSubtitles.TryGetValue(speakerID, out HUDMessageChild msg) && msg.Label == text)
            {
                deleteQueue.Add(msg);
                activeSubtitles.Remove(speakerID);
            }
        }

        public void ClearAll()
        {
            foreach (HUDMessageChild item in activeSubtitles.Values)
            {
                deleteQueue.Add(item);
            }
            activeSubtitles.Clear();
        }

        private static void ListComponents(GameObject go, int indent = 0)
        {
            string indentStr = new string(' ', indent * 4);
            Log.Message($"[DEBUG] {indentStr}> {go.name} position: {go.transform.localPosition}");

            Component[] components = go.GetComponents<Component>();
            if (components != null && components.Length != 0)
            {
                foreach (Component component in components)
                {
                    if (component is Transform) continue;

                    MonoBehaviour monoBehaviour = component as MonoBehaviour;
                    if (monoBehaviour == null || !monoBehaviour.isActiveAndEnabled)
                    {
                        if (component is UIWidget uiwidget)
                        {
                            Log.Message($"[DEBUG]   {indentStr}{component} {component.GetType()} w: {uiwidget.width} h: {uiwidget.height} localScale: {component.transform.localScale} localPosition; {component.transform.localPosition}");
                        }
                        else
                        {
                            Log.Message($"[DEBUG]   {indentStr}{component} {component.GetType()}");
                        }
                    }
                }

                foreach (Transform child in go.transform)
                {
                    if (child.gameObject != go)
                    {
                        ListComponents(child.gameObject, indent + 1);
                    }
                }
            }
        }
    }
}
