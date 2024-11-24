using FarmManagement.API.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FarmManagement.API.Services
{
    public interface IFieldsService
    {
        // Get a list of all fields asynchronously
        Task<List<Fields>> GetFieldsAsync();

        // Add a new field asynchronously
        Task<Fields> AddFieldAsync(Fields newField);

        // Get a specific field by its ID 
        Task<Fields> GetFieldsAsyncById(string id, string FieldName);

        // Update a field asynchronously 
        Task<Fields> UpdateFieldAsync(Fields updateField);

        // Delete a field by its ID
        Task<int> DeleteFieldAsync(string id, string FieldName);

        // Check if a field name is unique asynchronously
        Task<bool> IsFieldNameUniqueAsync(string fieldName);
    }
}
