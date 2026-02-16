using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.DTOs;
using MyApi.Interfaces;
using MyApi.Models;

namespace MyApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        var categories = await _unitOfWork.Categories.GetAllAsync();

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CategoryId = p.CategoryId,
            CategoryName = categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name
        });

        return Ok(productDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name
        };

        return Ok(productDto);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
    {
        var products = await _unitOfWork.Products
            .FindAsync(p => p.CategoryId == categoryId);

        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId);

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CategoryId = p.CategoryId,
            CategoryName = category?.Name
        });

        return Ok(productDtos);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createDto)
    {
        var categoryExists = await _unitOfWork.Categories
            .ExistsAsync(c => c.Id == createDto.CategoryId);

        if (!categoryExists)
        {
            return BadRequest("Category does not exist");
        }

        var product = new Product
        {
            Name = createDto.Name,
            Description = createDto.Description,
            Price = createDto.Price,
            CategoryId = createDto.CategoryId
        };

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.CompleteAsync();

        var category = await _unitOfWork.Categories.GetByIdAsync(product.CategoryId);

        var productDto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = category?.Name
        };

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto updateDto)
    {
        if (id != updateDto.Id)
        {
            return BadRequest();
        }

        var product = await _unitOfWork.Products.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        var categoryExists = await _unitOfWork.Categories
            .ExistsAsync(c => c.Id == updateDto.CategoryId);

        if (!categoryExists)
        {
            return BadRequest("Category does not exist");
        }

        product.Name = updateDto.Name;
        product.Description = updateDto.Description;
        product.Price = updateDto.Price;
        product.CategoryId = updateDto.CategoryId;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        await _unitOfWork.Products.DeleteAsync(product);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }
}