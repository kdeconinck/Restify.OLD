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
using Microsoft.Extensions.DependencyInjection;

using Restify.Core.Application.Abstractions;
using Restify.Core.Application.Abstractions.Configuration;
using Restify.Core.Application.Abstractions.Startup;
using Restify.Modules.Configuration;
using Restify.Modules.Middleware.Abstractions;
using Restify.Modules.Routing.Abstractions;
using Restify.Modules.Services.Abstractions;

using static Restify.Modules.Properties.Supressions;

internal sealed class RestifyApp : IRestifyApp
{
    private readonly RestifyAppServiceContainer serviceContainer;
    private readonly WebApplicationBuilder webApplicationBuilder;
    private readonly IServiceCollection services;
    private readonly RestifyAppConfiguration restifyAppConfiguration;

    internal RestifyApp(WebApplicationBuilder webApplicationBuilder)
    {
        this.serviceContainer = new RestifyAppServiceContainer(webApplicationBuilder.Services);
        this.restifyAppConfiguration = new RestifyAppConfiguration(this.serviceContainer);

        this.webApplicationBuilder = webApplicationBuilder;
        this.services = webApplicationBuilder.Services;
        this.Host = webApplicationBuilder.Host;
    }

    public ConfigureHostBuilder Host
    {
        get;
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp RegisterModule<TServicesModule>()
        where TServicesModule : class, IServicesModule
    {
        return this.RegisterServicesModule<TServicesModule>();
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp RegisterModule<TServicesModule, TRoutingModule>()
        where TServicesModule : class, IServicesModule
        where TRoutingModule : class, IRoutingModule
    {
        _ = this.RegisterServicesModule<TServicesModule>();

        return this.RegisterRoutingModule<TRoutingModule>();
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp RegisterModule<TServicesModule, TRoutingModule, TMiddlewareModule>()
        where TServicesModule : class, IServicesModule
        where TRoutingModule : class, IRoutingModule
        where TMiddlewareModule : class, IMiddlewareModule
    {
        _ = this.RegisterServicesModule<TServicesModule>();
        _ = this.RegisterRoutingModule<TRoutingModule>();

        return this.RegisterMiddlewareModule<TMiddlewareModule>();
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp RegisterServicesModule<TServicesModule>()
        where TServicesModule : class, IServicesModule
    {
        this.restifyAppConfiguration.RegisterServicesModule<TServicesModule>(this.services);
        this.restifyAppConfiguration.ServicesModules.RegisterServices(this.services);

        return this;
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp RegisterRoutingModule<TRoutingModule>()
        where TRoutingModule : class, IRoutingModule
    {
        this.restifyAppConfiguration.RegisterRoutingModule<TRoutingModule>();

        return this;
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp RegisterMiddlewareModule<TMiddlewareModule>()
        where TMiddlewareModule : class, IMiddlewareModule
    {
        this.restifyAppConfiguration.RegisterMiddlewareModule<TMiddlewareModule>();

        return this;
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp UseConfigurationProvider<TConfiguration>()
        where TConfiguration : class, IRestifyConfigurationProvider
    {
        this.serviceContainer.RegisterSingletonService<TConfiguration>();

        return this.serviceContainer.ResolveService<TConfiguration>().Apply(this);
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    public IRestifyApp OnBeforeStartup<TStartupAction>()
        where TStartupAction : class, IRestifyStartupAction
    {
        this.serviceContainer.RegisterScopedService<IRestifyStartupAction, TStartupAction>();

        return this;
    }

    public async Task RunAsync()
    {
        WebApplication webApplication = this.webApplicationBuilder.Build();

        this.restifyAppConfiguration.RoutingModules.RegisterRoutes(webApplication);
        this.restifyAppConfiguration.MiddlewareModules.Use(webApplication);

        await this.ExecuteOnBeforeActionAsync().ConfigureAwait(false);
        await webApplication.RunAsync().ConfigureAwait(false);
    }

    private async Task ExecuteOnBeforeActionAsync()
    {
        IRestifyStartupAction? onBeforeRunAction = this.serviceContainer.ResolveService<IRestifyStartupAction>();

        if (onBeforeRunAction == null)
        {
            return;
        }

        await onBeforeRunAction.RunAsync().ConfigureAwait(false);
    }
}
