using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using UserServices.Models;
using BCrypt.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace UserServices.Middlewares {
    public class PasswordEncryptor {
        private readonly RequestDelegate _next;

        public PasswordEncryptor() { }

        public PasswordEncryptor(RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext) {
            string body = new StreamReader(httpContext.Request.Body).ReadToEnd();
            if (!string.IsNullOrEmpty(body)) {
                var user = JsonConvert.DeserializeObject<User>(body);
                HashPassword(user);

                byte[] requestData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(user));
                httpContext.Request.Body = new MemoryStream(requestData);
            }
            await _next(httpContext);
        }
        public void Configure(IApplicationBuilder app) {
            app.UseMiddleware<PasswordEncryptor>();
        }

        internal void HashPassword(User user) {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password, SaltRevision.Revision2X);
            user.Password = hashedPassword;
        }
    }
}
