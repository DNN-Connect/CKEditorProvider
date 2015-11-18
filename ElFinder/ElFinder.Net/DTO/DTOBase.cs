using System;
using System.Runtime.Serialization;
using System.IO;
using System.Linq;
using DotNetNuke.Services.FileSystem;

namespace ElFinder.DTO
{
    [DataContract]
    internal abstract class DTOBase
    {
        protected static readonly DateTime _unixOrigin = new DateTime(1970, 1, 1, 0, 0, 0);
        
        /// <summary>
        ///  Name of file/dir. Required
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; protected set; }
        
        /// <summary>
        ///  Hash of current file/dir path, first symbol must be letter, symbols before _underline_ - volume id, Required.
        /// </summary>
        [DataMember(Name = "hash")]
        public string Hash { get; protected set; } 
       
        /// <summary>
        ///  mime type. Required.
        /// </summary>
        [DataMember(Name = "mime")]
        public string Mime { get; protected set; } 
 
        /// <summary>
        /// file modification time in unix timestamp. Required.
        /// </summary>
        [DataMember(Name = "ts")]
        public long UnixTimeStamp { get; protected set; } 

        /// <summary>
        ///  file size in bytes
        /// </summary>
        [DataMember(Name = "size")]
        public long Size { get; protected set; } 

        /// <summary>
        ///  is readable
        /// </summary>
        [DataMember(Name = "read")]
        public byte Read { get; protected set; }

        /// <summary>
        /// is writable
        /// </summary>
        [DataMember(Name = "write")]
        public byte Write { get; protected set; }

        /// <summary>
        ///  is file locked. If locked that object cannot be deleted and renamed
        /// </summary>
        [DataMember(Name = "locked")]
        public byte Locked { get; protected set; }

        public static DTOBase Create(IFileInfo info, Root root)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            if (root == null)
                throw new ArgumentNullException("root");

            var directory = FolderManager.Instance.GetFolder(info.FolderId);

            bool canWrite = DnnFileSystemDriver.HasWritePermissions(directory);

            FileDTO response;
            if (root.CanCreateThumbnail(info))
            {
                ImageDTO imageResponse = new ImageDTO();
                imageResponse.Thumbnail = root.GetExistingThumbHash(info) ?? (object)1;
                imageResponse.Dimension = string.Format("{0}x{1}", info.Width, info.Height);
                response = imageResponse;
            }
            else
            {
                response = new FileDTO();
            }
            response.Read = 1;
            response.Write = canWrite ? (byte)1 : (byte)0;
            response.Locked = (!canWrite || root.IsLocked) ? (byte)1 : (byte)0;
            response.Name = info.FileName;
            response.Size = info.Size;
            response.UnixTimeStamp = (long)(info.LastModificationTime - _unixOrigin).TotalSeconds;
            response.Mime = Helper.GetMimeType(info.Extension);
            response.Hash = "F_" + Helper.EncodeId(info.FileId);
            response.ParentHash = "D_" + Helper.EncodeId(info.FolderId);

            return response;
        }

        public static DTOBase Create(IFolderInfo directory, Root root)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");

            if (root == null)
                throw new ArgumentNullException("root");

            bool canWrite = DnnFileSystemDriver.HasWritePermissions(directory);

            bool hasSubdirs = false;
            var subdirs = FolderManager.Instance.GetFolders(directory);
            foreach (var item in subdirs)
            {
                if (DnnFileSystemDriver.HasReadPermissions(item))
                {
                    hasSubdirs = true;
                    break;
                }
            }

            if (root.Directory.FolderID == directory.FolderID)
            {
                RootDTO response = new RootDTO()
                {
                    Mime = "directory",
                    Dirs = hasSubdirs ? (byte)1 : (byte)0,
                    Hash = "D_" +  Helper.EncodeId(directory.FolderID),
                    Read = 1,
                    Write = canWrite ? (byte)1 : (byte)0,
                    Locked = (!canWrite || root.IsLocked) ? (byte)1 : (byte)0,
                    Name = root.Alias,
                    Size = 0,
                    UnixTimeStamp =  (long)(directory.LastUpdated - _unixOrigin).TotalSeconds,
                    VolumeId = root.VolumeId                    
                };
                return response;
            }
            else
            {
                DirectoryDTO response = new DirectoryDTO()
                {
                    Mime = "directory",
                    ContainsChildDirs = hasSubdirs ? (byte)1 : (byte)0,
                    Hash = "D_" + Helper.EncodeId(directory.FolderID),
                    Read = 1,
                    Write = canWrite ? (byte)1 : (byte)0,
                    Locked = (!canWrite || root.IsLocked) ? (byte)1 : (byte)0,
                    Size = 0,
                    Name = directory.FolderName,
                    UnixTimeStamp = (long)(directory.LastUpdated - _unixOrigin).TotalSeconds,
                    ParentHash = "D_" + Helper.EncodeId(directory.ParentID),
                };
                return response;
            }
        }
       
    }
}