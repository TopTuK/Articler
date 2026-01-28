using Articler.AppDomain.Models.Token;

namespace Articler.WebApi.Models.TokenResult
{
    internal class TokenResult<TResult> : ICalculateTokenResult<TResult>
    {
        public CalculateTokenStatus Status { get; init; }
        public TResult? Result { get; init; }
    }

    internal static class TokenResultFactory
    {
        public static ICalculateTokenResult<TResult> CreateTokenResult<TResult>(CalculateTokenStatus status, 
            TResult? result = default)
        {
            return new TokenResult<TResult>
            {
                Status = status,
                Result = result
            };
        }
    }
}
