using RMA_Docker.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;

namespace RMA_Docker.Models {

    public enum FileType {
        Folder, FolderWithDoc, File, Zip, Exe, Music, Video, Xml, Picture, Dll, Config, FixedRoot,
        NetworkRoot, RemovableRoot, DiscRoot, SysRoot, Computer
    }

    public class Category {
        FileType value;
        public String icon = "";
        public Category(FileType type) { value = type; IconInitialize(type); }
        public FileType Value { get { return value; } }
        public String Icon { get { return icon; } }
        public override string ToString() {
            switch (value) {
                case FileType.Folder: return "Folder";
                case FileType.FolderWithDoc: return "FolderWithDoc";
                case FileType.File: return "File";
                case FileType.Zip: return "Zip";
                case FileType.Config: return "Config";
                case FileType.Dll: return "Dll";
                case FileType.Exe: return "Exe";
                case FileType.Music: return "Music";
                case FileType.Picture: return "Picture";
                case FileType.Video: return "Video";
                case FileType.Xml: return "Xml";
                case FileType.FixedRoot: return "FixedRoot";
                case FileType.SysRoot: return "SysRoot";
                case FileType.NetworkRoot: return "NetworkRoot";
                case FileType.DiscRoot: return "DiscRoot";
                case FileType.RemovableRoot: return "RemovableRoot";
                case FileType.Computer: return "Computer";
                default: return "File";
            }
        }

        public void IconInitialize(FileType type) {
            switch (type) {
                case FileType.Folder: icon = "demo-psi-folder text-info"; break;
                case FileType.FolderWithDoc: icon = "demo-psi-folder-with-document text-info"; break;
                case FileType.File: icon = "demo-pli-file text-muted"; break;
                case FileType.Zip: icon = "demo-psi-folder-zip text-success"; break;
                case FileType.Config: icon = "demo-pli-file text-muted"; break;
                case FileType.Dll: icon = "demo-pli-window-2 text-muted"; break;
                case FileType.Exe: icon = "demo-pli-window-2 text-muted"; break;
                case FileType.Music: icon = "demo-pli-file-music text-muted"; break;
                case FileType.Picture: icon = "demo-pli-file-pictures text-info"; break;
                case FileType.Video: icon = "demo-pli-video text-muted"; break;
                case FileType.Xml: icon = "demo-pli-file text-muted"; break;
                case FileType.FixedRoot: icon = "demo-pli-home text-muted"; break;
                case FileType.SysRoot: icon = "demo-pli-home text-muted"; break;
                case FileType.NetworkRoot: icon = "demo-pli-home text-muted"; break;
                case FileType.DiscRoot: icon = "demo-pli-home text-muted"; break;
                case FileType.RemovableRoot: icon = "demo-pli-home text-muted"; break;
                case FileType.Computer: icon = "demo-psi-laptop text-muted"; break;
                default: icon = "demo-psi-file text-muted"; break;
            }
        }
    }

    public class FileModel {
        public String Description { get; set; }
        public String VirtualPath { get; set; }
        public String NiceNameOrAreaName { get; set; }
        public DateTime UploadedDT { get; set; }
        public String parentVirtualPath { get; set; }

        public String Extension { get; set; }
        public String Name { get; set; }
        public String Location { get; set; }
        public String FullPath { get; set; }

        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime? Accessed { get; set; }
        public Category Category { get; set; }
        public bool IsFolder { get; private set; } = false;
        public bool IsRootDir { get; set; } = false;
        public bool IsCommonFolder { get; set; } = false;
        public bool IsUserFolder { get; set; } = false;
        public bool IsSearchResult { get; set; } = false;
        public bool AllowToOCR { get; set; } = false;
        public String RootPath { get; set; }
        public IList<FileModel> CurrentFolderFiles { get; set; }
        public IList<FileModel> CommonFolderFiles { get; set; }
        public IList<String> Tags { get; set; }

        public bool IsOCR { get; set; }
        public String OcrDocType { get; set; }

        public FileModel() { }

        public FileModel(String virtualPath, String userPath) {
            Name = virtualPath;
            FullPath = Encode("\\");
            Category = new Category(FileType.DiscRoot);
            VirtualPath = virtualPath;
            String physicalPath = UtilityOperations.GetServerMapPath(virtualPath);
            CurrentFolderFiles = GetFoldersAndFiles(physicalPath);
            IsFolder = true;
            if (String.Equals(virtualPath, UtilityOperations.GetDockerCommonFolderPath())) { IsCommonFolder = true; }
            if (String.Equals(virtualPath, UtilityOperations.GetDockerRootPath() + userPath)) {
                IsRootDir = true;
                parentVirtualPath = virtualPath;
            } else {
                IsRootDir = false;
                parentVirtualPath = UtilityOperations.GetVirtualPath(Directory.GetParent(physicalPath).FullName);
            }
        }

