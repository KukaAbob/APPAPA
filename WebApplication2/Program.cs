using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Добавление контроллеров и Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Логирование в консоль
							  // Подключение к базе данных
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton<DataBaseService>(sp =>
{
	var logger = sp.GetRequiredService<ILogger<DataBaseService>>();
	return new DataBaseService(connectionString, logger);
});

// Настройка JWT с ключом из конфигурации или переменной окружения
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "fdsgiuasfogewnrIURibnwfeszidscfqweqfxs");

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = false; // Убедись, что это подходит для твоего окружения
	options.SaveToken = true;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = false,
		ValidateAudience = false,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key)
	};

	// Логирование ошибок аутентификации
	options.Events = new JwtBearerEvents
	{
		OnAuthenticationFailed = context =>
		{
			Console.WriteLine($"Authentication failed: {context.Exception.Message}");
			return Task.CompletedTask;
		}
	};
});

// CORS политика для разрешения всех запросов
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", builder =>
	{
		builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
	});
});

var app = builder.Build();
//http://localhost:8080/swagger/index.html

// Включаем Swagger только в разработке
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.UseSwagger();               // Регистрация Swagger генератора
	app.UseSwaggerUI();             // Регистрация Swagger UI
}

app.UseHttpsRedirection(); // Можно временно закомментировать, если окружение не поддерживает HTTPS

app.UseCors("AllowAll");

app.UseAuthentication();  // Включаем JWT аутентификацию
app.UseAuthorization();

app.MapControllers();

// Настройка прослушивания на порту 8080
app.Urls.Add("http://*:8080");

app.Run();
