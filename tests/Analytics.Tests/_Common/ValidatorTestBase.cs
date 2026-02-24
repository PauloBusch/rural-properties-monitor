using FluentValidation;
using NSubstituteAutoMocker;

namespace Analytics.Tests._Common;
public abstract class ValidatorTestBase<TValidator>(AnalyticsFixture fixture) : TestBase(fixture)
    where TValidator : class, IValidator
{
    protected readonly NSubstituteAutoMocker<TValidator> AutoMocker = new();
    protected TValidator Validator => AutoMocker.ClassUnderTest;
}
