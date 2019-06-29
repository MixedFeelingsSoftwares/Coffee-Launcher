using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WoW_Coffee_Launcher.Core.Downloader
{
    class Patcher
    {
        public const string PatchesURL = "https://www.dropbox.com/s/t3e7bz6u6axfpmx/patches.json?dl=1";

        public static void DownloadPatches()
        {
            Task.Run(async() =>
            {
                using (WebClient wc = new WebClient())
                {
                    string json = await wc.DownloadStringTaskAsync(PatchesURL);
                }
            });
        }
    }
}
