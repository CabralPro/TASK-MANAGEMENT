using System.Threading;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs.Requests;
using TaskManagement.Application.DTOs.Responses;

namespace TaskManagement.Application.UseCases.Auth;

public interface ISignInUseCase
{
    Task<SignInResponse> SignInAsync(SignInRequest request, CancellationToken cancellationToken = default);
}
