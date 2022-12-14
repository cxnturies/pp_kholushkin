using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using pp_kholushkin;

namespace pp_kholushkin
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}
		public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => {
			webBuilder.UseStartup<Startup>();
		});
	}
}