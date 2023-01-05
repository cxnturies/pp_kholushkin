using Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog;
using System.IO;
using pp_kholushkin.ActionFilters;
using Entities.DataTransferObjects;
using Repository.DataShaping;
using pp_kholushkin.Extensions;

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

			services.AddAutoMapper(typeof(Startup));
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
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}