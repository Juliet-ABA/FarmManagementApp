using FarmManagement.API.Data.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FarmManagement.API.Services
{
    public class CosmosDbService : IFieldsService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly ILogger<CosmosDbService> _logger;

        public CosmosDbService(CosmosClient cosmosClient, ILogger<CosmosDbService> logger)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;

            // Access the specific database and container
            var database = _cosmosClient.GetDatabase("FarmManagementDb");
            _container = database.GetContainer("Fields");
        }

        // Add a new field 
        public async Task<Fields> AddFieldAsync(Fields newField)
        {
            _logger.LogInformation("Attempting to add a new field with name '{FieldName}'", newField.FieldName);

            if (!await IsFieldNameUniqueAsync(newField.FieldName))
            {
                _logger.LogWarning("Field name '{FieldName}' is not unique. Operation aborted.", newField.FieldName);
                return null;
            }

            try
            {
                // Set the partition key as the FieldName internally
                string partitionKey = newField.FieldName;

                // Add the field with partitionKey derived from FieldName
                var response = await _container.CreateItemAsync(newField, new PartitionKey(partitionKey));
                _logger.LogInformation("Successfully added field with name '{FieldName}'.", newField.FieldName);
                return response.Resource;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error occurred while adding the field with name '{FieldName}'", newField.FieldName);
                throw;
            }
        }

        // Fetch all fields
        public async Task<List<Fields>> GetFieldsAsync()
        {
            _logger.LogInformation("Fetching all fields from the Cosmos DB container.");

            try
            {
                var query = _container.GetItemQueryIterator<Fields>();
                var fields = new List<Fields>();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    fields.AddRange(response.ToList());
                }

                _logger.LogInformation("Successfully fetched {FieldCount} fields.", fields.Count);
                return fields;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error occurred while fetching fields from Cosmos DB.");
                throw;
            }
        }

        // Fetch a specific field by ID and get FieldName (Partition Key) dynamically
        public async Task<Fields> GetFieldsAsyncById(string id, string FieldName)
        {
            _logger.LogInformation("Fetching field with ID {FieldId}.", id);

            try
            {

                var response = await _container.ReadItemAsync<Fields>(id, new PartitionKey(FieldName));
                _logger.LogInformation("Successfully fetched field with ID {FieldId}.", id);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Field with ID {FieldId} not found.", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching field with ID {FieldId}.", id);
                throw;
            }
        }

        // Update a field by ID and fetch PartitionKey dynamically from the document
        public async Task<Fields> UpdateFieldAsync(Fields updateField)
        {
            _logger.LogInformation("Attempting to update field with ID '{FieldId}'.", updateField.Id);

            try
            {
                // Fetch the existing document by ID to get the PartitionKey (FieldName)
                var existingField = await GetFieldsAsyncById(updateField.Id, updateField.FieldName);
                if (existingField == null)
                {
                    _logger.LogWarning("Field with ID '{FieldId}' not found for update.", updateField.Id);
                    return null;
                }

                // Use the PartitionKey (FieldName) from the document
                var partitionKey = existingField.FieldName;

                var response = await _container.UpsertItemAsync(updateField, new PartitionKey(partitionKey));
                _logger.LogInformation("Successfully updated field with ID '{FieldId}'.", updateField.Id);
                return response.Resource;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error occurred while updating field with ID '{FieldId}'.", updateField.Id);
                throw;
            }
        }

        // Delete a field by ID and fetch PartitionKey dynamically from the document
        public async Task<int> DeleteFieldAsync(string id,string FieldName)
        {
            _logger.LogInformation("Attempting to delete field with ID '{FieldId}'.", id);

            try
            {
                // Fetch the existing document by ID to get the PartitionKey (FieldName)
                var existingField = await GetFieldsAsyncById(id, FieldName);
                if (existingField == null)
                {
                    _logger.LogWarning("Field with ID '{FieldId}' not found for deletion.", id);
                    return 0; // Return 0 if not found
                }

                // Use the PartitionKey (FieldName) from the document
                var partitionKey = existingField.FieldName;

                // Now delete the item using the PartitionKey
                await _container.DeleteItemAsync<Fields>(id, new PartitionKey(partitionKey));
                _logger.LogInformation("Successfully deleted field with ID '{FieldId}'.", id);
                return 1; // Return 1 to indicate successful delete
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Field with ID '{FieldId}' not found. Deletion aborted.", id);
                return 0; // Return 0 if not found
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error occurred while deleting field with ID '{FieldId}'.", id);
                throw;
            }
        }

        // Check if a field name is unique before adding it
        public async Task<bool> IsFieldNameUniqueAsync(string fieldName)
        {
            _logger.LogInformation("Checking if field name '{FieldName}' is unique.", fieldName);

            try
            {
                var query = _container.GetItemQueryIterator<Fields>(new QueryDefinition("SELECT * FROM c WHERE c.FieldName = @fieldName")
                    .WithParameter("@fieldName", fieldName));

                var isUnique = !(await query.ReadNextAsync()).Any();
                _logger.LogInformation("Field name '{FieldName}' is {Status}.", fieldName, isUnique ? "unique" : "not unique");
                return isUnique;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, "Error occurred while checking if field name '{FieldName}' is unique.", fieldName);
                throw;
            }
        }
    }
}
