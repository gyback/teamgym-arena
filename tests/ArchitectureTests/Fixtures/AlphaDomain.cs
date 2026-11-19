using Microsoft.AspNetCore.Http;

namespace TeamGym.Arena.ArchitectureTests.Fixtures.Modules.Alpha.Domain;

/// <summary>Allowed: the domain uses only the base class library.</summary>
internal interface IPureAggregate
{
    DateTimeOffset CreatedAt { get; }
}

/// <summary>Violation: the domain depends on ASP.NET Core.</summary>
internal interface IHttpAwareAggregate
{
    HttpContext Context { get; }
}
