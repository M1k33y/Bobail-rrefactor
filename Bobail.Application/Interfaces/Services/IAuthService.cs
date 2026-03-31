using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobail.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<Guid> RegisterAsync(string email, string password);
        Task<string> LoginAsync(string email, string password);
    }
}
