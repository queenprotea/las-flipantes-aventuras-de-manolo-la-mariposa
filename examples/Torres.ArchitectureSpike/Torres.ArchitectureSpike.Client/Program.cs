using System;
using System.Globalization;
using System.ServiceModel;
using System.Threading.Tasks;

using Torres.ArchitectureSpike.Client;

const string ServerUnreachableMessage = "The spike server could not be reached: {0}";
const int ServerUnreachableExitCode = 100;

ClientSettings clientSettings = ClientSettings.Load();
bool isVictimRun = (args.Length == SpikeArguments.VictimArgumentCount) && (args[0] == SpikeArguments.Victim);
int exitCode;
if (isVictimRun)
{
    exitCode = await VictimRun.RunAsync(clientSettings, args[1]);
}
else
{
    exitCode = await RunVerificationAsync(clientSettings);
}

return exitCode;

static async Task<int> RunVerificationAsync(ClientSettings clientSettings)
{
    var spikeVerification = new SpikeVerification(clientSettings);
    int failureCount;
    try
    {
        failureCount = await spikeVerification.RunAsync();
    }
    catch (Exception exception) when ((exception is CommunicationException) || (exception is TimeoutException))
    {
        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, ServerUnreachableMessage, exception.Message));
        failureCount = ServerUnreachableExitCode;
    }

    return failureCount;
}
