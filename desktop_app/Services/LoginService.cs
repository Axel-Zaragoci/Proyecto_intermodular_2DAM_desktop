using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktop_app.Services
{
    /// <summary>
    /// Capa fina para login que delega en AuthService.
    /// </summary>
    public class LoginService
    {
        public Task<string> LoginAsync(string email, string password) => AuthService.LoginAsync(email, password);
    }
}

