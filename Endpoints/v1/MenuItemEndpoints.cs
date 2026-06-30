using Asp.Versioning;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MinimalAPI.Data;
using MinimalAPI.Models;
using MinimalAPI.Models.DTOs;
using System.Net;

namespace MinimalAPI.Endpoints.v1
{
    public static class MenuItemEndpoints
    {
        public static RouteGroupBuilder MapMenuItemEndpoints(this IEndpointRouteBuilder app)
        {
            var menuitemGroup = app.MapGroup("/api/v{version:apiVersion}/menuitems").WithTags("MenuItems");//.RequireAuthorization();


            menuitemGroup.MapGet("/", GetAllMenuItems)
                .WithName("GetAllMenuItems")
                .MapToApiVersion(new ApiVersion(1, 0))
                .Produces<APIResponse>(StatusCodes.Status200OK);

            menuitemGroup.MapGet("/{id:int}", GetMenuItemById)
                .WithName("GetMenuItemById")
                .Produces<APIResponse>(StatusCodes.Status200OK)
                .MapToApiVersion(new ApiVersion(1, 0))
                 .Produces<APIResponse>(StatusCodes.Status404NotFound);

            menuitemGroup.MapPost("/", CreateMenuItem)
                .WithName("CreateMenuItem")
                // .RequireAuthorization(u => u.RequireRole(SD.Role_Admin))
                .Accepts<MenuItemCreateDTO>("application/json")
                .Produces<APIResponse>(StatusCodes.Status201Created)
                .Produces<APIResponse>(StatusCodes.Status400BadRequest)
                .MapToApiVersion(new ApiVersion(1, 0))
                .Produces<APIResponse>(StatusCodes.Status404NotFound);

            menuitemGroup.MapPost("/{id:int}/image", UploadMenuItemImage)
               .WithName("UploadMenuItemImage")
               .DisableAntiforgery()
               // .RequireAuthorization(u => u.RequireRole(SD.Role_Admin))
               .Accepts<MenuItemCreateDTO>("multipart/form-data")
               .Produces<APIResponse>(StatusCodes.Status200OK)
               .Produces<APIResponse>(StatusCodes.Status400BadRequest)
               .MapToApiVersion(new ApiVersion(1, 0))
               .Produces<APIResponse>(StatusCodes.Status404NotFound);

            menuitemGroup.MapPut("/{id:int}", UpdateMenuItem)
                // .RequireAuthorization(u => u.RequireRole(SD.Role_Admin))
                .WithName("UpdateMenuItem")
                .Accepts<MenuItemCreateDTO>("application/json")
                .Produces<APIResponse>(StatusCodes.Status200OK)
                .Produces<APIResponse>(StatusCodes.Status400BadRequest)
                .MapToApiVersion(new ApiVersion(1, 0))
                .Produces<APIResponse>(StatusCodes.Status404NotFound);

            menuitemGroup.MapDelete("/{id:int}", DeleteMenuItem)
                // .RequireAuthorization(u => u.RequireRole(SD.Role_Admin))
                .WithName("DeleteMenuItem")
                .Accepts<MenuItemCreateDTO>("application/json")
                .Produces<APIResponse>(StatusCodes.Status200OK)
                .Produces<APIResponse>(StatusCodes.Status400BadRequest)
                .MapToApiVersion(new ApiVersion(1, 0))
                .Produces<APIResponse>(StatusCodes.Status404NotFound);

            return menuitemGroup;
        }


        private static async Task<IResult> GetAllMenuItems(ApplicationDBContext db, IMapper mapper)
        {
            var menuitems = await db.MenuItems.Include(u => u.Category).ToListAsync();
            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                Result = mapper.Map<List<MenuItemDTO>>(menuitems),
                StatusCode = HttpStatusCode.OK
            });
        }

        private static async Task<IResult> GetMenuItemById(int id, ApplicationDBContext db, IMapper mapper)
        {
            var menuitem = await db.MenuItems.FirstOrDefaultAsync(c => c.Id == id);
            if (menuitem is null)
            {
                return Results.NotFound(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["MenuItem not found"]
                });
            }
            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                Result = mapper.Map<MenuItemDTO>(menuitem),
                StatusCode = HttpStatusCode.OK
            });
        }

        private static async Task<IResult> CreateMenuItem(MenuItemCreateDTO menuitemCreateDTO, ApplicationDBContext db, IMapper mapper)
        {
            var menuitem = mapper.Map<MenuItem>(menuitemCreateDTO);
            menuitem.CreatedDate = DateTime.Now;
            db.MenuItems.Add(menuitem);
            await db.SaveChangesAsync();

            var menuitemDTO = mapper.Map<MenuItemDTO>(menuitem);

            return Results.CreatedAtRoute("GetMenuItemById", new { id = menuitem.Id }, new APIResponse
            {
                IsSuccess = true,
                Result = menuitemDTO,
                StatusCode = HttpStatusCode.Created
            });

        }

        private static async Task<IResult> UpdateMenuItem(int id, MenuItemUpdateDTO menuitemUpdateDTO, ApplicationDBContext db, IMapper mapper)
        {
            var menuitem = await db.MenuItems.FirstOrDefaultAsync(u => u.Id == id);
            if (menuitem is null)
            {
                return Results.NotFound(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["MenuItem not found"]
                });
            }
            mapper.Map(menuitemUpdateDTO, menuitem);
            await db.SaveChangesAsync();
            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            });

        }

        private static async Task<IResult> DeleteMenuItem(int id, ApplicationDBContext db)
        {
            var menuitem = await db.MenuItems.FirstOrDefaultAsync(u => u.Id == id);
            if (menuitem is null)
            {
                return Results.NotFound(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["MenuItem not found"]
                });
            }
            db.MenuItems.Remove(menuitem);
            await db.SaveChangesAsync();

            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            });
        }


        private static async Task<IResult> UploadMenuItemImage(int id, IFormFile file,
            ApplicationDBContext db, IMapper mapper, IWebHostEnvironment environment)
        {
            var menuitem = await db.MenuItems.FirstOrDefaultAsync(u => u.Id == id);

            if (menuitem is null)
            {
                return Results.NotFound(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["MenuItem not found"]
                });
            }

            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = ["No File Uploaded"]
                });
            }

            // Save new image (old one will be automatically deleted by SaveImageFile)
            var imageResult = await SaveImageFile(file, environment, menuitem.Id);
            if (!imageResult.Success)
            {
                return Results.BadRequest(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = [imageResult.Error!]
                });
            }

            menuitem.Image = imageResult.ImagePath;
            await db.SaveChangesAsync();


            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                Result = new { ImageUrl = menuitem.Image },
                StatusCode = HttpStatusCode.Created
            });

        }

        private static async Task<(bool Success, string? ImagePath, string? Error)> SaveImageFile(IFormFile file,
                IWebHostEnvironment environment,
                int menuItemId)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return (false, null, "Invalid file type. Only images are allowed (jpg, jpeg, png, gif)");
            }
            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return (false, null, "File size must not exceed 5MB");
            }

            var imageFolder = Path.Combine(environment.WebRootPath, "images", "menuitems");
            if (!Directory.Exists(imageFolder))
            {
                Directory.CreateDirectory(imageFolder);
            }

            var fileName = $"{menuItemId}{extension}";
            var filePath = Path.Combine(imageFolder, fileName);

            var existingFiles = Directory.GetFiles(imageFolder, $"{menuItemId}.*");
            foreach (var existingFile in existingFiles)
            {
                File.Delete(existingFile);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/images/menuitems/{fileName}";
            return (true, relativePath, null);
        }
    }
}
