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
        private static IAmazonS3 s3Client;

        private static IFolder folder = FileSystem.Current.LocalStorage;
        private string zipFileName = "result.zip";

        public AbsoluteLayoutExample()
        {
            Button btnGetPath = new Button
            {
                Text = "Get Path",

            };
            btnGetPath.Clicked += BtnGetPath_ClickedAsync;

            Button btnUpload = new Button
            {
                Text = "Upload",

            };
            btnUpload.Clicked += BtnUpload_ClickedAsync;

            Button btnChangePermission = new Button
            {
                Text = "Change Permission"
            };
            btnChangePermission.Clicked += BtnChangePermission_ClickedAsync;

            Button btnZipFiles = new Button
            {
                Text = "Zip Files"
            };
            btnZipFiles.Clicked += BtnZipFiles_Clicked;

            AbsoluteLayout absLayout = new AbsoluteLayout();
            absLayout.Children.Add(btnGetPath, new Point(20, 20));
            absLayout.Children.Add(btnZipFiles, new Point(120, 20));
            absLayout.Children.Add(btnUpload, new Point(220, 20));
            absLayout.Children.Add(btnChangePermission, new Point(320, 20));

            Content = absLayout;
        }

        private void BtnGetPath_ClickedAsync(object sender, EventArgs e)
        {
            Debug.WriteLine("Please paste your test files here: " + folder.Path);
        }

        private void BtnZipFiles_Clicked(object sender, EventArgs e)
        {
            try
            {
                string startPath = folder.Path + @"\start";
                string zipPath = folder.Path + @"\" + zipFileName;

                // LocalStorage Path - C:\Users\GIGABYTE\AppData\Local\Packages\a6e37ba2-4e37-4bd6-b5b6-16ae0b1e4f54_tbz3402trp7yy\LocalState
                Debug.WriteLine("Start Zip files in: " + startPath);

                // zip directory
                //ZipFile.CreateFromDirectory(startPath, zipPath);

                string filepath = folder.Path + @"\uploadFileList.json";
                List<string> filenames = new List<string>();

                using (StreamReader r = new StreamReader(filepath))
                {
                    var json = r.ReadToEnd();
                    var jobj = JObject.Parse(json);
                    
                    Debug.WriteLine(jobj.ToString());

                    foreach (var item in jobj.Properties())
                    {
                        filenames.Add(startPath + @"\" + item.Value);
                    }

                    // zip files in json
                    //using (ZipArchive zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                    //{
                    //    foreach (var file in filenames)
                    //    {
                    //        var fileInfo = new FileInfo(file);
                    //        zipArchive.CreateEntryFromFile(fileInfo.FullName, fileInfo.Name);
                    //    }
                    //}
                    AddFilesToZip(zipPath, filenames.ToArray());
                }
                
                Debug.WriteLine("Zip files complete!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception occur when zip files: '{0}'", ex.Message);
            }
        }

        public static void AddFilesToZip(string zipPath, string[] files)
        {
            if (files == null || files.Length == 0)
            {
                return;
            }

            using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    zipArchive.CreateEntryFromFile(fileInfo.FullName, fileInfo.Name);
                }
            }
        }

        private async void BtnUpload_ClickedAsync(object sender, System.EventArgs e)
        {
            try
            {
                bool isFileExist = await PCLHelper.IsFileExistAsync(zipFileName, folder);
                Debug.WriteLine("File Exist: " + isFileExist);

                s3Client = new AmazonS3Client("AKIAJCWY6VJ2VFOZ6JFQ", "Y7kBzSAGTY1mQB+9XthKg3t1mRgwoh3rzDJLG6yv", bucketRegion);

                TransferUtility fileTransferUtility = new TransferUtility(s3Client);
                uploadFilePath = folder.Path + @"\" + zipFileName;
                Debug.WriteLine("upload file: " + uploadFilePath);

                var fileToUpload =
                    new System.IO.FileStream(uploadFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                await fileTransferUtility.UploadAsync(fileToUpload, bucketName, keyName);

                #region "other upload method" // work in .net but not work in xamarin
                //using (var fileToUpload =
                //    new System.IO.FileStream(folder.Path + "\\" + "test_upload3.txt", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                //{
                //    await fileTransferUtility.UploadAsync(fileToUpload,
                //                               bucketName, keyName);
                //}

                //await s3Client.PutObjectAsync(new Amazon.S3.Model.PutObjectRequest()
                //{
                //    BucketName = bucketName,
                //    FilePath = folder.Path + "\\" + "test_upload3.txt",
                //    Key = keyName
                //});
                #endregion

                // await fileTransferUtility.UploadAsync(folder.Path + "\\" + "test_upload4.txt", "sradar.test1");
                Debug.WriteLine("Upload file complete");
            }
            catch (AmazonS3Exception s3Exception)
            {
                Debug.WriteLine(s3Exception.StackTrace);
            }

        }

        private async void BtnChangePermission_ClickedAsync(object sender, System.EventArgs e)
        {
            try
            {
                s3Client = new AmazonS3Client("AKIAJCWY6VJ2VFOZ6JFQ", "Y7kBzSAGTY1mQB+9XthKg3t1mRgwoh3rzDJLG6yv", bucketRegion);
                PutACLRequest request = new PutACLRequest()
                {
                    CannedACL = S3CannedACL.PublicReadWrite,
                    BucketName = bucketName,
                    Key = keyName
                };
                PutACLResponse response1 = await s3Client.PutACLAsync(request);

                Debug.WriteLine("Change Permission Complete");
            }
            catch (AmazonS3Exception ex)
            {
                Debug.WriteLine("Exception occur when Change Permission: '{0}'", ex.Message);
            }
        }
    }
}
