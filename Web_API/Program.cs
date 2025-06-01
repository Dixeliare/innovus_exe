using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Repository.Basic.Repositories;
using Repository.Data;
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.Run();