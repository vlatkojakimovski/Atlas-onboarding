@echo off
echo ============================================
echo Installing NuGet Packages for Atlas Solution
echo ============================================
echo.

echo [1/5] Installing packages for Atlas.Api...
dotnet add Atlas.Api\Atlas.Api.csproj package Microsoft.AspNetCore.OpenApi
dotnet add Atlas.Api\Atlas.Api.csproj package Swashbuckle.AspNetCore
dotnet add Atlas.Api\Atlas.Api.csproj package Serilog.AspNetCore
dotnet add Atlas.Api\Atlas.Api.csproj package Serilog.Sinks.Seq
dotnet add Atlas.Api\Atlas.Api.csproj package Serilog.Enrichers.Environment
echo.

echo [2/5] Installing packages for Atlas.Application...
dotnet add Atlas.Application\Atlas.Application.csproj package FluentValidation
dotnet add Atlas.Application\Atlas.Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions
echo.

echo [3/5] Installing packages for Atlas.Infrastructure...
dotnet add Atlas.Infrastructure\Atlas.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer
dotnet add Atlas.Infrastructure\Atlas.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add Atlas.Infrastructure\Atlas.Infrastructure.csproj package Microsoft.Extensions.Http.Polly
echo.

echo [4/5] Installing packages for Atlas.Tests.Unit...
dotnet add Atlas.Tests.Unit\Atlas.Tests.Unit.csproj package Moq
dotnet add Atlas.Tests.Unit\Atlas.Tests.Unit.csproj package FluentAssertions
dotnet add Atlas.Tests.Unit\Atlas.Tests.Unit.csproj package FsCheck
dotnet add Atlas.Tests.Unit\Atlas.Tests.Unit.csproj package FsCheck.Xunit
echo.

echo [5/5] Installing packages for Atlas.Tests.Integration...
dotnet add Atlas.Tests.Integration\Atlas.Tests.Integration.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add Atlas.Tests.Integration\Atlas.Tests.Integration.csproj package Microsoft.EntityFrameworkCore.InMemory
echo.

echo ============================================
echo All packages installed successfully!
echo ============================================
echo.
echo You can now build the solution in Visual Studio.
pause
