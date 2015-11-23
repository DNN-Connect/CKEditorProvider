using System;
using System.Collections.Generic;

using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;

using ElFinder.DTO;
using ElFinder.Response;
using DotNetNuke.Services.FileSystem;
using System.IO;
using DotNetNuke.Security.Permissions;

namespace ElFinder
{
    /// <summary>
    /// Represents a driver for local file system
    /// </summary>
    public class DnnFileSystemDriver : IDriver
    {
        #region private  
        private const string _volumePrefix = "v";
        private readonly string[] _allowedImageExt = { "bmp", "gif", "jpeg", "jpg", "png" };

        private List<Root> _roots;
        private bool _imagesOnly;

        static private PermissionProvider _permissionProvider = PermissionProvider.Instance();
        static private IFolderManager _folderManager = FolderManager.Instance;
        static private IFileManager _fileManager = FileManager.Instance;

        private JObject Json(object data)
        {
            return JObject.FromObject(data);
        }
        private void DirectoryCopy(IFolderInfo sourceDir, string destDirName, bool copySubDirs)
        {
            var dirs = _folderManager.GetFolders(sourceDir);

            // If the source directory does not exist, throw an exception.
            if (!_folderManager.FolderExists(sourceDir.PortalID, sourceDir.FolderPath))
            {
                throw new System.IO.DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDir.PhysicalPath);
            }

            IFolderInfo destDir;

            // If the destination directory does not exist, create it.
            if (!_folderManager.FolderExists(sourceDir.PortalID, destDirName))
                destDir = _folderManager.AddFolder(sourceDir.PortalID, destDirName);
            else
                destDir = _folderManager.GetFolder(sourceDir.PortalID, destDirName);

            // Get the file contents of the directory to copy.
            var files = _folderManager.GetFiles(sourceDir);

