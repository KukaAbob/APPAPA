using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YourNamespace;

[ApiController]
[Route("api/[controller]")]
public class LessonController : ControllerBase
{
	private readonly DataBaseService _databaseService;

	public LessonController(DataBaseService databaseService)
	{
		_databaseService = databaseService;
	}

	// Получение уроков по UIN учителя
	[HttpGet("teacher/{teacherUin}")]
	public async Task<IActionResult> GetLessonsByTeacherUin(string teacherUin)
	{
		var lessons = await _databaseService.GetLessonsByTeacherUinAsync(teacherUin);
		if (lessons == null)
		{
			return NotFound();
		}
		return Ok(lessons);
	}

	// Получение урока по ID
	[HttpGet("{lessonId}")]
	public async Task<IActionResult> GetLessonById(String lessonId)
	{
		var lesson = await _databaseService.GetLessonByIdAsync(lessonId);
		if (lesson == null)
		{
			return NotFound();
		}
		return Ok(lesson);
	}

	// Получение всех уроков
	[HttpGet]
	public async Task<IActionResult> GetAllLessons()
	{
		var lessons = await _databaseService.GetAllLessonsAsync();
		return Ok(lessons);
	}
}
