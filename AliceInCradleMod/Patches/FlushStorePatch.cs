using nel;
using System;

namespace BetterExperience
{
    internal partial class Patchs
    {
        public static class FlushStorePatch
        {
            public static void Flush()
            {
                try
                {
                    StoreManager.FlushAll();
                }
                catch (Exception ex)
                {
                    HLog.Error("FlushAllStore failed: " + ex.ToString());
                }
            }
        }
    }
}
