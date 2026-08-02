using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.DTOs;
using QuestionService.Models;

namespace QuestionService.Controllers;

[ApiController]
[Route("[controller]")]
public class TagsController(QuestionDbContext ctx) : Controller
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Tag>>> GetTags()
    {
        return await ctx.Tags.OrderBy(x=>x.Name).ToListAsync();
    }
    
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Tag>> CreateTag(CreateTagDto dto)
    {
        var tag = new Tag
        {
            Name = dto.Name,
            Slug = dto.Slug,
            Description = dto.Description
        };

        await ctx.Tags.AddAsync(tag);
        await ctx.SaveChangesAsync();

        return tag;
    }
}