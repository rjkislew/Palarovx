using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.DTO;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SportsController(Palaro2026Context context) : ControllerBase
    {
        private readonly Palaro2026Context _context = context;

        /// 
        /// 
        /// VIEWS
        /// 
        /// 

        [HttpGet("SportsDetails")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SportDetails.SD_SportCategoriesContent>>> GetSportsDetails()
        {
            try
            {
                // Fetch the data from the database
                var sports = await _context.SportDetails
                    .AsNoTracking()
                    .ToListAsync();

                // Group the sports by category
                var groupedSports = sports
                    .GroupBy(c => c.Category)
                    .Select(category => new SportsDTO.SportDetails.SD_SportCategoriesContent
                    {
                        Category = category.Key,
                        SportList = category
                        .GroupBy(s => new { s.Sport, s.Description })
                        .Select(sport => new SportsDTO.SportDetails.SD_SportsContent
                        {
                            Sport = sport.Key.Sport,
                            Description = sport.Key.Description,
                            LevelList = sport
                            .GroupBy(l => l.Level)
                            .Select(level => new SportsDTO.SportDetails.SD_SchoolLevelsContent
                            {
                                Level = level.Key,
                                GenderList = level
                                .GroupBy(gc => gc.Gender)
                                .Select(gender => new SportsDTO.SportDetails.SD_GenderCategoriesContent
                                {
                                    Gender = gender.Key,
                                    SubCategoryList = gender
                                    .Select(subCategory => new SportsDTO.SportDetails.SD_SubCategoriesContent
                                    {
                                        SubCategory = subCategory.SubCategory
                                    }).ToList()
                                }).ToList()
                            }).ToList()
                        }).ToList()
                    }).ToList();

                return Ok(groupedSports);
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database update error: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("SportsDetailsFiltered")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SportDetails.SD_SportCategoriesContent>>> GetSportsDetailsFiltered(
        [FromQuery] string? category = null,
        [FromQuery] string? sport = null,
        [FromQuery] string? level = null,
        [FromQuery] string? gender = null)
        {
            try
            {
                // Fetch the data from the database with optional filtering
                var sportsQuery = _context.SportDetails.AsNoTracking();

                // Apply filters if parameters are provided
                if (!string.IsNullOrEmpty(category))
                {
                    sportsQuery = sportsQuery.Where(s => s.Category == category);
                }
                if (!string.IsNullOrEmpty(sport))
                {
                    sportsQuery = sportsQuery.Where(s => s.Sport == sport);
                }
                if (!string.IsNullOrEmpty(level))
                {
                    sportsQuery = sportsQuery.Where(s => s.Level == level);
                }
                if (!string.IsNullOrEmpty(gender))
                {
                    sportsQuery = sportsQuery.Where(s => s.Gender == gender);
                }

                // Execute the query and get the list
                var sports = await sportsQuery.ToListAsync();

                // Group the sports by category
                var groupedSports = sports
                    .GroupBy(c => c.Category)
                    .Select(categoryGroup => new SportsDTO.SportDetails.SD_SportCategoriesContent
                    {
                        Category = categoryGroup.Key,
                        SportList = categoryGroup
                            .GroupBy(s => new { s.Sport, s.Description })
                            .Select(sportGroup => new SportsDTO.SportDetails.SD_SportsContent
                            {
                                Sport = sportGroup.Key.Sport,
                                Description = sportGroup.Key.Description,
                                LevelList = sportGroup
                                    .GroupBy(l => l.Level)
                                    .Select(levelGroup => new SportsDTO.SportDetails.SD_SchoolLevelsContent
                                    {
                                        Level = levelGroup.Key,
                                        GenderList = levelGroup
                                            .GroupBy(g => g.Gender)
                                            .Select(genderGroup => new SportsDTO.SportDetails.SD_GenderCategoriesContent
                                            {
                                                Gender = genderGroup.Key,
                                                SubCategoryList = genderGroup
                                                    .Select(subCategory => new SportsDTO.SportDetails.SD_SubCategoriesContent
                                                    {
                                                        SubCategory = subCategory.SubCategory
                                                    }).ToList()
                                            }).ToList()
                                    }).ToList()
                            }).ToList()
                    }).ToList();

                return Ok(groupedSports);
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database update error: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("SportsCategoriesDetails")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SportCategoryDetails.SCD_CategoriesContent>>> GetSportCategoriesDetails()
        {
            try
            {
                // Fetch the data from the database
                var sports = await _context.SportDetails
                    .AsNoTracking()
                    .ToListAsync();

                // Group the sports by category
                var groupedSports = sports
                    .GroupBy(c => c.Category)
                    .Select(category => new SportsDTO.SportCategoryDetails.SCD_CategoriesContent
                    {
                        Category = category.Key,
                        SportList = category
                        .DistinctBy(s => new { s.Sport, s.Description })
                        .Select(sport => new SportsDTO.SportCategoryDetails.SCD_SportsContent
                        {
                            Sport = sport.Sport,
                            Description = sport.Description
                        }).ToList()
                    }).ToList();

                return Ok(groupedSports);
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database update error: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("SportCategoriesAndSportsDetails")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SportCategoryAndSubCategoryDetails.SCASD_CategoriesContent>>> GetSportCategoriesAndSportsDetails()
        {
            try
            {
                // Fetch the data from the database
                var sports = await _context.SportDetails
                .AsNoTracking()
                .ToListAsync();

                // Group the sports by category
                var groupedSports = sports
                    .GroupBy(c => c.Category)
                    .Select(category => new SportsDTO.SportCategoryAndSubCategoryDetails.SCASD_CategoriesContent
                    {
                        Category = category.Key,
                        SportList = category
                        .GroupBy(s => new { s.Sport, s.Description })
                        .Select(sport => new SportsDTO.SportCategoryAndSubCategoryDetails.SCASD_SportsContent
                        {
                            Sport = sport.Key.Sport,
                            Description = sport.Key.Description
                        }).ToList()
                    }).ToList();

                return Ok(groupedSports);
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database update error: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("SportCategoriesAndSubCategoriesDetailsFiltered")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SportCategoryAndSubCategoryDetails.SCASD_CategoriesContent>>> GetSportsCategoriesAndSubCategoriesDetailsFiltered(
        [FromQuery] string? category = null,
        [FromQuery] string? sport = null)
        {
            try
            {
                // Fetch the data from the database
                var sportsQuery = _context.SportDetails.AsNoTracking();

                // Apply filtering if `category` is provided
                if (!string.IsNullOrEmpty(category))
                {
                    sportsQuery = sportsQuery.Where(s => s.Category == category);
                }

                // Apply filtering if `sport` is provided
                if (!string.IsNullOrEmpty(sport))
                {
                    sportsQuery = sportsQuery.Where(s => s.Sport == sport);
                }

                var sports = await sportsQuery.ToListAsync();

                // Group the sports by category
                var groupedSports = sports
                    .GroupBy(c => c.Category)
                    .Select(categoryGroup => new SportsDTO.SportCategoryAndSubCategoryDetails.SCASD_CategoriesContent
                    {
                        Category = categoryGroup.Key,
                        SportList = categoryGroup
                            .GroupBy(s => new { s.Sport, s.Description })
                            .Select(sportGroup => new SportsDTO.SportCategoryAndSubCategoryDetails.SCASD_SportsContent
                            {
                                Sport = sportGroup.Key.Sport,
                                Description = sportGroup.Key.Description,
                                SportSubCategoryList = sportGroup
                                .Select(subCategory => subCategory.SubCategory)
                                .Distinct()
                                .Select(distinctSubCategory => new SportsDTO.SportCategoryAndSubCategoryDetails.SCASD_SubCategoriesContent
                                {
                                    SubCategory = distinctSubCategory
                                })
                                .ToList()
                            }).ToList()
                    }).ToList();

                return Ok(groupedSports);
            }
            catch (DbUpdateException dbEx)
            {
                // Handle database update exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database update error: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Internal server error: {ex.Message}");
            }
        }



        /// 
        /// 
        /// SPORT CATEGORIES
        /// 
        /// 

        // Create
        [HttpPost("SportCategory")]
        public async Task<ActionResult<SportsDTO.SportCategories.SportCategoriesContent>> CreateSportCategory(SportsDTO.SportCategories.SportCategoriesContent sportCategoryContent)
        {

            try
            {
                var sportCategory = new SportCategories
                {
                    ID = sportCategoryContent.ID,
                    Category = sportCategoryContent.Category,
                };

                _context.SportCategories.Add(sportCategory);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSports), new { id = sportCategory.ID }, sportCategoryContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("SportCategory")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SportCategories.SportCategoriesContent>>> GetSportCategory()
        {
            try
            {
                var categories = await _context.SportCategories.AsNoTracking().ToListAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("SportCategory/{id}")]
        public async Task<IActionResult> UpdateSportCategory(int id, SportsDTO.SportCategories.SportCategoriesContent sportCategoryContent)
        {
            if (id != sportCategoryContent.ID)
            {
                return BadRequest("Event Versus ID mismatch");
            }

            try
            {
                var sportCategory = new SportCategories
                {
                    ID = sportCategoryContent.ID,
                    Category = sportCategoryContent.Category,
                };

                _context.SportCategories.Attach(sportCategory);
                _context.Entry(sportCategory).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SportCategories.Any(e => e.ID == id))
                {
                    return NotFound($"Event Versus with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("SportCategory/{id}")]
        public async Task<IActionResult> DeleteSportCategory(int id)
        {
            var sportCategory = await _context.SportCategories.FindAsync(id);
            if (sportCategory == null)
            {
                return NotFound($"Sport Category with ID {id} not found");
            }

            _context.SportCategories.Remove(sportCategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        /// 
        /// 
        /// SPORTS
        /// 
        /// 

        // Create
        [HttpPost("Sport")]
        public async Task<ActionResult<SportsDTO.Sports.SportsContent>> CreateSport([FromBody] SportsDTO.Sports.SportsContent sportContent)
        {
            try
            {
                var sport = new Sports
                {
                    ID = sportContent.ID,
                    Sport = sportContent.Sport,
                    Description = sportContent.Description,
                };

                _context.Sports.Add(sport);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSports), new { id = sport.ID }, sportContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Get By Sport Category ID
        [HttpGet("SportsByCategory")]
        public async Task<ActionResult<IEnumerable<SportsDTO.Sports.SportsContent>>> GetSportsByCategory(
        [FromQuery] int? SportCategoryID = null)
        {
            try
            {
                // If SportSubCategoryID is provided, filter based on it
                var query = _context.Sports.AsNoTracking();

                if (SportCategoryID.HasValue)
                {
                    query = query.Where(s => s.SportCategoryID == SportCategoryID);
                }

                // Fetch the filtered sports list
                var sports = await query
                    .Select(s => new SportsDTO.Sports.SportsContent
                    {
                        ID = s.ID,
                        Sport = s.Sport,
                        Description = s.Description,
                        SportCategoryID = s.SportCategoryID
                    })
                    .ToListAsync();

                return Ok(sports);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("Sport")]
        public async Task<ActionResult<IEnumerable<SportsDTO.Sports.SportsContent>>> GetSports()
        {
            try
            {
                var sports = await _context.Sports.AsNoTracking().ToListAsync();
                return Ok(sports);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("Sport/{id}")]
        public async Task<IActionResult> UpdateSport(int id, SportsDTO.Sports.SportsContent sportContent)
        {
            if (id != sportContent.ID)
            {
                return BadRequest("Event Versus ID mismatch");
            }

            try
            {
                var sport = new Sports
                {
                    ID = sportContent.ID,
                    Sport = sportContent.Sport,
                    Description = sportContent.Description,
                };

                _context.Sports.Attach(sport);
                _context.Entry(sport).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Sports.Any(e => e.ID == id))
                {
                    return NotFound($"Event Versus with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("Sport/{id}")]
        public async Task<IActionResult> DeleteSport(int id)
        {
            var sport = await _context.Sports.FindAsync(id);
            if (sport == null)
            {
                return NotFound($"Sport with ID {id} not found");
            }

            _context.Sports.Remove(sport);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        /// 
        /// 
        /// LEVELS
        /// 
        /// 

        // Create
        [HttpPost("Level")]
        public async Task<ActionResult<SportsDTO.SchoolLevels.SchoolLevelsContent>> CreateLevel(SportsDTO.SchoolLevels.SchoolLevelsContent schoolLevelContent)
        {
            try
            {
                var schoolLevel = new SchoolLevels
                {
                    ID = schoolLevelContent.ID,
                    Level = schoolLevelContent.Level,
                };

                _context.SchoolLevels.Add(schoolLevel);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSports), new { id = schoolLevel.ID }, schoolLevelContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("Level")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SchoolLevels.SchoolLevelsContent>>> GetLevel()
        {
            try
            {
                var levels = await _context.SchoolLevels.AsNoTracking().ToListAsync();
                return Ok(levels);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("Level/{id}")]
        public async Task<IActionResult> UpdateLevel(int id, SportsDTO.SchoolLevels.SchoolLevelsContent schoolLevelContent)
        {
            if (id != schoolLevelContent.ID)
            {
                return BadRequest("Event Versus ID mismatch");
            }

            try
            {
                var schoolLevel = new SchoolLevels
                {
                    ID = schoolLevelContent.ID,
                    Level = schoolLevelContent.Level,
                };

                _context.SchoolLevels.Attach(schoolLevel);
                _context.Entry(schoolLevel).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SchoolLevels.Any(e => e.ID == id))
                {
                    return NotFound($"Event Versus with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("Level/{id}")]
        public async Task<IActionResult> DeleteLevel(int id)
        {
            var level = await _context.SchoolLevels.FindAsync(id);
            if (level == null)
            {
                return NotFound($"Level with ID {id} not found");
            }

            _context.SchoolLevels.Remove(level);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        /// 
        /// 
        /// GENDER CATEGORIES
        /// 
        /// 

        // Create
        [HttpPost("GenderCategory")]
        public async Task<ActionResult<SportsDTO.GenderCategories.GenderCategoriesContent>> CreateGenderCategory(GenderCategories genderCategoryContent)
        {
            try
            {
                var genderCategory = new GenderCategories
                {
                    ID = genderCategoryContent.ID,
                    Gender = genderCategoryContent.Gender,
                };

                _context.GenderCategories.Add(genderCategory);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSports), new { id = genderCategory.ID }, genderCategoryContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Read
        [HttpGet("GenderCategory")]
        public async Task<ActionResult<IEnumerable<SportsDTO.GenderCategories.GenderCategoriesContent>>> GetGenderCategories()
        {
            try
            {
                var genderCategories = await _context.GenderCategories.AsNoTracking().ToListAsync();
                return Ok(genderCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("GenderCategory/{id}")]
        public async Task<IActionResult> UpdateGenderCategory(int id, SportsDTO.GenderCategories.GenderCategoriesContent genderCategoryContent)
        {
            if (id != genderCategoryContent.ID)
            {
                return BadRequest("Event Versus ID mismatch");
            }

            try
            {
                var genderCategory = new GenderCategories
                {
                    ID = genderCategoryContent.ID,
                    Gender = genderCategoryContent.Gender,
                };

                _context.GenderCategories.Attach(genderCategory);
                _context.Entry(genderCategory).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.GenderCategories.Any(e => e.ID == id))
                {
                    return NotFound($"Event Versus with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("GenderCategory/{id}")]
        public async Task<IActionResult> DeleteGenderCategory(int id)
        {
            var genderCategory = await _context.GenderCategories.FindAsync(id);
            if (genderCategory == null)
            {
                return NotFound($"Gender Category with ID {id} not found");
            }

            _context.GenderCategories.Remove(genderCategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        /// 
        /// 
        /// SPORT SUB CATEGORIES
        /// 
        /// 

        // Create
        [HttpPost("SportSubCategory")]
        public async Task<ActionResult<SportsDTO.SportSubCategories.SportSubCategoriesContent>> CreateSportSubCategory(SportSubCategories sportSubCategoryContent)
        {
            try
            {
                var sportSubCategory = new SportSubCategories
                {
                    ID = sportSubCategoryContent.ID,
                    SubCategory = sportSubCategoryContent.SubCategory,
                };

                _context.SportSubCategories.Add(sportSubCategory);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSports), new { id = sportSubCategory.ID }, sportSubCategoryContent);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Filtered
        [HttpGet("SportSubCategoriesFiltered")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SportSubCategories.SportSubCategoriesContent>>> GetSportSubCategoriesFiltered(
        [FromQuery] int? sport = null,
        [FromQuery] int? level = null,
        [FromQuery] int? gender = null)
        {
            try
            {
                // Start with the base query for SportSubCategories
                var query = _context.SportSubCategories.AsNoTracking();

                // Apply filtering based on the provided query parameters
                if (sport.HasValue)
                {
                    query = query.Where(s => s.SportID == sport);
                }

                if (level.HasValue)
                {
                    query = query.Where(s => s.SchoolLevelID == level);
                }

                if (gender.HasValue)
                {
                    query = query.Where(s => s.GenderCategoryID == gender);
                }

                // Fetch the filtered SportSubCategories list
                var subCategories = await query
                    .Select(s => new SportsDTO.SportSubCategories.SportSubCategoriesContent
                    {
                        ID = s.ID,
                        SubCategory = s.SubCategory,
                        SportID = s.SportID,
                        GenderCategoryID = s.GenderCategoryID,
                        SchoolLevelID = s.SchoolLevelID
                    })
                    .ToListAsync();

                return Ok(subCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }


        // Read
        [HttpGet("SportSubCategories")]
        public async Task<ActionResult<IEnumerable<SportsDTO.SportSubCategories.SportSubCategoriesContent>>> GetSportSubCategories()
        {
            try
            {
                var subCategories = await _context.SportSubCategories.AsNoTracking().ToListAsync();
                return Ok(subCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        // Update
        [HttpPut("SportSubCategory/{id}")]
        public async Task<IActionResult> UpdateSportSubCategory(int id, SportsDTO.SportSubCategories.SportSubCategoriesContent sportSubCategoryContent)
        {
            if (id != sportSubCategoryContent.ID)
            {
                return BadRequest("Event Versus ID mismatch");
            }

            try
            {
                var sportSubCategory = new SportSubCategories
                {
                    ID = sportSubCategoryContent.ID,
                    SubCategory = sportSubCategoryContent.SubCategory,
                };

                _context.SportSubCategories.Attach(sportSubCategory);
                _context.Entry(sportSubCategory).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SportSubCategories.Any(e => e.ID == id))
                {
                    return NotFound($"Event Versus with ID {id} not found");
                }
                throw;
            }

            return NoContent();
        }

        // Delete
        [HttpDelete("SportSubCategory/{id}")]
        public async Task<IActionResult> DeleteSportSubCategories(int id)
        {
            var sportSubCategories = await _context.SportSubCategories.FindAsync(id);
            if (sportSubCategories == null)
            {
                return NotFound($"Sport SubCategory with ID {id} not found");
            }

            _context.SportSubCategories.Remove(sportSubCategories);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
