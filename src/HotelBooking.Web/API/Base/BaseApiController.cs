using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelBooking.Web.API.Base
{
    [ApiController]
    [Route("api/base")]
    public abstract class BaseApiController : ControllerBase
    {
        protected Guid? CurrentUserId
        {
            get
            {
                if (User?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }

                return null;
            }
        }
        protected bool UserHasRole(string roleName)
        {
            return User?.IsInRole(roleName) == true;
        }
        protected bool UserHasPermission(string permission) 
        {
            return User?.HasClaim(c => c.Type == "Permission" && c.Value == permission) == true;
        }
        protected ActionResult Created<T>(string actionName, object routeValues, T data)
        {
            return CreatedAtAction(actionName, routeValues, data);
        }
        protected ActionResult Error(string message, int statusCode = 500)
        {
            return StatusCode(statusCode, new { error = message });
        }
    }
}
