using API.Extensions;
using API.Helpers;
using Application.DTO;
using Application.DTO.Cat;
using Application.DTO.Cat.Search;
using Application.Helpers;
using Application.Services.Interfaces;
using Domain.Enum;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MR.AspNetCore.Pagination;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace API.Controllers {
    [Route("[controller]")]
    [ApiController]
    public class CatController(ICatService service, IPaginationService pagination) : ControllerBase {
        private readonly ICatService service = service;
        private readonly IPaginationService pagination = pagination;

        [HttpGet]
        [EndpointDescription("All categories")]
        public async Task<Ok<KeysetPaginationResult<GetCatDTO>>> GetAll(
            [FromQuery] CatSearchModel search,
            [FromQuery] SortModel sort
        )
            => TypedResults.Ok(
                await pagination.KeysetPaginateAsync(
                service.All().Where(search.ToPredicate()),
                sort.ToExpression<Cat>().Compile(),
                async uuid => await service.Find(new Guid(uuid)),
                q => q.Select(e => GetCatDTO.FromEntity(e))
            ));

        [HttpGet("family")]
        [EndpointDescription($@"Category tree
            Nodes can be loaded dynamically by passing the `root_id` parameter.<br>
            ➕ Includes: ``
        ")]
        public async Task<List<GetCatFamilyEntry>> GetFamily(
            [FromQuery] [Description("Subtree root")] Guid? root_id,
            [FromQuery] HashSet<string> includes,
            [FromQuery] [Description("Maximum number of levels")] int max_depth = 6
        ) => await service.Family(max_depth, root_id, includes);

        [HttpGet("breadcrumbs")]
        [EndpointDescription($@"Path from the root to a specific category.<br>
            ➕ Includes: ``
        ")]
        public async Task<ICollection<GetCatBasic>> GetBreadcrumbs(
           [FromQuery][Description("Target category")][Required] Guid cat_id,
           [FromQuery] HashSet<string> includes
        ) {
            var tree = await service.Breadcrumbs(cat_id, 6, includes);
            var path = new List<GetCatBasic> { };
            var node = tree;
            while (node is not null) {
                path.Add(new GetCatBasic { 
                    Id = node.Id, 
                    Name = node.Name,
                });
                node = node.Children.FirstOrDefault();
            }
            return path;
        }

        [HttpGet("{uuid}")]
        [EndpointDescription("A specific category")]
        public async Task<Results<Ok<GetCatDTO>, NotFound>> Get(Guid uuid)
            => await service.Find(uuid) switch {
                Cat x => TypedResults.Ok(GetCatDTO.FromEntity(x)),
                _ => TypedResults.NotFound()
            };

        [HttpPost("{uuid}")]
        [EndpointDescription("Create a category")]
        [AuthorizeJWTRoles(Role.Admin)]
        public async Task<Results<Ok<GetCatDTO>, NotFound>> Create(Guid uuid, [FromBody] UpdateCat request)
            => (await service.Create(uuid, request)).Result switch {
                Ok<Cat> { Value: var x } => TypedResults.Ok(GetCatDTO.FromEntity(x!)),
                NotFound __ => __
            };

        [HttpPut("{uuid}")]
        [EndpointDescription("Update a category")]
        [AuthorizeJWTRoles(Role.Admin)]
        public async Task<Results<Ok<GetCatDTO>, NotFound>> Update(Guid uuid, [FromBody] UpdateCat request)
            => await service.Find(uuid) switch {
                Cat entity => TypedResults.Ok(GetCatDTO.FromEntity(
                    await service.Update(entity, request)!)),
                _ => TypedResults.NotFound()
            };

        [HttpDelete("{uuid}")]
        [EndpointDescription("Delete a category")]
        [AuthorizeJWTRoles(Role.Admin)]
        public async Task<Results<Ok, ValidationProblem, NotFound>> Delete(
            Guid uuid, 
            [FromQuery] [Description("Recursive")] bool recursive = false
        ) => await service.Delete(uuid, recursive);

        /*[HttpPost("{uuid}/move-products/{to_uuid}")]
        [EndpointDescription("Move all products to a new category")]
        [AuthorizeJWTRoles(Role.Admin)]
        public async Task<Results<Ok, NotFound>> BulkMovePosts(
            Guid uuid, 
            Guid to_uuid, 
            [FromQuery] [Description("Recursive")] bool recursive = false
        ) => await service.BulkMoveProducts(uuid, to_uuid, recursive);*/
    }
}
