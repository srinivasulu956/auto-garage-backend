using Auto_Garage.Data.AutoGarageAuthDb;
using Auto_Garage.Models.DomainModels;
using Auto_Garage.Models.DtoModels;
using Auto_Garage.Repositories.TokenRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace Auto_Garage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        UserManager<AutoGarageUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenRepositiry tokenRepositiry,
        AutoGarageAuthDbContext dbContext) : ControllerBase
    {
        private readonly UserManager<AutoGarageUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly ITokenRepositiry _tokenRepository = tokenRepositiry;
        private readonly AutoGarageAuthDbContext _dbContext = dbContext;

        // Roles that are NOT allowed via public self-registration
        private static readonly HashSet<string> StaffRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Admin", "Mechanic"
        };


        // ── POST /api/auth/register ────────────────────────────────────────────
        // Public endpoint — Customer only.
        // Rejects any attempt to register as Admin or Mechanic.

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto is null)
                return BadRequest(new { message = "Request body is required." });

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email is required." });

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Password is required." });

            // ✅ Block Admin and Mechanic from self-registering
            if (dto.Roles != null && dto.Roles.Any(r => StaffRoles.Contains(r)))
            {
                return BadRequest(new
                {
                    message = "Only customers can self-register. Contact your administrator for other roles."
                });
            }

            // If no roles provided, default to Customer
            var rolesToAssign = (dto.Roles != null && dto.Roles.Length > 0)
                ? dto.Roles
                : new[] { "Customer" };

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "User with this email already exists." });

            var newUser = new AutoGarageUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
            };

            var result = await _userManager.CreateAsync(newUser, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            await _userManager.AddToRolesAsync(newUser, rolesToAssign);

            return Ok(new
            {
                message = "User registered successfully.",
                userName = newUser.UserName
            });
        }


        // ── POST /api/auth/register-staff ─────────────────────────────────────
        // Admin-only endpoint — creates Admin or Mechanic accounts.
        // Regular customers cannot access this.

        [HttpPost("register-staff")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto is null)
                return BadRequest(new { message = "Request body is required." });

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email is required." });

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Password is required." });

            if (dto.Roles == null || dto.Roles.Length == 0)
                return BadRequest(new { message = "At least one role must be specified." });

            // Validate all provided roles exist
            var invalidRoles = new List<string>();
            foreach (var role in dto.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    invalidRoles.Add(role);
            }

            if (invalidRoles.Any())
            {
                return BadRequest(new
                {
                    message = "Invalid roles provided.",
                    invalidRoles
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "User with this email already exists." });

            var newUser = new AutoGarageUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
            };

            var result = await _userManager.CreateAsync(newUser, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            await _userManager.AddToRolesAsync(newUser, dto.Roles);

            return Ok(new
            {
                message = $"Staff account created successfully with role(s): {string.Join(", ", dto.Roles)}.",
                userName = newUser.UserName
            });
        }


        // ── POST /api/auth/login ──────────────────────────────────────────────

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (loginRequestDto is null)
                return BadRequest(new { message = "Request body is required." });

            var email = loginRequestDto.Email?.Trim();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(loginRequestDto.Password))
                return Unauthorized("Invalid email or password");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            if (user.IsDeleted || !user.IsActive)
                return Unauthorized("User is inactive. Please contact support.");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
            if (!isPasswordValid)
                return Unauthorized("Invalid email or password");

            var roles = await _userManager.GetRolesAsync(user);

            if (string.IsNullOrEmpty(loginRequestDto.Role) || !roles.Contains(loginRequestDto.Role))
                return Unauthorized("User does not have the required role");

            var token = _tokenRepository.CreateJWTToken(user, [.. roles]);

            var refreshToken = new RefreshTokenModel
            {
                Token = _tokenRepository.GenerateRefreshToken(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", refreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                AccessToken = token,
                User = new
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles,
                    Theme = user.ThemePreference
                }
            });
        }


        // ── GET /api/auth/currentUserData ─────────────────────────────────────

        [HttpGet("currentUserData")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserData()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles,
                Theme = user.ThemePreference
            });
        }


        // ── POST /api/auth/logout ─────────────────────────────────────────────

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var storedToken = await _dbContext.RefreshTokens
                    .FirstOrDefaultAsync(t => t.Token == refreshToken);

                if (storedToken != null)
                {
                    storedToken.IsRevoked = true;
                    await _dbContext.SaveChangesAsync();
                }
            }

            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
            });

            var authHeader = Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Replace("Bearer ", "").Trim();
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                await _dbContext.BlacklistedTokens.AddAsync(new BlacklistedTokenModel
                {
                    Token = token,
                    BlacklistedAt = DateTime.UtcNow,
                    ExpiresAt = jwtToken.ValidTo
                });

                await _dbContext.SaveChangesAsync();
            }

            return Ok(new { message = "Logged out successfully." });
        }


        // ── POST /api/auth/refresh ────────────────────────────────────────────

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("No refresh token found.");

            var storedToken = await _dbContext.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (storedToken == null) return Unauthorized("Invalid refresh token.");
            if (storedToken.IsRevoked) return Unauthorized("Refresh token has been revoked.");
            if (storedToken.ExpiresAt < DateTime.UtcNow) return Unauthorized("Refresh token expired. Please login again.");

            storedToken.IsRevoked = true;

            var user = storedToken.User;
            if (user == null) return Unauthorized("Invalid refresh token user.");

            var roles = await _userManager.GetRolesAsync(user);
            var newJwt = _tokenRepository.CreateJWTToken(user, [.. roles]);

            var newRefreshToken = new RefreshTokenModel
            {
                Token = _tokenRepository.GenerateRefreshToken(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            await _dbContext.RefreshTokens.AddAsync(newRefreshToken);
            await _dbContext.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", newRefreshToken.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7)

            });

            return Ok(new { AccessToken = newJwt });
        }


        // ── PUT /api/auth/update-profile ──────────────────────────────────────

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateRequestDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto is null) return BadRequest(new { message = "Request body is required." });

            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.IsDeleted) return Unauthorized("User not found or deleted");
            if (!user.IsActive) return Unauthorized("User is inactive");

            if (string.IsNullOrWhiteSpace(dto.FirstName) && string.IsNullOrWhiteSpace(dto.LastName))
                return BadRequest(new { message = "At least one field must be provided for update." });

            if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName.Trim();
            if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.ThemeName))
            {
                var theme = dto.ThemeName.Trim().ToLower();
                if (theme != "light" && theme != "dark")
                    return BadRequest(new { message = "Theme must be either 'light' or 'dark'." });
                user.ThemePreference = theme;
            }

            user.ModifiedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors.Select(e => e.Description));

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                message = "Profile updated successfully",
                User = new
                {
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    Roles = roles,
                    Theme = user.ThemePreference
                }
            });
        }
    }
}