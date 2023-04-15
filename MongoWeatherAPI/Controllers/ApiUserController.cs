using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using MongoWeatherAPI.Attributes;
using MongoWeatherAPI.Models;
using MongoWeatherAPI.Models.DTO;
using MongoWeatherAPI.Models.Operations;
using MongoWeatherAPI.Repository.Interfaces;

namespace MongoWeatherAPI.Controllers
{
    /// <summary>
    /// The API User Route for the API.
    /// </summary>
    [EnableCors("GooglePolicy")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiUserController : ControllerBase
    {
        private readonly IApiUserRepository _userRepository;


        /// <summary>
        /// Constructor. Inject user methods Repository
        /// </summary>
        /// <param name="userRepository"></param>
        public ApiUserController(IApiUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
#if DEBUG
        // POST api/<ApiUserController>/CreateMany
        /// <summary>
        /// Create many Users Only Available During Debug.
        /// </summary>
        /// <param name="newApiUsers">List of Usernames, passwords, roles</param>
        /// <returns>Status code 201 for success, 500 for bad</returns>
        /// <remarks>
        /// Body Parameters:
        /// 
        /// List of:
        /// 
        /// UserName: String,
        /// 
        /// PasswordHash: String,
        /// 
        /// UserRole: String
        /// 
        /// Authorization:
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: >= Teacher
        /// </remarks>
        [HttpPost("CreateMany")]
        [ApiKey(role: "Teacher")]
        public ActionResult PostMany([FromBody] List<ApiUserCreateDTO> newApiUsers, [FromHeader] string apiKey)
        {
            try
            {
                _userRepository.CreateManyApiUser(newApiUsers);
                _userRepository.UpdateUserLastLogin(apiKey);
                return CreatedAtAction("Post", null);
            }
            catch (Exception e)
            {
                return Problem(detail: e.Message, statusCode: 500);
            }
        }
#endif
        // POST api/<ApiUserController>/CreateNewUser
        /// <summary>
        /// Creates a new User of the weather data API. 
        /// </summary>
        /// <param name="newUser">Username, password, role</param>
        /// <returns>Status code 201 for success, 500 for bad</returns>
        /// <remarks>
        /// Body Parameters:
        /// 
        /// UserName: String,
        /// 
        /// PasswordHash: String,
        /// 
        /// UserRole: String
        /// 
        /// Authorization:
        /// 
        /// APIKey: String (Header)
        /// 
        /// </remarks>
        [HttpPost]
        [ApiKey(role: "Teacher")]
        public IActionResult Post([FromBody] ApiUserCreateDTO newUser, [FromHeader] string apiKey)
        {
            try
            {
                var userCreated = _userRepository.CreateApiUser(newUser);

                if (userCreated)
                {
                    _userRepository.UpdateUserLastLogin(apiKey);
                    return CreatedAtAction("Post", null);
                }
                else
                {
                    _userRepository.UpdateUserLastLogin(apiKey);
                    return BadRequest("Username is taken.");
                }
            }
            catch (Exception e)
            {
                return Problem(detail: e.Message, statusCode: 500);
            }
        }

        // PUT api/<ApiUserController>/640d692fd3341cc30e082455
        /// <summary>
        /// Replace some or all details for an API user.
        /// </summary>
        /// <param name="id">ID as string.</param>
        /// <param name="newApiUser">New Username, password and role.</param>
        /// <returns>Status code 200 for success, 500 for bad</returns>
        /// <remarks>
        /// Path Parameters:
        /// 
        /// ID: String
        /// 
        /// Body Parameters:
        /// 
        /// UserName: String,
        /// 
        /// PasswordHash: String,
        /// 
        /// UserRole: String
        /// 
        /// Authorization:
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: >= Teacher
        /// </remarks>
        [HttpPut("{id}")]
        [ApiKey(role: "Teacher")]
        public ActionResult Put(string id, [FromHeader] string apiKey, [FromBody] ApiUser newApiUser)
        {
            try
            {
                _userRepository.UpdateApiUser(id, newApiUser);
                _userRepository.UpdateUserLastLogin(apiKey);

                return Ok("Successfully updated the user.");
            }
            catch (Exception e)
            {
                return Problem(detail: e.Message, statusCode: 500);
            }

        }

        // PATCH api/<ApiUserController>/PatchManyByCreated
        /// <summary>
        /// Update users that were created within a time frame with a new role. 
        /// </summary>
        /// <param name="apiUserPatchRequest">Filter Property and value</param>
        /// <returns>Status code 200 for success, 500 for bad</returns>
        /// <remarks>
        /// Update the user with single or multiple new values, 
        /// 
        /// Body Parameters:
        /// 
        /// Property Name: String
        /// 
        /// Value: String
        /// 
        /// Enter non string values as strings. 
        /// 
        /// Example: PropertyName: "tempuratureC"  Value: "27.84"
        /// 
        /// Authorization:
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: >= Teacher
        /// </remarks>
        [HttpPatch("PatchManyByCreated")]
        [ApiKey(role: "Teacher")]
        public ActionResult UpdateManyUserDetails([FromBody] ApiUserPatchRequestObject apiUserPatchRequest, [FromHeader] string apiKey)
        {
            OperationResult<ApiUser> result;
            try
            {
                switch (apiUserPatchRequest.PropertyName)
                {
                    case "userName":
                        result = _userRepository.UpdateManyApiUsersUserName(apiUserPatchRequest);
                        break;
                    case "passwordHash":
                        result = _userRepository.UpdateManyApiUsersPassword(apiUserPatchRequest);
                        break;
                    case "userRole":
                        result = _userRepository.UpdateManyApiUsersAccess(apiUserPatchRequest);
                        break;
                    default:
                        return BadRequest("No Properties Matched");
                }

            }
            catch (Exception e)
            {

                return Problem(detail: e.Message, statusCode: 500);
            }

            _userRepository.UpdateUserLastLogin(apiKey);

            return Ok($"Successfully patched {apiUserPatchRequest.PropertyName} for {result.RecordsAffected}");
        }

        // DELETE api/<ApiUserController>/DeleteInactiveUsers
        /// <summary>
        /// Delete Inactive Users.
        /// </summary>
        /// <param name="days"> How many days are considered inactive </param>
        /// <returns>Status code 200 for success, 500 for bad</returns>
        /// <remarks>
        /// Default value is 30, Can enter any acceptable int32 number in its place. 
        /// 
        /// Path Parameters:
        /// 
        /// Days: int32
        /// 
        /// Authorization:
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: > Teacher
        /// </remarks>
        [HttpDelete("DeleteInactive")]
        [ApiKey(role: "Teacher")]
        public ActionResult DeleteInactiveUsers([FromHeader] string apiKey, int days = 30)
        {
            try
            {
                var result = _userRepository.DeleteInactiveUsers(days);
                _userRepository.UpdateUserLastLogin(apiKey);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception e)
            {

                return Problem(detail: e.Message, statusCode: 500);
            }
        }

        // DELETE api/<ApiUserController>/640d692fd3341cc30e082455
        /// <summary>
        /// Delete one User from MongoDB Id. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Status code 200 for success, 500 for bad</returns>
        /// <remarks>
        /// API user has been designated for removal, execute.
        /// 
        /// Path Parameters:
        /// 
        /// ID: string
        /// 
        /// Authorization:
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: > Teacher
        /// </remarks>
        [HttpDelete("Delete/{id}")]
        [ApiKey(role: "Teacher")]
        public ActionResult Delete(string id, [FromHeader] string apiKey)
        {
            try
            {
                var result = _userRepository.DeleteApiUser(id);
                _userRepository.UpdateUserLastLogin(apiKey);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception e)
            {
                return Problem(detail: e.Message, statusCode: 500);
            }
        }

        // Get api/<ApiUserController>
        /// <summary>
        /// Get the User's details from the database, for the current API key. 
        /// </summary>
        /// <param name="apiKey">API Key</param>
        /// <returns>User details</returns>
        /// <remarks>
        /// Retrieve the Username and Role for the User of the provided API Key.
        /// 
        /// Authorization:
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: > Student
        /// </remarks>
        [HttpGet]
        [ApiKey(role: "Student")]
        public UsernameRoleDTO Get([FromHeader] string apiKey)
        {
            HttpContext.Response.Headers.Add("x-customer-data-header", DateTime.Now.ToString());
            var apiUser = _userRepository.GetApiKeyUser(apiKey);
            _userRepository.UpdateUserLastLogin(apiKey);
            return apiUser;
        }

        // Get api/<ApiUserController>/GetById
        /// <summary>
        /// Get the User's details from the database, requires an API key and the role associated with that key is Teacher and up. 
        /// </summary>
        /// <param name="apiKey">API Key</param>
        /// <returns>User details</returns>
        /// <remarks>
        /// Retrieve the UserDetails for the provided user ID.
        /// 
        /// Path Parameters:
        /// 
        /// ID: string (Query)
        /// 
        /// Authorization:
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: > Teacher
        /// </remarks>
        [HttpGet("GetById")]
        [ApiKey(role: "Teacher")]
        public ApiUser GetbyId([FromQuery] string id, [FromHeader] string apiKey)
        {
            var apiUser = _userRepository.GetApiUserByID(id);
            _userRepository.UpdateUserLastLogin(apiKey);

            return apiUser;
        }

        // Get api/<ApiUserController>/GetByUsername
        /// <summary>
        /// Get the User's details from the database, requires an API key and the role associated with that key is Teacher and up. 
        /// </summary>
        /// <param name="apiKey">API Key</param>
        /// <returns>User details</returns>
        /// <remarks>
        /// Retrieve the UserDetails for the provided User Credentials.
        /// 
        /// Body Parameters:
        /// 
        /// Username: string
        /// 
        /// Password: string
        /// 
        /// </remarks>
        [HttpGet("Login")]
        public ApiUser GetbyUsername([FromBody] LoginDTO login)
        {
            var apiUser = _userRepository.GetUserDetailsByLoginDTO(login);
            if (apiUser != null)
            {
                _userRepository.UpdateUserLastLogin(apiUser.ApiKey);
            }
            return apiUser;
        }
    }
}
