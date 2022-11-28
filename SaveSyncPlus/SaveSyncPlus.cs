using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItemChanger;
using Modding;
using Newtonsoft.Json;
using RandomizerCore.Extensions;
using RandomizerMod.Logging;
using RandomizerMod.RC;
using UnityEngine;

namespace SaveSyncPlus
{
    public class SaveSyncPlus : Mod, IGlobalSettings<GlobalSettings>
    {
        internal static SaveSyncPlus instance;

        public static GlobalSettings GS = new();
        public void OnLoadGlobal(GlobalSettings s)
        {
            GS = s;
            GS.Clamp();
        }
        public GlobalSettings OnSaveGlobal() => GS;

        internal static readonly string ModDirectory = Path.GetDirectoryName(typeof(SaveSyncPlus).Assembly.Location);

        internal static string PackPath(string packName) => Path.Combine(ModDirectory, packName + ".json");
        internal static SyncedDataPack LoadPack(string packName)
        {
            string text = File.ReadAllText(PackPath(packName));
            return JsonConvert.DeserializeObject<SyncedDataPack>(text);
        }
        internal static void SavePack(string packName, SyncedDataPack pack) => Finder.Serialize(PackPath(packName), pack);

        internal static bool CheckPack(string packName) => File.Exists(PackPath(packName));

        internal static IEnumerable<string> GetPackNames()
        {
            DirectoryInfo main = new(ModDirectory);

            foreach (FileInfo f in main.EnumerateFiles("*.json"))
            {
                yield return f.Name.Substring(0, f.Name.Length - 5);
            }
        }

        public SaveSyncPlus() : base(null)
        {
            instance = this;
        }
        
        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }
        
        public override void Initialize()
        {
            Log("Initializing Mod...");

            RandoController.OnCalculateHash += OnCalculateHash;
            RandoController.OnExportCompleted += OnExportCompleted;
            SettingsLog.AfterLogSettings += AfterLogSettings;
            RequestBuilder.OnUpdate.Subscribe(-10000f, LoadPacks);

            MenuHolder.Hook();
            if (ModHooks.GetMod("RandoSettingsManager") is not null)
            {
                RandoSettingsManagerInterop.Hook();
            }
        }

        // Cache the synced data used by the hash and the export
        private SyncedDataPack _combinedDataPack;

        private void LoadPacks(RequestBuilder rb)
        {
            GS.Clamp();

            _combinedDataPack = new();

            if (!GS.IsEnabled()) return;

            foreach (string packName in GS.EnabledPackNames)
            {
                _combinedDataPack.AddFrom(LoadPack(packName));
            }
        }

        private void AfterLogSettings(LogArguments args, TextWriter tw)
        {
            if (!GS.IsEnabled())
            {
                tw.WriteLine("SaveSyncPlus disabled");
                return;
            }

            tw.WriteLine("SaveSyncPlus Synced Packs:");
            foreach (string packName in GS.EnabledPackNames.OrderBy(x => x))
            {
                tw.WriteLine(" - " + packName);
            }
        }

        private void OnExportCompleted(RandoController rc)
        {
            if (!ItemSyncMod.ItemSyncMod.ISSettings.IsItemSync) return;
            if (!GS.IsEnabled()) return;

            SyncableSaveDataModule mod = ItemChangerMod.Modules.Add<SyncableSaveDataModule>();
            mod.PdBoolNames.UnionWith(_combinedDataPack.PdBoolNames);
            mod.PdIntNames.UnionWith(_combinedDataPack.PdIntNames);
            mod.SceneDataBools.UnionWith(_combinedDataPack.SdBoolNames);
        }

        private int OnCalculateHash(RandoController rc, int hashValue)
        {
            if (!GS.IsEnabled()) return 0;

            int newHash = JsonConvert.SerializeObject(_combinedDataPack).GetStableHashCode();
            return newHash;
        }
    }
}