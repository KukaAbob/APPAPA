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
            var query = @"SELECT 
                            role_id AS RoleId,
                            last_name AS LastName,
                            first_name AS FirstName,
                            patronymic AS Patronymic,
                            iin AS IIN,
                            email,
                            phone_number AS PhoneNumber,
                            id_card AS IdCard,
                            password
                          FROM users 
                          WHERE iin = @UIN";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    return await connection.QueryFirstOrDefaultAsync<User>(query, new { UIN = uin });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching user by UIN.");
                    throw;
                }
            }
        }

        // Обновление пользователя
        public async Task UpdateUserAsync(string uin, string email, string phoneNumber, string idCard)
        {
            var query = @"UPDATE users 
                          SET 
                            email = @Email, 
                            phone_number = @PhoneNumber, 
                            id_card = @IdCard 
                          WHERE iin = @UIN";

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await connection.ExecuteAsync(query, new { UIN = uin, Email = email, PhoneNumber = phoneNumber, IdCard = idCard });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating user.");
                    throw;
                }
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
            var query = @"SELECT 
                            lesson_id AS LessonId,
                            teacher_id AS TeacherId,
                            subject_id AS SubjectId,
                            starttime,
                            endtime,
                            audience_id AS AudienceId,
                            group_id AS GroupId,
                            description,
                            pincode
                          FROM lessons 
                          WHERE group_id = @GroupId";

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
