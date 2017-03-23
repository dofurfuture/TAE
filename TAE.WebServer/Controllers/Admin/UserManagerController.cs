﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace TAE.WebServer.Controllers.Admin
{
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using TAE.Data.Model;
    using TAE.Utility.Common;
    using TAE.WebServer.Common;

    /// <summary>
    /// 用户管理
    /// </summary>
    public class UserManagerController : BaseApiController
    {
        [HttpGet]
        public HttpResponseMessage GetAllUsers(int pageNumber = 1, int pageSize = RequestArg.defualtPageSize, string orderName = "")
        {
            RequestArg arg = new RequestArg()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
            };
            string sqlGetAll = "select Id, Email,PhoneNumber,UserName from AspNetUsers";
            if (!string.IsNullOrEmpty(orderName))
            {
                SqlParameter para = new SqlParameter("@orderName", orderName);
                sqlGetAll = "select Id, Email,PhoneNumber,UserName from AspNetUsers order by @orderName";
            }
            var list = ServiceBase.FindAllByPage<UserViewModel>(sqlGetAll, arg);
            if (list != null)
            {
                return Response(list);
            }
            else
            {
                return Response(HttpStatusCode.NotFound, "未找到任何信息");
            }
        }
        [HttpGet]
        public async Task<HttpResponseMessage> GetUserDetail(string id)
        {
            var AppUser = await ServiceIdentity.FindUserById(id);
            var user = Map<AppUser, UserViewModel>(AppUser);
            if (user != null)
            {
                return Response(user);
            }
            else
            {
                return Response(HttpStatusCode.NotFound, "未找到任何信息");
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> SubUserData(UserViewModel model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                var user = new AppUser { UserName = model.UserName, Email = model.Email };
                //传入Password并转换成PasswordHash
                bool result = await ServiceIdentity.CreateUser(user, model.Password);
                if (result == true)
                {
                    return Response(user);
                }
                else
                {
                    return Response(HttpStatusCode.InternalServerError, "服务器错误");
                }
            }
            else
            {
                AppUser user = await ServiceIdentity.FindUserById(model.Id);
                if (user != null)
                {
                    bool result = await ServiceIdentity.UpdateUser(user);
                    if (result == true)
                    {
                        user.PasswordHash = ServiceIdentity.GetHashPassword(model.Password);
                        user.Email = model.Email;
                        return Response(user);
                    }
                    else
                    {
                        return Response(HttpStatusCode.InternalServerError, "服务器错误");
                    }
                }
                else
                {
                    return Response(HttpStatusCode.InternalServerError, "服务器错误");
                }
            }
        }
    }
}
