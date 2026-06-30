using Asp.Versioning;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Data;
using MinimalAPI.Models;
using MinimalAPI.Models.DTOs;
using System.Net;

namespace MinimalAPI.Endpoints.v1
{
    public static class CategoryEndpoints
    {
        public static RouteGroupBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var categoryGroup = app.MapGroup("/api/v{version:apiVersion}/categories").WithTags("Categories");//.RequireAuthorization();


            categoryGroup.MapGet("/", GetAllCategories)
                .WithName("GetAllCategories")
                .Produces<APIResponse>(StatusCodes.Status200OK)
                .MapToApiVersion(new ApiVersion(1, 0));

            categoryGroup.MapGet("/{id:int}", GetCategoryById)
                .WithName("GetCategoryById")
                .Produces<APIResponse>(StatusCodes.Status200OK)
                 .Produces<APIResponse>(StatusCodes.Status404NotFound)
                 .MapToApiVersion(new ApiVersion(1, 0))
                 .AddEndpointFilter(async (context, next) =>
                 {
                     var id = context.GetArgument<int>(0);
                     if (id <= 0)
                     {
                         return Results.BadRequest("Cannot have 0 or lesser number as the id");
                     }
                     Console.WriteLine("Before 1st Filter");
                     var result = await next(context);
                     Console.WriteLine("After 1st Filter");
                     return result;
                 })
                 .AddEndpointFilter(async (context, next) =>
                 {
                     Console.WriteLine("Before 2nd Filter");
                     var result = await next(context);
                     Console.WriteLine("After 2nd Filter");
                     return result;
                 });

            categoryGroup.MapPost("/", CreateCategory)
                .WithName("CreateCategory")
                // .RequireAuthorization(u => u.RequireRole(SD.Role_Admin))
                .Accepts<CategoryCreateDTO>("application/json")
                .Produces<APIResponse>(StatusCodes.Status201Created)
                .Produces<APIResponse>(StatusCodes.Status400BadRequest)
                .Produces<APIResponse>(StatusCodes.Status404NotFound)
                .MapToApiVersion(new ApiVersion(1, 0));

            categoryGroup.MapPut("/{id:int}", UpdateCategory)
                // .RequireAuthorization(u => u.RequireRole(SD.Role_Admin))
                .WithName("UpdateCategory")
                .Accepts<CategoryCreateDTO>("application/json")
                .Produces<APIResponse>(StatusCodes.Status200OK)
                .Produces<APIResponse>(StatusCodes.Status400BadRequest)
                .Produces<APIResponse>(StatusCodes.Status404NotFound)
                .MapToApiVersion(new ApiVersion(1, 0));

            categoryGroup.MapDelete("/{id:int}", DeleteCategory)
                // .RequireAuthorization(u => u.RequireRole(SD.Role_Admin))
                .WithName("DeleteCategory")
                .Accepts<CategoryCreateDTO>("application/json")
                .Produces<APIResponse>(StatusCodes.Status200OK)
                .Produces<APIResponse>(StatusCodes.Status400BadRequest)
                .Produces<APIResponse>(StatusCodes.Status404NotFound)
                .MapToApiVersion(new ApiVersion(1, 0));

            return categoryGroup;
        }


        private static async Task<IResult> GetAllCategories(ApplicationDBContext db, IMapper mapper)
        {
            var categories = await db.Categories.ToListAsync();
            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                Result = mapper.Map<List<CategoryDTO>>(categories),
                StatusCode = HttpStatusCode.OK
            });
        }

        private static async Task<IResult> GetCategoryById(int id, ApplicationDBContext db, IMapper mapper)
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category is null)
            {
                return Results.NotFound(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["Category not found"]
                });
            }
            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                Result = mapper.Map<CategoryDTO>(category),
                StatusCode = HttpStatusCode.OK
            });
        }

        private static async Task<IResult> CreateCategory(CategoryCreateDTO categoryCreateDTO, ApplicationDBContext db, IMapper mapper)
        {
            var category = mapper.Map<Category>(categoryCreateDTO);
            category.AddedDate = DateTime.Now;
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            var categoryDTO = mapper.Map<CategoryDTO>(category);

            return Results.CreatedAtRoute("GetCategoryById", new { id = category.Id }, new APIResponse
            {
                IsSuccess = true,
                Result = categoryDTO,
                StatusCode = HttpStatusCode.Created
            });
        }

        private static async Task<IResult> UpdateCategory(int id, CategoryUpdateDTO categoryUpdateDTO, ApplicationDBContext db, IMapper mapper)
        {
            var category = await db.Categories.FirstOrDefaultAsync(u => u.Id == id);
            if (category is null)
            {
                return Results.NotFound(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["Category not found"]
                });
            }
            mapper.Map(categoryUpdateDTO, category);
            await db.SaveChangesAsync();
            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            });

        }

        private static async Task<IResult> DeleteCategory(int id, ApplicationDBContext db)
        {
            var category = await db.Categories.FirstOrDefaultAsync(u => u.Id == id);
            if (category is null)
            {
                return Results.NotFound(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["Category not found"]
                });
            }
            db.Categories.Remove(category);
            await db.SaveChangesAsync();

            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            });
        }
    }
}
