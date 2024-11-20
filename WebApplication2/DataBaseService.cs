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

        // Получение пользователя по UIN
       public async Task<User> GetUserByUinAsync(string uin)
{
    var query = @"SELECT user_role AS Role, surname AS LastName, given_name AS FirstName, middle_name AS Patronymic, 
                         unique_id AS UIN, email, contact_number AS PhoneNumber, 
                         identity_card AS IdCard, password_hash AS Password, user_group AS Group 
                  FROM users 
                  WHERE unique_id = @UIN";

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
                  SET email = @Email, contact_number = @PhoneNumber, identity_card = @IdCard 
                  WHERE unique_id = @UIN";

    using (var connection = new NpgsqlConnection(_connectionString))
    {
        await connection.ExecuteAsync(query, new { Email = email, PhoneNumber = phoneNumber, IdCard = idCard, UIN = uin });
    }
}


        // Получение списка всех аудиторий
        public async Task<IEnumerable<Audience>> GetAudiencesAsync()
        {
            var query = @"SELECT 
                            audience_id AS AudienceId,
                            audience_number AS AudienceNumber,
                            audience_name AS AudienceName,
                            audience_qr AS AudienceQR,
                            audience_qr_last_update AS QrLastUpdate,
                            audiences_type AS AudienceType
                          FROM audiences";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    return await connection.QueryAsync<Audience>(query);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching audiences.");
                    throw;
                }
            }
        }

        // Получение расписания уроков для определенной группы
        public async Task<IEnumerable<Lesson>> GetLessonsByGroupAsync(int groupId)
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
                    return await connection.QueryAsync<Lesson>(query, new { GroupId = groupId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching lessons by group.");
                    throw;
                }
            }
        }

        // Проверка и обновление статуса посещения студента
        public async Task UpdateAttendanceStatusAsync(int attendanceId, int statusId, bool manuallyOverridden)
        {
            var query = @"UPDATE student_attendance 
                          SET 
                            status_id = @StatusId, 
                            is_status_manually_overriden = @ManuallyOverridden 
                          WHERE attendance_id = @AttendanceId";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(query, new { AttendanceId = attendanceId, StatusId = statusId, ManuallyOverridden = manuallyOverridden });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating attendance status.");
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

    public class Lesson
    {
        public int LessonId { get; set; }
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AudienceId { get; set; }
        public int GroupId { get; set; }
        public string Description { get; set; }
        public int Pincode { get; set; }
    }
}
