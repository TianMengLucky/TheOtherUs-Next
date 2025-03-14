using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using AmongUs.Data.Legacy;
using TheOtherUs.Languages;

namespace TheOtherUs.Helper;

public static class DownloadHelper
{
    public static string FastUrl => "https://github.moeyy.xyz";

    public static bool IsCN()
    {
        return RegionInfo.CurrentRegion.ThreeLetterISORegionName == "CHN"
               ||
               (SupportedLangs)LegacySaveManager.LastLanguage == SupportedLangs.SChinese
               ||
               LanguageManager.Instance.CurrentLang == SupportedLangs.SChinese
               ||
               CultureInfo.CurrentCulture.Name.StartsWith("zh");
    }

    public static string GithubUrl(this string url)
    {
        if (IsCN() && !url.Contains(FastUrl))
            return url
                .Replace("https://github.com", $"{FastUrl}/https://github.com")
                .Replace("https://raw.githubusercontent.com", $"{FastUrl}/https://raw.githubusercontent.com");

        return url;
    }

    public static string ReadToEnd(this Stream stream)
    {
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static string RepoRawURL => GetRepoRawURL();
    public static string RepoApiURL => GetRepoApiURL();
    
    public static string GetRepoApiURL()
    {
        return string.Empty;
    }
    
    public static string GetRepoRawURL()
    {
        return string.Empty;
    }
}

public interface IFastDownloader : IDisposable
{
    public void Download(string downloadPath, string file, string LocalPath = "");
}

public class RepoDownloader(CodeRepo repo) : IFastDownloader
{
    public DirectoryInfo directory;
    public Repos repoType = repo.repoType;
    public List<IGet> Gets = [];


    #nullable enable
    private T? Get<T>() where T : class, IGet, new()
    {
        return Gets.FirstOrDefault(n => n is T) as T;
    }
    #nullable disable

    public RepoDownloader BuildGet()
    {
        if (repoType is Repos.Github)
        {
            Gets.Add(GithubGet.Get(repo));
        }
        
        return this;
    }

    public RepoDownloader SetBaseDirectory(string path)
    {
        directory = new DirectoryInfo(path);
        return this;
    }
    public void Download(string downloadPath, string file, string LocalPath = "")
    {
        
    }
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

public enum Repos
{
    Github,
    GitLab,
    GitCode,
    Gitee,
    Gitea,
    Nuget
}

public sealed class CodeRepo
{
    public string Url { get; set; }
    public string pingUrl { get; set; }
    public int Time { get; set; }
    public string Branch { get; set; }
    public Repos repoType { get; set; }
    
    public void Download()
    {
        
    }

    public static readonly HashSet<CodeRepo> Repos = [];

    public static CodeRepo GetFastRepo()
    {
        var list = new List<CodeRepo>();
        using var p = new Ping();
        foreach (var repo in Repos)
        { 
            var r = p.Send(repo.pingUrl);
            if (r is not { Status: IPStatus.Success }) continue;
            repo.Time = r.Options!.Ttl;
            list.Add(repo);
        }
        
        list.Sort((x, y) => x.Time.CompareTo(y.Time));
        return list.FirstOrDefault();
    }
}

public interface IGet : IDisposable;

public interface IRepoGet : IGet;

public class ThunderStoreGet : IGet
{
    public void Dispose()
    {
    }
}

public class NugetGet : IGet
{
    public string NugetURL { get; set; }

    public List<IGet> Packages { get; set; } = [];
    
    public static NugetGet CreateNewGet(string url)
    {
        return new NugetGet
        {
            NugetURL = url
        };
    }

    public NugetGet AddPackage()
    {
        Packages.Add(new PackageGet()
        {
            
        });
        return this;
    }
    
    public class PackageGet : IGet
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Framework { get; set; }
        public string Author { get; set; }
        public Version Version { get; set; }
        
        public void Dispose()
        {
            
        }
        
    }
    
