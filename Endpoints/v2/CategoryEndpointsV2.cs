using Asp.Versioning;
using Asp.Versioning.Builder;
using AutoMapper;
using MinimalAPI.Data;
using MinimalAPI.Models;
using System.Net;

namespace MinimalAPI.Endpoints.v2
{
    public static class CategoryEndpointsV2
    {
        public static void MapCategoryEndpointsV2(this IEndpointRouteBuilder app, ApiVersionSet? versionSet = null)
        {
            var categoryGroup = app.MapGroup("/api/v{version:apiVersion}/categories").WithTags("Categories")
                .WithApiVersionSet(versionSet!);//.RequireAuthorization();


            categoryGroup.MapGet("/", GetAllCategories)
                .WithName("GetAllCategoriesV2")
                .Produces<APIResponse>(StatusCodes.Status200OK).MapToApiVersion(new ApiVersion(2, 0));


        }


        private static async Task<IResult> GetAllCategories(ApplicationDBContext db, IMapper mapper)
        {
            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                Result = new
                {
                    Message = "This is V2 of the Category API!",
                    Version = "2.0",
                    Features = new[]
                    {
                        "Enhanced response format",
                        "Additional metadata",
                        "Improved performance"
                    },
                    Demo = "This is a simple demo showing API versioning"
                },
                StatusCode = HttpStatusCode.OK
            });
        }
    }
}
