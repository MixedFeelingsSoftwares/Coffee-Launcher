using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WoW_Coffee_Launcher.Core.Downloader
{
    class Patcher
    {
        public const string PatchesURL = "https://www.dropbox.com/s/t3e7bz6u6axfpmx/patches.json?dl=1";

        public static string GetDataPath { get { return $"{Environment.CurrentDirectory}/Data/"; } }

        public static void DownloadPatches()
        {
            Task.Run(async() =>
            {
                State.CurrentState = State.state.Deleting;
                using (WebClient wc = new WebClient())
                {
                    string json = await wc.DownloadStringTaskAsync(PatchesURL);

                    Patches patch = JsonConvert.DeserializeObject<Patches>(json);
                    Installer.DeletePreviousPatch(new Action(() => 
                    {
                        State.CurrentState = State.state.Installing;
                        patch.Install();
                        patch.SetRealmlist();
                    }));
                }
            });
        }

        public static async Task<string> GetRealmlistIP()
        {
            return await Task.Run(async () =>
            {
                using (WebClient wc = new WebClient())
                {
                    string json = await wc.DownloadStringTaskAsync(PatchesURL);

                    Patches patch = JsonConvert.DeserializeObject<Patches>(json);
                    return patch.Realmlist;
                }
            });
        }
    }

    public static class State
    {
        private static state cState = state.Default;
        public static event System.EventHandler<state> OnStateChanged;

        public static state CurrentState
        {
            get
            {
                return cState;
            }
            set
            {
                cState = value;
                StateChange();
            }
        }

        private static void StateChange()
        {
            OnStateChanged?.Invoke(null, cState);
        }

        public enum state
        {
            Default = 0,
            Deleting = 1,
            Installing = 2,
            Patching = 3,
            Ready = 4,
        }
    }

    public static class Installer
    {
        public static ProgressBar downloadBar = null;

        public static async void Install(this Patches patch)
        {
            if (patch == null) { return; }
            for (int i = 0; i < patch.patches.Count; i++)
            {
                Patch pth = patch.patches[i];
                await Task.Run(async() =>
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadProgressChanged += (s, g) =>
                        {
                            downloadBar?.Dispatcher.BeginInvoke(new Action(() => downloadBar.SetPercent(g.ProgressPercentage, 1f)));
                        };
                        await wc.DownloadFileTaskAsync(pth.Url, $"{Patcher.GetDataPath}{pth.Name}");
                        Debug.WriteLine(pth.Name);
                    }
                });
            }
            State.CurrentState = State.state.Ready;
        }

        public static void SetRealmlist(this Patches patch)
        {
            string path = $"{Environment.CurrentDirectory}/data/";
            List<string> realmlist = Directory.EnumerateFiles(path, "*.wtf", SearchOption.AllDirectories).ToList();
            foreach (string lst in realmlist.FindAll(x => x.Contains("realmlist")))
            {
                File.WriteAllText(lst, $"set realmlist {patch.Realmlist}");
            }
        }

        public static void DeletePreviousPatch(Action callback)
        {
            if (isWoWDiretory())
            {
                List<string> datas = Directory.EnumerateFiles($"{Environment.CurrentDirectory}/Data/", "*.MPQ", SearchOption.TopDirectoryOnly).ToList();
                foreach (string dat in datas)
                {
                    string name = Path.GetFileNameWithoutExtension(dat);

                    bool matches = Regex.IsMatch(name, @"(Patch-)[A-Za-z]");
                    if (matches)
                    {
                        Debug.WriteLine($"Name: {name} - {matches}");
                        File.Delete(dat);
                    }
                }
                callback();
            }
        }

        public static bool isWoWDiretory()
        {
            return File.Exists($"{Environment.CurrentDirectory}/Wow.exe") && Directory.Exists($"{Environment.CurrentDirectory}/Data/");
        }

        public static bool isWoWDiretory(this string path)
        {
            return File.Exists($"{path}/Wow.exe") && Directory.Exists($"{path}/Data/");
        }
    }

    public static class Interface
    {
        public static ColorAnimation AnimationByStatus(this ContentControl ctrl, bool status)
        {
            ColorAnimation cAnim = null;

            switch (status)
            {
                case true:
                    cAnim = new ColorAnimation()
                    {
                        From = ((SolidColorBrush)ctrl.Foreground).Color,
                        To = (Color)ColorConverter.ConvertFromString("#FF4AC345"),
                        Duration = TimeSpan.FromSeconds(1),
                    };
                    break;

                case false:
                    cAnim = new ColorAnimation()
                    {
                        From = ((SolidColorBrush)ctrl.Foreground).Color,
                        To = (Color)ColorConverter.ConvertFromString("#FF911818"),
                        Duration = TimeSpan.FromSeconds(1),
                    };
                    break;
            }
            return cAnim;
        }
    }

    public class Patch
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Patches
    {
        [JsonProperty("realmlist")]
        public string Realmlist { get; set; }

        [JsonProperty("ver")]
        public string Version { get; set; }

        public List<Patch> patches { get; set; }
    }
}
