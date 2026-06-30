using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MinimalAPI.Models;
using MinimalAPI.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace MinimalAPI.Endpoints
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            var authGroup = app.MapGroup("/api/auth").WithTags("Authentication");



            authGroup.MapPost("/login", Login)
                .WithName("Login")
                .Accepts<LoginRequestDTO>("application/json")
                .Produces<APIResponse>(StatusCodes.Status200OK)
                .Produces<APIResponse>(StatusCodes.Status400BadRequest)
                .Produces<APIResponse>(StatusCodes.Status404NotFound);

            authGroup.MapPost("/register", Register)
                            .WithName("Register")
                            .Accepts<RegistrationRequestDTO>("application/json")
                            .Produces<APIResponse>(StatusCodes.Status200OK)
                            .Produces<APIResponse>(StatusCodes.Status400BadRequest)
                            .Produces<APIResponse>(StatusCodes.Status404NotFound);


        }

        private static async Task<IResult> Login(LoginRequestDTO loginRequestDTO,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            try
            {
                var user = await userManager.FindByNameAsync(loginRequestDTO.Email);


                if (user == null)
                {
                    return Results.BadRequest(new APIResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = ["Username or password is incorrect"]
                    });
                }

                var result = await signInManager.CheckPasswordSignInAsync(user, loginRequestDTO.Password, false);
                if (!result.Succeeded)
                {
                    return Results.BadRequest(new APIResponse
                    {
                        IsSuccess = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        ErrorMessages = ["Username or password is incorrect"]
                    });
                }

                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? SD.Role_Customer;


                //create JWT Token
                var key = Encoding.ASCII.GetBytes(configuration["ApiSettings:Secret"]!);
                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name,user.Name),
                        new Claim(ClaimTypes.NameIdentifier,user.Id),
                        new Claim(ClaimTypes.Role,role),
                        new Claim("API","Minimal API Demo")
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                var loginResponse = new LoginResponseDTO
                {
                    Token = tokenString,
                    Email = loginRequestDTO.Email

                };

                return Results.Ok(new APIResponse
                {
                    IsSuccess = true,
                    Result = loginResponse,
                    StatusCode = HttpStatusCode.OK
                });
            }
            catch (Exception ex)
            {
                return Results.InternalServerError(new APIResponse
                {
                    IsSuccess = true,
                    Result = "Exception Encountered",
                    StatusCode = HttpStatusCode.InternalServerError
                });
            }
        }



        private static async Task<IResult> Register(RegistrationRequestDTO registrationRequestDTO,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var exisitingUser = await userManager.FindByNameAsync(registrationRequestDTO.Email);

            if (exisitingUser != null)
            {
                return Results.BadRequest(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = ["Username already exists"]
                });
            }

            if (!await roleManager.RoleExistsAsync(SD.Role_Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
            }
            var newUser = new ApplicationUser
            {
                UserName = registrationRequestDTO.Email,
                Name = registrationRequestDTO.Name,
                Email = registrationRequestDTO.Email,
                NormalizedEmail = registrationRequestDTO.Email.ToUpper()
            };

            var result = await userManager.CreateAsync(newUser, registrationRequestDTO.Password);
            if (!result.Succeeded)
            {
                return Results.BadRequest(new APIResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = result.Errors.Select(e => e.Description).ToList()
                });
            }



            if (registrationRequestDTO.Role.ToLower() == SD.Role_Admin.ToLower())
            {
                await userManager.AddToRoleAsync(newUser, SD.Role_Admin);
            }
            else
            {
                await userManager.AddToRoleAsync(newUser, SD.Role_Customer);
            }

            return Results.Ok(new APIResponse
            {
                IsSuccess = true,
                Result = "",
                StatusCode = HttpStatusCode.OK
            });
        }
    }
}
