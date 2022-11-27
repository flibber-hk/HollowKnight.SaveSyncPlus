namespace SaveSyncPlus
{
    public record SceneDataKey(string SceneName, string Id)
    {
        public static SceneDataKey FromPersistentBoolData(PersistentBoolData pbd) => new(pbd.sceneName, pbd.id);
    }
}