        public FileModel(FileInfo fi) {
            Name = fi.Name;
            Modified = fi.LastWriteTime;
            Created = fi.CreationTime;
            Accessed = fi.LastAccessTime;
            Extension = fi.Extension.ToLower();
            Location = fi.DirectoryName;
            FullPath = Encode(fi.FullName);
            IsFolder = false;
            IsRootDir = false;
            switch (Extension) {
                case ".exe": Category = new Category(FileType.Exe); break;
                case ".config": Category = new Category(FileType.Config); break;
                case ".dll": Category = new Category(FileType.Dll); break;
                case ".zip": Category = new Category(FileType.Zip); break;
                case ".xml": Category = new Category(FileType.Xml); break;
                case ".mp3": Category = new Category(FileType.Music); break;
                case ".wmv": Category = new Category(FileType.Video); break;
                case ".bmp":
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".gif":
                case ".cur":
                case ".jp2":
                case ".ami":
                case ".ico": Category = new Category(FileType.Picture); break;
                default: Category = new Category(FileType.File); break;
            }
        }

        public FileModel(DirectoryInfo di) {
            Name = di.Name;
            FullPath = Encode(di.FullName);
            Location = di.Parent != null ? di.Parent.FullName : "";
            Modified = di.LastWriteTime;
            Created = di.CreationTime;
            Accessed = di.LastAccessTime;
            Category = new Category(FileType.Folder);
            if (di.EnumerateFiles().Count() > 0 || di.EnumerateDirectories().Count() > 0 ) {
                Category = new Category(FileType.FolderWithDoc);
            }
            IsFolder = true;
            IsRootDir = false;
        }

        public static string Encode(string filepath) { return filepath.Replace("\\", "/"); }
        public static string Decode(string filepath) { return filepath.Replace("/", "\\"); }

        private static IList<FileModel> GetRootDirectories() {
            List<FileModel> result = new List<FileModel>();
            DriveInfo[] drives = DriveInfo.GetDrives();
            string winPath = Environment.GetEnvironmentVariable("windir");
            string winRoot = Path.GetPathRoot(winPath);
            foreach (DriveInfo di in drives) {
                if (!di.IsReady) continue;
                if (di.RootDirectory == null) continue;
                if (di.RootDirectory.FullName == winRoot) {
                    result.Add(new FileModel(di.RootDirectory) {
                        Category = new Category(FileType.SysRoot),
                        Accessed = null,
                        Created = null,
                        Modified = null
                    });
                    continue;
                }
                switch (di.DriveType) {
                    case DriveType.CDRom: result.Add(new FileModel(di.RootDirectory) { Category = new Category(FileType.DiscRoot), Accessed = null, Created = null, Modified = null }); break;
                    case DriveType.Fixed: result.Add(new FileModel(di.RootDirectory) { Category = new Category(FileType.FixedRoot), Accessed = null, Created = null, Modified = null }); break;
                    case DriveType.Network: result.Add(new FileModel(di.RootDirectory) { Category = new Category(FileType.NetworkRoot), Accessed = null, Created = null, Modified = null }); break;
                    case DriveType.Removable: result.Add(new FileModel(di.RootDirectory) { Category = new Category(FileType.RemovableRoot), Accessed = null, Created = null, Modified = null }); break;
                    default: result.Add(new FileModel(di.RootDirectory) { Accessed = null, Created = null, Modified = null }); break;
                }
            }
            return result;
        }

