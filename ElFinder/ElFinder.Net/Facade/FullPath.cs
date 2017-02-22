using System.IO;
using System;
using DotNetNuke.Services.FileSystem;

namespace ElFinder
{
    public class FullPath
    {
        public Root Root
        {
            get { return _root; }
        }
        public bool IsDirectoty
        {
            get { return Directory != null; }
        }
        public string RelativePath
        {
            get
            {
                return _relativePath;
            }
        }
        public IFolderInfo Directory { get; set; }

        public IFileInfo File { get; set; }

        public FullPath(Root root, IFolderInfo folder)
        {
            if (root == null)
                throw new ArgumentNullException("root", "Root can not be null");
            Directory = folder;
            _root = root;
        }

        public FullPath(Root root, IFileInfo file)
        {
            if (root == null)
                throw new ArgumentNullException("root", "Root can not be null");
            File = file;
            _root = root;
        }


        private Root _root;
        private bool _isDirectory;
        private string _relativePath = string.Empty;       
    }
}