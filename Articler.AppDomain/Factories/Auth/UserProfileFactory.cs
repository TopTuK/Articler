using Articler.AppDomain.Models.Auth;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Factories.Auth
{
    public static class UserProfileFactory
    {
        [GenerateSerializer]
        public class UserProfile(string email, string firstName, string lastName,
            AccountType accountType, int tokenCount, DateTime createdDate) : IUserProfile
        {
            [Id(0)]
            public string Email { get; } = email;
            [Id(1)]
            public string FirstName { get; } = firstName;
            [Id(2)]
            public string LastName { get; } = lastName;

            [Id(3)]
            public AccountType AccountType { get; } = accountType;

            [Id(4)]
            public int TokenCount { get; } = tokenCount;

            [Id(5)]
            public DateTime CreatedDate { get; } = createdDate.Date;
        }

        public static IUserProfile CreateUserProfile(string email, string firstName, string lastName, 
            AccountType accountType, int tokenCount, DateTime? createdDate = null)
        {
            return new UserProfile(email, firstName, lastName, accountType, tokenCount, createdDate ?? DateTime.UtcNow);
        }
    }
}
