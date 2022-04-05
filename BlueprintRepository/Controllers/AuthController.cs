using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

using BlueprintRepository.Models;
using BlueprintRepository.Repositories;
using BlueprintRepository.Statics;
using System.Collections.Generic;

namespace BlueprintRepository.Controllers {
    [ApiController]
    [Route("[controller]")]
    public sealed class AuthController : Controller {
        private readonly AUserRepository userRepository;
        private readonly AUserTokenRepository userTokenRepository;

        private readonly SigningCredentials signingCredentials;
        private readonly string jwtIssuer;
        private readonly long jwtLifespanEpoch;
        private readonly int jwtLimit;

        public AuthController(AUserRepository userRepository, AUserTokenRepository userTokenRepository, IConfiguration configuration) {
            this.userRepository = userRepository;
            this.userTokenRepository = userTokenRepository;

            byte[] jwtSecret = Encoding.ASCII.GetBytes(configuration.GetConfigurationWFallback(EnvironmentVariables.JWT_SECRET));

            signingCredentials = new SigningCredentials(new SymmetricSecurityKey(jwtSecret), SecurityAlgorithms.HmacSha256);
            jwtIssuer = configuration.GetValue<string>(EnvironmentVariables.JWT_ISSUER);
            jwtLifespanEpoch = Convert.ToInt64(configuration.GetConfigurationWFallback(EnvironmentVariables.JWT_LIFESPAN));
            jwtLimit = Convert.ToInt32(configuration.GetConfigurationWFallback(EnvironmentVariables.JWT_MAX_COUNT));
        }

        public class LoginRequestBody {
            [Required]
            public string Credential { get; set; }

            [Required]
            public string Password { get; set; }
        }

        [HttpGet]
        public async Task<ActionResult> Get(LoginRequestBody loginRequestBody) {
            bool usingEmail = new EmailAddressAttribute().IsValid(loginRequestBody.Credential);
            UserModel userModel = null;

            if (usingEmail) {
                userModel = await userRepository.GetAsync(x => x.Email == loginRequestBody.Credential);
            }

            else {
                userModel = await userRepository.GetAsync(x => x.Username == loginRequestBody.Credential);
            }

            if (userModel == null || !userModel.ValidatePassword(loginRequestBody.Password)) {
                return BadRequest(new GenericResponseModel() {
                    ResponseCode = ResponseCodes.LOGIN_FAILED,
                    Message = "Invalid credentials",
                    Errors = null,
                    Data = null
                });
            }

            {
                IEnumerator<UserTokenModel> tokenEnumerator = userModel.UserTokenModels.GetEnumerator();
                while (tokenEnumerator.MoveNext()) {
                    UserTokenModel userTokenModel = tokenEnumerator.Current;

                    if (userTokenModel.Invalidate()) {
                        userTokenRepository.Delete(userTokenModel);
                        userModel.UserTokenModels.Remove(userTokenModel);
                    }
                }
            }
            
            if (userModel.UserTokenModels.Count >= jwtLimit) {
                userModel.UserTokenModels.OrderBy(token => token.CreationDate).TakeLast((userModel.UserTokenModels.Count - jwtLimit) + 1).ToList().ForEach(token => {
                    userTokenRepository.Delete(token);
                    userModel.UserTokenModels.Remove(token);
                });
            }

            UserTokenModel newToken = new UserTokenModel() {
                UserId = userModel.Id,
                TokenId = Guid.NewGuid(),
                TokenExpirationDate = DateTime.UtcNow.AddMilliseconds(jwtLifespanEpoch)
            };

            userModel.UserTokenModels.Add(newToken);
            userTokenRepository.Create(newToken);

            await userRepository.BlueprintsContext.SaveChangesAsync();

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor() {
                Expires = newToken.TokenExpirationDate,
                SigningCredentials = signingCredentials,
                Issuer = jwtIssuer,

                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim("username", Convert.ToString(userModel.Username)),
                    new Claim("email", Convert.ToString(userModel.Email)),
                    new Claim("accountCreationDate", Convert.ToString(userModel.CreationDate.ToUnixTime())),
                    new Claim("roles", JsonConvert.SerializeObject(userModel.UserRoleModels))
                })
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            return Ok(new GenericResponseModel() {
                ResponseCode = ResponseCodes.LOGIN_SUCCESS,
                Message = "You've been logged in successfully",
                Errors = null,
                Data = tokenHandler.WriteToken(tokenHandler.CreateToken(securityTokenDescriptor))
            });
        }

        public class RegisterRequestBody {
            [Required]
            [MaxLength(16, ErrorMessage = "Username is too long, maximum length of 16 characters.")]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            [MaxLength(320, ErrorMessage = "Email address is too long, maximum length of 320 characters.")]
            public string Email { get; set; }

            [Required]
            [MinLength(8, ErrorMessage = "Password is too short, minimum length of 12 characters.")]
            [MaxLength(256, ErrorMessage = "Password is too long, maximum length of 256 characters.")]
            public string Password { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> Post(RegisterRequestBody registerRequestBody) {
            if (new EmailAddressAttribute().IsValid(registerRequestBody.Username)) {
                return BadRequest(new GenericResponseModel() {
                    ResponseCode = ResponseCodes.REGISTER_USERNAME_EMAIL,
                    Message = "Username cannot be an email address!",
                    Errors = new {
                        Username = new[] {
                            "Username cannot be a valid email address"
                        }
                    },
                    Data = null
                });
            }

            if (await userRepository.GetAsync(x => x.Username == registerRequestBody.Username) != null) {
                return BadRequest(new GenericResponseModel() {
                    ResponseCode = ResponseCodes.REGISTER_EXISTING_USERNAME,
                    Message = "Username must be unique",
                    Errors = new {
                        Username = new[] {
                            "An account already exists with this username"
                        }
                    },
                    Data = null
                });
            }

            if (await userRepository.GetAsync(x => x.Email == registerRequestBody.Email) != null) {
                return BadRequest(new GenericResponseModel() {
                    ResponseCode = ResponseCodes.REGISTER_EXISTING_EMAIL,
                    Message = "Email address must be unique",
                    Errors = new {
                        Email = new[] {
                            "An account already exists with this email address"
                        }
                    },
                    Data = null
                });
            }

            UserModel userModel = new UserModel() {
                Username = registerRequestBody.Username,
                Email = registerRequestBody.Email,

                Password = registerRequestBody.Password
            };

            userRepository.RegisterAsync(userModel);
            return Ok(new GenericResponseModel() {
                ResponseCode = ResponseCodes.REGISTER_SUCCESS,
                Message = "Your account has been registered successfully!",
                Errors = null,
                Data = null
            });
        }
    }
}
