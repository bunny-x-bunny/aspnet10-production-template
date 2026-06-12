using Application.DTO.Cat;
using Application.DTO.File;
using Application.Helpers;
using Application.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Linq.Expressions;

namespace Application.Services {
  public class CatService(AppDbContext context) : ICatService {
    private readonly AppDbContext context = context;

    public IQueryable<Cat> All() => context.Cats;

    public async Task<List<GetCatFamilyEntry>> Family(int max_depth, Guid? root_id, HashSet<string>? includes = null)
        => await context.Cats
        .Where(c => root_id != null
            ? c.Id == root_id
            : c.ParentId == null)
        .Select(CatServiceExt.GetCatProjection(max_depth, 0, includes))
        .OrderBy(x => x.Name)
        .ToListAsync();

    public Task<GetCatFamilyEntry> Breadcrumbs(Guid child_cat, int max_depth, HashSet<string>? includes = null)
        => Breadcrumbs(child_cat, null, max_depth, 0, 5, includes);

    private async Task<GetCatFamilyEntry> Breadcrumbs(Guid child_cat, Guid? root_id, int depth_per_iteration, int iterations, int max_iterations, HashSet<string>? includes = null) {
      var _child_cat = context.Cats.Find(child_cat);
      var tree = await context.Cats
      .Where(c => root_id != null
          ? c.Id == root_id
          : c.ParentId == null)
      .Select(CatServiceExt.GetCatProjectionConstrained(_child_cat!, depth_per_iteration, 0, includes))
      .FirstAsync();

      GetCatFamilyEntry leaf = tree;
      while (leaf.Children.Count != 0)
        leaf = leaf.Children.First();
      if (leaf.Id != child_cat && iterations < max_iterations)
        // NOTE: double recursive call
        leaf.Children = (await Breadcrumbs(child_cat, leaf.Id, depth_per_iteration, iterations + 1, max_iterations, includes)).Children;

      return tree;
    }

    public async Task<Results<Ok<Cat>, NotFound>> Create(Guid parent_id, UpdateCat request) {
      if (await context.Cats
          .Include(c => c.Children)
          .SingleOrDefaultAsync(c => c.Id == parent_id) is not { } parent)
        return TypedResults.NotFound();
      var id = Guid.CreateVersion7();
      Cat new_cat;
      if (!parent.HasChildren())
        new_cat = await context.CreateChildCat(parent_id, id)
            .SingleAsync();
      else
        new_cat = await context.CreateSiblingCat(parent.Children.First().Id, id)
            .SingleAsync();
      new_cat = request.UpdateEntity(new_cat);
      await context.SaveChangesAsync();
      return TypedResults.Ok(new_cat);
    }

    public async Task<Results<Ok, ValidationProblem, NotFound>> Delete(Guid uuid, bool recursive) {
      if (await Find(uuid) is not Cat entity) return TypedResults.NotFound();
      if (entity.ParentId is null)
        return Validation.CreateValidationProblem("NotAChildCat", "Удаление коренной категории запрещено");
      if (entity.HasChildren() && !recursive)
        return Validation.CreateValidationProblem("NotALeafCat", "У данной категории имеются потомки");
      //if (await context.Products
      //    .Where(p => p.Cat.LeftEar >= entity.LeftEar && p.Cat.RightEar <= entity.RightEar)
      //    .AnyAsync())
      //  return Validation.CreateValidationProblem("CatHasProducts", "У данной категории имеются товары");

      var cats = recursive
          ? await All().Where(c => c.LeftEar >= entity.LeftEar && c.RightEar <= entity.RightEar)
              .OrderByDescending(c => c.LeftEar)
              .ToListAsync()
          : [entity];

      await using var transaction = await context.Database.BeginTransactionAsync();
      foreach (var cat in cats) {
        await context.DeleteLeafCat(entity.Id);
      }
      await transaction.CommitAsync();
      return TypedResults.Ok();
    }


    public async Task<Cat?> Find(Guid uuid) => await All().SingleOrDefaultAsync(c => c.Id == uuid);

    public async Task<Cat> Update(Cat entity, UpdateCat request) {
      entity = request.UpdateEntity(entity);
      await context.SaveChangesAsync();
      return entity;
    }

    /// <summary>
    /// Move all products from old cat to new cat
    /// </summary>
    /*public async Task<Results<Ok, NotFound>> BulkMoveProducts(Guid old_cat_id, Guid new_cat_id, bool recursive) {
      if (await context.Cats.FindAsync(old_cat_id) is not { } old_cat)
        return TypedResults.NotFound();
      if (await context.Cats.FindAsync(new_cat_id) is not { } new_cat)
        return TypedResults.NotFound();
      var query = context.Products.AsQueryable();
      query = recursive
          ? query.Where(p => p.Cat.LeftEar >= old_cat.LeftEar && p.Cat.RightEar <= old_cat.RightEar)
          : query.Where(p => p.CatId == old_cat_id);
      await query
          .ExecuteUpdateAsync(p => p.SetProperty(p => p.CatId, new_cat_id));
      return TypedResults.Ok();
    }*/
  }

  public static class CatServiceExt {
    /// <summary>
    /// Implementation of the recursive projection method
    /// </summary>
    public static Expression<Func<Cat, GetCatFamilyEntry>> GetCatProjection(int max_depth, int current_depth = 0, HashSet<string>? includes = null)
        => cat => new GetCatFamilyEntry() {
          Id = cat.Id,
          Name = cat.Name,
          LeftEar = cat.LeftEar,
          RightEar = cat.RightEar,
          Children = current_depth + 1 == max_depth
                ? new List<GetCatFamilyEntry>()
                : cat.Children.AsQueryable()
                    .Select(GetCatProjection(max_depth, current_depth + 1, includes))
                    .OrderBy(x => x.Name).ToList()
        };

    public static Expression<Func<Cat, GetCatFamilyEntry>> GetCatProjectionConstrained(Cat child_cat, int max_depth, int current_depth = 0, HashSet<string>? includes = null)
        => cat => new GetCatFamilyEntry() {
          Id = cat.Id,
          Name = cat.Name,
          LeftEar = cat.LeftEar,
          RightEar = cat.RightEar,
          Children = current_depth + 1 == max_depth
                ? new List<GetCatFamilyEntry>()
                : cat.Children.AsQueryable()
                    .Where(c => c.LeftEar <= child_cat.LeftEar && c.RightEar >= child_cat.RightEar)
                    .Select(GetCatProjectionConstrained(child_cat, max_depth, current_depth + 1, includes))
                    .ToList()
        };
  }
}
