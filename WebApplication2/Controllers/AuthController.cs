using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using YourNamespace;

public class AuthController : ControllerBase
{
	private readonly ILogger<AuthController> _logger;
	private readonly DataBaseService _dataBaseService;

	public AuthController(ILogger<AuthController> logger, DataBaseService dataBaseService)
	{
		_logger = logger;
		_dataBaseService = dataBaseService;
	}

	[HttpPost("login")]
	public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
	{
		if (string.IsNullOrEmpty(request.UIN) || string.IsNullOrEmpty(request.Password))
		{
			_logger.LogWarning("UIN or Password is null or empty.");
			return BadRequest(new { Message = "UIN and Password are required" });
		}

		try
		{
			_logger.LogInformation("Fetching user for UIN: {UIN}", request.UIN);
			var user = await _dataBaseService.GetUserByUinAsync(request.UIN);

			if (user == null)
			{
				_logger.LogWarning("User not found for UIN: {UIN}", request.UIN);
				return Unauthorized(new { Message = "Invalid UIN or password" });
			}

			// Проверяем, если пароль пуст или null
			if (string.IsNullOrEmpty(user.Password))
			{
				_logger.LogWarning("Password for UIN {UIN} is null or empty.", request.UIN);
				return Unauthorized(new { Message = "Invalid UIN or password" });
			}

			// Проверяем пароль
			if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
			{
				_logger.LogWarning("Invalid password for UIN: {UIN}", request.UIN);
				return Unauthorized(new { Message = "Invalid UIN or password" });
			}

			// Генерируем JWT токен после успешного логина
			_logger.LogInformation("Password is valid, generating JWT token...");
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.ASCII.GetBytes("fdsgiuasfogewnrIURibnwfeszidscfqweqfxs");
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
					new Claim(ClaimTypes.NameIdentifier, request.UIN),
					new Claim(ClaimTypes.Name, request.UIN)
				}),
				Expires = DateTime.UtcNow.AddHours(1),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);
			var tokenString = tokenHandler.WriteToken(token);

			_logger.LogInformation("Login successful for UIN: {UIN}", request.UIN);
			return Ok(new { Token = tokenString, Message = "Login successful." });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during login for UIN: {UIN}", request.UIN);
			return StatusCode(500, new { Message = "Internal server error", Details = ex.Message });
		}
	}

	[HttpPost("register")]
	public async Task<IActionResult> Register([FromBody] RegisterRequest request)
	{
		try
		{
			_logger.LogInformation("Attempting registration for UIN: {UIN}", request.UIN);

			// Хэшируем пароль перед сохранением
			var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

			// Сохраняем пользователя
			await _dataBaseService.RegisterUserAsync(
				request.Role,
				request.LastName,
				request.FirstName,
				request.Patronymic,
				request.UIN,
				request.Email,  // Может быть null
				request.PhoneNumber,  // Может быть null
				request.IdCard,
				hashedPassword,  // Сохраняем хэшированный пароль
				request.Group  // Может быть null
			);

			_logger.LogInformation("User successfully registered with UIN: {UIN}", request.UIN);
			return Ok(new { Message = "Registration successful" });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during registration for UIN: {UIN}", request.UIN);
			return StatusCode(500, new { Message = "Server error", Details = ex.Message });
		}
	}
	[HttpPost("login/student")]
	public async Task<IActionResult> LoginStudentAsync([FromBody] LoginRequest request)
	{
		if (string.IsNullOrEmpty(request.UIN) || string.IsNullOrEmpty(request.Password))
		{
			_logger.LogWarning("UIN or Password is null or empty.");
			return BadRequest(new { Message = "UIN and Password are required" });
		}

		try
		{
			_logger.LogInformation("Validating student UIN: {UIN}", request.UIN);
			var isValid = await _dataBaseService.ValidateStudentAsync(request.UIN, request.Password);

			if (!isValid)
			{
				_logger.LogWarning("Invalid UIN or password for student: {UIN}", request.UIN);
				return Unauthorized(new { Message = "Invalid UIN or password" });
			}

			// Генерация JWT токена (не изменяется)
			var tokenString = GenerateJwtToken(request.UIN);
			return Ok(new { Token = tokenString, Message = "Login successful." });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during student login for UIN: {UIN}", request.UIN);
			return StatusCode(500, new { Message = "Internal server error", Details = ex.Message });
		}
	}

	[HttpPost("login/teacher")]
	public async Task<IActionResult> LoginTeacherAsync([FromBody] LoginRequest request)
	{
		if (string.IsNullOrEmpty(request.UIN) || string.IsNullOrEmpty(request.Password))
		{
			_logger.LogWarning("UIN or Password is null or empty.");
			return BadRequest(new { Message = "UIN and Password are required" });
		}

		try
		{
			_logger.LogInformation("Validating teacher UIN: {UIN}", request.UIN);
			var isValid = await _dataBaseService.ValidateTeacherAsync(request.UIN, request.Password);

			if (!isValid)
			{
				_logger.LogWarning("Invalid UIN or password for teacher: {UIN}", request.UIN);
				return Unauthorized(new { Message = "Invalid UIN or password" });
			}

			// Генерация JWT токена
			var tokenString = GenerateJwtToken(request.UIN);
			return Ok(new { Token = tokenString, Message = "Login successful." });
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error during teacher login for UIN: {UIN}", request.UIN);
			return StatusCode(500, new { Message = "Internal server error", Details = ex.Message });
		}
	}

	private string GenerateJwtToken(string uin)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes("fdsgiuasfogewnrIURibnwfeszidscfqweqfxs");
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new Claim[]
			{
				new Claim(ClaimTypes.NameIdentifier, uin),
				new Claim(ClaimTypes.Name, uin)
			}),
			Expires = DateTime.UtcNow.AddHours(1),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};
		var token = tokenHandler.CreateToken(tokenDescriptor);
		return tokenHandler.WriteToken(token);
	}
	// Классы запросов
	public class RegisterRequest
	{
		public string Role { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string Patronymic { get; set; }
		public string UIN { get; set; }
		public string? Email { get; set; }  // Необязательное поле
		public string? PhoneNumber { get; set; }  // Необязательное поле
		public string IdCard { get; set; }
		public string Password { get; set; }
		public string? Group { get; set; }  // Необязательное поле
	}

	public class LoginRequest
	{
		public string UIN { get; set; }
		public string Password { get; set; }
	}
}
