using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Models.Token
{
    public interface ICalculateTokenResult
    {
        CalculateTokenStatus Status { get; }
        int MaxTokenCount { get; }
    }
}
