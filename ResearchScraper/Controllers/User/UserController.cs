using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using ResearchScrapper.Api.MiddleWare;
using ResearchScrapper.Api.Models;
using ResearchScrapper.Api.Service;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ResearchScrapper.Api.Controller
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthController> _logger;
        private readonly IPasswordHasher _passwordHasher;
        private readonly JwtService _jwtTokenService;
        private readonly IWebHostEnvironment _env;
        private readonly IFavoriteArticleRepository _favoriteRepo;
        private readonly IToReadArticleRepository _readRepo;
        private readonly IFavoriteScientificArticleRepository _favoriteScienceRepo;
        private readonly IToReadScientificArticleRepository _readScienceRepo;

        public AuthController(IUserRepository userRepository, IMapper mapper, ILogger<AuthController> logger, JwtService jwtService, IPasswordHasher passwordHasher,
        IWebHostEnvironment env, IFavoriteArticleRepository favoriteRepo, IToReadArticleRepository readRepo, IFavoriteScientificArticleRepository favoriteScienceRepo,
        IToReadScientificArticleRepository readScienceRepo)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _jwtTokenService = jwtService;
            _passwordHasher = passwordHasher;
            _env = env;
            _favoriteRepo = favoriteRepo;
            _readRepo = readRepo;
            _readScienceRepo = readScienceRepo;
            _favoriteScienceRepo = favoriteScienceRepo;
        }

        [HttpGet("google")]
        public IActionResult GoogleLogin()
        {
            try
            {
                var scheme = Request.Scheme;

                var properties = new AuthenticationProperties
                {
                    RedirectUri = $"{scheme}://{Request.Host}/api/auth/callback"
                };
                _logger.LogInformation("RedirectUri: " + properties.RedirectUri);

                return Challenge(properties, GoogleDefaults.AuthenticationScheme);
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpGet("discord")]
        public IActionResult DiscordLogin()
        {
            try
            {
                var scheme = Request.Scheme;
                var properties = new AuthenticationProperties
                {
                    RedirectUri = $"{scheme}://{Request.Host}/api/auth/callback"
                };
                _logger.LogInformation("RedirectUri: " + properties.RedirectUri);

                return Challenge(properties, "Discord");
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }

        }



        [HttpGet("callback")]
        public async Task<IActionResult> Callback()
        {
            try
            {
                var identity = User.Identity as ClaimsIdentity;
                if (identity == null || !identity.IsAuthenticated)
                    return Unauthorized();

                var email = identity.FindFirst(ClaimTypes.Email).Value;
                var name = identity.FindFirst(ClaimTypes.Name).Value;

                if (string.IsNullOrEmpty(email))
                    return BadRequest("Email not available from provider.");


                var picture = identity.FindFirst("picture")?.Value;

                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    var createUser = new CreateUser
                    {
                        Email = email,
                        Name = name,
                        ProfileImageUrl = picture
                    };

                    user = await _userRepository.CreateUserAsync(createUser);
                }

                var jwt = _jwtTokenService.CreateToken(user);
                var isProd = _env.IsProduction();

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isProd, // true in prod, false otherwise
                    SameSite = isProd ? SameSiteMode.None : SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(365)
                };

                Response.Cookies.Append("jwt", jwt, cookieOptions);
                
                return Redirect(_env.IsProduction()? "https://articulio.netlify.app": "http://localhost:4200");
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return StatusCode(500, new { error = "Unexpected error try again" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null)
                    return Unauthorized(new { error = "Invalid credentials." });

                if (user.PasswordHash == null)
                {
                    return Unauthorized(new { error = "Invalid credentials." });
                }

                var valid = _passwordHasher.Verify(request.Password, user.PasswordHash);
                if (!valid)
                    return Unauthorized(new { error = "Invalid credentials." });

                var token = _jwtTokenService.CreateToken(user);


                var isProd = _env.IsProduction();

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isProd,
                    SameSite = isProd ? SameSiteMode.None : SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(365)
                };

                Response.Cookies.Append("jwt", token, cookieOptions);


                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return StatusCode(500, new { error = "Unexpected error, try again" });
            }
        }

        [Authorize]
        [HttpPost("profile")]
        public async Task<IActionResult> ProfileInfo()
        {
            try
            {
                var userIdentity = (UserIdentity)Request.HttpContext.Items["User"]!;
                var user = await _userRepository.GetUserByEmailAsync(userIdentity.Email);
                if (user == null) return NotFound();

                var favoritesTask = _favoriteRepo.GetAllByUserIdAsync(user.Id);
                var toReadTask = _readRepo.GetAllByUserIdAsync(user.Id);
                var favoritesScientificTask = _favoriteScienceRepo.GetAllByUserIdAsync(user.Id);
                var toReadScientificTask = _readScienceRepo.GetAllByUserIdAsync(user.Id);

                await Task.WhenAll(favoritesTask, toReadTask, favoritesScientificTask, toReadScientificTask);

                var userInfo = new UserInfo
                {
                    UserMainInfo = _mapper.Map<UserDto>(user),
                    FavoriteArticles = favoritesTask.Result,
                    ToReadArticles = toReadTask.Result,
                    FavoriteScientificArticles = favoritesScientificTask.Result,
                    ToReadScientificArticles = toReadScientificTask.Result,
                };

                return Ok(userInfo);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in ProfileInfo");
                return StatusCode(500, new { error = "Unexpected error, try again" });
            }
        }


        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8 || request.Password.Length > 128)
                    return BadRequest(new { error = "Password must be 8–128 characters." });

                if (string.IsNullOrWhiteSpace(request.Email) || !Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    return BadRequest(new { error = "Invalid email format." });

                var existing = await _userRepository.GetUserByEmailAsync(request.Email);
                if (existing != null)
                    return Conflict(new { error = "User already exists" });

                var passwordHash = _passwordHasher.Hash(request.Password);

                var user = await _userRepository.CreateUserWithEmailPasswordAsync(new CreateUserWithPassword
                {
                    Email = request.Email,
                    Name = "default",
                    Password = request.Password
                });


                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                return StatusCode(500, new { error = "Unexpected error" });
            }
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var isProd = _env.IsProduction();

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isProd,
                SameSite = isProd ? SameSiteMode.None : SameSiteMode.Lax,
                Path = "/"
            };

            Response.Cookies.Delete("jwt", cookieOptions);

            return Ok(new { message = "Logged out" });
        }



    }
}

