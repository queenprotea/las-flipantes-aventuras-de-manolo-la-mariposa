using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Torres.Client.Threading;

namespace Torres.Client
{
    internal sealed class DoomLauncher
    {
        private const string WadArgument = "-iwad";

        private readonly ClientSettings _clientSettings;

        private Task _doomLaunch = Task.CompletedTask;

        internal DoomLauncher(ClientSettings clientSettings)
        {
            _clientSettings = clientSettings;
        }

        internal void LaunchIfEnabled()
        {
            bool canLaunch = _clientSettings.LaunchesDoomAfterPlacement && File.Exists(_clientSettings.DoomProgramPath) && File.Exists(_clientSettings.DoomWadPath);
            if (!canLaunch || !_doomLaunch.IsCompleted)
            {
                return;
            }

            var doomStartInfo = new ProcessStartInfo(_clientSettings.DoomProgramPath) { UseShellExecute = false };
            doomStartInfo.ArgumentList.Add(WadArgument);
            doomStartInfo.ArgumentList.Add(_clientSettings.DoomWadPath);
            _doomLaunch = new BackgroundWork<ProcessStartInfo>(StartDoom, doomStartInfo).StartAsync();
        }

        private static void StartDoom(ProcessStartInfo doomStartInfo)
        {
            using Process? doomProcess = Process.Start(doomStartInfo);
        }
    }
}
