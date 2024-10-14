using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Logging;

public class DataBaseService
{
	private readonly string _connectionString;
	private readonly ILogger<DataBaseService> _logger;

	public DataBaseService(string connectionString, ILogger<DataBaseService> logger)
	{
		_connectionString = connectionString;
		_logger = logger;
	}

	public async Task<User> GetUserByUinAsync(string uin)
	{
		var query = @"SELECT role AS Role, last_name AS LastName, first_name AS FirstName, patronymic AS Patronymic, uin, email, 
                             phone_number AS PhoneNumber, id_card AS IdCard, password, ""group"" AS Group 
                      FROM users WHERE uin = @UIN";

		using (var connection = new NpgsqlConnection(_connectionString))
		{
			var user = await connection.QueryFirstOrDefaultAsync<User>(query, new { UIN = uin });
			return user;
		}
	}

	public async Task UpdateUserAsync(string uin, string email, string phoneNumber, string idCard)
	{
		var query = @"UPDATE users 
                      SET email = @Email, phone_number = @PhoneNumber, id_card = @IdCard 
                      WHERE uin = @UIN";

		using (var connection = new NpgsqlConnection(_connectionString))
		{
			await connection.ExecuteAsync(query, new { Email = email, PhoneNumber = phoneNumber, IdCard = idCard, UIN = uin });
		}
	}

	// Register a new user
	public async Task RegisterUserAsync(string role, string lastName, string firstName, string patronymic, string uin, string? email, string? phoneNumber, string idCard, string password, string? group)
	{
		using (var connection = new NpgsqlConnection(_connectionString))
		{
			var query = @"
            INSERT INTO users (role, last_name, first_name, patronymic, uin, email, phone_number, id_card, password, ""group"")
            VALUES (@Role, @LastName, @FirstName, @Patronymic, @UIN, @Email, @PhoneNumber, @IdCard, @Password, @Group)";

			var parameters = new
			{
				Role = role,
				LastName = lastName,
				FirstName = firstName,
				Patronymic = patronymic ?? (object)DBNull.Value,
				UIN = uin,
				Email = email ?? (object)DBNull.Value,
				PhoneNumber = phoneNumber ?? (object)DBNull.Value,
				IdCard = idCard,
				Password = password, // сохраняем хэшированный пароль
				Group = group ?? (object)DBNull.Value
			};

			try
			{
				_logger.LogInformation("Trying to register user with UIN: {UIN}", uin);
				await connection.ExecuteAsync(query, parameters);
				_logger.LogInformation("User registered successfully with UIN: {UIN}");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while registering user with UIN: {UIN}. Exception:");
		}
		}
	}


	public class User
	{
		public string UIN { get; set; }
		public string Password { get; set; }
		public string Role { get; set; }
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string Patronymic { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string IdCard { get; set; }
		public string Group { get; set; }
	}
	public class UserDto
	{
		public string LastName { get; set; }
		public string FirstName { get; set; }
		public string Patronymic { get; set; }
		public string UIN { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string IdCard { get; set; }
		public string Password { get; set; }
		public string Group { get; set; }
	}

}
