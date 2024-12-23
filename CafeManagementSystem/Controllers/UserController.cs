﻿using CafeManagementSystem.Models;
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
        CafeEntities _db = new CafeEntities();

        [HttpPost, Route("signup")]
        public HttpResponseMessage Signup([FromBody] User user)
        {
            try
            {
                User userObj = _db.Users
                    .Where(u => u.email == user.email).FirstOrDefault();
                if (userObj == null)
                {
                    user.role = "user";
                    user.status = "false";
                    _db.Users.Add(user);
                    _db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK, new { message = "Successfully registered." });
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

        [HttpPost, Route("login")]
        public HttpResponseMessage Login([FromBody] User user)
        {
            try
            {
                User userObj = _db.Users
                    .Where(u => (u.email == user.email && u.password == user.password)).FirstOrDefault();
                if (userObj != null)
                {
                    if (userObj.status == "true")
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                            new { token = TokenManager.GenerateToken(userObj.email, userObj.role) });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized,
                            new { message = "Wait for Admin approval" });
                    }
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized,
                        new { message = "Incorrect Username or Password" });
                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        // check received token is ok
        [HttpGet, Route("checkToken")]
        [CustomAuthenticationFilter] // if token is not correct then 401 Unauthorized
        public HttpResponseMessage CheckToken()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { message = "true" });
        }

        // if admin, return list of users
        [HttpGet, Route("getAllUsers")]
        [CustomAuthenticationFilter] // if token is not correct then 401 Unauthorized
        public HttpResponseMessage GetAllUsers()
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin") // if not admin then 401 Unauthorized
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                
                // get users with role == "user" from database with specified fields 
                var result = _db.Users
                    .Select(u => new { u.id, u.name, u.contactNumber, u.email, u.status, u.role })
                    .Where(x => (x.role == "user"))
                    .ToList();
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }
    }
}