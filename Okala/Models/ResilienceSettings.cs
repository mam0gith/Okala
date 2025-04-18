namespace Okala.Models
{
    public class ResilienceSettings
    {
        public int RetryCount { get; set; } = 3;
        public int ExceptionsAllowedBeforeBreaking { get; set; } = 5;
        public int BreakDurationSeconds { get; set; } = 30;
    }
}
