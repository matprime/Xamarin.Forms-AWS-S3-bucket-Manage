using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using System.Diagnostics;
using System.IO;

namespace XamarinAWS
{
    public static class ManageAWS
    {
        public static async void UploadFileToAWS(string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint region, string bucketName, string uploadFilePath, string keyName)
        {
            try
            {
                bool isFileExist = File.Exists(uploadFilePath);

                if (!isFileExist) {
                    Debug.WriteLine("Error! File Not Exist: " + uploadFilePath);
                    return;
                }

                IAmazonS3 s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, region);
                
                TransferUtility fileTransferUtility = new TransferUtility(s3Client);

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

        public static async void ChangeFilePermission(string awsAccessKeyId, string awsSecretAccessKey, RegionEndpoint region, string bucketName, string keyName, S3CannedACL permission)
        {
            try
            {
                IAmazonS3 s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, region);
                PutACLRequest request = new PutACLRequest()
                {
                    CannedACL = permission,
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
