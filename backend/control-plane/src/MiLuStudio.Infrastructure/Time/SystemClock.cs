namespace MiLuStudio.Infrastructure.Time;

using MiLuStudio.Application.Abstractions;

public sealed class SystemClock : IClock
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
