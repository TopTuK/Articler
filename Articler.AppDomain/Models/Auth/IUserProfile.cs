using Orleans;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Models.Auth
{
    public interface IUserProfile
    {
        [EmailAddress]
        string Email { get; }

        string FirstName { get; }
        string LastName { get; }

        AccountType AccountType { get; }
        int TokenCount { get; }

        DateTime CreatedDate { get; }
    }
}
