using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;

namespace PayRexApplication.Controllers
{
    [ApiController]
    [Route("api/debug")]
    public class DebugController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DebugController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("db")]
        public IActionResult TestDb()
        {
            try
            {
                _db.Database.OpenConnection();
                _db.Database.CloseConnection();
                return Ok("DATABASE CONNECTED OK");
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }
    }
}
