using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuestionService.Data;
using QuestionService.Models;

namespace QuestionService.Services;

public class TagService(IMemoryCache cache, QuestionDbContext ctx)
{
    private const string TagCacheKey = "Tag";

    public async Task<List<Tag>> GetTags()
    {
        return await cache.GetOrCreateAsync(TagCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await ctx.Tags.AsNoTracking().ToListAsync();
        }) ?? [];
    }

    public async Task<bool> AreTagsValidAsync(List<string> slugs)
    {
        var tags = await GetTags();
        var tagSet = tags.Select(x=>x.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return slugs.All(x => tagSet.Contains(x));
    }
}