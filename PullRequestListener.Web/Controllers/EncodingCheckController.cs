using Json;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace PullRequestListener.web.Controllers
{
    public class EncodingCheckController : ApiController
    {
        private string CollectionUrl = ConfigurationManager.AppSettings["CollectionUrl"].ToString();
        private string AuthenticationToken = ConfigurationManager.AppSettings["AuthenticationToken"].ToString();

        public HttpResponseMessage Post()
        {
            string requestContent = Request.Content.ReadAsStringAsync().Result;

            var request = JsonParser.Deserialize(requestContent);
            var repoId = request.resource.repository.id.ToString();
            var pullRequestId = Int32.Parse(request.resource.pullrequestid.ToString());
            var title = request.resource.title;

            GitPullRequestStatus pullRequestStatus = new GitPullRequestStatus()
            {
                State = GitStatusState.Pending,
                Description = "Checking Encoding",
                Context = new GitStatusContext()
                {
                     Genre = "validation",
                     Name = "encoding-checker"
                }
            };
            SetPullRequestStatus(repoId, pullRequestId, pullRequestStatus);




            pullRequestStatus.State = GitStatusState.Succeeded;
            SetPullRequestStatus(repoId, pullRequestId, pullRequestStatus);



            
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "Received Post");
            response.Content = new StringContent("Received Post", Encoding.Unicode);
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                MaxAge = TimeSpan.FromMinutes(20)
            };
            return response;

        }
        private async void SetPullRequestStatus(string repositoryId, int pullRequestId, GitPullRequestStatus pullRequestStatus)
        {
            VssConnection connection = new VssConnection(new Uri(CollectionUrl), new VssBasicCredential("", AuthenticationToken));
            
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();
            GitRepository gitRepository = await gitClient.GetRepositoryAsync(repositoryId);
            GitPullRequest pullRequest = await gitClient.GetPullRequestByIdAsync(pullRequestId);

            await gitClient.CreatePullRequestStatusAsync(pullRequestStatus, gitRepository.Id.ToString(), pullRequestId);
        }

    }
}

