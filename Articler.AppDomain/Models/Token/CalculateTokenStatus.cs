using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Models.Token
{
    public enum CalculateTokenStatus
    {
        Success = 0,
        NoTokens = 1,
        NotEnoughTokens = 2,
        InternalError = 100,
        ExceptionRaised = 200,
    }
}
