using Articler.AppDomain.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.User
{
    [GenerateSerializer]
    [Alias("Articler.GrainClasses.User.CalculateTokenResult")]
    public class CalculateTokenResult : ICalculateTokenResult
    {
        [Id(0)]
        public CalculateTokenStatus Status { get; init; }
        [Id(1)]
        public int MaxTokenCount { get; init; }
    }
}
