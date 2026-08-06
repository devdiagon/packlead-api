using Packlead.Domain.Exceptions;

namespace Packlead.Domain.Tests;
public class DomainExceptionHierarchyTests
{
    [Fact]
    public void AllDomainExceptions_InheritFromDomainException()
    {
        var domainAssembly = typeof(DomainExceptions).Assembly;

        var exceptionTypes = domainAssembly
            .GetTypes()
            .Where(t => t.Namespace == "Packlead.Domain.Exceptions")
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => typeof(Exception).IsAssignableFrom(t))
            .ToList();

        Assert.NotEmpty(exceptionTypes);

        var offenders = exceptionTypes
            .Where(t => !typeof(DomainExceptions).IsAssignableFrom(t))
            .Select(t => t.FullName)
            .ToList();

        Assert.True(
            offenders.Count == 0,
            $"Estos tipos en Domain/Exceptions no heredan de DomainException: {string.Join(", ", offenders)}");
    }
}
