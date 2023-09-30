using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Smart.TicketHelpDesktop.BLL;
using System.Text;

namespace ProtocolApi
{

    /// <summary>
    /// Startup class
    /// </summary>
    public class Startup
    {

        /// <summary>
        /// Rif to current environment
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        private readonly IConfiguration _jwtConfiguration;


        /// <summary>
        /// Rif to current configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="environment"></param>
        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
            Configuration = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            _jwtConfiguration = Configuration.GetSection("JwtSettings");
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // bll setup

            services.AddScoped<UserService>();
            Factory.Initialize(Configuration);
            Factory.Setup(Configuration.GetSection("ConnectionStringTestDb").Value);
            // init controllers api
            services.AddControllers();


            var jwtSettings = Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JwtSettings:SecretKey not found in appsettings.json");
            }
            // Configurar la autenticación JWT
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidIssuer = issuer,
                ValidAudience = audience,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = tokenValidationParameters;
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy =>
                {
                    policy.RequireRole("admin");
                });
                options.AddPolicy("UserPolicy", policy =>
                {
                    policy.RequireRole("user");
                });

            });

            // init soket use
            // services.AddSignalR();

            // init cors origin
            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        // .AllowAnyOrigin()
                        .AllowCredentials()
                        .Build();
                });
            });

            // init swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Help Desktop Ticket API", Version = "v1" });
                c.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, "TicketHelpDesktopApi.xml"));
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert token",
                    Name = "Autorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "bearer"


                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                    {

                        new OpenApiSecurityScheme{
                        Reference = new OpenApiReference{
                        Type= ReferenceType.SecurityScheme,
                        Id="Bearer"

                        }

                    },
                        new string[]{}


                   }


                });
            });

            // services.AddMvc().AddNewtonsoftJson();
        }



        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseDeveloperExceptionPage();
            app.UseCors("default");
            app.UseRouting();

            // Habilitar la autenticación y la autorización
            app.UseAuthentication();
            app.UseAuthorization();


            app.UseSwagger();

            // Comentar la exposición de SwaggerUI si lo deseas
            // app.UseSwaggerUI(c =>
            // {
            //     c.SwaggerEndpoint("v1/swagger.json", "Ticket Help Desktop API - " + env.EnvironmentName);
            // });

            // Configurar SwaggerUI para que redirija a la página de inicio si no estás autenticado
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "Ticket Help Desktop API - " + env.EnvironmentName);
                c.OAuthClientId("swagger-ui-client");
                c.OAuthAppName("Swagger UI");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // endpoints.MapHub<SignalIRHub>("/signalir");
            });
        }



    }
}
