using Android.App;
using Android.OS;
using Newtonsoft.Json.Linq;
using PCLStorage;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace HelloXamarinFormsWorld.Droid
{
    [Activity(Label = "HelloXamarinFormsWorld", Theme = "@style/MainTheme", MainLauncher = true)]
	public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Forms.Init(this, bundle);

			LoadApplication (new App ());

            makeTestFiles();
        }

        void makeTestFiles()
        {
            IFolder folder = FileSystem.Current.LocalStorage;

            // create "start" folder
            var dir = new Java.IO.File(folder.Path + "/start/");
            if (!dir.Exists())
                dir.Mkdirs();

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
