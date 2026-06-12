using API.Extensions;
using Application.Helpers;
using Application.Services;
using Application.Services.Interfaces;
using Application.SignalR;
using Domain.Models;
using Domain.Models.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Extensions;
using Persistence;
using Persistence.Extensions;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
  .AddJsonOptions(json => {
    json.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    json.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    json.JsonSerializerOptions.PropertyNamingPolicy = null;
  });
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(json => {
  json.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
  json.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
  json.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.Configure<AppSettings>(builder.Configuration.GetRequiredSection("AppSettings"));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddPagination(opts => {
  opts.DefaultSize = 50;
  opts.MaxSize = 1000;
  opts.CanChangeSizeFromQuery = true;
});

// auth
builder.Services.AddAuthentication()
  .AddJwtBearer(opts => {
    if (builder.Configuration.GetRequiredSection("AppSettings")
        .Get<AppSettings>() is not { } settings)
      throw new Exception("invalid appsettings.json");
    opts.TokenValidationParameters = new TokenValidationParameters {
      ValidateIssuer = true,
      ValidIssuer = settings.Authentication.JWTIssuer,
      ValidateAudience = true,
      ValidAudience = settings.Authentication.JWTAudience,
      ValidateLifetime = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Authentication.JWTKey)),
      ValidateIssuerSigningKey = true,
    };
    opts.Events = new JwtBearerEvents {
      OnMessageReceived = context => {
        if (context.HttpContext.Request.Path.StartsWithSegments("/hubs")) // SignalR Auth
          context.Token = context.Request.Query["access_token"];
        else // HTTP Auth 
          context.Token = context.Request.Cookies[$".AspNetCore.{IdentityConstants.ApplicationScheme}"];
        return Task.CompletedTask;
      }
    };
  });

builder.Services.AddIdentityCore<AppUser>(opts => {
  opts.Password = new PasswordOptions {
    RequireDigit = false,
    RequiredLength = 8,
    RequireLowercase = false,
    RequireNonAlphanumeric = false,
    RequireUppercase = false
  };
  opts.User.RequireUniqueEmail = true;
  opts.SignIn.RequireConfirmedEmail = false; //builder.Environment.IsProduction();
})
  //.AddRoles<AppRole>()
  .AddEntityFrameworkStores<AppDbContext>()
  .AddErrorDescriber<CustomIdentityErrorDescriber>()
  .AddApiEndpoints();

// swagger
builder.Services.AddSwaggerGen(opt => {
  opt.OperationFilter<Swagger.AppendAuthorizeToSummaryOperationFilter>();
  //opt.OperationFilter<Vernou.Swashbuckle.HttpResultsAdapter.HttpResultsOperationFilter>();
  opt.SchemaFilter<DescribeEnumMemberValues>();
  opt.SupportNonNullableReferenceTypes();
  //opt.ConfigurePagination();
  opt.AddSignalRSwaggerGen(opt => opt.ScanAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
  opt.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo {
    Version = "v1",
    Title = "Project API",
    Description = @"Legend:
- 🔍: Filter parameter
- ♓: Full-text/fuzzy search
- 📈📉: Sort parameter
- 🔑: Composite primary key
- ⚡: Derivatives
- 🚥: Context-derivatives
- 🈲: Access policies
- 🛜: SignalR datagram"
  });
});

// mail
builder.Services.AddMailKit(opts => {
  if (builder.Configuration.GetRequiredSection("AppSettings")
      .Get<AppSettings>() is not { } settings)
    throw new Exception("invalid appsettings.json");
  opts.UseMailKit(new() {
    Server = settings.SMTP.Host,
    Port = settings.SMTP.Port,
    SenderName = settings.SMTP.SenderName,
    SenderEmail = settings.SMTP.SenderEmail,
    Account = settings.SMTP.Login,
    Password = settings.SMTP.Password,
    Security = true,
  });
});

// data services
builder.Services.RegisterDataServices(builder.Configuration);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICatService, CatService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// infrastructure services
builder.Services.AddTransient<Infrastructure.EmailSender.IEmailSender, Infrastructure.EmailSender.EmailSender>();
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, Infrastructure.EmailSender.EmailSender>();
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.IEmailSender<AppUser>, Infrastructure.EmailSender.EmailSender>();

// SignalR
builder.Services.AddSignalR(opts => {
  opts.EnableDetailedErrors = true;
});

// ###############################




var app = builder.Build();

// seed db
using (var scope = app.Services.CreateScope()) {
  await AppDbContext.Seed(scope.ServiceProvider, builder.Environment);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsStaging()) {
  app.MapOpenApi();
  app.MapScalarApiReference(opts => opts
      .WithTitle("NewProject - API Reference")
      .WithDefaultHttpClient(ScalarTarget.Http, ScalarClient.Http11)
      .WithCustomCss(".operation-details .markdown strong {color: #f00;}")
  //.WithApiKeyAuthentication(opts => opts.Token = "Bearer " + builder.Configuration["Scalar:DefaultAuthToken"])
  );
  app.UseSwagger(opt => {
    opt.RouteTemplate = "openapi/{documentName}.json";
  });
  app.UseDeveloperExceptionPage();
}

app.UseCors(b => {
  //b.AllowAnyOrigin();
  b.AllowAnyHeader();
  b.AllowAnyMethod();
  b.AllowCredentials();
  b.WithOrigins(
    "https://domain.com",
    "https://domain.dev",
    "http://domain.internal:3000"
  );
});

app.MapHub<MainHub>("/hubs/MainHub");

app.UseAuthorization();
app.MapControllers();
app.Run();
