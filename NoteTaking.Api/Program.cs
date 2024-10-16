using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using NoteTaking.Api.Context;
using NoteTaking.Api.Context.Migrations;
using NoteTaking.Api.Interfaces;
using NoteTaking.Api.Middleware;
using NoteTaking.Api.Model;
using NoteTaking.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithAuth();
builder.Services.ConfigureHttpClient();
builder.Services.AddRouting();

//Add Controllers
builder.Services.AddControllers().AddJsonOptions(options => 
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddLogging();
//builder.Services.AddScoped<INoteTakingServices, NoteTakingServices>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<EmailVerificationLinkFactory>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<INoteTakingServices, NoteTakingServices>();
builder.Services.AddSingleton<TokenProvider>();

//Adding dependency injection for running background jobs.
builder.Services.AddQuartzService();


//Inject the desired DbSettings for connection string
//builder.Configuration.GetConnectionString("ConnectionString");

builder.Services.Configure<DbSettings>(builder.Configuration.GetSection("ApplicationSettings"));
builder.Services.AddScoped<DbSettings>(resolver => resolver.GetRequiredService<IOptions<DbSettings>>().Value); //This creates a single instance of the dbsettings which is shared across the application.
builder.Services.AddDbContext<NoteTakingDbContext>(); //THis creates a scoped instance.
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ClockSkew = TimeSpan.Zero,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secrets"]!))
    };
});


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddBearerToken();

builder.Services.AddFluentEmail(
    builder.Configuration["Sender-Email"], builder.Configuration["Sender-Name"]
).AddSmtpSender(builder.Configuration["Sender-Host"], builder.Configuration.GetValue<int>("Sender-Port"), builder.Configuration["Sender-Username"], builder.Configuration["Sender-Password"]);

var app = builder.Build();

{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<NoteTakingDbContext>();
    //context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c => {
        c.RouteTemplate = "NoteTakingAPI/swagger/{documentname}/swagger.json";
    });
    app.UseSwaggerUI
    (
        c => 
        {
            c.SwaggerEndpoint("/NoteTakingAPI/swagger/v1/swagger.json", "NoteTakingAPI");
            c.RoutePrefix = "NoteTakingAPI";
        }
    );
}


//app.UseHttpsRedirection();
app.MigrateDatabase();

app.UseExceptionHandler();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
