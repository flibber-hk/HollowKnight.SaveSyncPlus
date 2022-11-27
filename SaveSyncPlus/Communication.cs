using MultiWorldLib;
using System;

namespace SaveSyncPlus
{
    internal static class Communication
    {
        private const char Sep = '@';
        private const string PlayerDataBool = nameof(SaveSyncPlus) + "PlayerDataBool";
        private const string PlayerDataInt = nameof(SaveSyncPlus) + "PlayerDataInt";
        private const string SceneDataBool = nameof(SaveSyncPlus) + "SceneDataBool";

        public static event Action<string, bool> SetPlayerDataBool;
        public static event Action<string, int> SetPlayerDataInt;
        public static event Action<SceneDataKey, PersistentBoolData> SetSceneDataBool;

        public static void Hook()
        {
            ItemSyncMod.ItemSyncMod.Connection.OnDataReceived += OnDataReceived;
        }

        private static void OnDataReceived(DataReceivedEvent dataReceivedEvent)
        {
            if (dataReceivedEvent.Handled) return;
            if (!dataReceivedEvent.Label.StartsWith(nameof(SaveSyncPlus))) return;

            if (TryReceiveData(dataReceivedEvent.Label, dataReceivedEvent.Content))
            {
                dataReceivedEvent.Handled = true;
            }
        }

        public static bool TryReceiveData(string label, string content)
        {
            string[] pieces = content.Split(Sep);

            switch (label)
            {
                case PlayerDataBool:
                    string boolName = pieces[0];
                    bool boolValue = Convert.ToBoolean(pieces[1]);
                    SetPlayerDataBool?.Invoke(boolName, boolValue);
                    return true;

                case PlayerDataInt:
                    string intName = pieces[0];
                    int intValue = Convert.ToInt32(pieces[1]);
                    SetPlayerDataInt?.Invoke(intName, intValue);
                    return true;

                case SceneDataBool:
                    string sceneName = pieces[0];
                    string id = pieces[1];
                    bool activated = Convert.ToBoolean(pieces[2]);
                    bool semiPersistent = Convert.ToBoolean(pieces[3]);
                    SetSceneDataBool?.Invoke(new(sceneName, id), new PersistentBoolData()
                    {
                        sceneName = sceneName,
                        id = id,
                        activated = activated,
                        semiPersistent = semiPersistent
                    });
                    return true;
            }

            return false;
        }

        public static void SendPDBool(string pdBoolName, bool? value = null)
        {
            bool sentValue = value ?? PlayerData.instance.GetBool(pdBoolName);

            Send(PlayerDataBool, pdBoolName, Convert.ToString(sentValue));
        }
        public static void SendPDInt(string pdIntName, int? value = null)
        {
            int sentValue = value ?? PlayerData.instance.GetInt(pdIntName);

            Send(PlayerDataInt, pdIntName, Convert.ToString(sentValue));
        }
        public static void SendSDBool(string sceneName, string id)
        {
            PersistentBoolData pbdPrefab = new() { sceneName = sceneName, id = id };
            PersistentBoolData pbd = SceneData.instance.FindMyState(pbdPrefab);
            SendSDBool(pbd);
        }

        public static void SendSDBool(PersistentBoolData state)
        {
            Send(SceneDataBool, state.sceneName, state.id, Convert.ToString(state.activated), Convert.ToString(state.semiPersistent));
        }

        private static void Send(string label, params string[] args)
        {
            if (!ItemSyncMod.ItemSyncMod.Connection?.IsConnected() ?? false)
            {
                return;
            }

            string message = string.Join(Sep.ToString(), args);

            ItemSyncMod.ItemSyncMod.Connection.SendDataToAll(label, message);
        }
    }
}
