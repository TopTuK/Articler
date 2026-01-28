using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Services.TokenService
{
    public interface ICalculateTokenService
    {
        string Model { get; }
        int CountTokens(string text);
    }
}
