using Articler.AppDomain.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.Token
{
    internal static class CalculateTokenResultFactory<TResult>
    {
        public static ICalculateTokenResult<TResult> CreateTokenResult(CalculateTokenStatus status, TResult? result = default)
        {
            return new CalculateTokenResult<TResult>
            {
                Status = status,
                Result = result,
            };
        }
    }
}
