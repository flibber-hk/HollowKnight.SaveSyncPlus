using System.Collections.Generic;
using System.Linq;

namespace SaveSyncPlus
{
    public class SyncedDataPack
    {
        public HashSet<string> PdBoolNames = new();
        public HashSet<string> PdIntNames = new();
        public HashSet<SceneDataKey> SdBoolNames = new();

        public void AddFrom(SyncedDataPack pack)
        {
            PdBoolNames.UnionWith(pack?.PdBoolNames ?? Enumerable.Empty<string>());
            PdIntNames.UnionWith(pack?.PdIntNames ?? Enumerable.Empty<string>());
            SdBoolNames.UnionWith(pack?.SdBoolNames ?? Enumerable.Empty<SceneDataKey>());
        }
    }
}
