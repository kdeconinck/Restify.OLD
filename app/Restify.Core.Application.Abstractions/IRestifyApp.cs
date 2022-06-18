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
namespace Restify.Core.Application.Abstractions;

using System.Diagnostics.CodeAnalysis;

using Microsoft.AspNetCore.Builder;

using Restify.Core.Application.Abstractions.Configuration;
using Restify.Core.Application.Abstractions.Startup;
using Restify.Modules.Middleware.Abstractions;
using Restify.Modules.Routing.Abstractions;
using Restify.Modules.Services.Abstractions;

using static Restify.Modules.Properties.Supressions;

public interface IRestifyApp
{
    ConfigureHostBuilder Host
    {
        get;
    }

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    IRestifyApp RegisterModule<TServicesModule>()
        where TServicesModule : class, IServicesModule;

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    IRestifyApp RegisterModule<TServicesModule, TRoutingModule>()
        where TServicesModule : class, IServicesModule
        where TRoutingModule : class, IRoutingModule;

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    IRestifyApp RegisterModule<TServicesModule, TRoutingModule, TMiddlewareModule>()
        where TServicesModule : class, IServicesModule
        where TRoutingModule : class, IRoutingModule
        where TMiddlewareModule : class, IMiddlewareModule;

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    IRestifyApp RegisterServicesModule<TServicesModule>()
        where TServicesModule : class, IServicesModule;

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    IRestifyApp RegisterRoutingModule<TRoutingModule>()
        where TRoutingModule : class, IRoutingModule;

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    IRestifyApp RegisterMiddlewareModule<TMiddlewareModule>()
        where TMiddlewareModule : class, IMiddlewareModule;

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    IRestifyApp UseConfigurationProvider<TConfiguration>()
        where TConfiguration : class, IRestifyConfigurationProvider;

    [SuppressMessage(Categories.MinorCodeSmell, Identifiers.S4018, Justification = Justifications.ApiDesign)]
    IRestifyApp OnBeforeStartup<TStartupAction>()
        where TStartupAction : class, IRestifyStartupAction;

    Task RunAsync();
}
