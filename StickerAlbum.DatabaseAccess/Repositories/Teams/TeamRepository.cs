﻿using Microsoft.EntityFrameworkCore;
using StickerAlbum.Common;
using StickerAlbum.Filters;
using StickerAlbum.Model.Teams;

namespace StickerAlbum.DatabaseAccess.Repositories.Teams
{
    public class TeamRepository : ITeamRepository
    {
        private readonly StickerAlbumContext _context;

        private IQueryable<Team> Teams => _context.Teams.AsQueryable();

        public TeamRepository(StickerAlbumContext context)
        {
            _context = context;
        }

        public async Task<ApplicationResult> Create(Team team)
        {
            team.DateCreated = DateTime.UtcNow;
            team.DateModified = DateTime.UtcNow;

            await _context.AddAsync(team);
            await _context.SaveChangesAsync();

            return new ApplicationResult();
        }

        public async Task<ApplicationResult> Update(Team team)
        {
            var item = await _context.Teams.FindAsync(team.Id);

            item.Name = team.Name;
            item.PrimaryColor = team.PrimaryColor;
            item.SecondaryColor = team.SecondaryColor;
            item.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApplicationResult();
        }

        public async Task<ApplicationResult> Delete(Guid id)
        {
            var item = await _context.Teams.FindAsync(id);

            item.DateDeleted = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApplicationResult();
        }

        public async Task<ApplicationResult<Team>> Get(Guid id)
        {
            var item = await Teams.Where(x => x.Id == id).ToListAsync();

            var result = new ApplicationResult<Team>()
            {
                Result = item.FirstOrDefault()
            };

            if (!item.Any())
            {
                result.Errors.Add($"No items found for the id: {id}");
            }
            return result;
        }

        public async Task<ApplicationResult<CollectionResult<Team>>> GetAll(Filter filter, PagingOptions pagingOptions)
        {
            var query = Teams;

            if(filter.Name != null)
            {
                query = query.Where(x => x.Name == filter.Name);
            }

            var items = await query.Skip(pagingOptions.Offset).Take(pagingOptions.Limit).ToListAsync();
            var total = await query.CountAsync();// Lucas expli dessa duas linhas

            var result = new ApplicationResult<CollectionResult<Team>>
            {
                Result = new CollectionResult<Team>
                {
                    Total = total,
                    Items = items
                }
            };

            if(total == 0)
            {
                result.Errors.Add("No items found for the specified criteria");
            }

            return result;
        }
    }
}
