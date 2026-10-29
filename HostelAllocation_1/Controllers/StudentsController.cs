
using HostelAllocation.Data;
using HostelAllocation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HostelAllocation_1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/students
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
    {
        return Ok(await _context.Students.ToListAsync());
    }

    // GET: api/students/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Student>> GetStudent(int id)
    {
        var student = await _context.Students.FindAsync(id);

        if (student == null)
        {
            return NotFound(new
            {
                message = $"Student with ID {id} was not found."
            });
        }

        return Ok(student);
    }

    // POST: api/students
    [HttpPost]
    public async Task<ActionResult<Student>> CreateStudent(Student student)
    {
        var registerExists = await _context.Students
            .AnyAsync(s => s.RegisterNumber == student.RegisterNumber);

        if (registerExists)
        {
            return Conflict(new
            {
                message = "This register number is already registered."
            });
        }

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetStudent),
            new { id = student.Id },
            student);
    }

    // PUT: api/students/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateStudent(
        int id, Student updatedStudent)
    {
        if (id != updatedStudent.Id)
        {
            return BadRequest(new
            {
                message = "The route ID and student ID must match."
            });
        }

        var existingStudent = await _context.Students.FindAsync(id);

        if (existingStudent == null)
        {
            return NotFound(new
            {
                message = $"Student with ID {id} was not found."
            });
        }

        var registerExists = await _context.Students.AnyAsync(s =>
            s.RegisterNumber == updatedStudent.RegisterNumber &&
            s.Id != id);

        if (registerExists)
        {
            return Conflict(new
            {
                message = "This register number is already registered."
            });
        }

        existingStudent.Name = updatedStudent.Name;
        existingStudent.RegisterNumber = updatedStudent.RegisterNumber;
        existingStudent.Department = updatedStudent.Department;
        existingStudent.Email = updatedStudent.Email;
        existingStudent.PhoneNumber = updatedStudent.PhoneNumber;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/students/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var student = await _context.Students.FindAsync(id);

        if (student == null)
        {
            return NotFound(new
            {
                message = $"Student with ID {id} was not found."
            });
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
