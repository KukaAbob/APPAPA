using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Microsoft.Extensions.Logging;
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

        // Метод для получения уроков по UIN преподавателя
        public async Task<IEnumerable<Lesson>> GetLessonsByTeacherUinAsync(string teacherUin)
        {
            var query = @"SELECT lesson_id AS LessonId, teacher_name AS Teacher, start_time AS StartTime, 
                             end_time AS EndTime, room_number AS Room, 
                             class_group AS Group, lesson_description AS Description, 
                             access_code AS Pincode, teacher_unique_id AS TeacherUin 
                      FROM lessons 
                      WHERE teacher_unique_id = @TeacherUin";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    return await connection.QueryAsync<Lesson>(query, new { TeacherUin = teacherUin });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching lessons by teacher UIN.");
                    throw;
                }
            }
        }

        // Метод для получения конкретного урока по ID
        public async Task<Lesson> GetLessonByIdAsync(int lessonId)
        {
            var query = @"SELECT lesson_id AS LessonId, teacher_name AS Teacher, start_time AS StartTime, 
                             end_time AS EndTime, room_number AS Room, 
                             class_group AS Group, lesson_description AS Description, 
                             access_code AS Pincode, teacher_unique_id AS TeacherUin 
                      FROM lessons 
                      WHERE lesson_id = @LessonId";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    return await connection.QueryFirstOrDefaultAsync<Lesson>(query, new { LessonId = lessonId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching lesson by ID.");
                    throw;
                }
            }
        }

        // Метод для получения всех уроков
        public async Task<IEnumerable<Lesson>> GetAllLessonsAsync()
        {
            var query = @"SELECT lesson_id AS LessonId, teacher_name AS Teacher, start_time AS StartTime, 
                             end_time AS EndTime, room_number AS Room, 
                             class_group AS Group, lesson_description AS Description, 
                             access_code AS Pincode, teacher_unique_id AS TeacherUin 
                      FROM lessons";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    return await connection.QueryAsync<Lesson>(query);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching all lessons.");
                    throw;
                }
            }
        }

        // Метод для регистрации пользователя
        public async Task RegisterUserAsync(User user)
        {
            var query = @"INSERT INTO users (surname, given_name, middle_name, unique_id, email, 
                                         contact_number, identity_card, password_hash, user_group, user_role)
                      VALUES (@LastName, @FirstName, @Patronymic, @UIN, @Email, 
                              @PhoneNumber, @IdCard, @Password, @Group, @Role)";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(query, user);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error registering user.");
                    throw;
                }
            }
        }

        // Метод для проверки данных студента
        public async Task<bool> ValidateStudentAsync(string studentUin)
        {
            var query = @"SELECT COUNT(*) 
                      FROM users 
                      WHERE unique_id = @StudentUin AND user_role = 'student'";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var count = await connection.ExecuteScalarAsync<int>(query, new { StudentUin = studentUin });
                    return count > 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating student.");
                    throw;
                }
            }
        }

        // Метод для проверки данных преподавателя
        public async Task<bool> ValidateTeacherAsync(string teacherUin)
        {
            var query = @"SELECT COUNT(*) 
                      FROM users 
                      WHERE unique_id = @TeacherUin AND user_role = 'teacher'";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    var count = await connection.ExecuteScalarAsync<int>(query, new { TeacherUin = teacherUin });
                    return count > 0;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating teacher.");
                    throw;
                }
            }
        }
    }


    // Модели данных
    public class User
    {
        public int RoleId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public string IIN { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string IdCard { get; set; }
        public string Password { get; set; }
    }

    public class Audience
    {
        public int AudienceId { get; set; }
        public string AudienceNumber { get; set; }
        public string AudienceName { get; set; }
        public string AudienceQR { get; set; }
        public DateTime QrLastUpdate { get; set; }
        public int AudienceType { get; set; }
    }



}
