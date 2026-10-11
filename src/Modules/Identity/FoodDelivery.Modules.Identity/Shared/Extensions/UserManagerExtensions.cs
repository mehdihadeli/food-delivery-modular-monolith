using AutoMapper;
using AutoMapper.QueryableExtensions;
using Squirrel.Abstractions.Core.Paging;
using Squirrel.Core.Extensions;
using Sieve.Services;
using Squirrel.Abstractions.Queries;
using Squirrel.Core.Queries;
using Squirrel.Core.Persistence.EfCore;
using FoodDelivery.Modules.Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Identity.Shared.Extensions;

public static class UserManagerExtensions
{
    public static async Task<IReadOnlyList<ApplicationUser>> FindAllUserWithRoleAsync(
        this UserManager<ApplicationUser> userManager)
    {
        return await userManager.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    public static async Task<IPageList<TResult>> FindAllUsersByPageAsync<TResult>(
        this UserManager<ApplicationUser> userManager,
        IMapper mapper,
        IPageRequest request,
        ISieveProcessor sieveProcessor,
        CancellationToken cancellationToken)
        where TResult : class
    {
        // https://benjii.me/2018/01/expression-projection-magic-entity-framework-core/
        // we don't use include for loading nested navigation because with mapping we load them explicitly
        return await userManager.Users
            .AsNoTracking()
            .ApplyPagingAsync<ApplicationUser, TResult, DateTime>(
                request,
                sieveProcessor,
                query => query.ProjectTo<TResult>(mapper.ConfigurationProvider),
                user => user.CreatedAt,
                cancellationToken: cancellationToken);
    }

    public static async Task<ApplicationUser?> FindUserWithRoleByIdAsync(
        this UserManager<ApplicationUser> userManager,
        Guid userId)
    {
        return await userManager.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.Id == userId);
    }

    public static async Task<ApplicationUser?> FindUserWithRoleByUserNameAsync(
        this UserManager<ApplicationUser> userManager,
        string userName)
    {
        return await userManager.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.UserName == userName);
    }

    public static async Task<ApplicationUser?> FindUserWithRoleByEmailAsync(
        this UserManager<ApplicationUser> userManager,
        string email)
    {
        return await userManager.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x => x.Email == email);
    }
}
