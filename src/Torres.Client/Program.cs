using System.Threading.Tasks;

using Torres.Client;

Task<ClientSettings?> settingsReading = ClientSettings.ReadAsync();
Task.WaitAny(settingsReading);
ClientSettings? clientSettings = settingsReading.IsCompletedSuccessfully ? settingsReading.Result : null;
if (clientSettings is null)
{
    using var settingsFailureGame = new SettingsFailureGame();
    settingsFailureGame.Run();
}
else
{
    using var game = new TorresGame(clientSettings);
    game.Run();
}
