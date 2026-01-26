using Articler.AppDomain.Services.TokenService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Factories.Token
{
    public static class TokenServiceFactory
    {
        public static ITokenService CreateTokenService(string model)
        {
            return new TokenService(model);
        }
    }
}
