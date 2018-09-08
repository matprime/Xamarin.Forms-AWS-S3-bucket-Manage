using Foundation;
using UIKit;
using Newtonsoft.Json.Linq;
using PCLStorage;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace HelloXamarinFormsWorld.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        UIWindow window;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Forms.Init();

			LoadApplication (new App ());

            makeTestFiles();

			return base.FinishedLaunching (app, options);
        }

        void makeTestFiles()
        {
            IFolder folder = FileSystem.Current.LocalStorage;

            // create "start" folder
            (new FileInfo(folder.Path + "/start/")).Directory.Create();

            // make json
            FileStream fs = null;
            string jsonPath = folder.Path + @"\" + Global.JSON_FILE_NAME;
            fs = File.Open(jsonPath, FileMode.OpenOrCreate);

            StreamWriter sw = new StreamWriter(fs);
            sw.Write("{" +
              "\"fileA\": \"fileA.txt\"," +
              "\"fileB\": \"fileB.txt\"," +
              "\"fileC\": \"fileC.txt\"," +
              "\"fileD\": \"fileD.txt\"" +
            "}");
            sw.Close();

            // make txts in folder
            string startPath = folder.Path + @"\start";

            using (StreamReader r = new StreamReader(jsonPath))
            {
                var json = r.ReadToEnd();
                var jobj = JObject.Parse(json);

                System.Diagnostics.Debug.WriteLine(jobj.ToString());

                foreach (var item in jobj.Properties())
                {
                    string filePath = startPath + @"\" + item.Value;
                    fs = File.Open(filePath, FileMode.OpenOrCreate);

                    sw = new StreamWriter(fs);
                    sw.Write("AAAAAAAAA");
                    sw.Close();
                }

                r.Close();
            }
        }
    }
}
