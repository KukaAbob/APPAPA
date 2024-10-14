using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ���������� ������������ � Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // ����������� � �������
							  // ����������� � ���� ������
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton<DataBaseService>(sp =>
{
	var logger = sp.GetRequiredService<ILogger<DataBaseService>>();
	return new DataBaseService(connectionString, logger);
});

// ��������� JWT � ������ �� ������������ ��� ���������� ���������
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "fdsgiuasfogewnrIURibnwfeszidscfqweqfxs");

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = false; // �������, ��� ��� �������� ��� ������ ���������
	options.SaveToken = true;
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = false,
		ValidateAudience = false,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key)
	};

	// ����������� ������ ��������������
	options.Events = new JwtBearerEvents
	{
		OnAuthenticationFailed = context =>
		{
			Console.WriteLine($"Authentication failed: {context.Exception.Message}");
			return Task.CompletedTask;
		}
	};
});

// CORS �������� ��� ���������� ���� ��������
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", builder =>
	{
		builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
	});
});

var app = builder.Build();
//http://localhost:8080/swagger/index.html

// �������� Swagger ������ � ����������
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.UseSwagger();               // ����������� Swagger ����������
	app.UseSwaggerUI();             // ����������� Swagger UI
}

app.UseHttpsRedirection(); // ����� �������� ����������������, ���� ��������� �� ������������ HTTPS

app.UseCors("AllowAll");

app.UseAuthentication();  // �������� JWT ��������������
app.UseAuthorization();

app.MapControllers();

// ��������� ������������� �� ����� 8080
app.Urls.Add("http://*:8080");

app.Run();
