using Xamarin.Forms;

using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System;

using PCLStorage;
using System.Threading.Tasks;
using Amazon.S3.Model;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using XamarinAWS;
using XamarinFile;

namespace HelloXamarinFormsWorld
{
#region "PCLHelper"
    public static class PCLHelper
    {

        public async static Task<bool> IsFileExistAsync(this string fileName, IFolder rootFolder = null)
        {
            // get hold of the file system  
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            ExistenceCheckResult folderexist = await folder.CheckExistsAsync(fileName);
            // already run at least once, don't overwrite what's there  
            if (folderexist == ExistenceCheckResult.FileExists)
            {
                return true;

            }
            return false;
        }

        public async static Task<bool> IsFolderExistAsync(this string folderName, IFolder rootFolder = null)
        {
            // get hold of the file system  
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            ExistenceCheckResult folderexist = await folder.CheckExistsAsync(folderName);
            // already run at least once, don't overwrite what's there  
            if (folderexist == ExistenceCheckResult.FolderExists)
            {
                return true;

            }
            return false;
        }

        public async static Task<IFolder> CreateFolder(this string folderName, IFolder rootFolder = null)
        {
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            folder = await folder.CreateFolderAsync(folderName, CreationCollisionOption.ReplaceExisting);
            return folder;
        }

        public async static Task<IFile> CreateFile(this string filename, IFolder rootFolder = null)
        {
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            IFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            return file;
        }
        public async static Task<bool> WriteTextAllAsync(this string filename, string content = "", IFolder rootFolder = null)
        {
            IFile file = await filename.CreateFile(rootFolder);
            await file.WriteAllTextAsync(content);
            return true;
        }

        public async static Task<string> ReadAllTextAsync(this string fileName, IFolder rootFolder = null)
        {
            string content = "";
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            bool exist = await fileName.IsFileExistAsync(folder);
            if (exist == true)
            {
                IFile file = await folder.GetFileAsync(fileName);
                content = await file.ReadAllTextAsync();
            }
            return content;
        }
        public async static Task<bool> DeleteFile(this string fileName, IFolder rootFolder = null)
        {
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;
            bool exist = await fileName.IsFileExistAsync(folder);
            if (exist == true)
            {
                IFile file = await folder.GetFileAsync(fileName);
                await file.DeleteAsync();
                return true;
            }
            return false;
        }
        public async static Task SaveImage(this byte[] image, String fileName, IFolder rootFolder = null)
        {
            // get hold of the file system  
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;

            // create a file, overwriting any existing file  
            IFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            // populate the file with image data  
            using (System.IO.Stream stream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
            {
                stream.Write(image, 0, image.Length);
            }
        }

        public async static Task<byte[]> LoadImage(this byte[] image, String fileName, IFolder rootFolder = null)
        {
            // get hold of the file system  
            IFolder folder = rootFolder ?? FileSystem.Current.LocalStorage;

            //open file if exists  
            IFile file = await folder.GetFileAsync(fileName);
            //load stream to buffer  
            using (System.IO.Stream stream = await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite))
            {
                long length = stream.Length;
                byte[] streamBuffer = new byte[length];
                stream.Read(streamBuffer, 0, (int)length);
                return streamBuffer;
            }

        }
    }
#endregion
    public class AbsoluteLayoutExample : ContentPage
    {
        private const string bucketName = "sradar.test1";
        private string keyName = "result.zip";
        private string uploadFilePath = "";
        // Specify your bucket region (an example region is shown).
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USEast1;
        
        private static IFolder folder = FileSystem.Current.LocalStorage;
        private string zipFileName = "result.zip";

        public AbsoluteLayoutExample()
        {
            Button btnGetPath = new Button
            {
                Text = "Get Path",

            };
            btnGetPath.Clicked += BtnGetPath_Clicked;

            Button btnUpload = new Button
            {
                Text = "Upload",

            };
            btnUpload.Clicked += BtnUpload_Clicked;

            Button btnChangePermission = new Button
            {
                Text = "Change Permission"
            };
            btnChangePermission.Clicked += BtnChangePermission_Clicked;

            Button btnZipFiles = new Button
            {
                Text = "Zip Files"
            };
            btnZipFiles.Clicked += BtnZipFiles_Clicked;

            AbsoluteLayout absLayout = new AbsoluteLayout();
            absLayout.Children.Add(btnGetPath, new Point(20, 20));
            absLayout.Children.Add(btnZipFiles, new Point(20, 80));
            absLayout.Children.Add(btnUpload, new Point(20, 140));
            absLayout.Children.Add(btnChangePermission, new Point(20, 200));

            Content = absLayout;
        }

#region "Button Events"
        private void BtnGetPath_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Please paste your test files here: " + folder.Path);
            Debug.WriteLine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal));
        }

        private void BtnZipFiles_Clicked(object sender, EventArgs e)
        {
            try
            {
                string zipPath = folder.Path + @"\" + zipFileName;

                // LocalStorage Path
                // Win - C:\Users\GIGABYTE\AppData\Local\Packages\a6e37ba2-4e37-4bd6-b5b6-16ae0b1e4f54_tbz3402trp7yy\LocalState
                // Android - /data/user/0/HelloXamarinFormsWorld.Android/files
                
                // zip directory
                //ZipFile.CreateFromDirectory(startPath, zipPath);

                string jsonpath = folder.Path + @"\" + Global.JSON_FILE_NAME;

                ManageFile.AddFilesToZip(zipPath, ManageFile.ReadJSONAndCreateFileList(jsonpath, folder.Path + @"\start"));

                Debug.WriteLine("Zip files complete!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception occur when zip files: '{0}'", ex.Message);
            }
        }

        private void BtnUpload_Clicked(object sender, System.EventArgs e)
        {
            uploadFilePath = folder.Path + @"\" + zipFileName;
            ManageAWS.UploadFileToAWS("AKIAJCWY6VJ2VFOZ6JFQ", "Y7kBzSAGTY1mQB+9XthKg3t1mRgwoh3rzDJLG6yv", bucketRegion, bucketName, uploadFilePath, keyName);
        }

        private void BtnChangePermission_Clicked(object sender, System.EventArgs e)
        {
            ManageAWS.ChangeFilePermission("AKIAJCWY6VJ2VFOZ6JFQ", "Y7kBzSAGTY1mQB+9XthKg3t1mRgwoh3rzDJLG6yv", bucketRegion, bucketName, keyName, S3CannedACL.PublicReadWrite);
        }
#endregion
    }
}
