using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace WebApplication2
{
	

	public class DatabaseService
	{
		private readonly string _connectionString;

		public DatabaseService()
		{
			_connectionString = "Host=10.250.0.64;Port=5432;Username=postgres;Password=postgres;Database=attendance;";
		}

		// Регистрация пользователя
		public async Task<string> RegisterUserAsync(RegisterRequest request)
		{
			using (var connection = new NpgsqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				string query = @"
				INSERT INTO users (full_name, email, phone, password_hash, role_id, group_id, id_card)
				VALUES (@FullName, @Email, @Phone, @PasswordHash, @RoleId, @GroupId, @IdCard)
				RETURNING user_id;";

				var parameters = new
				{
					request.FullName,
					request.Email,
					request.Phone,
					PasswordHash = request.Password,  // Хэшируйте пароль перед сохранением
					RoleId = GetRoleId(request.Role),
					GroupId = request.Group,
					request.IdCard
				};

				return await connection.ExecuteScalarAsync<string>(query, parameters);
			}
		}

		// Получение пользователя по ID
		public async Task<object> GetUserByIdAsync(string userId)
		{
			using (var connection = new NpgsqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				string query = "SELECT * FROM users WHERE user_id = @UserId;";
				return await connection.QueryFirstOrDefaultAsync<object>(query, new { UserId = userId });
			}
		}

		// Обновление данных пользователя
		public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
		{
			using (var connection = new NpgsqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				string query = @"
				UPDATE users 
				SET email = @Email, phone = @Phone, id_card = @IdCard
				WHERE user_id = @UserId";

				var parameters = new
				{
					request.Email,
					request.Phone,
					request.IdCard,
					UserId = userId
				};

				int rowsAffected = await connection.ExecuteAsync(query, parameters);
				return rowsAffected > 0;
			}
		}

		// Пример метода для проверки студента
		public async Task<bool> ValidateStudentAsync(string login, string password)
		{
			using (var connection = new NpgsqlConnection(_connectionString))
			{
				await connection.OpenAsync();

				string query = @"
				SELECT COUNT(1)
				FROM users u
				INNER JOIN roles r ON u.role_id = r.role_id
				WHERE u.email = @Login AND u.password_hash = @Password AND r.role_name = 'student';";

				var parameters = new { Login = login, Password = password };
				return await connection.ExecuteScalarAsync<bool>(query, parameters);
			}
		}

		// Пример метода для проверки преподавателя
		public async Task<bool> ValidateStudentAsync(string uin, string password)
		{
			using (var connection = new NpgsqlConnection(_connectionString))
			{
				var query = @"
            SELECT COUNT(1) 
            FROM users 
            WHERE uin = @UIN AND password = @Password AND role = 'student'";

				try
				{
					return await connection.ExecuteScalarAsync<bool>(query, new { UIN = uin, Password = password });
				}
				catch (Exception ex)
				{
					// Логируем ошибку для диагностики
					_logger.LogError(ex, "Database error during fetching user.");
					throw;  // Повторно выбрасываем исключение для обработки
				}
			}
		}

	}

}
