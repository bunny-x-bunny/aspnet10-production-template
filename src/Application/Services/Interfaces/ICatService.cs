using Application.DTO.Cat;
using Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Application.Services.Interfaces {
  public interface ICatService {
    IQueryable<Cat> All();
    Task<GetCatFamilyEntry> Breadcrumbs(Guid child_cat, int max_depth, HashSet<string>? includes = null);
    Task<Results<Ok<Cat>, NotFound>> Create(Guid parent_id, UpdateCat request);
    Task<Results<Ok, ValidationProblem, NotFound>> Delete(Guid uuid, bool recursive);
    Task<List<GetCatFamilyEntry>> Family(int max_depth, Guid? root_id, HashSet<string>? includes = null);
    Task<Cat?> Find(Guid uuid);
    Task<Cat> Update(Cat entity, UpdateCat request);
  }
}