        public static IList<FileModel> GetFoldersAndFiles(string path) {
            List<FileModel> resultFile = new List<FileModel>();
            List<FileModel> resultFolder = new List<FileModel>();
            if (string.IsNullOrEmpty(path)) { return GetRootDirectories(); }
            else { path = Decode(path); }
            DocumentsOperations docOps = new DocumentsOperations();
            try {
                foreach (string file in Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)) {
                    FileInfo fi = new FileInfo(file);
                    FileModel newFileModel = new FileModel(fi);
                    var dbFile = docOps.GetFileByVirtualPath(UtilityOperations.GetVirtualPath(file));
                    if (dbFile != null) {
                        newFileModel.Description = dbFile.Description;
                        newFileModel.NiceNameOrAreaName = dbFile.NiceNameOrAreaName;
                        newFileModel.UploadedDT = dbFile.DateTimeUploaded;
                        newFileModel.VirtualPath = dbFile.VirtualPath;
                        var fileOCR = docOps.GetFileBeenOCR(dbFile.ID);
                        if (fileOCR != null) {
                            newFileModel.IsOCR = true;
                            newFileModel.AllowToOCR = false;
                            newFileModel.OcrDocType = fileOCR.DocumentType;
                        } else {
                            newFileModel.IsOCR = false;
                            newFileModel.AllowToOCR = false;
                            if (docOps.IsFileBeenOCRByFileName(dbFile.Name.Replace(".pdf", "_ocr.pdf"))) { newFileModel.AllowToOCR = false; } 
                            else { 
                                newFileModel.AllowToOCR = ((new DCEOperations()).GetFileExtensionsAllowToOCR()
                                    .Select(fileExt => fileExt.FileExtension.ToLower()))
                                    .Contains(newFileModel.Extension.ToLower()); }                
                            newFileModel.OcrDocType = "";
                        }
                    }
                    resultFile.Add(newFileModel);
                }
                foreach (string dir in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly)) {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    FileModel newFileModel = new FileModel(di);
                    String getVirtualPath = UtilityOperations.GetVirtualPath(dir);
                    RMA_Docker.Models.File dbFile = docOps.GetFileByVirtualPath(getVirtualPath);
                    if (dbFile != null) {
                        newFileModel.Description = dbFile.Description;
                        newFileModel.NiceNameOrAreaName = dbFile.NiceNameOrAreaName;
                        newFileModel.UploadedDT = dbFile.DateTimeUploaded;
                        newFileModel.VirtualPath = dbFile.VirtualPath;
                    }
                    resultFolder.Add(newFileModel);
                }
                resultFile.Sort((a, b) => { var name1 = a.Name; var name2 = b.Name;
                    if (a.Category.Value == FileType.Folder) { name1 = " " + name1; }
                    if (b.Category.Value == FileType.Folder) { name2 = " " + name2; }
                    return name1.CompareTo(name2);
                });
                resultFolder.AddRange(resultFile);
            } catch (Exception) { }
            return resultFolder;
        }

        public FileModel GetFolderOrFile(String virtualPath) {
            String physicalPath = UtilityOperations.GetServerMapPath(virtualPath);
            if (!string.IsNullOrEmpty(physicalPath)) { physicalPath = Decode(physicalPath); }
            DocumentsOperations docOps = new DocumentsOperations();
            FileModel fileModel = null;
            try {
                FileAttributes attr = System.IO.File.GetAttributes(physicalPath);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                    DirectoryInfo di = new DirectoryInfo(physicalPath);
                    fileModel = new FileModel(di);
                    RMA_Docker.Models.File dbFile = docOps.GetFileByVirtualPath(UtilityOperations.GetVirtualPath(physicalPath));
                    if (dbFile != null) {
                        fileModel.Description = dbFile.Description;
                        fileModel.NiceNameOrAreaName = dbFile.NiceNameOrAreaName;
                        fileModel.UploadedDT = dbFile.DateTimeUploaded;
                        fileModel.VirtualPath = dbFile.VirtualPath; }
                } else {
                    FileInfo fi = new FileInfo(physicalPath);
                    fileModel = new FileModel(fi);
                    var dbFile = docOps.GetFileByVirtualPath(UtilityOperations.GetVirtualPath(physicalPath));
                    if (dbFile != null) {
                        fileModel.Description = dbFile.Description;
                        fileModel.NiceNameOrAreaName = dbFile.NiceNameOrAreaName;
                        fileModel.UploadedDT = dbFile.DateTimeUploaded;
                        fileModel.VirtualPath = dbFile.VirtualPath;
                        var fileOCR = docOps.GetFileBeenOCR(dbFile.ID);
                        if (fileOCR != null) {
                            fileModel.IsOCR = true;
                            fileModel.AllowToOCR = false;
                            fileModel.OcrDocType = fileOCR.DocumentType;
                        } else {
                            fileModel.IsOCR = false;
                            fileModel.AllowToOCR = false;
                            if (docOps.IsFileBeenOCRByFileName(dbFile.Name.Replace(".pdf", "_ocr.pdf"))) { fileModel.AllowToOCR = false; }
                            else {
                                fileModel.AllowToOCR = ((new DCEOperations()).GetFileExtensionsAllowToOCR()
                                        .Select(fileExt => fileExt.FileExtension.ToLower()))
                                        .Contains(fileModel.Extension.ToLower()); }
                            fileModel.OcrDocType = "";
                        }
                    }
                }
            } catch (Exception) { }
            fileModel.parentVirtualPath = virtualPath;
            return fileModel;
        }
    }

    public class AddNewFolder {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} ~ 100 characters long.", MinimumLength = 1)]
        [Display(Name = "Folder Name")]
        public String FolderName { get; set; }
    }

    public class TagsMetadata {
        public String TagName { get; set; }
        public String LastModified { get; set; }
    }
}