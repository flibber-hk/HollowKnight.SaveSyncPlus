using System.Collections.Generic;

namespace SaveSyncPlus
{
    public class GlobalSettings
    {
        public List<string> EnabledPackNames = new();

        public bool IsEnabled() => EnabledPackNames.Count > 0;
    }
}
