using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.User
{
    [GenerateSerializer]
    public class UserProjectsGrainState
    {
        [Id(0)]
        public List<Guid> ProjectIds { get; set; } = new List<Guid>();
    }
}
