using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Models.Chat
{
    public enum MessageResult : byte
    {
        Error = 0,
        Success = 1,
        NotEnoughTokens = 2,
        NotFound = 3,
    }
}
