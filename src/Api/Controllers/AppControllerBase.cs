using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class AppControllerBase : ControllerBase
    {
        protected CancellationToken Ct => HttpContext.RequestAborted;
        protected string? UserId => User.GetUserId();

        protected string GetIpAddress()
            => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}