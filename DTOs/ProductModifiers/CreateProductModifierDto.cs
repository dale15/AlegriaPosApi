namespace AlegriaPosApi.DTOs.ProductModifiers
{
    public class CreateProductModifierDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public bool IsMultiple { get; set; }

        public List<CreateProductModifierOptionDto> Options { get; set; }
            = new();
    }
}
