using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class LessonService
{
	private readonly string _connectionString = "Host=10.250.0.64;Port=5432;Username=postgres;Password=postgres;Database=attendance;";

	// Получение всех уроков
	public async Task<List<Lesson>> GetLessonsAsync()
	{
		using (var connection = new NpgsqlConnection(_connectionString))
		{
			await connection.OpenAsync();
			string query = "SELECT * FROM lessons";
			var lessons = await connection.QueryAsync<Lesson>(query);
			return lessons.ToList();
		}
	}

	// Получение уроков по UIN преподавателя
	public async Task<List<Lesson>> GetLessonsByUINAsync(string teacherUIN)
	{
		using (var connection = new NpgsqlConnection(_connectionString))
		{
			await connection.OpenAsync();
			string query = "SELECT * FROM lessons WHERE teacheruin = @UIN";
			var lessons = await connection.QueryAsync<Lesson>(query, new { TeacherUIN = teacherUIN });
			return lessons.ToList();
		}
	}

	// Получение уроков по ФИО преподавателя
	public async Task<List<Lesson>> GetLessonsByTeacherNameAsync(string teacherName)
	{
		using (var connection = new NpgsqlConnection(_connectionString))
		{
			await connection.OpenAsync();
			string query = @"SELECT * FROM lessons 
							 WHERE CONCAT(lastname, ' ', firstname, ' ', patronymic) ILIKE '%' || @TeacherName || '%'";
			var lessons = await connection.QueryAsync<Lesson>(query, new { TeacherName = teacherName });
			return lessons.ToList();
		}
	}

	// Получение урока по ID
	public async Task<Lesson> GetLessonByIdAsync(string lessonId)
	{
		using (var connection = new NpgsqlConnection(_connectionString))
		{
			await connection.OpenAsync();
			string query = "SELECT * FROM lessons WHERE lessonid = @LessonId";
			return await connection.QueryFirstOrDefaultAsync<Lesson>(query, new { LessonId = lessonId });
		}
	}

	// Получение пин-кодов для списка уроков
	public async Task<Dictionary<string, string>> GetPinCodesForLessonsAsync(List<string> lessonIds)
	{
		using (var connection = new NpgsqlConnection(_connectionString))
		{
			await connection.OpenAsync();
			string query = @"SELECT lessonid, pincode FROM lessons WHERE lessonid = ANY(@LessonIds)";
			var result = await connection.QueryAsync<(string LessonId, string PinCode)>(query, new { LessonIds = lessonIds });
			return result.ToDictionary(r => r.LessonId, r => r.PinCode);
		}
	}

	// Получение пин-кода для одного урока
	public async Task<string> GetPinCodeForLessonAsync(string lessonId)
	{
		using (var connection = new NpgsqlConnection(_connectionString))
		{
			await connection.OpenAsync();
			string query = "SELECT pincode FROM lessons WHERE lessonid = @LessonId";
			return await connection.QueryFirstOrDefaultAsync<string>(query, new { LessonId = lessonId });
		}
	}

	// Генерация случайного пин-кода
	public string GeneratePinCode()
	{
		var random = new Random();
		return random.Next(1000, 9999).ToString();
	}

	// Сохранение пин-кода в базе данных
	public void SavePinCode(string lessonId, string pinCode)
	{
		using (var connection = new NpgsqlConnection(_connectionString))
		{
			connection.Open();
			string query = @"UPDATE lessons SET pincode = @PinCode WHERE lessonid = @LessonId";
			connection.Execute(query, new { PinCode = pinCode, LessonId = lessonId });
		}
	}
}

public class Lesson
{
	public string LessonId { get; set; }
	public string TeacherUIN { get; set; }
	public string LastName { get; set; }   // Фамилия преподавателя
	public string FirstName { get; set; }  // Имя преподавателя
	public string Patronymic { get; set; } // Отчество преподавателя
	public string Room { get; set; }
	public string Group { get; set; }
	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public string Description { get; set; }
	public string PinCode { get; set; }
	public string QRCodeImage { get; set; } // Base64 image string for the QR code
}
