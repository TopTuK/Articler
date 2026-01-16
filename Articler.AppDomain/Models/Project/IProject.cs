using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Models.Project
{
    public interface IProject
    {
        Guid Id { get; }
        ProjectState State { get; }

        string Title { get; }
        string Description { get; }

        DateTime CreatedDate { get; }
    }
}
