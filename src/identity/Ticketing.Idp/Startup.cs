using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Ticketing.Idp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddMvc();

            services.AddIdentityServer()
                .AddDeveloperSigningCredential(true) // in prod a real cert should be used (multuple instance of idsrv will have different KID)
                //TODO add certificate instead of develper signing credentials.
                //var signingCertificate = new X509Certificate2(CertificatePath, CertificatePassword);
                .AddTestUsers(InMemoryConfiguration.GetUsersFromIdentityRepository())
                .AddInMemoryIdentityResources(InMemoryConfiguration.GetIdentityResources())
                .AddInMemoryApiResources(InMemoryConfiguration.GetSecuredApiResources())
                .AddInMemoryClients(InMemoryConfiguration.GetAllowedOAuthClients());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