    public void Dispose()
    {
    }
}

public class GithubGet(string RepoOwner, string RepoName) : IRepoGet
{
    public const string Api = "https://api.github.com";
    public const string Web = "https://github.com";
    public const string Raw = "https://raw.githubusercontent.com";

    public string Owner { get; } = RepoOwner;
    public string Name { get; } = RepoName;
    public HttpClient _client = new();

    #nullable enable
    public WorkflowsGet? Workflows;
    public ReleaseGet? Release;
    public RawGet? RawContent;
    public ApiGet? ApiContent;

    public static GithubGet? Get(CodeRepo repo)
    {
        if (!repo.Url.StartsWith("https://github.com")) return null;
        var strings = repo.Url.Replace("https://github.com/", string.Empty).Split("/");
        return new GithubGet(strings[0], strings[1]);
    }
    
    public static GithubGet? GetAll(CodeRepo repo)
    {
        return Get(repo)?.AddAPI().AddRelease().AddRawContent().AddWorkflow();
    }
    
    #nullable disable
    public GithubGet AddWorkflow()
    {
        AddAPI();
        Workflows ??= new WorkflowsGet(this);
        return this;
    }
    
    public GithubGet AddRelease()
    {
        AddAPI();
        Release ??= new ReleaseGet(this);
        return this;
    }
    
    public GithubGet AddRawContent()
    {
        RawContent ??= new RawGet(this);
        return this;
    }

    public GithubGet AddAPI()
    {
        ApiContent ??= new ApiGet(this);
        return this;
    }

    public void Dispose()
    {
        _client.Dispose();
        _client = null;
        
        Workflows?.Dispose();
        Workflows = null;
        
        Release?.Dispose();
        Release = null;
        
        ApiContent?.Dispose();
        ApiContent = null;
        
        RawContent?.Dispose();
        RawContent = null;
    }
    
    public class WorkflowsGet(GithubGet get) : IGithubGet
    {
        public GithubGet Github { get; } = get;
        public List<Workflow> workflows = [];
        public void Dispose()
        {
            workflows = null;
        }

        public List<Workflow> getWorkflows()
        {
            return Github.ApiContent?.GetWorkflows().RootElement.GetProperty("workflows").Deserialize<List<Workflow>>();
        }
        
        public Stream GetLatest()
        {
            return new MemoryStream();
        }
        
        public class Workflow
        {
            public int id { get; set; }
            public string node_id { get; set; }
            public string name { get; set; }
            public string path { get; set; }
            public string active { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
            public string url { get; set; }
            public string html_url { get; set; }
            public string badge_url { get; set; }
        } 
        
        public class WorkflowRun
        {
            public long id;
        }
        
        public class Repository
        {
            
        }
        
        public class Actor
        {
            
        }
    }
    
    
    public class ReleaseGet(GithubGet get) : IGithubGet
    {
        public GithubGet Github { get; } = get;
        public void Dispose()
        {
        }
    }
    
    public class RawGet(GithubGet get) : IGithubGet
    {
        public GithubGet Github { get; } = get;
        public void Dispose()
        {
        }
    }
    
    public class ApiGet(GithubGet get) : IGithubGet
    {
        public GithubGet Github { get; } = get;

        public string RepoAPI { get; } = string.Join("/", Api, "repos", get.Owner, get.Name);

        public JsonDocument GetWorkflows()
        {
            return JsonDocument.Parse(Github._client.GetStringAsync($"{RepoAPI}/actions/workflows").Result);
        }
        
        public JsonDocument GetRuns(int WorkflowId)
        {
            return JsonDocument.Parse(Github._client.GetStringAsync($"{RepoAPI}/actions/workflows/{WorkflowId}/runs").Result);
        }
        
        public void Dispose()
        {
        }
    }
    
    public interface IGithubGet : IGet
    {
        public GithubGet Github { get; }
    }
}