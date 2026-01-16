using Articler.AppDomain.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.GrainClasses.User
{
    [GenerateSerializer]
    public class UserGrainState
    {
        [Id(0)]
        public UserProfileStatus Status { get; set; } = UserProfileStatus.Unknown;

        [Id(1)]
        public string UserEmail { get; set; } = string.Empty;

        [Id(2)]
        public string FirstName { get; set; } = string.Empty;

        [Id(3)]
        public string LastName { get; set; } = string.Empty;

        [Id(4)]
        public AccountType AccountType { get; set; } = AccountType.None;

        [Id(5)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
