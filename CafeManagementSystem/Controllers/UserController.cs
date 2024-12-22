using CafeManagementSystem.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CafeManagementSystem.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        CafeEntities db = new CafeEntities();

        [HttpPost, Route("signup")]
        public HttpResponseMessage Signup([FromBody] User user)
        {
            try
            {
                User userObj = db.Users
                    .Where(u => u.email == user.email).FirstOrDefault();
                if (userObj == null)
                {
                    user.role = "user";
                    user.status = "false";
                    db.Users.Add(user);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, new { message = "Succesfully registred." });

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = "Email already exists." });
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}
