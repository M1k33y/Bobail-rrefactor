using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobail.Domain.Users
{
    public class User
    {
        public Guid Id { get; set; }

        public string Nickname { get; set; } = null!;

        public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

        public int Role { get; set; } // 0 = User, 1 = Admin

        public DateTime CreatedAt { get; set; }

        public bool IsEmailVerified { get; set; }

        public DateTime? EmailVerifiedAtUtc { get; set; }
    }
}
