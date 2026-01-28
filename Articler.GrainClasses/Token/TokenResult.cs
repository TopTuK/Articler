using Articler.AppDomain.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.Token
{
    [GenerateSerializer]
    public class CalculateTokenResult<TResult> : ICalculateTokenResult<TResult>
    {
        [Id(0)]
        public CalculateTokenStatus Status { get; init; }
        [Id(1)]
        public TResult? Result { get; init; }
    }
}
