using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
	private readonly DataBaseService _dbService;

	public UserController(DataBaseService dbService)
	{
		_dbService = dbService;
	}

	// GET api/user/{uin}
	[HttpGet("{uin}")]
	public async Task<IActionResult> GetUserById(string uin)
	{
		Console.WriteLine($"Fetching user for UIN: {uin}");

		// Получаем пользователя из базы данных
		var user = await _dbService.GetUserByUinAsync(uin);

		// Проверяем, найден ли пользователь
		if (user == null)
		{
			Console.WriteLine($"User with UIN {uin} not found.");
			return NotFound(new { message = "User not found." });
		}

		Console.WriteLine($"User with UIN {uin} found: {user.FirstName} {user.LastName}");
		return Ok(user);
	}

	// PATCH api/user/{uin}
	[HttpPatch("{uin}")]
	public async Task<IActionResult> UpdateUser(string uin, [FromBody] UpdateUserRequest request)
	{
		Console.WriteLine($"Updating user with UIN: {uin}");

		try
		{
			// Обновляем пользователя в базе данных
			await _dbService.UpdateUserAsync(uin, request.Email, request.PhoneNumber, request.IdCard);
			Console.WriteLine($"User with UIN {uin} updated successfully.");
			return Ok(new { status = "User information updated successfully" });
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error updating user with UIN {uin}: {ex.Message}");
			return StatusCode(500, new { message = "Error updating user", details = ex.Message });
		}
	}

	// Модель для запроса обновления данных пользователя
	public class UpdateUserRequest
	{
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string IdCard { get; set; }
	}
}
