using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Memoria.Assets
{
    public sealed class BattleExporter : SingleFileExporter
    {
        protected override String TypeName => nameof(BattleExporter);
        protected override TextResourcePath ExportPath => ModTextResources.Export.Battle;

        protected override TxtEntry[] PrepareEntries()
        {
            Dictionary<String, String> dic = new Dictionary<String, String>(1024);

            foreach (KeyValuePair<String, Int32> pair in FF9BattleDB.SceneData)
            {
                Int32 index = pair.Value;
                if (index == 220 || index == 238) // Junk?
                    continue;

                String path = EmbadedTextResources.GetCurrentPath("/Battle/" + index + ".mes");
                String[] text = EmbadedSentenseLoader.LoadSentense(path);
                if (text == null)
                    continue;

                foreach (String line in text)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;

                    String key = BattleFormatter.GetKey(line);
                    if (!dic.ContainsKey(key))
                        dic.Add(key, BattleFormatter.GetValue(line));
                }
            }

            return dic.Select(p => new TxtEntry { Prefix = p.Key, Value = p.Value }).ToArray();
        }

        protected override IEnumerator PrepareEntriesCoroutine(Action<TxtEntry[]> onCompleted)
        {
            Dictionary<String, String> dic = new Dictionary<String, String>(1024);
            Int32 total = FF9BattleDB.SceneData.Count();
            Int32 current = 0;
            String symbol = ModTextResources.Export.CurrentSymbol;

            foreach (KeyValuePair<String, Int32> pair in FF9BattleDB.SceneData)
            {
                current++;
                Int32 index = pair.Value;
                if (index == 220 || index == 238) // Junk?
                    continue;

                ResourceExporter.UpdateProgress($"[{symbol}] BattleExporter : {pair.Key} ({index}.mes)", current, total);
                if (ResourceExporter.ShouldYield())
                    yield return null;

                String path = EmbadedTextResources.GetCurrentPath("/Battle/" + index + ".mes");
                String[] text = EmbadedSentenseLoader.LoadSentense(path);
                if (text == null)
                    continue;

                foreach (String line in text)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;

                    String key = BattleFormatter.GetKey(line);
                    if (!dic.ContainsKey(key))
                        dic.Add(key, BattleFormatter.GetValue(line));
                }
            }

            onCompleted?.Invoke(dic.Select(p => new TxtEntry { Prefix = p.Key, Value = p.Value }).ToArray());
        }
    }
}
