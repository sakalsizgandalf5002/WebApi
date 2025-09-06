using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Api.Security
{
    public class PepperedPasswordHasher : IPasswordHasher<AppUser>
    {
        private readonly IPasswordHasher<AppUser> _inner = new PasswordHasher<AppUser>();
        private readonly string _pepper;
        public class Options
        {
            public string Pepper { get; set; } = "";
        }
        public PepperedPasswordHasher(IOptions<Options> options)
        {
            _pepper = options.Value.Pepper ?? "";
        }

        public string HashPassword(AppUser user, string password)
        {
            return _inner.HashPassword(user, password + _pepper);
        }

        public PasswordVerificationResult VerifyHashedPassword(AppUser user, string hashedPassword, string providedPassword)
        {
            var withPepper = _inner.VerifyHashedPassword(user, hashedPassword, providedPassword + _pepper);
            if (withPepper != PasswordVerificationResult.Failed)
            {
                return withPepper;
            }

            var withoutPepper = _inner.VerifyHashedPassword(user, hashedPassword, providedPassword);
            if (withoutPepper == PasswordVerificationResult.Success)
                return PasswordVerificationResult.SuccessRehashNeeded;

            return PasswordVerificationResult.Failed;
        }
    }
}