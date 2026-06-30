using Asp.Versioning;
using AutoMapper;
using MinimalAPI.Data;
using MinimalAPI.Models;
using System.Net;

namespace MinimalAPI.Endpoints.v2
{
    public static class MenuItemEndpointsV2
    {
        public static RouteGroupBuilder MapMenuItemEndpointsV2(this IEndpointRouteBuilder app)
        {
            var menuitemGroup = app.MapGroup("/api/v{version:apiVersion}/menuitems").WithTags("MenuItems");//.RequireAuthorization();


            menuitemGroup.MapGet("/", GetAllMenuItems)
                .WithName("GetAllMenuItemsV2")
                .Produces<APIResponse>(StatusCodes.Status200OK)
                .MapToApiVersion(new ApiVersion(2, 0));

            return menuitemGroup;

        }


        private static async Task<IResult> GetAllMenuItems(ApplicationDBContext db, IMapper mapper)
        {

            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                Result = new
                {
                    Message = "This is V2 of the Menu Items API!",
                    Version = "2.0",
                    NewFeatures = new[]
                    {
                        "Support for nutritional information",
                        "Allergen warnings",
                        "Special dietary flags (vegan, gluten-free, etc.)",
                        "Chef recommendations"
                    },
                    Demo = "Future versions can add these features without breaking V1 clients"
                },
                StatusCode = HttpStatusCode.OK
            });
        }

    }
}
