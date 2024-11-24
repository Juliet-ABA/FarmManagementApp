using FarmManagement.API.Data.Entities;
using FarmManagement.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FarmManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FieldsController : ControllerBase
    {
        private readonly IFieldsService _fieldsService;

        public FieldsController(IFieldsService fieldsService)
        {
            _fieldsService = fieldsService;
        }

        // Get all fields
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var results = await _fieldsService.GetFieldsAsync();
            if (results == null || !results.Any())
            {
                return NotFound();
            }
            return Ok(results);
        }

        // Add a new field
        [HttpPost]
        public async Task<IActionResult> PostAsync(Fields newField)
        {
            if (newField == null)
            {
                return BadRequest("Field data is required.");
            }

            newField.Id = Guid.NewGuid().ToString(); // Ensure that a unique ID is generated for the new field

            if (newField.FieldArea <= 0)
            {
                return BadRequest("Field Area must be a positive number.");
            }

           
            // Add the field, ensuring that the field name is unique
            var result = await _fieldsService.AddFieldAsync(newField);
            if (result == null)
            {
                return BadRequest("Field Name must be unique.");
            }

            return CreatedAtAction(nameof(GetAsync), new { id = result.Id }, result);
        }

        // Get field by ID
        [HttpGet("{id}/{FieldName}")]
        public async Task<IActionResult> GetAsync(string id, string FieldName)
        {
            string partitionKey = id.ToString();
            var result = await _fieldsService.GetFieldsAsyncById(id, FieldName);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        // Update an existing field
        [HttpPut]
        public async Task<IActionResult> PutAsync(Fields updateField)
        {
            if (updateField == null)
            {
                return BadRequest("Field data is required.");
            }


            var result = await _fieldsService.UpdateFieldAsync(updateField);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // Delete a field by ID
        [HttpDelete("{id}/{FieldName}")]
        public async Task<IActionResult> DeleteAsync(string id, string FieldName)
        {
            var result = await _fieldsService.DeleteFieldAsync(id, FieldName);
            if (result == 0)
            {
                return NotFound();
            }
            return NoContent();
        }

        // Check if the FieldName is unique
        [HttpGet("IsFieldNameUnique")]
        public async Task<IActionResult> IsFieldNameUnique(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                return BadRequest("Field name is required.");
            }

            // Assuming partitionKey here is FieldName as well
            var partitionKey = fieldName;

            var isUnique = await _fieldsService.IsFieldNameUniqueAsync(fieldName);
            return Ok(isUnique);
        }
    }
}
