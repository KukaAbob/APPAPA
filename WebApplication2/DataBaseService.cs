using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Security.Cryptography;

namespace YourNamespace
{
	public class DataBaseService
	{
		private readonly string _connectionString;
		private readonly ILogger<DataBaseService> _logger;

		public DataBaseService(string connectionString, ILogger<DataBaseService> logger)
		{
			_connectionString = connectionString;
			_logger = logger;
		}

		// Получение пользователя по UIN
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

		// Обновление пользователя
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

		// Регистрация нового пользователя
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
					Password = password,
					Group = group ?? (object)DBNull.Value
				};

				try
				{
					_logger.LogInformation("Trying to register user with UIN: {UIN}", uin);
					await connection.ExecuteAsync(query, parameters);
					_logger.LogInformation("User registered successfully with UIN: {UIN}", uin);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error while registering user with UIN: {UIN}. Exception:");
					throw; // Рекомендуется пробросить исключение дальше
				}
			}
		}

		public async Task<IEnumerable<Lesson>> GetAllLessonsAsync()
		{
			var query = @"SELECT lessonid, teacher, starttime, endtime, room, ""group"", description, pincode, teacheruin 
                          FROM lessons";

			using (var connection = new NpgsqlConnection(_connectionString))
			{
				return await connection.QueryAsync<Lesson>(query);
			}
		}

		// Получение уроков по UIN учителя
		public async Task<IEnumerable<Lesson>> GetLessonsByTeacherUinAsync(string teacherUin)
		{
			var query = @"SELECT lessonid, teacher, starttime, endtime, room, ""group"", description, pincode, teacheruin 
                          FROM lessons 
                          WHERE teacheruin = @TeacherUin";

			using (var connection = new NpgsqlConnection(_connectionString))
			{
				return await connection.QueryAsync<Lesson>(query, new { TeacherUin = teacherUin });
			}
		}

		// Получение урока по ID
		public async Task<Lesson> GetLessonByIdAsync(string lessonId)
		{
			var query = @"SELECT lesson_id, teacher, starttime, endtime, room, ""group"", description, pincode, teacheruin 
                          FROM lessons 
                          WHERE lesson_id = @LessonId";

			using (var connection = new NpgsqlConnection(_connectionString))
			{
				return await connection.QueryFirstOrDefaultAsync<Lesson>(query, new { LessonId = lessonId });
			}
		}
		public async Task<bool> ValidateStudentAsync(string uin, string password)
		{
			try
			{
				using (var connection = new NpgsqlConnection(_connectionString))
				{
					await connection.OpenAsync();

					string hashedPassword = HashPassword(password);  // Хэшируем пароль

					string query = @"
                    SELECT COUNT(1)
                    FROM users u
                    INNER JOIN roles r ON u.role_id = r.role_id
                    WHERE u.uin = @UIN AND u.password_hash = @Password AND r.role_name = 'student'";

					var parameters = new { UIN = uin, Password = hashedPassword };
					return await connection.ExecuteScalarAsync<bool>(query, parameters);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при проверке учетных данных студента");
				return false;
			}
		}

		// Метод для проверки валидности преподавателя
		public async Task<bool> ValidateTeacherAsync(string uin, string password)
		{
			try
			{
				using (var connection = new NpgsqlConnection(_connectionString))
				{
					await connection.OpenAsync();

					string hashedPassword = HashPassword(password);  // Хэшируем пароль

					string query = @"
                    SELECT COUNT(1)
                    FROM users u
                    INNER JOIN roles r ON u.role_id = r.role_id
                    WHERE u.uin = @UIN AND u.password_hash = @Password AND r.role_name = 'teacher'";

					var parameters = new { UIN = uin, Password = hashedPassword };
					return await connection.ExecuteScalarAsync<bool>(query, parameters);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при проверке учетных данных преподавателя");
				return false;
			}
		}

		// Метод для хэширования пароля (SHA-256)
		private string HashPassword(string password)
		{
			using (var sha256 = SHA256.Create())
			{
				var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
				return Convert.ToBase64String(hashedBytes);
			}
		}

		// Классы моделей
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

		public class Lesson
		{
			public string LessonId { get; set; } // Изменено с Guid на string
			public string Teacher { get; set; }
			public DateTime StartTime { get; set; }
			public DateTime EndTime { get; set; }
			public string Room { get; set; }
			public string Group { get; set; }
			public string Description { get; set; }
			public string Pincode { get; set; }
			public string TeacherUin { get; set; }
		}
	}
}
