using AuditService.DTO;
using AuditService.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AuditService.Controllers
{
    [ApiController]
    [Route("API/[controller]")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IAuditService auditService, ILogger<AuditController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new audit entry by comparing before and after objects
        /// </summary>
        /// <param name="request">Audit request containing entity information and objects to compare</param>
        /// <returns>Audit response with changed fields and metadata</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AuditResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuditResponse>> CreateAuditEntry([FromBody] AuditRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _auditService.CreateAuditEntryAsync(request);
                return CreatedAtAction(nameof(GetAuditEntry), new { id = result.AuditId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit entry");
                return StatusCode(500, "An error occurred while creating the audit entry");
            }
        }

        /// <summary>
        /// Retrieves audit entries based on query parameters
        /// </summary>
        /// <param name="query">Query parameters for filtering audit entries</param>
        /// <returns>Paged list of audit entries</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<AuditResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<AuditResponse>>> GetAuditEntries([FromQuery] AuditQueryRequest query)
        {
            try
            {
                var result = await _auditService.GetAuditEntriesAsync(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit entries");
                return StatusCode(500, "An error occurred while retrieving audit entries");
            }
        }

        /// <summary>
        /// Retrieves a specific audit entry by ID
        /// </summary>
        /// <param name="id">Audit entry ID</param>
        /// <returns>Audit entry details</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AuditResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AuditResponse>> GetAuditEntry([Required] int id)
        {
            try
            {
                var result = await _auditService.GetAuditEntryByIdAsync(id);

                if (result == null)
                    return NotFound($"Audit entry with ID {id} not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit entry {AuditId}", id);
                return StatusCode(500, "An error occurred while retrieving the audit entry");
            }
        }

        /// <summary>
        /// Retrieves the complete audit history for a specific entity
        /// </summary>
        /// <param name="entityName">Name of the entity</param>
        /// <param name="entityId">ID of the entity</param>
        /// <returns>List of audit entries for the entity</returns>
        [HttpGet("entity/{entityName}/{entityId}")]
        [ProducesResponseType(typeof(List<AuditResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<AuditResponse>>> GetEntityHistory(
            [Required] string entityName,
            [Required] string entityId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityName) || string.IsNullOrWhiteSpace(entityId))
                    return BadRequest("Entity name and entity ID are required");

                var result = await _auditService.GetEntityHistoryAsync(entityName, entityId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity history for {EntityName} {EntityId}", entityName, entityId);
                return StatusCode(500, "An error occurred while retrieving entity history");
            }
        }
    }

}
