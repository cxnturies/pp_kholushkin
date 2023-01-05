using Contracts;
using Entities.DataTransferObjects;
using pp_kholushkin.ActionFilters;
using pp_kholushkin.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
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

		// This method gets called by the runtime. Use this method to add services to the container.
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
			services.ConfigureSwagger();
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

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
			app.UseSwagger();
			app.UseSwaggerUI(s =>
			{
				s.SwaggerEndpoint("/swagger/v1/swagger.json", "Code Maze API v1");
				s.SwaggerEndpoint("/swagger/v2/swagger.json", "Code Maze API v2");
				s.SwaggerEndpoint("/swagger/v1Customer/swagger.json", "Customer API v1");
				s.SwaggerEndpoint("/swagger/v2Customer/swagger.json", "Customer API v2");
			});
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}