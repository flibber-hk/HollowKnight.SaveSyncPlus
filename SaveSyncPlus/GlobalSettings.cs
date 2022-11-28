using System;
using System.Collections.Generic;
using System.Linq;

namespace SaveSyncPlus
{
    public class GlobalSettings
    {
        public List<string> EnabledPackNames = new();

        public bool IsEnabled() => EnabledPackNames.Count > 0;

        internal void Clamp()
        {
            EnabledPackNames ??= new();
            EnabledPackNames = EnabledPackNames.Where(p => SaveSyncPlus.CheckPack(p)).ToList();
        }
    }
}
