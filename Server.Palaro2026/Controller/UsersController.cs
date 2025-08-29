using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;
using System.Text.Json;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class UsersController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public UsersController(Palaro2026Context context)
        {
            _context = context;
        }

        // ------------------------------------------------------------------------------------------------------------------

        // User Views

        // this method is used to get the user details by userID (single user)
        [HttpGet("UserDetails")] // GET: api/Users/UserDetails?userID={userID}
        public async Task<ActionResult<UsersDTO.UserDetails>> GetUserDetails(string? userID)
        {
            var user = await _context.Users
                .Include(r => r.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ID == userID);

            if (user == null)
                return NotFound();

            var mappedUser = new UsersDTO.UserDetails
            {
                ID = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Affiliation = user.Affiliation,
                EmailAddress = user.EmailAddress,
                ContactNumber = user.ContactNumber,
                CreatedAt = user.CreatedAt,
                UpdateAt = user.UpdateAt,
                LastLogin = user.LastLogin,
                Active = user.Active,
                Role = user.Role?.Role
            };

            return Ok(mappedUser);
        }

        // this method is used to get the list of all users with their details
        [HttpGet("UsersDetails")] // GET: api/Users/UsersDetails
        public async Task<ActionResult<IEnumerable<UsersDTO.UserDetails>>> GetUsersInformation()
        {
            var users = await _context.Users
                .Include(r => r.Role)
                .AsNoTracking()
                .ToListAsync();

            var mappedUsers = users.Select(user => new UsersDTO.UserDetails
            {
                ID = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Affiliation = user.Affiliation,
                EmailAddress = user.EmailAddress,
                ContactNumber = user.ContactNumber,
                CreatedAt = user.CreatedAt,
                UpdateAt = user.UpdateAt,
                LastLogin = user.LastLogin,
                Active = user.Active,
                Role = user.Role?.Role
            }).ToList();

            return Ok(mappedUsers);
        }

        // this method is used to get the list of all tally clerks 
        [HttpGet("TallyClerkList")] // GET: api/Users/TallyClerkList
        public async Task<ActionResult<IEnumerable<UsersDTO.UserList>>> GetTallyClerkList()
        {
            var users = await _context.Users
                .Include(r => r.Role)
                .Where(user => user.Role != null && user.Role.Role == "Tally Clerk" && user.Active == true)
                .AsNoTracking()
                .ToListAsync();

            var mappedUsers = users.Select(user => new UsersDTO.UserList
            {
                ID = user.ID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role?.Role
            }).ToList();

            return Ok(mappedUsers);
        }
        
        // this method is used to get the list of all usernames
        [HttpGet("Usernames")] // GET: api/Users/Usernames
        public async Task<ActionResult<IEnumerable<UsersDTO.UsernameList>>> GetUsernamesList()
        {
            var users = await _context.Users
                .Include(r => r.Role)
                .AsNoTracking()
                .ToListAsync();

            var mappedUsers = users.Select(user => new UsersDTO.UsernameList
            {
                Username = user.Username,
            }).ToList();

            return Ok(mappedUsers);
        }
    }
}
