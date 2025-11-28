using Microsoft.AspNetCore.Mvc;
using WhoOwesWho.DatabaseMigrationService.Services;

namespace WhoOwesWho.DatabaseMigrationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseMigrationController(IDatabaseMigrationService databaseMigrationService) : ControllerBase
    {
        [HttpGet]
        [Route("up")]
        public async Task<IActionResult> MigrateOnUp()
        {
            try
            {
                return Ok(await databaseMigrationService.RestoreDatabases());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("down")]
        public async Task<IActionResult> MigrateOnDown()
        {
            try
            {
                return Ok(await databaseMigrationService.BackupDatabases());
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
