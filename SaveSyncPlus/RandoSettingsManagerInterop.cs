using Modding;
using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;
using System.Collections.Generic;

namespace SaveSyncPlus
{
    public class RandoSettingsManagerData
    {
        public Dictionary<string, SyncedDataPack> ActivePacks = new();

        public void Apply()
        {
            foreach ((string packName, SyncedDataPack pack) in ActivePacks)
            {
                SaveSyncPlus.SavePack(packName, pack);
            }
            SaveSyncPlus.GS.EnabledPackNames.Clear(); 
            SaveSyncPlus.GS.EnabledPackNames.AddRange(ActivePacks.Keys);

            MenuHolder.Instance.ResetMenu();
        }

        public static RandoSettingsManagerData Create()
        {
            RandoSettingsManagerData data = new();

            foreach (string packName in SaveSyncPlus.GS.EnabledPackNames)
            {
                data.ActivePacks.Add(packName, SaveSyncPlus.LoadPack(packName));
            }

            return data;
        }
    }

    internal static class RandoSettingsManagerInterop
    {
        public static void Hook()
        {
            RandoSettingsManagerMod.Instance.RegisterConnection(new RandoPlusSettingsProxy());
        }
    }

    internal class RandoPlusSettingsProxy : RandoSettingsProxy<RandoSettingsManagerData, string>
    {
        public override string ModKey => SaveSyncPlus.instance.GetName();

        public override VersioningPolicy<string> VersioningPolicy { get; }
            = new EqualityVersioningPolicy<string>(SaveSyncPlus.instance.GetVersion());

        public override void ReceiveSettings(RandoSettingsManagerData settings)
        {
            settings ??= new();

            settings.Apply();
        }

        public override bool TryProvideSettings(out RandoSettingsManagerData settings)
        {
            if (!SaveSyncPlus.GS.IsEnabled())
            {
                settings = default;
                return false;
            }

            settings = RandoSettingsManagerData.Create();
            return true;
        }
    }
}