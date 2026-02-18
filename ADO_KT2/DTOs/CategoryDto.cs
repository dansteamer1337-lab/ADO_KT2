using System.ComponentModel.DataAnnotations;

namespace MyApi.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class CreateCategoryDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    public string Description { get; set; }
}

public class UpdateCategoryDto
{

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    public string Description { get; set; }
}