namespace AlegriaPosApi.DTOs.ProductModifiers
{
    public class ProductModifierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsMultiple { get; set; }

        public List<ProductModifierOptionDto> Options { get; set; } = new();
    }
}
