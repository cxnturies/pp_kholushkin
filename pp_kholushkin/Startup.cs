using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using pp_kholushkin.ActionFilters;
using pp_kholushkin.Extensions;
using Repository;
using Repository.DataShaping;
using System.IO;

namespace pp_kholushkin
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<ApiBehaviorOptions>(options =>
			{
				options.SuppressModelStateInvalidFilter = true;
			});
			services.ConfigureCors();
			services.ConfigureIISIntegration();
			services.ConfigureLoggerService();
			services.ConfigureSqlContext(Configuration);
			services.ConfigureRepositoryManager();
			services.AddScoped<ValidateCompanyExistsAttribute>();
			services.AddScoped<ValidateOrderExistsAttribute>();
			services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>();
			services.AddScoped<ValidateProductForOrderExistsAttribute>();
			services.AddScoped<ValidationFilterAttribute>();
			services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();
			services.AddScoped<IDataShaper<ProductDto>, DataShaper<ProductDto>>();
			services.AddScoped<IAuthenticationManager, AuthenticationManager>();
			services.AddAutoMapper(typeof(Startup));
			services.ConfigureVersioning();
			services.AddAuthentication();
			services.ConfigureIdentity();
			services.ConfigureIdentityCustomer();
			services.ConfigureJWT(Configuration);
			services.AddControllers(config =>
			{
				config.RespectBrowserAcceptHeader = true;
				config.ReturnHttpNotAcceptable = true;
			}).AddNewtonsoftJson()
			.AddXmlDataContractSerializerFormatters()
			.AddCustomCSVFormatterCompany()
			.AddCustomCSVFormatterOrder();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerManager logger)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.ConfigureExceptionHandler(logger);
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCors("CorsPolicy");
			app.UseForwardedHeaders(new ForwardedHeadersOptions
			{
				ForwardedHeaders = ForwardedHeaders.All
			});
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}