using Infrastructure.Persistence;
using Infrastructure.Services;
using Infrastructure.Repositories;
using Apllication.IService;
using Apllication.Service;
using Apllication.IRepositories;
using api.Hubs;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using api.Attributes;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.SetIsOriginAllowed(_ => true) // Cho phép tất cả origin với SignalR credentials
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

// Dang ky Repository
builder.Services.AddScoped<INguoiDungRepository, NguoiDungRepository>();
builder.Services.AddScoped<IQuyenRepository, QuyenRepository>();
builder.Services.AddScoped<IVaiTroRepository, VaiTroRepository>();
builder.Services.AddScoped<INhomQuyenRepository, NhomQuyenRepository>();
builder.Services.AddScoped<IDuAnRepository, DuAnRepository>();
builder.Services.AddScoped<ICongViecRepository, CongViecRepository>();
builder.Services.AddScoped<ISprintRepository, SprintRepository>();
builder.Services.AddScoped<ITaiLieuDuAnRepository, TaiLieuDuAnRepository>();
builder.Services.AddScoped<INhatKyCongViecRepository, NhatKyCongViecRepository>();
builder.Services.AddScoped<IQuyTacGiaoViecAIRepository, QuyTacGiaoViecAIRepository>();
builder.Services.AddScoped<IKyNangRepository, KyNangRepository>();
builder.Services.AddScoped<IThongBaoRepository, ThongBaoRepository>();

// Dang ky Service
builder.Services.AddScoped<IDichVuToken, DichVuToken>();
builder.Services.AddScoped<ITaiKhoanService, TaiKhoanService>();
builder.Services.AddScoped<INguoiDungService, NguoiDungService>();
builder.Services.AddScoped<IMatKhauService, MatKhauService>();
builder.Services.AddScoped<IQuyenService, QuyenService>();
builder.Services.AddScoped<IVaiTroService, VaiTroService>();
builder.Services.AddScoped<INhomQuyenService, NhomQuyenService>();
builder.Services.AddScoped<IDuAnService, DuAnService>();
builder.Services.AddScoped<ICongViecService, CongViecService>();
builder.Services.AddScoped<ISprintService, SprintService>();
builder.Services.AddScoped<ITaiLieuDuAnService, TaiLieuDuAnService>();
builder.Services.AddScoped<INhatKyCongViecService, NhatKyCongViecService>();
builder.Services.AddScoped<IGiaoViecAIService, GiaoViecAIService>();
builder.Services.AddScoped<IKyNangService, KyNangService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IKanbanNotificationService, KanbanNotificationService>();
builder.Services.AddScoped<IThongBaoService, ThongBaoService>();
builder.Services.AddScoped<IQuyTacGiaoViecAIService, QuyTacGiaoViecAIService>();
builder.Services.AddSignalR();

// Dang ky Phan quyen Attribute
builder.Services.AddSingleton<IAuthorizationPolicyProvider, QuyenHanPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, QuyenHanHandler>();

// Cấu hình xác thực bằng JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(tùyChỉnh =>
    {
        tùyChỉnh.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:KhoaBimat"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:NguoiPhatHanh"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:NguoiDung"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };

        // Logic de hỗ trợ nhận Token không cần chữ 'Bearer '
        tùyChỉnh.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authorization = context.Request.Headers["Authorization"].ToString();

                if (!string.IsNullOrEmpty(authorization))
                {
                    // Nếu có chữ 'Bearer ' thì cắt bỏ, nếu không có thì lấy nguyên chuỗi
                    if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Token = authorization.Substring("Bearer ".Length).Trim();
                    }
                    else
                    {
                        context.Token = authorization.Trim();
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "DoAn API", Version = "v1" });
    
    // Cau hinh JWT cho Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Sử dụng Middleware xử lý lỗi toàn cục
app.UseMiddleware<api.Middlewares.ExceptionMiddleware>();

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Thêm dòng này để kích hoạt xác thực
app.UseAuthorization();

app.MapControllers();
app.MapHub<KanbanHub>("/hubs/kanban");

app.Run();
