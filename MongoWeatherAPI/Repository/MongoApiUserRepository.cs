using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoWeatherAPI.Models;
using MongoWeatherAPI.Models.DTO;
using MongoWeatherAPI.Models.Filters;
using MongoWeatherAPI.Models.Operations;
using MongoWeatherAPI.Repository.Interfaces;
using MongoWeatherAPI.Services;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MongoWeatherAPI.Repository
{
    public class MongoApiUserRepository : IApiUserRepository
    {
        private readonly IMongoCollection<ApiUser> _apiUser;

        public MongoApiUserRepository(MongoConnection connection)
        {
            _apiUser = connection.GetDatabase().GetCollection<ApiUser>("Users");
        }

        /// <summary>
        /// Create a new user. 
        /// </summary>
        /// <param name="userCreateDTO">new user data transfer object.</param>
        /// <returns></returns>
        /// <remarks>Create a new Api User, cannot reuse that same Username</remarks>
        public bool CreateApiUser(ApiUserCreateDTO userCreateDTO)
        {
            ApiUser user = new ApiUser()
            {
                UserName = userCreateDTO.UserName,
                UserRole = userCreateDTO.UserRole,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreateDTO.Password)
            };

            var filter = Builders<ApiUser>.Filter.Eq(u => u.UserName, user.UserName);
            var existingUser = _apiUser.Find(filter).FirstOrDefault();

            if (existingUser != null)
            {
                return false;
            }

            user.ApiKey = Guid.NewGuid().ToString();
#if DEBUG
            user.LastLogin = DateTime.Now.AddDays(-4);
#else
            user.LastLogin = DateTime.Now;
#endif
            user.Created = DateTime.Now;
            _apiUser.InsertOne(user);
            return true;
        }
#if DEBUG
        /// <summary>
        /// Create many users from a batch.
        /// </summary>
        /// <param name="userDTOs">new users details.</param>
        public void CreateManyApiUser(List<ApiUserCreateDTO> userDTOs)
        {
            var newApiUsers = userDTOs.Select(u => new ApiUser
            {
                UserName = u.UserName,
                UserRole = u.UserRole,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(u.Password),
                LastLogin = DateTime.Now.AddDays(-4),
                Created = DateTime.Now
            }).ToList();

            _apiUser.InsertMany(newApiUsers);
        }
#endif
        /// <summary>
        /// Delete one user by Id.
        /// </summary>
        /// <param name="id">MongoDb Id as string.</param>
        /// <remarks>
        /// 
        /// Intended to only be used through application interface.
        /// 
        /// </remarks>
        public OperationResult<ApiUser> DeleteApiUser(string id)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var filter = Builders<ApiUser>.Filter.Eq(u => u._id, objectId);

            var result = _apiUser.DeleteOne(filter);

            if (result.DeletedCount > 0)
            {
                return new OperationResult<ApiUser>
                {
                    Message = "API User Successfully Deleted",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.DeletedCount)
                };
            }
            else
            {
                return new OperationResult<ApiUser>
                {
                    Message = "API User Not Deleted",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.DeletedCount)
                };
            }
        }

        /// <summary>
        /// Delete many users at once.
        /// </summary>
        /// <param name="filter">All users that meet this criteria are deleted.</param>
        public OperationResult<ApiUser> DeleteInactiveUsers(int days)
        {
            var filter = Builders<ApiUser>.Filter.Lte(u => u.LastLogin, DateTime.Now.AddDays(-days));
            //filter &= Builders<ApiUser>.Filter.Eq(u => u.UserRole, "User");
            filter &= Builders<ApiUser>.Filter.Eq(u => u.UserRole, Roles.Student.ToString());

            var result = _apiUser.DeleteMany(filter);

            if (result.DeletedCount > 0)
            {
                return new OperationResult<ApiUser>
                {
                    Message = "API Users Successfully Deleted",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.DeletedCount)
                };
            }
            else
            {
                return new OperationResult<ApiUser>
                {
                    Message = "No API Users Deleted",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.DeletedCount)
                };
            }
        }

        /// <summary> 
        /// Update one users details.
        /// </summary>
        /// <param name="id">MongoDb Id as string.</param>
        /// <param name="user">User's Updated details.</param>
        public OperationResult<ApiUser> UpdateApiUser(string id, ApiUser updatedUser)
        {
            ObjectId objectId = ObjectId.Parse(id);

            var filter = Builders<ApiUser>.Filter.Eq(u => u._id, objectId);
            updatedUser._id = objectId;

            var oldUser = _apiUser.Find(u => u._id == updatedUser._id).FirstOrDefault();
            if (oldUser == null)
            {
                return new OperationResult<ApiUser>
                {
                    Message = "No user found.",
                    Success = false,
                    RecordsAffected = 0
                };
            }


            updatedUser.Created = oldUser.Created;
            updatedUser.LastLogin = oldUser.LastLogin;
            updatedUser.ApiKey = oldUser.ApiKey;
            updatedUser.Expiry = oldUser.Expiry;

            var result = _apiUser.ReplaceOne(filter, updatedUser);

            if (result.ModifiedCount > 0)
            {
                return new OperationResult<ApiUser>
                {
                    Message = "API User Successfully Updated",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
            else
            {
                return new OperationResult<ApiUser>
                {
                    Message = "API User Not Updated",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
        }

        /// <summary>
        /// Updates all User's UserRoles for all users that meet the filter requirements.
        /// </summary>
        /// <param name="userFilter">All users that meet this criteria are have their access changed.</param>
        /// <param name="newRole">The new User Role value to be saved.</param>
        public OperationResult<ApiUser> UpdateManyApiUsersAccess(ApiUserPatchRequestObject apiUserPatchRequest)
        {
            var filter = ProcessFilter(apiUserPatchRequest.Filter);

            var updateDefinition = Builders<ApiUser>.Update.Set(u => u.UserRole, apiUserPatchRequest.PropertyValue);

            var result = _apiUser.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount > 0)
            {
                return new OperationResult<ApiUser>
                {
                    Message = "API Users Successfully Updated",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
            else
            {
                return new OperationResult<ApiUser>
                {
                    Message = "No API Users Updated",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
        }

        /// <summary>
        /// Updates all User's UserNames for all users that meet the filter requirements.
        /// </summary>
        /// <param name="apiUserPatchRequest"></param>
        public OperationResult<ApiUser> UpdateManyApiUsersUserName(ApiUserPatchRequestObject apiUserPatchRequest)
        {
            var filter = ProcessFilter(apiUserPatchRequest.Filter);

            var updateDefinition = Builders<ApiUser>.Update.Set(u => u.UserName, apiUserPatchRequest.PropertyValue);

            var result = _apiUser.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount > 0)
            {
                return new OperationResult<ApiUser>
                {
                    Message = "API Users Successfully Updated",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
            else
            {
                return new OperationResult<ApiUser>
                {
                    Message = "No API Users Updated",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
        }

        /// <summary>
        /// Updates all User's PasswordHash for all users that meet the filter requirements.
        /// </summary>
        /// <param name="apiUserPatchRequest"></param>
        public OperationResult<ApiUser> UpdateManyApiUsersPassword(ApiUserPatchRequestObject apiUserPatchRequest)
        {
            var filter = ProcessFilter(apiUserPatchRequest.Filter);

            var updateDefinition = Builders<ApiUser>.Update.Set(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword(apiUserPatchRequest.PropertyValue));

            var result = _apiUser.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount > 0)
            {
                return new OperationResult<ApiUser>
                {
                    Message = "API Users Successfully Updated",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
            else
            {
                return new OperationResult<ApiUser>
                {
                    Message = "No API Users Updated",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
        }

        /// <summary>
        /// Get the API user details from the database using the logged in users API key. 
        /// </summary>
        /// <param name="apiKey">The persons user name </param>
        /// <returns></returns>\
        /// <remarks>For referencing the current users details </remarks>
        public UsernameRoleDTO GetApiKeyUser(string apiKey)
        {
            var user = _apiUser.AsQueryable()
                .Where(u => u.ApiKey.Equals(apiKey))
                .Select(nu => new UsernameRoleDTO
                {
                    Username = nu.UserName,
                    Role = nu.UserRole
                }).FirstOrDefault();
            UpdateUserLastLogin(apiKey);
            return user;
        }

        /// <summary>
        /// Request a users details with an Id, Requires an API key with a role of teacher or higher
        /// </summary>
        /// <param name="id">A user id</param>
        /// <returns>The user details</returns>
        /// <remarks>Requires an API Key and the role associated with that key needs to be a Teacher or higher</remarks>
        public ApiUser GetApiUserByID(string id)
        {
            ObjectId objId = ObjectId.Parse(id);
            var filter = Builders<ApiUser>.Filter.Eq(u => u._id, objId);
            var user = _apiUser.Find(filter).FirstOrDefault();
            UpdateUserLastLogin(user.ApiKey);
            return user;
        }

        /// <summary>
        /// Request a users details with an Username, Requires an API key with a role of teacher or higher
        /// </summary>
        /// <param name="id">A user id</param>
        /// <returns>The user details</returns>
        /// <remarks>Requires an API Key and the role associated with that key needs to be a Teacher or higher.</remarks>
        public ApiUser GetUserDetailsByLoginDTO(LoginDTO login)
        {
            var filter = Builders<ApiUser>.Filter.Eq(u => u.UserName, login.Username);
            var user = _apiUser.Find(filter).FirstOrDefault();

            if (user != null)
            {
                if (BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
                {
                    UpdateUserLastLogin(user.ApiKey);
                    return user;
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// Processes the filter into an applicable Type.
        /// </summary>
        /// <param name="userFilter">Filter Object</param>
        /// <returns>A filter definition for the query.</returns>
        private FilterDefinition<ApiUser> ProcessFilter(ApiUserFilter userFilter)
        {
            var builder = Builders<ApiUser>.Filter;

            var filter = builder.Empty;

            if (userFilter?.CreatedFrom != null)
            {
                filter &= builder.Gte(t => t.Created, userFilter.CreatedFrom.Value);
            }

            if (userFilter?.CreatedTo != null)
            {
                filter &= builder.Lte(t => t.Created, userFilter.CreatedTo.Value);
            }

            return filter;
        }

        /// <summary>
        /// checks the API key exists in the database and checks the role against the required role for the endpoint. 
        /// </summary>
        /// <param name="apiKey">GUID, string, authentication token</param>
        /// <param name="requiredAccess">Enum, value gained from given API Key must be equal or higher.</param>
        /// <returns>The users details</returns>
        public ApiUser AuthenticateUser(string apiKey, Roles requiredAccess)
        {
            var filter = Builders<ApiUser>.Filter.Eq(u => u.ApiKey, apiKey);

            var user = _apiUser.Find(filter).FirstOrDefault();

            if (user == null || !AuthenticateRole(user.UserRole, requiredAccess))
            {
                return null;
            }
            return user;
        }

        /// <summary>
        /// Using an API key find the user and update that users last logged in date. 
        /// </summary>
        /// <param name="apikey">GUID, string, authentication token.</param>
        /// <param name="loginDate">current time.</param>
        public void UpdateUserLastLogin(string apikey)
        {
            DateTime loginDate = DateTime.Now;
#if DEBUG
            loginDate = loginDate.AddDays(-4);
#endif
            var filter = Builders<ApiUser>.Filter.Eq(u => u.ApiKey, apikey);

            var updateDeffinition = Builders<ApiUser>.Update.Set(u => u.LastLogin, loginDate);

            _apiUser.UpdateOne(filter, updateDeffinition);
        }

        /// <summary>
        /// Compares user role against required role.
        /// Check that user role is a user role that exists in the Enum. 
        /// </summary>
        /// <param name="userRole">String search for user role after authenticating that the user exists. </param>
        /// <param name="requiredRole">Provided from the Authorize attribute. </param>
        /// <returns>True if access allowed, false if denied. </returns>
        public bool AuthenticateRole(string userRole, Roles requiredRole)
        {
            if (!Enum.TryParse(userRole, out Roles userRoleNumber))
            {
                return false;
            }

            int userRoleIndicator = (int)userRoleNumber;

            int requiredRoleIndicator = (int)requiredRole;

            return userRoleIndicator <= requiredRoleIndicator;
        }
    }
}
