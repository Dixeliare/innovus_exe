using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository.Basic;
using Repository.Basic.IRepositories;
using Repository.Basic.Repositories;
using Repository.Basic.UnitOfWork;
using Repository.Data;
using Services.Configurations;
using Services.IServices;
using Services.Services;
using Web_API.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.
#region Repositories
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<ITimeslotRepository, TimeslotRepository>();
builder.Services.AddScoped<IWeekRepository, WeekRepository>();
builder.Services.AddScoped<IClassSessionRepository, ClassSessionRepository>();
builder.Services.AddScoped<IClassRepository, ClassRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<ISheetRepository, SheetRepository>();
builder.Services.AddScoped<IInstrumentRepository, InstrumentRepository>();
builder.Services.AddScoped<IConsultationTopicRepository, ConsultationTopicRepository>();
builder.Services.AddScoped<IOpeningScheduleRepository, OpeningScheduleRepository>();
builder.Services.AddScoped<IConsultationRequestRepository, ConsultationRequestRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<ISheetMusicRepository, SheetMusicRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStatisticRepository, StatisticRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IGenderRepository, GenderRepository>();
builder.Services.AddScoped<IDayRepository, DayRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IDayOfWeekLookupRepository, DayOfWeekLookupRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
#endregion

#region UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
#endregion

#region Services
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddHostedService<ScheduleCreationBackgroundService>();
builder.Services.AddScoped<ITimeslotService, TimeslotService>();
builder.Services.AddScoped<IWeekService, WeekService>();
builder.Services.AddScoped<IClassSessionService, ClassSessionService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<ISheetService, SheetService>();
builder.Services.AddScoped<IInstrumentService, InstrumentService>();
builder.Services.AddScoped<IConsultationTopicService, ConsultationTopicService>();
builder.Services.AddScoped<IOpeningScheduleService, OpeningScheduleService>();
builder.Services.AddScoped<IConsultationRequestService, ConsultationRequestService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<ISheetMusicService, SheetMusicService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStatisticService, StatisticService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IGenderService, GenderService>();
builder.Services.AddScoped<IDayService, DayService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IDayOfWeekLookupService, DayOfWeekLookupService>();
#endregion

#region Azure service
// 1. Cấu hình Azure Blob Storage để đọc từ appsettings.json
builder.Services.Configure<AzureBlobStorageConfig>(builder.Configuration.GetSection("AzureBlobStorage"));
// 2. Đăng ký AzureBlobFileStorageService là triển khai của IFileStorageService
builder.Services.AddScoped<IFileStorageService, AzureBlobFileStorageService>();
#endregion

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.AddControllers().AddJsonOptions(options =>
// {
//     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
//     options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
//     options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
// });

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    // options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never; // Dòng này thường không cần thiết
});

// builder.Services.AddControllers().AddJsonOptions(options =>
// {
//     options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
//     // options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never; // Dòng này thường không cần thiết
// });

// builder.Services.AddControllers().AddJsonOptions(options =>
// {
//     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
//     options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
//     options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
// });

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            // ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddSwaggerGen(option =>
{
    ////JWT Config
    option.DescribeAllParametersInCamelCase();
    option.ResolveConflictingActions(conf => conf.First());     // duplicate API name if any, ex: Get() & Get(string id)
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()      
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger", false);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<Web_API.Middlewares.ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.MapControllers();
app.Run();


// "Host=shortline.proxy.rlwy.net;Port=56746;Username=postgres;Password=AeFjmbZKuSjploofxcnXtNTmIUIZCIUk;Database=railway;SSL Mode=Require;Trust Server Certificate=true"

// "Host=localhost;Port=5432;Database=innovus_updated_db;Username=postgres;Password=12345"

// "Host=ep-lingering-poetry-a1gbdn8i-pooler.ap-southeast-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_5XSUwGDx9uNv;SslMode=Require"