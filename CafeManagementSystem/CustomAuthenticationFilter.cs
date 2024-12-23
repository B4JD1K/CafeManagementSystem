using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;

namespace CafeManagementSystem
{
    public class CustomAuthenticationFilter : AuthorizeAttribute, IAuthenticationFilter
    {
        // check does http headers contains correct token
        public async Task AuthenticateAsync(HttpAuthenticationContext context,
            CancellationToken cancellationToken)
        {
            HttpRequestMessage request = context.Request;
            AuthenticationHeaderValue authorization = request.Headers.Authorization;
            if (authorization == null || authorization.Scheme != "Bearer" ||
                string.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationFailureResult();
                return;
            }

            // setting up user context 
            context.Principal = TokenManager.GetPrincipal(authorization.Parameter);
        }

        public async Task ChallengeAsync(HttpAuthenticationChallengeContext context,
            CancellationToken cancellationToken)
        {
            // doing controller's result to check http status  
            var result = await context.Result.ExecuteAsync(cancellationToken);
            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                // adding to WWW-Authenticate header "basic" value in whole "localhost" realm
                result.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Basic", "real=localhost"));
            }

            context.Result = new ResponseMessageResult(result);
        }
    }

    // class that is responsible for creating http result if authentication is failed
    public class AuthenticationFailureResult : IHttpActionResult
    {
        public AuthenticationFailureResult()
        {
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            // creating response with Unauthorized status
            HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            return Task.FromResult(responseMessage); // returning as an async task
        }
    }
}