            foreach (var file in files)
                _fileManager.CopyFile(file, destDir);

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {
                foreach (IFolderInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = System.IO.Path.Combine(destDirName, subdir.FolderName);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir, temppath, copySubDirs);
                }
            }
        }

        private void RemoveThumbs(FullPath path)
        {
            //if (path.Directory != null)
            //{
            //    string thumbPath = path.Root.GetExistingThumbPath(path.Directory);
            //    if (thumbPath != null)
            //        _folderManager.DeleteFolder( Directory.Delete(thumbPath, true);
            //}
            //else
            //{
            //    string thumbPath = path.Root.GetExistingThumbPath(path.File);
            //    if (thumbPath != null)
            //        File.Delete(thumbPath);
            //}
        }
        #endregion

        #region public 

        public FullPath ParsePath(string target)
        {
            var parts = target.Split('_');

            if (parts.Length != 2)
                return null;

            string volumePrefix = parts[0];
            string pathHash = parts[1];

            Root root = _roots[0];//.First()(r => r.VolumeId == target);

            int id = Helper.DecodeId(pathHash);

            if (volumePrefix == "D")
            {
                return new FullPath(root, _folderManager.GetFolder(id));
            }
            else
            {
                var file = _fileManager.GetFile(id);

                return new FullPath(root, file);
            }
        }

        /// <summary>
        /// Initialize new instance of class ElFinder.FileSystemDriver 
        /// </summary>
        public DnnFileSystemDriver(bool imagesOnly)
        {
            _roots = new List<Root>();
            _imagesOnly = imagesOnly;
        }

        bool IsImage(IFileInfo file)
        {
            string ext = file.Extension.ToLower();

            return _allowedImageExt.FirstOrDefault(f => f == ext) != null;
        }

        /// <summary>
        /// Adds an object to the end of the roots.
        /// </summary>
        /// <param name="item"></param>
        public void AddRoot(Root item)
        {
            _roots.Add(item);
            item.VolumeId = _volumePrefix + _roots.Count + "_";
        }

        /// <summary>
        /// Gets collection of roots
        /// </summary>
        public IEnumerable<Root> Roots { get { return _roots; } }
        #endregion public

        #region   IDriver
        JObject IDriver.Open(string target, bool tree)
        {
            FullPath fullPath = ParsePath(target);
            OpenResponse answer = new OpenResponse(DTOBase.Create(fullPath.Directory, fullPath.Root), fullPath);
            foreach (var item in _folderManager.GetFiles(fullPath.Directory))
            {
                if (!_imagesOnly || IsImage(item))
                    answer.Files.Add(DTOBase.Create(item, fullPath.Root));
            }
            foreach (var item in _folderManager.GetFolders(fullPath.Directory))
            {
                if (HasReadPermissions(item))
                    answer.Files.Add(DTOBase.Create(item, fullPath.Root));
            }
            return Json(answer);
        }
        JObject IDriver.Init(string target)
        {
            FullPath fullPath;
            if (string.IsNullOrEmpty(target))
            {
                Root root = _roots.FirstOrDefault(r => r.StartPath != null);
                if (root == null)
                    root = _roots.First();
                fullPath = new FullPath(root, root.StartPath ?? root.Directory);
            }
            else
            {
                fullPath = ParsePath(target);
            }
            InitResponse answer = new InitResponse(DTOBase.Create(fullPath.Directory, fullPath.Root), new Options(fullPath));

            //foreach (FileInfo item in fullPath.Directory.GetFiles())
            //{
            //    if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
            //        answer.Files.Add(DTOBase.Create(item, fullPath.Root));
            //}
            //foreach (IFolderInfo item in fullPath.Directory.GetDirectories())
            //{
            //    if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
            //        answer.Files.Add(DTOBase.Create(item, fullPath.Root));
            //}
            foreach (var item in _folderManager.GetFiles(fullPath.Directory))
            {
                if (!_imagesOnly || IsImage(item))
                    answer.Files.Add(DTOBase.Create(item, fullPath.Root));
            }
            foreach (var item in _folderManager.GetFolders(fullPath.Directory))
            {
                if (HasReadPermissions(item))
                    answer.Files.Add(DTOBase.Create(item, fullPath.Root));
            }

            foreach (Root item in _roots)
            {
                answer.Files.Add(DTOBase.Create(item.Directory, item));
            }
            if (fullPath.Root.Directory.FolderID != fullPath.Directory.FolderID)
            {
                //foreach (IFolderInfo item in fullPath.Root.Directory.GetDirectories())
                //{
                //    if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                //        answer.Files.Add(DTOBase.Create(item, fullPath.Root));
                //}
                foreach (var item in _folderManager.GetFolders(fullPath.Root.Directory))
                {
                    if (HasReadPermissions(item))
                        answer.Files.Add(DTOBase.Create(item, fullPath.Root));
                }

            }
            if (fullPath.Root.MaxUploadSize.HasValue)
            {
                answer.UploadMaxSize = fullPath.Root.MaxUploadSizeInKb.Value + "K";
            }
            return Json(answer);
        }
        DownloadFileResult IDriver.File(string target, bool download)
        {
            FullPath fullPath = ParsePath(target);
            if (fullPath.IsDirectoty)
                throw new HttpException(403, "You can not download whole folder");
            //if (!_fileManager.FileExists(fullPath.File)) // TODO: file exists
            //    return new HttpNotFoundResult("File not found");
            if (fullPath.Root.IsShowOnly)
                throw new HttpException(403, "Access denied. Volume is for show only");

            return new DownloadFileResult(fullPath.File, download);
        }
        JObject IDriver.Parents(string target)
        {
            FullPath fullPath = ParsePath(target);
            TreeResponse answer = new TreeResponse();
            if (fullPath.Directory.FolderID == fullPath.Root.Directory.FolderID)
            {
                answer.Tree.Add(DTOBase.Create(fullPath.Directory, fullPath.Root));
            }
            else
            {
                IFolderInfo parent = _folderManager.GetFolder(fullPath.Directory.ParentID);
                foreach (var item in _folderManager.GetFolders(parent))
                {
                    answer.Tree.Add(DTOBase.Create(item, fullPath.Root));
                }
                while (parent.FolderID != fullPath.Root.Directory.FolderID)
                {
                    //parent = parent.Parent;
                    answer.Tree.Add(DTOBase.Create(parent, fullPath.Root));
                    parent = _folderManager.GetFolder(parent.ParentID);
                }
            }
            return Json(answer);
        }
        JObject IDriver.Tree(string target)
        {
            FullPath fullPath = ParsePath(target);
            TreeResponse answer = new TreeResponse();
            foreach (var item in _folderManager.GetFolders(fullPath.Directory))
            {
                if (HasReadPermissions(item))
                    answer.Tree.Add(DTOBase.Create(item, fullPath.Root));
            }
            return Json(answer);
        }
        JObject IDriver.List(string target)
        {
            FullPath fullPath = ParsePath(target);
            ListResponse answer = new ListResponse();
            foreach (var item in _folderManager.GetFiles(fullPath.Directory))
            {
                if (!_imagesOnly || IsImage(item))
                    answer.List.Add(item.FileName);
            }
            return Json(answer);
        }
        JObject IDriver.MakeDir(string target, string name)
        {
            FullPath fullPath = ParsePath(target);
            IFolderInfo newDir = _folderManager.AddFolder(fullPath.Directory.PortalID, Path.Combine(fullPath.Directory.FolderPath, name));
            return Json(new AddResponse(newDir, fullPath.Root));
        }
        //JObject IDriver.MakeFile(string target, string name)
        //{
        //    FullPath fullPath = ParsePath(target);
        //    FileInfo newFile = new FileInfo(Path.Combine(fullPath.Directory.FullName, name));
        //    newFile.Create().Close();
        //    return Json(new AddResponse(newFile, fullPath.Root));
        //}
        JObject IDriver.Rename(string target, string name)
        {
            FullPath fullPath = ParsePath(target);
            var answer = new ReplaceResponse();
            answer.Removed.Add(target);
            RemoveThumbs(fullPath);
            if (fullPath.Directory != null)
            {
                //string newPath = Path.Combine(fullPath.Directory.Parent.FullName, name);
                //System.IO.Directory.Move(fullPath.Directory.FullName, newPath);
                _folderManager.RenameFolder(fullPath.Directory, name);
                answer.Added.Add(DTOBase.Create(fullPath.Directory, fullPath.Root));
            }
            else
            {
                //string newPath = Path.Combine(fullPath.File.DirectoryName, name);
                //File.Move(fullPath.File.FullName, newPath);
                _fileManager.RenameFile(fullPath.File, name);
                answer.Added.Add(DTOBase.Create(fullPath.File, fullPath.Root));
            }
            return Json(answer);
        }
        JObject IDriver.Remove(IEnumerable<string> targets)
        {
            RemoveResponse answer = new RemoveResponse();
            foreach (string item in targets)
            {
                FullPath fullPath = ParsePath(item);
                RemoveThumbs(fullPath);
                if (fullPath.Directory != null)
                {
                    _folderManager.DeleteFolder(fullPath.Directory);

                    //                    System.IO.Directory.Delete(fullPath.Directory.FullName, true);
                }
                else
                {
                    _fileManager.DeleteFile(fullPath.File);
                    //                    File.Delete(fullPath.File.FullName);
                }
                answer.Removed.Add(item);
            }
            return Json(answer);
        }
        JObject IDriver.Get(string target)
        {
            FullPath fullPath = ParsePath(target);
            GetResponse answer = new GetResponse();
            using (StreamReader reader = new StreamReader(_fileManager.GetFileContent(fullPath.File)))
            {
                answer.Content = reader.ReadToEnd();
            }
            return Json(answer);
        }
        //JObject IDriver.Put(string target, string content)
        //{
        //    FullPath fullPath = ParsePath(target);
        //    ChangedResponse answer = new ChangedResponse();
        //    using (StreamWriter writer = new StreamWriter(fullPath.File.FullName, false))
        //    {
        //        writer.Write(content);
        //    }
        //    answer.Changed.Add((FileDTO)DTOBase.Create(fullPath.File, fullPath.Root));
        //    return Json(answer);
        //}
        JObject IDriver.Paste(string source, string dest, IEnumerable<string> targets, bool isCut)
        {
            FullPath destPath = ParsePath(dest);
            ReplaceResponse response = new ReplaceResponse(); // TODO:
            //foreach (var item in targets)
            //{
            //    FullPath src = ParsePath(item);
            //    if (src.Directory != null)
            //    {
            //        IFolderInfo newDir = new IFolderInfo(Path.Combine(destPath.Directory.FullName, src.Directory.Name));
            //        if (newDir.Exists)
            //            Directory.Delete(newDir.FullName, true);
            //        if (isCut)
            //        {
            //            RemoveThumbs(src);
            //            src.Directory.MoveTo(newDir.FullName);
            //            response.Removed.Add(item);
            //        }
            //        else
            //        {
            //            DirectoryCopy(src.Directory, newDir.FullName, true);
            //        }
            //        response.Added.Add(DTOBase.Create(newDir, destPath.Root));
            //    }
            //    else
            //    {
            //        string newFilePath = Path.Combine(destPath.Directory.FullName, src.File.Name);
            //        if (File.Exists(newFilePath))
            //            File.Delete(newFilePath);
            //        if (isCut)
            //        {
            //            RemoveThumbs(src);
            //            src.File.MoveTo(newFilePath);
            //            response.Removed.Add(item);
            //        }
            //        else
            //        {
            //            File.Copy(src.File.FullName, newFilePath);
            //        }
            //        response.Added.Add(DTOBase.Create(new FileInfo(newFilePath), destPath.Root));
            //    }
            //}
            return Json(response);
        }
        JObject IDriver.Upload(string target, System.Web.HttpFileCollection targets)
        {
            FullPath dest = ParsePath(target);
            var response = new AddResponse();
            if (dest.Root.MaxUploadSize.HasValue)
            {
                for (int i = 0; i < targets.AllKeys.Length; i++)
                {
                    HttpPostedFile file = targets[i];
                    if (file.ContentLength > dest.Root.MaxUploadSize.Value)
                    {
                        return Error.MaxUploadFileSize();
                    }
                }
            }
            for (int i = 0; i < targets.AllKeys.Length; i++)
            {
                HttpPostedFile file = targets[i];
                string fileName = Path.GetFileName(file.FileName);
                //FileInfo path = new FileInfo(Path.Combine(dest.Directory.FullName, Path.GetFileName(file.FileName)));
                IFileInfo newFile;
                if (_fileManager.FileExists(dest.Directory, fileName))
                {
                    if (dest.Root.UploadOverwrite)
                    {
                        newFile = _fileManager.AddFile(dest.Directory, fileName, file.InputStream, true);
                        //if file already exist we rename the current file, 
                        //and if upload is succesfully delete temp file, in otherwise we restore old file
                        //string tmpPath = path.FullName + Guid.NewGuid(); // TODO:
                        //bool uploaded = false;
                        //try
                        //{
                        //    file.SaveAs(tmpPath);
                        //    uploaded = true;
                        //}
                        //catch { }
                        //finally
                        //{
                        //    if (uploaded)
                        //    {
                        //        File.Delete(path.FullName);
                        //        File.Move(tmpPath, path.FullName);
                        //    }
                        //    else
                        //    {
                        //        File.Delete(tmpPath);
                        //    }
                        //}
                    }
                    else
                    {
                        //                        file.SaveAs(Path.Combine(path.DirectoryName, Helper.GetDuplicatedName(path)));
                        newFile = _fileManager.AddFile(dest.Directory, fileName, file.InputStream);
                    }
                }
                else
                {
                    newFile = _fileManager.AddFile(dest.Directory, fileName, file.InputStream);
                }
                response.Added.Add((FileDTO)DTOBase.Create(newFile, dest.Root));
            }
            return Json(response);
        }
        //JObject IDriver.Duplicate(IEnumerable<string> targets)
        //{
        //    AddResponse response = new AddResponse();
        //    foreach (var target in targets)
        //    {
        //        FullPath fullPath = ParsePath(target);
        //        if (fullPath.Directory != null)
        //        {
        //            var parentPath = fullPath.Directory.Parent.FullName;
        //            var name = fullPath.Directory.Name;
        //            var newName = string.Format(@"{0}\{1} copy", parentPath, name);
        //            if (!Directory.Exists(newName))
        //            {
        //                DirectoryCopy(fullPath.Directory, newName, true);
        //            }
        //            else
        //            {
        //                for (int i = 1; i < 100; i++)
        //                {
        //                    newName = string.Format(@"{0}\{1} copy {2}", parentPath, name, i);
        //                    if (!Directory.Exists(newName))
        //                    {
        //                        DirectoryCopy(fullPath.Directory, newName, true);
        //                        break;
        //                    }
        //                }
        //            }
        //            response.Added.Add(DTOBase.Create(new IFolderInfo(newName), fullPath.Root));
        //        }
        //        else
        //        {
        //            var parentPath = fullPath.File.Directory.FullName;
        //            var name = fullPath.File.Name.Substring(0, fullPath.File.Name.Length - fullPath.File.Extension.Length);
        //            var ext = fullPath.File.Extension;

        //            var newName = string.Format(@"{0}\{1} copy{2}", parentPath, name, ext);

        //            if (!File.Exists(newName))
        //            {
        //                fullPath.File.CopyTo(newName);
        //            }
        //            else
        //            {
        //                for (int i = 1; i < 100; i++)
        //                {
        //                    newName = string.Format(@"{0}\{1} copy {2}{3}", parentPath, name, i, ext);
        //                    if (!File.Exists(newName))
        //                    {
        //                        fullPath.File.CopyTo(newName);
        //                        break;
        //                    }
        //                }
        //            }
        //            response.Added.Add(DTOBase.Create(new FileInfo(newName), fullPath.Root));
        //        }
        //    }
        //    return Json(response);
        //}
        JObject IDriver.Thumbs(IEnumerable<string> targets)
        {
            ThumbsResponse response = new ThumbsResponse();
            foreach (string target in targets)
            {
                FullPath path = ParsePath(target);
                response.Images.Add(target, path.Root.GenerateThumbHash(path.File));
            }
            return Json(response);
        }
        JObject IDriver.Dim(string target)
        {
            FullPath path = ParsePath(target);
            DimResponse response = new DimResponse(string.Format("{0}x{1}", path.File.Width, path.File.Height));//path.Root.GetImageDimension(path.File));
            return Json(response);
        }
        //JObject IDriver.Resize(string target, int width, int height)
        //{
        //    FullPath path = ParsePath(target);
        //    RemoveThumbs(path);
        //    path.Root.PicturesEditor.Resize(path.File, width, height);
        //    var output = new ChangedResponse();
        //    output.Changed.Add((FileDTO)DTOBase.Create(path.File, path.Root));
        //    return Json(output);
        //}
        //JObject IDriver.Crop(string target, int x, int y, int width, int height)
        //{
        //    FullPath path = ParsePath(target);
        //    RemoveThumbs(path);
        //    path.Root.PicturesEditor.Crop(path.File.FullName, x, y, width, height);
        //    var output = new ChangedResponse();
        //    output.Changed.Add((FileDTO)DTOBase.Create(path.File, path.Root));
        //    return Json(output);
        //}
        //JObject IDriver.Rotate(string target, int degree)
        //{
        //    FullPath path = ParsePath(target);
        //    RemoveThumbs(path);
        //    path.Root.PicturesEditor.Rotate(path.File.FullName, degree);
        //    var output = new ChangedResponse();
        //    output.Changed.Add((FileDTO)DTOBase.Create(path.File, path.Root));
        //    return Json(output);
        //}

        JObject IDriver.Url(string target)
        {
            FullPath path = ParsePath(target);

            UrlResponse response = new UrlResponse() { Url = _fileManager.GetUrl(path.File), Name = path.File.FileName };

            return Json(response);
        }

        #endregion IDriver

        internal static bool HasReadPermissions(IFolderInfo folder)
        {
            return _permissionProvider.CanBrowseFolder(folder as FolderInfo);
        }
        internal static bool HasWritePermissions(IFolderInfo folder)
        {
            return _permissionProvider.CanManageFolder(folder as FolderInfo);
        }
    }
}