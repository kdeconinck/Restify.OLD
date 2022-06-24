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

using Restify.Modules.Middleware.Abstractions;
using Restify.Modules.Models.Collections;
using Restify.Modules.Routing.Abstractions;
using Restify.Modules.Services.Abstractions;

using static Restify.Modules.Properties.Supressions;

internal sealed class RestifyAppConfiguration
{
    private readonly RestifyAppServiceContainer serviceContainer;

    internal RestifyAppConfiguration(RestifyAppServiceContainer serviceContainer)
    {
        this.serviceContainer = serviceContainer;
        this.ServicesModules = new RegisteredServicesModulesCollection();
        this.RoutingModules = new RegisteredRoutingModulesCollection();
        this.MiddlewareModules = new RegisteredMiddlewareModulesCollection();
    }

    internal RegisteredServicesModulesCollection ServicesModules
    {
        get;
    }

    internal RegisteredRoutingModulesCollection RoutingModules
    {
        get;
    }

    internal RegisteredMiddlewareModulesCollection MiddlewareModules
    {
        get;
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    internal void RegisterServicesModule<TServicesModule>(IServiceCollection serviceCollection)
        where TServicesModule : class, IServicesModule
    {
        this.serviceContainer.RegisterSingletonService<TServicesModule>();
        this.serviceContainer.ResolveService<TServicesModule>().RegisterServices(serviceCollection);
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    internal void RegisterRoutingModule<TRoutingModule>()
        where TRoutingModule : class, IRoutingModule
    {
        this.serviceContainer.RegisterSingletonService<TRoutingModule>();
        this.RoutingModules.Register(this.serviceContainer.ResolveService<TRoutingModule>());
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    internal void RegisterMiddlewareModule<TMiddlewareModule>()
        where TMiddlewareModule : class, IMiddlewareModule
    {
        this.serviceContainer.RegisterSingletonService<TMiddlewareModule>();
        this.MiddlewareModules.Register(this.serviceContainer.ResolveService<TMiddlewareModule>());
    }
}
