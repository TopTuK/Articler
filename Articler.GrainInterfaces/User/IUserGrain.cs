using Articler.AppDomain.Models.Auth;
using Articler.AppDomain.Models.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainInterfaces.User
{
    // should use IGrainWithStringKey where string is email
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<IUserProfile> Authenticate(string email, string firstName, string lastName);
        
        Task<IUserProfile> GetUser();
        Task<IUserProfile> UpdateUser(string firstName, string lastName);

        Task<IProject> CreateProject(string title, string description);
        Task<IProject> RemoveProject(Guid projectId);

        Task<IEnumerable<IProject>> GetProjects();
        Task<IProject?> GetProjectById(string projectId);
    }
}
