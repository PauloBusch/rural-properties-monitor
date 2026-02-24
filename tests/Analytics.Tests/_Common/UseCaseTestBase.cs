using NSubstituteAutoMocker;
using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics.Tests._Common;
public abstract class UseCaseTestBase<TUseCase>(AnalyticsFixture fixture) : TestBase(fixture)
    where TUseCase : class
{
    protected readonly NSubstituteAutoMocker<TUseCase> AutoMocker = new();
    protected TUseCase UseCase => AutoMocker.ClassUnderTest;
    protected TMock GetMock<TMock>() where TMock : class => AutoMocker.Get<TMock>();
}
