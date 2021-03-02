using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using System.Data.Entity;
using WellFitPlus.Database.Contexts;
using System.Web.Http;

[assembly: OwinStartup(typeof(WellFitPlus.WebAPI.Startup))]

namespace WellFitPlus.WebAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<WellFitAuthContext, Database.AuthenticationMigrations.Configuration>());
            WellFitAuthContext auth = new WellFitAuthContext();
            auth.Database.Initialize(true);

            System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<WellFitDataContext, Database.Migrations.Configuration>());
            WellFitDataContext db = new WellFitDataContext();
            db.Database.Initialize(true);

            HttpConfiguration config = new HttpConfiguration();
            //config.Filters.Add(new HostAuthenticationAttribute("bearer"));
            //config.Filters.Add(new System.Web.Http.AuthorizeAttribute());

            WebApiConfig.Register(config);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);
        }
    }
}
