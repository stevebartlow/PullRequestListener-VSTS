using Json;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Linq;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Collections.Generic;

namespace PullRequestListener.web.Controllers
{
    public class EncodingCheckController : ApiController
    {
        private string CollectionUrl = ConfigurationManager.AppSettings["CollectionUrl"].ToString();
        private string AuthenticationToken = ConfigurationManager.AppSettings["AuthenticationToken"].ToString();
        private GitHttpClient GitClient
        {
            get
            {
                VssConnection connection = new VssConnection(new Uri(CollectionUrl), new VssBasicCredential("", AuthenticationToken));
                return connection.GetClient<GitHttpClient>();
            }
        }

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


            CheckPullRequestFilesEncoding(repoId, pullRequestId);
            
            
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "Checking Encoding");
            response.Content = new StringContent("Checking Encoding", Encoding.Unicode);
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                MaxAge = TimeSpan.FromMinutes(20)
            };
            return response;

        }

        private async void SetPullRequestStatus(string repositoryId, int pullRequestId, GitPullRequestStatus pullRequestStatus)
        {
            await GitClient.CreatePullRequestStatusAsync(pullRequestStatus, repositoryId, pullRequestId);
        }
        private async void CheckPullRequestFilesEncoding(string repositoryId, int pullRequestId)
        {
            GitPullRequest pullRequest = await GitClient.GetPullRequestByIdAsync(pullRequestId);
            GitCommitRef commitRef = pullRequest.LastMergeSourceCommit;


            List<GitPullRequestIteration> iterationList = new List<GitPullRequestIteration>();
            if(pullRequest.SupportsIterations)
            {
                iterationList = await GitClient.GetPullRequestIterationsAsync(repositoryId, pullRequestId);
            }

            List<GitCommitRef> commitRefList = await GitClient.GetPullRequestIterationCommitsAsync(repositoryId, pullRequestId, 1);

            //gitClient.GetItemsAsync
            



        }


    }
}

