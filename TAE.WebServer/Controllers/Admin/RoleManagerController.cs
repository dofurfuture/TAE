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
    using TAE.WebServer.Common;
    public class RoleManagerController : BaseApiController
    {
        [HttpGet]
        public HttpResponseMessage GetAllRoles(int pageNumber = 1, int pageSize = RequestArg.defualtPageSize, string orderName = "")
        {
            string sqlGetAll = "select * from AspNetRoles";
            return GetDataList<AppRole>(pageNumber, pageSize, orderName, sqlGetAll);
        }
        [HttpGet]
        public async Task<HttpResponseMessage> GetRoleDetail(string id)
        {
            var appRole = await ServiceIdentity.FindRoleById(id);
            if (appRole != null)
            {
                return Response(appRole);
            }
            else
            {
                return Response(HttpStatusCode.NotFound);
            }
        }
        [HttpPost]
        public async Task<HttpResponseMessage> SubUserData(AppRole model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                var role = new AppRole { Name = model.Name};
                //传入Password并转换成PasswordHash
                bool result = await ServiceIdentity.CreateRole(role);
                if (result == true)
                {
                    return Response(role);
                }
                else
                {
                    return Response(HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                AppRole role = await ServiceIdentity.FindRoleById(model.Id);
                if (role != null)
                {
                    role.Name = model.Name;
                    bool result = await ServiceIdentity.UpdateRole(role);
                    if (result == true)
                    {
                        return Response(role);
                    }
                    else
                    {
                        return Response(HttpStatusCode.InternalServerError);
                    }
                }
                else
                {
                    return Response(HttpStatusCode.InternalServerError);
                }
            }
        }
        [HttpGet]
        public async Task<HttpResponseMessage> RoleAllocation(string id)
        {
            AppRole role = await ServiceIdentity.FindRoleById(id);
            string[] memberIDs = role.Users.Select(x => x.UserId).ToArray();
            IEnumerable<AppUser> members = ServiceIdentity.FindUser(x => memberIDs.Any(y => y == x.Id)).ToList();
            IEnumerable<AppUser> nonMembers = ServiceIdentity.FindUser().Except(members).ToList();
            return Response(new { UserIn = members, UserNotIn = nonMembers });
        }
    }
}
