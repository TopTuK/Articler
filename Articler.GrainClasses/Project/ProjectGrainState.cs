using Articler.AppDomain.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.Project
{
    [GenerateSerializer]
    public class ProjectGrainState
    {
        [Id(0)]
        public ProjectGrainStatus Status { get; set; } = ProjectGrainStatus.Unknown;

        [Id(1)]
        public Guid ProjectId { get; set; } = Guid.Empty;

        [Id(2)]
        public ProjectState ProjetState { get; set; } = ProjectState.None;

        [Id(3)]
        public string Title { get; set; } = string.Empty;

        [Id(4)]
        public string Description { get; set; } = string.Empty;

        [Id(5)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
