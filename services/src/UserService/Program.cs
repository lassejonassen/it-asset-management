using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using UserService.Data;
using UserService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        byte[] secretKey = null;
        if (builder.Environment.IsProduction())
        {
            secretKey = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("jwtsecretkey"));
        }
        else
        {
            secretKey = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtConfig:Secret").Value);
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "ITAM", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://itam.arinco.net")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	Console.WriteLine("========= IT Asset Management ========");
	Console.WriteLine("Service running: User Management Service");

	if (builder.Environment.IsProduction())
	{
		Console.WriteLine("Environment is: PRODUCTION");
		string connectionString = Environment.GetEnvironmentVariable("connectionstring");
		options.UseSqlServer(connectionString);
	}
	else
	{
		Console.WriteLine("Environment is: DEVELOPMENT");
		string connectionString = builder.Configuration.GetConnectionString("Connection1");
		options.UseSqlServer(connectionString);
	}
});

builder.Services
	.AddIdentityCore<ApplicationUser>()
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<IdentityOptions>(opt =>
{
	opt.Password.RequireNonAlphanumeric = false;
	opt.Password.RequiredLength = 6;
	opt.Password.RequireUppercase = true;
	opt.Password.RequireLowercase = true;
	opt.Password.RequireDigit = true;
	opt.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
	opt.User.RequireUniqueEmail = true;
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("ITAM");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
DataSeeding.SeedDefaultUser(app);

app.Run();