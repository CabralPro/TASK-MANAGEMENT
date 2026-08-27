using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Application.Auth;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;

namespace TaskManagement.Application.UseCases.Auth;

public interface IRegisterUseCase
{
    Task<AuthenticatedUser> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
