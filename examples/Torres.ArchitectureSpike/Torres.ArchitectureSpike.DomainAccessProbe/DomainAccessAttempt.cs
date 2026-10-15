using Torres.ArchitectureSpike.Domain;

namespace Torres.ArchitectureSpike.DomainAccessProbe
{
    public sealed class DomainAccessAttempt
    {
        public StepOutcome AttemptedOutcome { get; } = StepOutcome.Applied;
    }
}
