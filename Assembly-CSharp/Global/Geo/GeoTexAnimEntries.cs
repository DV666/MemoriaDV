using Memoria.Assets;
using Memoria.Prime;
using System;
using System.Collections.Generic;

namespace FF9
{
    public class GeoTexAnimEntries
    {
        public static readonly Dictionary<String, List<GeoTexAnimData>> GeoTexAnimDict;

        static GeoTexAnimEntries()
        {
            GeoTexAnimDict = LoadEntries();
        }

        private static Dictionary<String, List<GeoTexAnimData>> LoadEntries()
        {
            try
            {
                String inputPath = DataResources.Models.PureDirectory + DataResources.Models.GeoTexAnimEntriesFile;
                Dictionary<String, List<GeoTexAnimData>> result = new Dictionary<String, List<GeoTexAnimData>>();
                foreach (GeoTexAnimData[] geoanimdata in AssetManager.EnumerateCsvFromLowToHigh<GeoTexAnimData>(inputPath))
                    foreach (GeoTexAnimData entry in geoanimdata)
                    {
                        if (!result.ContainsKey(entry.ModelName))
                            result[entry.ModelName] = new List<GeoTexAnimData>();

                        result[entry.ModelName].Add(entry);
                    }

                if (result.Count > 0)
                    Log.Message($"[GeoTexAnimEntries] Loaded texture animations CSV with {result.Count} entries.");

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[GeoTexAnimEntries] Failed to load texture animations CSV.");
                return null;
            }
        }
    }
}
