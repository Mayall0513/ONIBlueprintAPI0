using System;

using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using BlueprintRepository.Repositories;
using BlueprintRepository.Models;
using BlueprintRepository.Statics;

namespace BlueprintRepository {
    public sealed class Startup {
        private readonly IConfiguration configuration;

        public Startup (IConfiguration configuration) {
            this.configuration = configuration;
            UserModel.PasswordSalt = configuration.GetConfigurationWFallback(EnvironmentVariables.PASSWORD_SALT);
        }

        public void ConfigureServices(IServiceCollection services) {
            services.AddControllers();
            services.AddDbContextPool<BlueprintsDbContext>(loginModel => {
                loginModel.UseMySql(
                    configuration.GetConfigurationWFallback(EnvironmentVariables.CONNECTION_STRING),

                    options => {
                        options.EnableRetryOnFailure();
                    }
                );
            });

            services.AddSingleton<BlueprintsDbContext>();
            services.AddSingleton<AUserRepository, UserRepository>();
            services.AddSingleton<AUserTokenRepository, UserTokenRepository>();
        }

        public void Configure(IApplicationBuilder applicationBuilder, IWebHostEnvironment hostEnvironment) {
            if (hostEnvironment.IsDevelopment()) {
                applicationBuilder.UseDeveloperExceptionPage();
            }

            applicationBuilder.UseHttpsRedirection();
            applicationBuilder.UseRouting();
            applicationBuilder.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
