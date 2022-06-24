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
namespace Restify.Modules.Application;

using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Restify.Core.Application.Abstractions;
using Restify.Core.Application.Abstractions.Configuration;
using Restify.Core.Application.Abstractions.Startup;
using Restify.Modules.Abstractions;
using Restify.Modules.Middleware.Abstractions;
using Restify.Modules.Models.Collections;
using Restify.Modules.Routing.Abstractions;

using static Restify.Modules.Properties.Supressions;

internal sealed class RestifyApp : IRestifyApp
{
    private readonly IServiceCollection services;
    private readonly WebApplication webApplication;
    private readonly RegisteredMiddlewareModulesCollection middlewareModules;
    private readonly RegisteredRouteModulesCollection routeModules;
    private readonly RegisteredServicesModulesCollection servicesModules;

    internal RestifyApp(WebApplicationBuilder webApplicationBuilder)
    {
        this.services = webApplicationBuilder.Services;
        this.Host = webApplicationBuilder.Host;
        this.middlewareModules = new RegisteredMiddlewareModulesCollection();
        this.routeModules = new RegisteredRouteModulesCollection();
        this.servicesModules = new RegisteredServicesModulesCollection();

        this.webApplication = webApplicationBuilder.Build();
    }

    private IRestifyStartupAction? OnBeforeRunAction
    {
        get; set;
    }

    public ConfigureHostBuilder Host
    {
        get;
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp RegisterModule<TServicesModule>()
        where TServicesModule : IServicesModule, new()
    {
        return this.RegisterServicesModule<TServicesModule>();
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp RegisterModule<TServicesModule, TRouteModule>()
        where TServicesModule : IServicesModule, new()
        where TRouteModule : IRoutingModule, new()
    {
        _ = this.RegisterServicesModule<TServicesModule>();
        return this.RegisterRoutingModule<TRouteModule>();
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp RegisterModule<TServicesModule, TRouting, TMiddlewareModule>()
        where TServicesModule : IServicesModule, new()
        where TRouting : IRoutingModule, new()
        where TMiddlewareModule : IMiddlewareModule, new()
    {
        _ = this.RegisterServicesModule<TServicesModule>();
        _ = this.RegisterRoutingModule<TRouting>();

        return this.RegisterMiddlewareModule<TMiddlewareModule>();
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp RegisterServicesModule<TModule>()
        where TModule : IServicesModule, new()
    {
        this.servicesModules.Register<TModule>();

        return this;
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp RegisterRoutingModule<TRoutingModule>()
        where TRoutingModule : IRoutingModule, new()
    {
        this.routeModules.Register<TRoutingModule>();

        return this;
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp RegisterMiddlewareModule<TMiddlewareModule>()
        where TMiddlewareModule : IMiddlewareModule, new()
    {
        this.middlewareModules.Register<TMiddlewareModule>();

        return this;
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp UseConfigurationProvider<TConfiguration>()
        where TConfiguration : IRestifyConfigurationProvider, new()
    {
        return new TConfiguration().Apply(this, this.webApplication.Configuration);
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp OnBeforeStartup<TStartupAction>()
        where TStartupAction : IRestifyStartupAction
    {
        this.OnBeforeRunAction = this.ResolveService<TStartupAction>();

        return this;
    }

    public async Task RunAsync()
    {
        this.RegisterModule();

        if (this.OnBeforeRunAction != null)
        {
            await this.OnBeforeRunAction.RunAsync(this.webApplication).ConfigureAwait(false);
        }

        await this.webApplication.RunAsync().ConfigureAwait(false);
    }

    private void RegisterModule()
    {
        this.servicesModules.RegisterServices(this.services, this.ResolveService<IConfiguration>());
        this.routeModules.RegisterRoutes(this.webApplication);
        this.middlewareModules.Use(this.webApplication);
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    private TService ResolveService<TService>()
        where TService : notnull
    {
        using IServiceScope serviceProviderScope = this.webApplication.Services.CreateScope();

        return this.webApplication.Services.GetRequiredService<TService>();
    }
}
