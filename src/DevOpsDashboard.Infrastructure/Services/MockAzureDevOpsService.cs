using DevOpsDashboard.Core.Interfaces;
using DevOpsDashboard.Core.Models;

namespace DevOpsDashboard.Infrastructure.Services;

public class MockAzureDevOpsService : IAzureDevOpsService
{
    private static readonly string[] Pipelines = ["api-build", "frontend-build", "deploy-staging", "deploy-prod", "run-tests"];
    private static readonly string[] Branches = ["main", "develop", "feature/auth", "feature/dashboard", "hotfix/login"];
    private static readonly string[] Users = ["alice", "bob", "charlie", "diana"];
    private static readonly string[] Statuses = ["succeeded", "failed", "succeeded", "succeeded", "canceled"];

    public Task<IEnumerable<PipelineRun>> GetRecentPipelineRunsAsync(string project, int top = 20)
    {
        var runs = Enumerable.Range(1, top).Select(i => new PipelineRun
        {
            Id = i.ToString(),
            Name = Pipelines[i % Pipelines.Length],
            Project = project,
            Status = i == 1 ? "running" : Statuses[i % Statuses.Length],
            BranchName = Branches[i % Branches.Length],
            TriggeredBy = Users[i % Users.Length],
            StartTime = DateTime.UtcNow.AddMinutes(-(i * 15)),
            FinishTime = i == 1 ? null : DateTime.UtcNow.AddMinutes(-(i * 15) + 8)
        });

        return Task.FromResult(runs);
    }

    public Task<IEnumerable<PipelineRun>> GetRunningPipelinesAsync()
    {
        var running = new List<PipelineRun>
        {
            new()
            {
                Id = "999",
                Name = "api-build",
                Project = "MyProject",
                Status = "running",
                BranchName = "main",
                TriggeredBy = "alice",
                StartTime = DateTime.UtcNow.AddMinutes(-3),
                FinishTime = null
            }
        };

        return Task.FromResult<IEnumerable<PipelineRun>>(running);
    }
}