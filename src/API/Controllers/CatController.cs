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
        [EndpointDescription("Все категории")]
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
        [EndpointDescription($@"Древо категорий  
            Доступна динамическая подгрузка узлов передачей параметра `root_id`.<br>  
            ➕ Includes: ``
        ")]
        public async Task<List<GetCatFamilyEntry>> GetFamily(
            [FromQuery] [Description("Корень поддерева")] Guid? root_id,
            [FromQuery] HashSet<string> includes,
            [FromQuery] [Description("Максимальное количество уровней")] int max_depth = 6
        ) => await service.Family(max_depth, root_id, includes);

        [HttpGet("breadcrumbs")]
        [EndpointDescription($@"Путь от корня до конкретной категории.<br>  
            ➕ Includes: ``
        ")]
        public async Task<ICollection<GetCatBasic>> GetBreadcrumbs(
           [FromQuery][Description("Конечная категория")][Required] Guid cat_id,
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
        [EndpointDescription("Конкретная категория")]
        public async Task<Results<Ok<GetCatDTO>, NotFound>> Get(Guid uuid)
            => await service.Find(uuid) switch {
                Cat x => TypedResults.Ok(GetCatDTO.FromEntity(x)),
                _ => TypedResults.NotFound()
            };

        [HttpPost("{uuid}")]
        [EndpointDescription("Создание категории")]
        [AuthorizeJWTRoles(Role.Admin)]
        public async Task<Results<Ok<GetCatDTO>, NotFound>> Create(Guid uuid, [FromBody] UpdateCat request)
            => (await service.Create(uuid, request)).Result switch {
                Ok<Cat> { Value: var x } => TypedResults.Ok(GetCatDTO.FromEntity(x!)),
                NotFound __ => __
            };

        [HttpPut("{uuid}")]
        [EndpointDescription("Обновление категории")]
        [AuthorizeJWTRoles(Role.Admin)]
        public async Task<Results<Ok<GetCatDTO>, NotFound>> Update(Guid uuid, [FromBody] UpdateCat request)
            => await service.Find(uuid) switch {
                Cat entity => TypedResults.Ok(GetCatDTO.FromEntity(
                    await service.Update(entity, request)!)),
                _ => TypedResults.NotFound()
            };

        [HttpDelete("{uuid}")]
        [EndpointDescription("Удаление категории")]
        [AuthorizeJWTRoles(Role.Admin)]
        public async Task<Results<Ok, ValidationProblem, NotFound>> Delete(
            Guid uuid, 
            [FromQuery] [Description("Рекурсивно")] bool recursive = false
        ) => await service.Delete(uuid, recursive);

        /*[HttpPost("{uuid}/move-products/{to_uuid}")]
        [EndpointDescription("Перемещение всех товаров в новую категорию")]
        [AuthorizeJWTRoles(Role.Admin)]
        public async Task<Results<Ok, NotFound>> BulkMovePosts(
            Guid uuid, 
            Guid to_uuid, 
            [FromQuery] [Description("Рекурсивно")] bool recursive = false
        ) => await service.BulkMoveProducts(uuid, to_uuid, recursive);*/
    }
}
