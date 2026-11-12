using System.Diagnostics;
using System.IO;

namespace Torres.Client
{
    internal sealed class DoomLauncher
    {
        private const string WadArgument = "-iwad";

        private readonly ClientSettings _clientSettings;

        internal DoomLauncher(ClientSettings clientSettings)
        {
            _clientSettings = clientSettings;
        }

        internal void LaunchIfEnabled()
        {
            bool canLaunch = _clientSettings.LaunchesDoomAfterPlacement && File.Exists(_clientSettings.DoomProgramPath) && File.Exists(_clientSettings.DoomWadPath);
            if (!canLaunch)
            {
                return;
            }

            var doomStartInfo = new ProcessStartInfo(_clientSettings.DoomProgramPath)
            {
                UseShellExecute = false,
            };
            doomStartInfo.ArgumentList.Add(WadArgument);
            doomStartInfo.ArgumentList.Add(_clientSettings.DoomWadPath);
            using Process? doomProcess = Process.Start(doomStartInfo);
        }
    }
}
