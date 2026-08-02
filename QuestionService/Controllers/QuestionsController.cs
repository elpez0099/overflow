using System.Security.Claims;
using Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionService.Data;
using QuestionService.DTOs;
using QuestionService.Models;
using QuestionService.Services;
using Wolverine;

namespace QuestionService.Controllers;

[ApiController]
[Route("[controller]")]
public class QuestionsController(QuestionDbContext ctx, IMessageBus bus, TagService tagService): ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Question>> CreateQuestion(CreateQuestionDto questionDto)
    {
        // Look for tags that exist in the DB
        /*
        var validTags = await ctx.Tags.Where(x => questionDto.Tags.Contains(x.Slug)).ToListAsync();
        var missingTags = questionDto.Tags.Except(validTags.Select(x => x.Slug).ToList()).ToList();
        
        if (missingTags.Any()) return BadRequest($"Tags contains invalid slugs: {string.Join(",", missingTags)}");
        */

        if (!await tagService.AreTagsValidAsync(questionDto.Tags))
        {
            return BadRequest("Tags are invalid");
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
        var userName = User.FindFirstValue("name"); // alt "name"
        
        if (userId is null || userName is null) return BadRequest("Cannot get user identity");

        var question = new Question
        {
            Title = questionDto.Title,
            Content = questionDto.Content,
            TagSlugs = questionDto.Tags,
            AskerId = userId,
            AskerDisplayName = userName
        };
        
        ctx.Questions.Add(question);
        await ctx.SaveChangesAsync();
        
        await bus.PublishAsync(new QuestionCreated(question.Id, question.Title, question.Content, question.CreatedAt, question.TagSlugs));
        
        return Created($"/questions/{question.Id}", question);
    }

    [HttpGet]
    public async Task<ActionResult<List<Question>>> GetQuestions(string? tag )
    {
        var query = ctx.Questions.AsQueryable();
        if (!string.IsNullOrEmpty(tag))
        {
            query = query.Where(x => x.TagSlugs.Contains(tag));
        }
        return await query.OrderByDescending(x=>x.CreatedAt).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Question>> GetQuestionById(string id)
    {
        var question = await ctx.Questions.FindAsync(id);
        if (question is null) return NotFound();

        await ctx.Questions.Where(x => Equals(x.Id, id))
            .ExecuteUpdateAsync<Question>(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + 1));
        
        return question;
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<Question>> UpdateQuestion(string id, CreateQuestionDto questionDto)
    {
        var question = await ctx.Questions.FindAsync(id);
        if (question is null) return NotFound();
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId != question.AskerId) return Forbid();
        
        /*
        var validTags = await ctx.Tags.Where(x => questionDto.Tags.Contains(x.Slug)).ToListAsync();
        var missingTags = questionDto.Tags.Except(validTags.Select(x => x.Slug).ToList()).ToList();
        
        if (missingTags.Any()) return BadRequest($"Tags contains invalid slugs: {string.Join(",", missingTags)}");
        */
        if (!await tagService.AreTagsValidAsync(questionDto.Tags))
        {
            return BadRequest("Tags are invalid");
        }

        question.Title = questionDto.Title;
        question.Content = questionDto.Content;
        question.TagSlugs = questionDto.Tags;

        await ctx.SaveChangesAsync();
        
        await bus.PublishAsync(new QuestionUpdated(question.Id, question.Title, question.Content, question.TagSlugs.ToArray()));
        
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteQuestion(string id)
    {
        var question = await ctx.Questions.FindAsync(id);
        if (question is null) return NotFound();
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId != question.AskerId) return Forbid();

        ctx.Questions.Remove(question);
        await ctx.SaveChangesAsync();
        await bus.PublishAsync(new QuestionDeleted(question.Id));
        return NoContent();
    }
}