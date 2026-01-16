using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.Project
{
    [GenerateSerializer]
    public class ProjectTextGrainState
    {
        [Id(0)]
        public string Text { get; set; } = string.Empty;
    }
}
