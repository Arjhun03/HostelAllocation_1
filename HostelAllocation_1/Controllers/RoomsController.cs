
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
    
    // GET: api/rooms
    // GET: api/rooms?availableOnly=true
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> GetRooms(
        [FromQuery] bool availableOnly = false)
    {
        var query = _context.Rooms.AsQueryable();

        if (availableOnly)
        {
            query = query.Where(room =>
                room.OccupiedBeds < room.Capacity);
        }

        var rooms = await query.ToListAsync();

        return Ok(rooms);
    }
    
    // GET: api/rooms/type/Shared
    [HttpGet("type/{roomType}")]
    public async Task<ActionResult<IEnumerable<Room>>> GetRoomsByType(
        string roomType)
    {
        if (roomType != "Single" && roomType != "Shared")
        {
            return BadRequest(new
            {
                message = "Room type must be Single or Shared."
            });
        }

        var rooms = await _context.Rooms
            .Where(room => room.RoomType == roomType)
            .ToListAsync();

        return Ok(rooms);
    }
    
    
    // GET: api/rooms/price?minFee=3000&maxFee=7000
    [HttpGet("price")]
    public async Task<ActionResult<IEnumerable<Room>>> GetRoomsByPrice(
        [FromQuery] decimal minFee,
        [FromQuery] decimal maxFee)
    {
        if (minFee < 0 || maxFee < minFee)
        {
            return BadRequest(new
            {
                message = "Enter a valid monthly fee range."
            });
        }

        var rooms = await _context.Rooms
            .Where(room =>
                room.MonthlyFee >= minFee &&
                room.MonthlyFee <= maxFee)
            .ToListAsync();

        return Ok(rooms);
    }
    
    
    // GET: api/rooms/paged?pageNumber=1&pageSize=10
    [HttpGet("paged")]
    public async Task<ActionResult<IEnumerable<Room>>> GetRoomsPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                message = "Page number must be at least 1, and page size must be between 1 and 100."
            });
        }

        var rooms = await _context.Rooms
            .OrderBy(room => room.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(rooms);
    }

    
    
    // GET: api/rooms/sort?sortBy=monthlyFee&descending=false
    [HttpGet("sort")]
    public async Task<ActionResult<IEnumerable<Room>>> SortRooms(
        [FromQuery] string sortBy = "roomNumber",
        [FromQuery] bool descending = false)
    {
        var query = _context.Rooms.AsQueryable();

        query = sortBy.ToLower() switch
        {
            "roomnumber" => descending
                ? query.OrderByDescending(room => room.RoomNumber)
                : query.OrderBy(room => room.RoomNumber),

            "monthlyfee" => descending
                ? query.OrderByDescending(room => room.MonthlyFee)
                : query.OrderBy(room => room.MonthlyFee),

            _ => null!
        };

        if (query == null)
        {
            return BadRequest(new
            {
                message = "sortBy must be roomNumber or monthlyFee."
            });
        }

        var rooms = await query.ToListAsync();

        return Ok(rooms);
    }


    // GET: api/rooms/statistics
    [HttpGet("statistics")]
    public async Task<IActionResult> GetRoomStatistics()
    {
        var totalRooms = await _context.Rooms.CountAsync();

        var totalCapacity = await _context.Rooms
            .SumAsync(room => (int?)room.Capacity) ?? 0;

        var occupiedBeds = await _context.Rooms
            .SumAsync(room => (int?)room.OccupiedBeds) ?? 0;

        var availableBeds = totalCapacity - occupiedBeds;

        var occupancyPercentage = totalCapacity > 0
            ? Math.Round((double)occupiedBeds / totalCapacity * 100, 2)
            : 0;

        return Ok(new
        {
            TotalRooms = totalRooms,
            TotalCapacity = totalCapacity,
            OccupiedBeds = occupiedBeds,
            AvailableBeds = availableBeds,
            OccupancyPercentage = occupancyPercentage
        });
    }



}
