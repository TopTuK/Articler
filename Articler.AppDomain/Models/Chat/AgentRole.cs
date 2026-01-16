using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Models.Chat
{
    public enum ChatRole : byte
    {
        Unknown = 255,
        User = 0,
        Assistant = 1,
        System = 2,
        Tool = 3,
    }
}
