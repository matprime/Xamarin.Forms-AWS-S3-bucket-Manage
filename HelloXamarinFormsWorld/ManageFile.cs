using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace XamarinFile
{
    class ManageFile
    {
        public static string[] ReadJSONAndCreateFileList(string jsonPath, string startPath)
        {
            List<string> filenames = new List<string>();

            using (StreamReader r = new StreamReader(jsonPath))
            {
                var json = r.ReadToEnd();
                var jobj = JObject.Parse(json);

                Debug.WriteLine("JSON content: /n" + jobj.ToString());

                foreach (var item in jobj.Properties())
                {
                    filenames.Add(startPath + @"\" + item.Value);
                }

                Debug.WriteLine("Start Zip files in: " + startPath);
            }

            return filenames.ToArray();
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
    }
}
