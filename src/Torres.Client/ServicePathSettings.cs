using System.Linq;

namespace Torres.Client
{
    internal sealed class ServicePathSettings
    {
        public string ServerStatus { get; set; } = string.Empty;

        public string Account { get; set; } = string.Empty;

        public string Ranking { get; set; } = string.Empty;

        public string GameSession { get; set; } = string.Empty;

        internal bool IsComplete()
        {
            string[] servicePaths = [ServerStatus, Account, Ranking, GameSession];
            bool isComplete = servicePaths.All(servicePath => !string.IsNullOrWhiteSpace(servicePath));

            return isComplete;
        }
    }
}
