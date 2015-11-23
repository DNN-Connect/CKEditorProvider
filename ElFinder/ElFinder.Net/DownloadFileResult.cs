using DotNetNuke.Services.FileSystem;
using System.IO;
using System.Web;
using Newtonsoft.Json.Linq;


namespace ElFinder
{
    public class DownloadFileResult 
    {
        public IFileInfo File { get; private set; }
        public bool IsDownload { get; private set; }
        public DownloadFileResult(IFileInfo file, bool isDownload)
        {
            File = file;
            IsDownload = isDownload;
        }


        public void Download(HttpResponseBase response, HttpRequestBase request)
        {
            if (!HttpCacheHelper.IsFileFromCache(File, request, response))
            {

                string fileName;
                string fileNameEncoded = HttpUtility.UrlEncode(File.FileName);

                if (request.UserAgent.Contains("MSIE")) // IE < 9 do not support RFC 6266 (RFC 2231/RFC 5987)
                {
                    fileName = "filename=\"" + fileNameEncoded + "\"";
                }
                else
                {
                    fileName = "filename*=UTF-8\'\'" + fileNameEncoded; // RFC 6266 (RFC 2231/RFC 5987)
                }
                string mime;
                string disposition;
                if (IsDownload)
                {
                    mime = "application/octet-stream";
                    disposition = "attachment; " + fileName;
                }
                else
                {
                    mime = Helper.GetMimeType(File.Extension);
                    disposition = (mime.Contains("image") || mime.Contains("text") || mime == "application/x-shockwave-flash" ? "inline; " : "attachment; ") + fileName;
                }

                var stream = FileManager.Instance.GetFileContent(File);

                response.ContentType = mime;
                response.AppendHeader("Content-Disposition", disposition);
                response.AppendHeader("Content-Location", File.FileName);
                response.AppendHeader("Content-Transfer-Encoding", "binary");
                response.AppendHeader("Content-Length", File.Size.ToString());

                byte[] buff = new byte[0x8000];

                int bytes = -1;
                while (bytes != 0)
                {
                    bytes = stream.Read(buff, 0, buff.Length);

                    if (bytes > 0)
                        response.OutputStream.Write(buff, 0, bytes);
                }

                response.End();
                response.Flush();
            }
            else
            {
                response.ContentType = IsDownload ? "application/octet-stream" : Helper.GetMimeType(File.Extension);
                response.End();
            }
        }
    }
}
