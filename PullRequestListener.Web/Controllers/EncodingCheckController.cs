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
using System.IO;
using PullRequestListener.EncodingChecker;
using System.Threading;

namespace PullRequestListener.web.Controllers
{
    public class EncodingCheckController : ApiController
    {
        private string CollectionUrl = ConfigurationManager.AppSettings["CollectionUrl"].ToString();
        private string VstsAuthenticationToken = ConfigurationManager.AppSettings["VstsAuthenticationToken"].ToString();
        private string AuthenticationToken = ConfigurationManager.AppSettings["AuthenticationToken"].ToString();
        private List<string> extensionBlackList = ConfigurationManager.AppSettings["ExtensionBlackList"].ToString().Split(',').ToList<string>();
        private GitHttpClient gitHttpClient;
        private GitHttpClient GitClient
        {
            get
            {
                if (gitHttpClient == null)
                {
                    VssConnection connection = new VssConnection(new Uri(CollectionUrl), new VssBasicCredential("", VstsAuthenticationToken));
                    gitHttpClient = connection.GetClient<GitHttpClient>();
                }
                return gitHttpClient;
            }
        }

        public HttpResponseMessage Post()
        {
            var token = Request.Headers.Authorization;
            if(token.Scheme.ToLower() != "basic" || Encoding.UTF8.GetString(Convert.FromBase64String(token.Parameter)) != AuthenticationToken)
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized, "Unauthorized");
            }

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
            GitClient.CreatePullRequestStatusAsync(pullRequestStatus, repoId, pullRequestId);


            Task.Factory.StartNew(() =>
            {
                CheckPullRequestFilesEncoding(repoId, pullRequestId);
            });
            
            
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, "Checking Encoding");
            response.Content = new StringContent("Checking Encoding", Encoding.Unicode);
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                MaxAge = TimeSpan.FromMinutes(20)
            };
            return response;

        }
        private async void CheckPullRequestFilesEncoding(string repositoryId, int pullRequestId)
        {
            GitPullRequestStatus pullRequestStatus = new GitPullRequestStatus()
            {
                Context = new GitStatusContext()
                {
                    Genre = "validation",
                    Name = "encoding-checker"
                }
            };
            GitPullRequestCommentThread pullRequestCommentThread = new GitPullRequestCommentThread();
            pullRequestCommentThread.Comments = new List<Comment>();
            try
            {
                GitPullRequest pullRequest = await GitClient.GetPullRequestByIdAsync(pullRequestId);
                List<GitPullRequestIteration> iterationList = new List<GitPullRequestIteration>();


                if (pullRequest.SupportsIterations)
                {
                    iterationList = await GitClient.GetPullRequestIterationsAsync(repositoryId, pullRequestId);
                }

                List<GitCommitRef> commitRefList = await GitClient.GetPullRequestIterationCommitsAsync(repositoryId, pullRequestId, iterationList.Max(i => i.Id).Value);
                List<GitItem> itemsToCheck = new List<GitItem>();
                List<GitItem> failedItems = new List<GitItem>();
                List<GitChange> renamedChanges = new List<GitChange>();
                foreach (GitCommitRef commitRef in commitRefList.OrderByDescending(r => r.Author.Date))
                {
                    GitCommitChanges gitCommitChanges = await GitClient.GetChangesAsync(commitRef.CommitId, Guid.Parse(repositoryId));

                    renamedChanges.AddRange(gitCommitChanges.Changes
                        .Where(i => i.ChangeType == VersionControlChangeType.Rename));

                    itemsToCheck.AddRange(gitCommitChanges.Changes
                        .Where(i => i.ChangeType != VersionControlChangeType.Delete)
                        .Where(i => i.ChangeType != (VersionControlChangeType.Delete | VersionControlChangeType.SourceRename))
                        .Select(c => c.Item).Where(i => !i.IsFolder)
                        .Where(i => !itemsToCheck.Select(f => f.Path).Contains(i.Path)));
                }
                
                //Clean out renamed items from the list of items to check
                foreach (GitChange change in renamedChanges)
                {

                    var matches = itemsToCheck.Where(i => i.Path == change.SourceServerItem).ToList();
                    foreach (GitItem remove in matches)
                    {
                        itemsToCheck.Remove(remove);
                    }
                }
                
                foreach (GitItem item in itemsToCheck)
                {
                    GitVersionDescriptor versionDescriptor = new GitVersionDescriptor()
                    {
                        Version = item.CommitId,
                        VersionType = GitVersionType.Commit,
                        VersionOptions = GitVersionOptions.None
                    };

                    if (!extensionBlackList.Select(s => s.ToLower()).Contains(Path.GetExtension(item.Path).Substring(1).ToLower()))
                    {
                        using (Stream itemStream = await GitClient.GetItemContentAsync(Guid.Parse(repositoryId), item.Path, versionDescriptor: versionDescriptor))
                        {
                            if (!Encoding.UTF8.IsOfEncoding(itemStream, false))
                            {
                                failedItems.Add(item);
                            }
                        }
                    }
                }
                
                if (failedItems.Count > 0)
                {
                    pullRequestStatus.State = GitStatusState.Failed;
                    pullRequestStatus.Description = "Encoding Check Failed";

                    string Message = $"## The following files do not have the correct encoding {Environment.NewLine}";
                    Message += $"File Name | Commit Id {Environment.NewLine}";
                    Message += $":----|:----{Environment.NewLine}";
                    foreach(GitItem failedItem in failedItems)
                    {
                        Message += $"{failedItem.Path}|{failedItem.CommitId} {Environment.NewLine}";
                    }

                    pullRequestCommentThread.Comments.Add(new Comment { Content = Message, CommentType = CommentType.System, ParentCommentId = 0 });
                    await GitClient.CreateThreadAsync(pullRequestCommentThread, repositoryId, pullRequestId);
                }
                else
                {
                    pullRequestStatus.State = GitStatusState.Succeeded;
                    pullRequestStatus.Description = "Encoding Check Passed";
                }
                
            }
            catch (Exception ex)
            {
                pullRequestStatus.State = GitStatusState.Failed;
                pullRequestStatus.Description = "Encoding Check Failure";
                pullRequestCommentThread.Comments.Add(new Comment { Content = "Encoding Validation Failed: " + ex.Message, CommentType = CommentType.System, ParentCommentId = 0 });
                await GitClient.CreateThreadAsync(pullRequestCommentThread, repositoryId, pullRequestId);
            }
            finally
            {
                try
                {
                    await GitClient.CreatePullRequestStatusAsync(pullRequestStatus, repositoryId, pullRequestId);
                }
                catch
                {
                    //If this fails, then all is lost
                }
            }
        }


    }
}

