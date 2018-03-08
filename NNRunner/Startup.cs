using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NNRunner.NeuralNet;
using NNRunner.StockEvents;
using Swashbuckle.AspNetCore.Swagger;

namespace NNRunner
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<IEventRepository, EventRepository>()
                .AddSingleton<IProcessRepository<TrainingJob>, TrainingJobRepository>()
                .AddSwaggerGen(c => c.SwaggerDoc("NNRunner", new Info {Title = "NN Runner", Version = "v1"}))
                .AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/NNRunner/swagger.json", "NN Runner");
            });
        }
    }
}
