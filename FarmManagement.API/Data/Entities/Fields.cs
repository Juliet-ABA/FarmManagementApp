using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace FarmManagement.API.Data.Entities
{
    public class Fields
    {
        [Required]
        public string? FieldName { get; set; }

        [Range(0.1, double.MaxValue, ErrorMessage = "Field Area must be a positive number.")]
        public decimal FieldArea { get; set; }

        public string? CropName { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

    }
}
