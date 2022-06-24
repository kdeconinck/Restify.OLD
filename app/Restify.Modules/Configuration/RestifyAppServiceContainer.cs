// =====================================================================================================================
// = LICENSE:       Copyright (c) 2022 Kevin De Coninck
// =
// =                Permission is hereby granted, free of charge, to any person
// =                obtaining a copy of this software and associated documentation
// =                files (the "Software"), to deal in the Software without
// =                restriction, including without limitation the rights to use,
// =                copy, modify, merge, publish, distribute, sublicense, and/or sell
// =                copies of the Software, and to permit persons to whom the
// =                Software is furnished to do so, subject to the following
// =                conditions:
// =
// =                The above copyright notice and this permission notice shall be
// =                included in all copies or substantial portions of the Software.
// =
// =                THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// =                EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// =                OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// =                NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// =                HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// =                WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// =                FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// =                OTHER DEALINGS IN THE SOFTWARE.
// =====================================================================================================================
namespace Restify.Modules.Configuration;

using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

using static Restify.Modules.Properties.Supressions;

internal sealed class RestifyAppServiceContainer
{
    private readonly IServiceCollection services;

    internal RestifyAppServiceContainer(IServiceCollection services)
    {
        this.services = services;
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    internal void RegisterSingletonService<TService>()
        where TService : class
    {
        _ = this.services.AddSingleton<TService>();
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    internal void RegisterScopedService<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        _ = this.services.AddScoped<TService, TImplementation>();
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    internal TService ResolveService<TService>()
        where TService : notnull
    {
        return this.services.BuildServiceProvider().GetRequiredService<TService>();
    }
}
