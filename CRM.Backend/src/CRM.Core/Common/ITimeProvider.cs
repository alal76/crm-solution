namespace CRM.Core.Common
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}