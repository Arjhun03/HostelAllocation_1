
using HostelAllocation.Data;
using HostelAllocation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HostelAllocation_1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomAllocationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RoomAllocationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/roomallocations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomAllocation>>> GetAllocations()
    {
        var allocations = await _context.RoomAllocations
            .Include(a => a.Student)
            .Include(a => a.Room)
            .ToListAsync();

        return Ok(allocations);
    }

    // GET: api/roomallocations/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RoomAllocation>> GetAllocation(int id)
    {
        var allocation = await _context.RoomAllocations
            .Include(a => a.Student)
            .Include(a => a.Room)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (allocation == null)
        {
            return NotFound(new
            {
                message = $"Allocation with ID {id} was not found."
            });
        }

        return Ok(allocation);
    }

    // POST: api/roomallocations
    [HttpPost]
    public async Task<ActionResult<RoomAllocation>> AllocateRoom(
        RoomAllocation allocation)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var student = await _context.Students
            .FindAsync(allocation.StudentId);

        if (student == null)
        {
            return BadRequest(new
            {
                message = "Student does not exist."
            });
        }

        var room = await _context.Rooms
            .FindAsync(allocation.RoomId);

        if (room == null)
        {
            return BadRequest(new
            {
                message = "Room does not exist."
            });
        }

        if (room.OccupiedBeds >= room.Capacity)
        {
            return Conflict(new
            {
                message = "The room has no available beds."
            });
        }

        var alreadyAllocated = await _context.RoomAllocations
            .AnyAsync(a =>
                a.StudentId == allocation.StudentId &&
                a.VacatedDate == null);

        if (alreadyAllocated)
        {
            return Conflict(new
            {
                message = "This student already has an active allocation."
            });
        }

        allocation.AllocationDate = DateTime.UtcNow;
        allocation.VacatedDate = null;

        room.OccupiedBeds++;

        _context.RoomAllocations.Add(allocation);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return CreatedAtAction(
            nameof(GetAllocation),
            new { id = allocation.Id },
            allocation);
    }

    // PUT: api/roomallocations/1/vacate
    [HttpPut("{id:int}/vacate")]
    public async Task<IActionResult> VacateRoom(int id)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var allocation = await _context.RoomAllocations
            .Include(a => a.Room)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (allocation == null)
        {
            return NotFound(new
            {
                message = $"Allocation with ID {id} was not found."
            });
        }

        if (allocation.VacatedDate != null)
        {
            return BadRequest(new
            {
                message = "This allocation has already been vacated."
            });
        }

        allocation.VacatedDate = DateTime.UtcNow;

        if (allocation.Room.OccupiedBeds > 0)
        {
            allocation.Room.OccupiedBeds--;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = "Room vacated successfully."
        });
    }

    // GET: api/roomallocations/student/1
    [HttpGet("student/{studentId:int}")]
    public async Task<ActionResult<IEnumerable<RoomAllocation>>>
        GetStudentAllocationHistory(int studentId)
    {
        var studentExists = await _context.Students
            .AnyAsync(student => student.Id == studentId);

        if (!studentExists)
        {
            return NotFound(new
            {
                message = $"Student with ID {studentId} was not found."
            });
        }

        var history = await _context.RoomAllocations
            .Where(allocation => allocation.StudentId == studentId)
            .Include(allocation => allocation.Room)
            .OrderByDescending(allocation => allocation.AllocationDate)
            .ToListAsync();

        return Ok(history);
    }
}
