using Articler.AppDomain.Models.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.Project
{
    [GenerateSerializer]
    public class ProjectDocumentGrainState
    {
        [Id(0)]
        public List<IDocument> Documents { get; set; } = [];
    }
}
