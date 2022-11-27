using ItemChanger.Util;
using Modding;
using Newtonsoft.Json;
using System.Collections.Generic;
using Module = ItemChanger.Modules.Module;

namespace SaveSyncPlus
{
    public class SyncableSaveDataModule : Module
    {
        /* We prevent re-sending of received data in two ways:
         * 1. Before assigning incoming data, we store the value in a private dictionary.
         *    When the data is then assigned, it is removed from the dictionary instead of being sent.
         * 2. When assigning incoming data, we check if it will change anything. 
         *    If not (such as in the event the first check failed), 
         *    then we don't reassign so it won't be re-sent.
         *    This check will apply in the case of scene data, which gets applied twice.
         */

        public readonly HashSet<string> PdBoolNames = new();
        public readonly HashSet<string> PdIntNames = new();
        public readonly HashSet<SceneDataKey> SceneDataBools = new();

        public bool SyncDeactivatedSemiPersistentData = false;

        [JsonIgnore] private readonly Dictionary<string, bool> _pdBoolData = new();
        [JsonIgnore] private readonly Dictionary<string, int> _pdIntData = new();
        [JsonIgnore] private readonly Dictionary<SceneDataKey, PersistentBoolData> _sdBoolData = new();

        public override void Initialize()
        {
            // TODO - make sure not to resend data

            ModHooks.SetPlayerBoolHook += SyncBools;
            ModHooks.SetPlayerIntHook += SyncInts;
            On.SceneData.SaveMyState_PersistentBoolData += SyncSDBools;

            Communication.SetPlayerDataBool += ReceiveBool;
            Communication.SetPlayerDataInt += ReceiveInt;
            Communication.SetSceneDataBool += ReceiveSDBool;
        }

        public override void Unload()
        {
            ModHooks.SetPlayerBoolHook -= SyncBools;
            ModHooks.SetPlayerIntHook -= SyncInts;
            On.SceneData.SaveMyState_PersistentBoolData -= SyncSDBools;

            Communication.SetPlayerDataBool -= ReceiveBool;
            Communication.SetPlayerDataInt -= ReceiveInt;
            Communication.SetSceneDataBool -= ReceiveSDBool;
        }

        private void ReceiveBool(string key, bool value)
        {
            if (!PdBoolNames.Contains(key)) return;
            if (PlayerData.instance.GetBool(key) == value) return;

            _pdBoolData.Add(key, value);
            PlayerData.instance.SetBool(key, value);
        }

        private void ReceiveInt(string key, int value)
        {
            if (!PdIntNames.Contains(key)) return;
            if (PlayerData.instance.GetInt(key) == value) return;
            
            _pdIntData.Add(key, value);
            PlayerData.instance.SetInt(key, value);
        }

        private void ReceiveSDBool(SceneDataKey key, PersistentBoolData pbd)
        {
            if (!SceneDataBools.Contains(key)) return;

            if (!SyncDeactivatedSemiPersistentData
                && !pbd.activated
                && pbd.semiPersistent)
            {
                return;
            }

            _sdBoolData.Add(key, pbd);
            SceneDataUtil.Save(pbd.sceneName, pbd.id, pbd.activated, pbd.semiPersistent);            
        }

        private bool SyncBools(string name, bool orig)
        {
            if (!PdBoolNames.Contains(name)) return orig;

            if (_pdBoolData.TryGetValue(name, out bool data) && data == orig)
            {
                _pdBoolData.Remove(name);
                return orig;
            }

            Communication.SendPDBool(name, orig);
            return orig;
        }

        private int SyncInts(string name, int orig)
        {
            if (!PdIntNames.Contains(name)) return orig;

            if (_pdIntData.TryGetValue(name, out int data) && data == orig)
            {
                _pdIntData.Remove(name);
                return orig;
            }

            Communication.SendPDInt(name, orig);
            return orig;
        }

        private void SyncSDBools(On.SceneData.orig_SaveMyState_PersistentBoolData orig, SceneData self, PersistentBoolData pbd)
        {
            orig(self, pbd);

            SceneDataKey key = SceneDataKey.FromPersistentBoolData(pbd);

            if (!SceneDataBools.Contains(key)) return;

            if (!SyncDeactivatedSemiPersistentData
                && !pbd.activated
                && pbd.semiPersistent)
            {
                return;
            }

            if (_sdBoolData.TryGetValue(key, out PersistentBoolData data)
                && data.activated == pbd.activated
                && data.semiPersistent == pbd.semiPersistent)
            {
                _sdBoolData.Remove(key);
                return;
            }

            Communication.SendSDBool(pbd);
        }
    }
}
