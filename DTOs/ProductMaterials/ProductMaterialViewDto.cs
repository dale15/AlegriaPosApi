namespace AlegriaPosApi.DTOs.ProductMaterials
{
    public class ProductMaterialViewDto
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public decimal QuantityUsed { get; set; }
    }
}
