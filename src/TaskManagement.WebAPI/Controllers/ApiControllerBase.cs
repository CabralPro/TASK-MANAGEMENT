using TaskManagement.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.WebAPI.Controllers;

/// <summary>
/// Shared API controller helpers and default response metadata.
/// </summary>
[ApiController]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Returns an HTTP 200 response wrapped in <see cref="ApiResponse{T}"/>.
    /// </summary>
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string message = null)
        => Ok(ApiResponse<T>.Ok(data, message));

    /// <summary>
    /// Returns an HTTP 201 response wrapped in <see cref="ApiResponse{T}"/> with a Location header.
    /// </summary>
    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(string location, T data, string message = null)
        => Created(location, ApiResponse<T>.Ok(data, message));
}
