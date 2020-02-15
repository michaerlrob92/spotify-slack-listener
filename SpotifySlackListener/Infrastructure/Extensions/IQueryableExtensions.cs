using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SpotifySlackListener.Infrastructure.Extensions
{
    public static class IQueryableExtensions
    {
        public static async IAsyncEnumerable<List<T>> QueryInChunksOfAsync<T>(this IQueryable<T> queryable, int chunkSize,
            int chunkNumber = 0)
        {
            while (true)
            {
                var query = chunkNumber == 0
                    ? queryable
                    : queryable.Skip(chunkNumber * chunkSize);
                
                var chunk = await query.Take(chunkSize).ToListAsync();
                if (chunk.Count == 0)
                {
                    yield break;
                }

                yield return chunk;
                
                chunkNumber++;
            }
        }
    }
}