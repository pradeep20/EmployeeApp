using EmployeeAPI.Data;
using EmployeeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;
        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        // READ ALL (GET)
        [HttpGet]
        public async Task<ActionResult<PagedResult<Employee>>> GetEmployees(
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 5, 
            [FromQuery] string? search = null)
        {
            IQueryable<Employee> query = _context.Employees;

            // 1. Server-side Filtering (Search)
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(e =>
                    e.Name.ToLower().Contains(searchLower) ||
                    e.Department.ToLower().Contains(searchLower) ||
                    e.Id.ToString().Contains(searchLower)
                );
            }

            // 2. Count total rows matching the search criteria
            int totalRecords = await query.CountAsync();

            // 3. Server-side Pagination (Skip/Take execution)
            var employees = await query
                .OrderBy(e => e.Id) // Ensure deterministic ordering
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new PagedResult<Employee>
            {
                Data = employees,
                TotalRecords = totalRecords
            });
        }


        // READ BY ID (GET)
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();
            return employee;
        }

        // CREATE (POST)
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }

        // UPDATE (PUT)
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.Id) return BadRequest();

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Employees.Any(e => e.Id == id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE (DELETE)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
