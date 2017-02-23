using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json.Linq;

namespace ElFinder
{
    public interface IDriver
    {
        JObject Open(string target, bool tree); 
        JObject Init(string target);
        JObject Parents(string target);
        JObject Tree(string target);
        JObject List(string target);
        JObject MakeDir(string target, string name);
        //JObject MakeFile(string target, string name);
        JObject Rename(string target, string name);
        JObject Remove(IEnumerable<string> targets);
      //  JObject Duplicate(IEnumerable<string> targets);
        JObject Get(string target);
        //JObject Put(string target, string content);        
        JObject Paste(string  source, string dest, IEnumerable<string> targets, bool isCut);
        JObject Upload(string target, HttpFileCollection targets);
        JObject Thumbs(IEnumerable<string> targets);
        JObject Dim(string target);
        //  JObject Resize(string target, int width, int height);
        // JObject Crop(string target, int x, int y, int width, int height);
        //  JObject Rotate(string target, int degree);
        DownloadFileResult File(string target, bool download);
        JObject Url(string target);
        FullPath ParsePath(string target);
    }
}