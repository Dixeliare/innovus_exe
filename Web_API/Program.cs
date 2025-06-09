using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository.Basic.Repositories;
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
builder.Services.AddScoped<ScheduleRepository>();
builder.Services.AddScoped<TimeslotRepository>();
builder.Services.AddScoped<WeekRepository>();
builder.Services.AddScoped<ClassSessionRepository>();
builder.Services.AddScoped<ClassRepository>();
builder.Services.AddScoped<AttendanceRepository>();
builder.Services.AddScoped<GenreRepository>();
builder.Services.AddScoped<SheetRepository>();
builder.Services.AddScoped<InstrumentRepository>();
builder.Services.AddScoped<ConsultationTopicRepository>();
builder.Services.AddScoped<OpeningScheduleRepository>();
builder.Services.AddScoped<ConsultationRequestRepository>();
builder.Services.AddScoped<DocumentRepository>();
builder.Services.AddScoped<SheetMusicRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<StatisticRepository>();
builder.Services.AddScoped<RoleRepository>();
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

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapControllers();
app.Run();