using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Server.Palaro2026.Entities;

namespace Server.Palaro2026.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class SportSubcategoriesController : ControllerBase
    {
        private readonly Palaro2026Context _context;

        public SportSubcategoriesController(Palaro2026Context context)
        {
            _context = context;
        }

    }
}
