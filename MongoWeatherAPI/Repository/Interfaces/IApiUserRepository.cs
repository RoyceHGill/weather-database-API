using MongoWeatherAPI.Models;
using MongoWeatherAPI.Models.DTO;
using MongoWeatherAPI.Models.Operations;
using System.Data;

namespace MongoWeatherAPI.Repository.Interfaces
{
    public interface IApiUserRepository
    {
        public UsernameRoleDTO GetApiKeyUser(string apiKey);

        public ApiUser GetApiUserByID(string id);

        public ApiUser GetUserDetailsByLoginDTO(LoginDTO credentials);

        public OperationResult<ApiUser> DeleteApiUser(string username);

        public bool CreateApiUser(ApiUserCreateDTO user);

        public void CreateManyApiUser(List<ApiUserCreateDTO> users);

        public OperationResult<ApiUser> UpdateApiUser(string id, ApiUser user);

        public OperationResult<ApiUser> DeleteInactiveUsers(int days);

        public OperationResult<ApiUser> UpdateManyApiUsersAccess(ApiUserPatchRequestObject apiUserPatchRequest);

        public OperationResult<ApiUser> UpdateManyApiUsersUserName(ApiUserPatchRequestObject apiUserPatchRequest);

        public OperationResult<ApiUser> UpdateManyApiUsersPassword(ApiUserPatchRequestObject apiUserPatchRequest);

        ApiUser AuthenticateUser(string apiKey, Roles requiredAccess);

        void UpdateUserLastLogin(string apikey);


    }
}
