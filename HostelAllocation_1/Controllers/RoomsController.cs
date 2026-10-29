
using HostelAllocation.Data;
using HostelAllocation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HostelAllocation_1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RoomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/rooms
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
    {
        var rooms = await _context.Rooms.ToListAsync();
        return Ok(rooms);
    }

    // GET: api/rooms/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Room>> GetRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null)
        {
            return NotFound(new { message = $"Room with ID {id} was not found." });
        }

        return Ok(room);
    }

    // POST: api/rooms
    [HttpPost]
    public async Task<ActionResult<Room>> CreateRoom(Room room)
    {
        if (!room.IsValidOccupiedBeds())
        {
            return BadRequest(new
            {
                message = "Occupied beds cannot exceed room capacity."
            });
        }

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
    }

    // PUT: api/rooms/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateRoom(int id, Room updatedRoom)
    {
        if (id != updatedRoom.Id)
        {
            return BadRequest(new
            {
                message = "The route ID and room ID must match."
            });
        }

        if (!updatedRoom.IsValidOccupiedBeds())
        {
            return BadRequest(new
            {
                message = "Occupied beds cannot exceed room capacity."
            });
        }

        var existingRoom = await _context.Rooms.FindAsync(id);

        if (existingRoom == null)
        {
            return NotFound(new { message = $"Room with ID {id} was not found." });
        }

        existingRoom.RoomNumber = updatedRoom.RoomNumber;
        existingRoom.RoomType = updatedRoom.RoomType;
        existingRoom.Capacity = updatedRoom.Capacity;
        existingRoom.OccupiedBeds = updatedRoom.OccupiedBeds;
        existingRoom.MonthlyFee = updatedRoom.MonthlyFee;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/rooms/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);

        if (room == null)
        {
            return NotFound(new { message = $"Room with ID {id} was not found." });
        }

        _context.Rooms.Remove(room);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
