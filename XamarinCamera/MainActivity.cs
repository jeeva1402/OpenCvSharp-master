using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android;
using Plugin.Media;
using Android.Graphics;
using OpenCvSharp;
//using Emgu.CV;
//using Emgu.CV.Structure;
//using Emgu.CV.CvEnum;
using System.Collections;
using Android.Graphics.Drawables;
using System.Net.Sockets;
using System.Net;
using System.Drawing;
using Environment = Android.OS.Environment;
using Bitmap = Android.Graphics.Bitmap;
using System.IO;
using System;
using Org.Apache.Http.Util;
using System.Linq;
using Orientation = Android.Widget.Orientation;
using Plugin.Media.Abstractions;
using Java.Lang;
using Rect = OpenCvSharp.Rect;
using Exception = System.Exception;
using Size = OpenCvSharp.Size;
using Android.Media;
using Color = Android.Graphics.Color;

namespace XamarinCamera
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button captureButton;
        Button uploadButton;
        ImageView thisImageView;
        LinearLayout linearLayout;
        //Button btnConnect;
        CascadeClassifier cascadeClasifier_;
        string path;
        //TcpClient client_;
        //NetworkStream stream_;
        Bitmap bitmap_;
        HorizontalScrollView DetectedscrollView_;
        HorizontalScrollView resultscrollView_;
        TextView txtVwStatus_;

        EditText classID;
        //EditText IPAddress;
        clsJDFirebase firebase;
        readonly string[] permissionGroup =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera,
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            captureButton = (Button)FindViewById(Resource.Id.captureButton);
            uploadButton = (Button)FindViewById(Resource.Id.uploadButton);
            thisImageView = (ImageView)FindViewById(Resource.Id.thisImageView);

            //btnConnect = (Button)FindViewById(Resource.Id.ConnectServer);
            linearLayout = (LinearLayout)FindViewById(Resource.Id.detectedImages);

            //detectedImage1 = (ImageView)FindViewById(Resource.Id.detectedImages1);
            //txtvw1 = (TextView)FindViewById(Resource.Id.detectedId1);
            DetectedscrollView_ = (HorizontalScrollView)FindViewById(Resource.Id.detectedImagesScrollView);
            resultscrollView_ = (HorizontalScrollView)FindViewById(Resource.Id.ResultImagesScrollView);
            classID = (EditText)FindViewById(Resource.Id.classID);
            txtVwStatus_ = (TextView)FindViewById(Resource.Id.TextViewStatus);
            //IPAddress = (EditText)FindViewById(Resource.Id.IPAddress);
            captureButton.Click += CaptureButton_Click;
            uploadButton.Click += UploadButton_Click;
            //btnConnect.Click += BtnConnect_Click;
            RequestPermissions(permissionGroup, 0);

            path = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryDocuments).AbsolutePath;
            string filePath = System.IO.Path.Combine(path, "haarcascade_frontalface_default.xml");
            //Emgu.CV.CvInvoke.Init();
            this.cascadeClasifier_ = new CascadeClassifier(filePath);
            firebase = new clsJDFirebase();
            if (this.firebase.IsConnected)
            {
                this.txtVwStatus_.Text = "Connected";
            }
            else
            {
                this.txtVwStatus_.Text = "Unable to Connect check network";
            }
        }

        //private void BtnConnect_Click(object sender, System.EventArgs e)
        //{
        //    //string ipAddressString = "192.168.0.105";
        //    string ipAddressString = this.IPAddress.Text;
        //    IPAddress ipAddress;

        //    ipAddress = System.Net.IPAddress.Parse(ipAddressString);
        //    int port = 4444;
        //    this.client_ = new TcpClient();
        //    //while (!this.client_.Connected)
        //    //{
        //    try
        //    {
        //        this.client_.Connect(ipAddress, port);
        //        if (this.client_.Connected)
        //        {
        //            this.stream_ = this.client_.GetStream();
        //            this.btnConnect.Text = "Connected";
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }

        //    //}         

        //}

        private void UploadButton_Click(object sender, System.EventArgs e)
        {
            UploadPhoto();
        }

        private void CaptureButton_Click(object sender, System.EventArgs e)
        {
            TakePhoto();
        }

        async void TakePhoto()
        {
            await CrossMedia.Current.Initialize();

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 40,
                Name = "myimage.jpg",
                Directory = "sample"

            });

            if (file == null)
            {
                return;
            }

            // Convert file to byte array and set the resulting bitmap to imageview
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            //this.ConvertToBitmap(imageArray);

            var bm = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);

            thisImageView.SetImageBitmap(bm);

            this.PrepareImages(imageArray);

            //this.DetectImages(imageArray);

        }

        private void PrepareImages(byte[] imageArray)
        {
            this.txtVwStatus_.Text = "Detecting...";
            Mat MainImage = Cv2.ImDecode(imageArray, ImreadModes.Color);
            var faces = this.cascadeClasifier_.DetectMultiScale(MainImage);
            clsImagePackager package = new clsImagePackager();
            package.ClassID = Convert.ToInt32(this.classID.Text);
            LinearLayout llayout = new LinearLayout(this);
            llayout.Orientation = Orientation.Horizontal;
            int totalFaces = 0;

            foreach (Rect item in faces)
            {

                //if (item.Width > 200 && item.Height > 200)
                //{
                    totalFaces++;

                    Mat Faceimage = new Mat(MainImage, item);
                    byte[] imageData = null;
                    bool isSucess = Cv2.ImEncode(".jpeg", Faceimage, out imageData);
                    package.ImageSizeList.Add(imageData.Length);
                    package.BitmapList.Add(imageData);


                    ImageView imgView = new ImageView(this);
                    imgView.SetMaxWidth(10);
                    imgView.SetMaxHeight(10);
                    this.bitmap_ = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
                    imgView.SetImageBitmap(this.bitmap_);
                    llayout.AddView(imgView);
                //}
            }
            package.ImageCount = totalFaces;
            this.DetectedscrollView_.RemoveAllViews();
            this.DetectedscrollView_.AddView(llayout);
            this.DetectImages(package);

        }
        async void UploadPhoto()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Toast.MakeText(this, "Upload not supported on this device", ToastLength.Short).Show();
                return;
            }

            //var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
            //{
            //    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Full,
            //    CompressionQuality = 40

            //}); 
            var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions() { CustomPhotoSize = 50, MaxWidthHeight = 500 });

            //byte[] imageArray = null;
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);

            Bitmap bm = BitmapFactory.DecodeFile(file.Path);

            thisImageView.SetImageBitmap(bm);
            this.PrepareImages(imageArray);

            //Mat MainImage = Cv2.ImDecode(imageArray, ImreadModes.Color);
            //var faces = this.cascadeClasifier_.DetectMultiScale(MainImage);
            //clsImagePackager package = new clsImagePackager();
            
            //LinearLayout llayout = new LinearLayout(this);
            //llayout.Orientation = Orientation.Horizontal;
            //int totalFaces = 0;

            


            //foreach (Rect item in faces)
            //{
                
            //    if (item.Width > 200 && item.Height > 200)
            //    {
            //        totalFaces++;

            //        Mat Faceimage = new Mat(MainImage, item);
            //        //Size newSize = new Size(500, 500);
            //        //Mat resizedImage = new Mat();
            //        //Cv2.Resize(Faceimage, resizedImage, newSize, 0, 0);

            //        byte[] imageData = null;
            //        //Cv2.ImEncode(".jpeg", Faceimage, out imageData);
            //        bool isSucess = Cv2.ImEncode(".jpeg", Faceimage, out imageData);
            //        //bool isSucess = Cv2.ImEncode(".jpeg", resizedImage, out imageData);
            //        package.ImageSizeList.Add(imageData.Length);
            //        package.BitmapList.Add(imageData);


            //        ImageView imgView = new ImageView(this);
            //        imgView.SetMaxWidth(10);
            //        imgView.SetMaxHeight(10);
            //        this.bitmap_ = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
            //        imgView.SetImageBitmap(this.bitmap_);
            //        llayout.AddView(imgView);
            //    }                
            //}
            //package.ImageCount = totalFaces;
            ////this.linearLayout.AddView(llayout);
            //this.DetectedscrollView_.RemoveAllViews();
            //this.DetectedscrollView_.AddView(llayout);

            ////this.DetectImages(imageArray);
            //this.DetectImages(package);

        }
        public void ConvertToBitmap(byte[] imageArray)
        {
            //this.bitmap_ = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            //var faces = this.DetectImages(imageArray);
            //thisImageView.SetImageBitmap(faces[0]);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        //public Android.Graphics.Bitmap[] DetectImages(clsPackage package)
        //{
        //    Android.Graphics.Bitmap[] result = null;

        //    if (this.client_.Connected)
        //    {

        //        //clsPackage resultPackage = new clsPackage();
        //        clsImagePackager resultPackage = new clsImagePackager();
        //        //package.BitmapList.Add(imageArray);
        //        package.ClassID = Convert.ToInt32(this.classID.Text);
        //        //package.ImageCount = 1;
        //        //package.ImageSizeList.Add(imageArray.Length);
        //        //package.ResultIdList = new System.Collections.Generic.List<int>();
        //        var packageBytes = package.GetAsBytes();
        //        //var packageBytes = package.ObjectToByteArray(package);

        //        byte[] packageSize = BitConverter.GetBytes(packageBytes.Length);
        //        this.stream_.Write(packageSize, 0, packageSize.Length);
        //        var receivedbyte = this.stream_.ReadByte();
        //        if (receivedbyte == 1)
        //        {
        //            this.stream_.Write(packageBytes, 0, packageBytes.Length);
        //            this.stream_.WriteByte(1);
        //            //for (int i = 0; i < package.ImageCount; i++)
        //            //{
        //            //    File.WriteAllBytes(System.IO.Path.Combine(this.path, "image" + "_" + i.ToString()+".jpeg"), package.BitmapList[i]);
        //            //    this.stream_.Write(package.BitmapList[i], 0, package.BitmapList[i].Length);
        //            //    while (!this.stream_.DataAvailable)
        //            //    {

        //            //    }                        
        //            //}
        //            bool waiting = true;
        //            while (waiting)
        //            {
        //                try
        //                {
        //                    if (this.stream_.DataAvailable)
        //                    {
        //                        byte[] receivedDataSize = BitConverter.GetBytes(1);
        //                        this.stream_.Read(receivedDataSize, 0, receivedDataSize.Length);
        //                        byte[] resultArray = new byte[BitConverter.ToInt32(receivedDataSize, 0)];
        //                        this.stream_.WriteByte(1);
        //                        Thread.Sleep(500);
        //                        bool wait = true;
        //                        int waitTime = 0;
        //                        while (wait && waitTime < 5000)
        //                        {
        //                            if (this.stream_.DataAvailable)
        //                            {
        //                                this.stream_.Read(resultArray, 0, resultArray.Length);
        //                                resultPackage.SetFromBytes(resultArray);
        //                                waiting = false;
        //                                wait = false;
        //                            }
        //                            Thread.Sleep(1);
        //                            waitTime++;
        //                        }


        //                    }
        //                }
        //                catch (Exception ex)
        //                {

        //                }
        //            }
        //            if (!waiting && resultPackage.ImageCount > 0)
        //            {
        //                LinearLayout llayout = new LinearLayout(this);
        //                llayout.Orientation = Orientation.Horizontal;
        //                for (int i = 0; i < resultPackage.ImageCount; i++)
        //                {

        //                    ImageView imgView = new ImageView(this);
        //                    var bmp = BitmapFactory.DecodeByteArray(resultPackage.BitmapList[i], 0, resultPackage.BitmapList[i].Length);
        //                    imgView.SetImageBitmap(bmp);
        //                    llayout.AddView(imgView);

        //                    TextView txtvwID = new TextView(this);
        //                    txtvwID.Text = "ID : ";
        //                    llayout.AddView(txtvwID);
        //                    TextView txtvw = new TextView(this);
        //                    if (resultPackage.ResultIdList.Count > i)
        //                    {
        //                        txtvw.Text = resultPackage.ResultIdList[i].ToString();
        //                    }
        //                    else
        //                    {
        //                        txtvw.Text = "NONE";
        //                    }
        //                    llayout.AddView(txtvw);

        //                    string DataFileName = System.IO.Path.Combine(this.path, "DataBase");
        //                    if (package.ResultIdList.Count >= i + 1)
        //                    {
        //                        try
        //                        {
        //                            if (File.Exists(DataFileName))
        //                            {
        //                                var lines = File.ReadAllLines(DataFileName);
        //                                lines.Append(DateTime.Now.ToShortDateString() + "--" + resultPackage.ResultIdList[i].ToString());
        //                                File.AppendAllLines(DataFileName, lines);
        //                            }
        //                            else
        //                            {
        //                                string[] Datalines = { "JDC" };
        //                                Datalines[0] = DateTime.Now.ToShortDateString() + "--" + resultPackage.ResultIdList[i].ToString();
        //                                File.WriteAllLines(DataFileName, Datalines);
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {

        //                        }

        //                    }

        //                }
        //                this.resultscrollView_.AddView(llayout);
        //            }
        //        }
        //    }

        //    return result;
        //}

        public Android.Graphics.Bitmap[] DetectImages(clsImagePackager package)
        {
            this.txtVwStatus_.Text = "Uploading to server...";
            this.firebase.SendPackage(package);
            int time = 0;
            bool received = false;
            clsImagePackager resultPackage = new clsImagePackager();
            while(time < 20000 && !received)
            {
                if (this.firebase.GetCommandString() == "ResultGiven")
                {
                    resultPackage = this.firebase.GetPackage();
                    received = true;
                }
                Thread.Sleep(1);
            }
            if (received && resultPackage.ImageCount > 0)
            {
                this.txtVwStatus_.Text = "Got Result !";
                LinearLayout llayout = new LinearLayout(this);
                llayout.Orientation = Orientation.Horizontal;

                string[] studentList = { "Dharun,1,Absent", "abinash,3,Absent", "rithick,4,Absent", "naveena,5,Absent", "sriDevi,7,Absent" };

                for (int i = 0; i < resultPackage.ImageCount; i++)
                {
                    LinearLayout llayoutIDImage = new LinearLayout(this);
                    llayoutIDImage.Orientation = Orientation.Vertical;

                    LinearLayout llayoutText = new LinearLayout(this);
                    llayoutText.Orientation = Orientation.Horizontal;

                    ImageView imgView = new ImageView(this);
                    var bmp = BitmapFactory.DecodeByteArray(resultPackage.BitmapList[i], 0, resultPackage.BitmapList[i].Length);
                    imgView.SetImageBitmap(bmp);
                    llayoutIDImage.AddView(imgView);

                    TextView txtvwID = new TextView(this);
                    txtvwID.Text = "ID : ";
                    llayoutText.AddView(txtvwID);
                    TextView txtvw = new TextView(this);
                    if (resultPackage.ResultIdList.Count > i)
                    {
                        txtvw.Text = resultPackage.ResultIdList[i].ToString();
                    }
                    else
                    {
                        txtvw.Text = "NONE";
                    }
                    llayoutText.AddView(txtvw);

                    llayoutIDImage.AddView(llayoutText);

                    
                    if (resultPackage.ResultIdList.Count >= i + 1)
                    {
                        try
                        {
                            int studentCount = 0;
                            foreach (string item in studentList)
                            {
                                var splitedstring = item.Split(",");
                                if (resultPackage.ResultIdList[i].ToString() == splitedstring[1])
                                {
                                    studentList[studentCount] = splitedstring[0] + "," + splitedstring[1] + "," + "Present";
                                    break;
                                }
                                studentCount++;
                            }
                            //if (File.Exists(DataFileName))
                            //{
                            //    var lines = File.ReadAllText(DataFileName);
                            //    lines = lines + "\n" + (DateTime.Now.ToShortDateString() + "," + resultPackage.ResultIdList[i].ToString());
                            //    File.WriteAllText(DataFileName, lines);
                            //}
                            //else
                            //{
                            //    var lines = (DateTime.Now.ToShortDateString() + "," + resultPackage.ResultIdList[i].ToString());
                            //    File.WriteAllText(DataFileName, lines);
                            //}
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                    llayout.AddView(llayoutIDImage);

                }
                string DataFileName = System.IO.Path.Combine(this.path, "DataBase.csv");
                var finalData = "";
                foreach (var item in studentList)
                {
                    finalData = finalData + "\n" + item;
                }
                File.WriteAllText(DataFileName,finalData);
                this.resultscrollView_.RemoveAllViews();
                this.resultscrollView_.AddView(llayout);
            }
            else
            {
                this.txtVwStatus_.Text = "CheckServer";
            }


        Android.Graphics.Bitmap[] result = null;

            //if (this.client_.Connected)
            //{

            //    clsImagePackager resultPackage = new clsImagePackager();
            //    //package.BitmapList.Add(imageArray);
            //    package.ClassID = Convert.ToInt16(this.classID.Text);
            //    //package.ImageCount = 1;
            //    //package.ImageSizeList.Add(imageArray.Length);
            //    //package.ResultIdList = new System.Collections.Generic.List<int>();
            //    var packageBytes = package.GetAsBytes();
            //    //var packageBytes = package.ObjectToByteArray(package);

            //    byte[] packageSize = BitConverter.GetBytes(packageBytes.Length);
            //    this.stream_.Write(packageSize, 0, packageSize.Length);
            //    var receivedbyte = this.stream_.ReadByte();
            //    if (receivedbyte == 1)
            //    {
            //        this.stream_.Write(packageBytes, 0, packageBytes.Length);
            //        bool waiting = true;
            //        while (waiting)
            //        {
            //            try
            //            {
            //                if (this.stream_.DataAvailable)
            //                {
            //                    byte[] receivedDataSize = BitConverter.GetBytes(1);
            //                    this.stream_.Read(receivedDataSize, 0, receivedDataSize.Length);
            //                    byte[] resultArray = new byte[BitConverter.ToInt32(receivedDataSize, 0)];
            //                    this.stream_.WriteByte(1);
            //                    Thread.Sleep(500);
            //                    bool wait = true;
            //                    int waitTime = 0;
            //                    while (wait && waitTime < 500)
            //                    {
            //                        if (this.stream_.DataAvailable)
            //                        {
            //                            this.stream_.Read(resultArray, 0, resultArray.Length);
            //                            resultPackage.SetFromBytes(resultArray);
            //                            waiting = false;
            //                            wait = false;
            //                        }
            //                        Thread.Sleep(1);
            //                        waitTime++;
            //                    }


            //                }
            //            }
            //            catch (Exception ex)
            //            {

            //            }
            //        }
            //        if (!waiting && resultPackage.ImageCount > 0)
            //        {
            //            LinearLayout llayout = new LinearLayout(this);
            //            llayout.Orientation = Orientation.Horizontal;
            //            for (int i = 0; i < resultPackage.ImageCount; i++)
            //            {

            //                ImageView imgView = new ImageView(this);
            //                var bmp = BitmapFactory.DecodeByteArray(resultPackage.BitmapList[i], 0, resultPackage.BitmapList[i].Length);
            //                imgView.SetImageBitmap(bmp);
            //                llayout.AddView(imgView);

            //                TextView txtvwID = new TextView(this);
            //                txtvwID.Text = "ID : ";
            //                llayout.AddView(txtvwID);
            //                TextView txtvw = new TextView(this);
            //                if (resultPackage.ResultIdList.Count > i)
            //                {
            //                    txtvw.Text = resultPackage.ResultIdList[i].ToString();
            //                }
            //                else
            //                {
            //                    txtvw.Text = "NONE";
            //                }
            //                llayout.AddView(txtvw);

            //                string DataFileName = System.IO.Path.Combine(this.path, "DataBase");
            //                if (package.ResultIdList.Count >= i + 1)
            //                {
            //                    try
            //                    {
            //                        if (File.Exists(DataFileName))
            //                        {
            //                            var lines = File.ReadAllLines(DataFileName);
            //                            lines.Append(DateTime.Now.ToShortDateString() + "--" + resultPackage.ResultIdList[i].ToString());
            //                            File.AppendAllLines(DataFileName, lines);
            //                        }
            //                        else
            //                        {
            //                            string[] Datalines = { "JDC" };
            //                            Datalines[0] = DateTime.Now.ToShortDateString() + "--" + resultPackage.ResultIdList[i].ToString();
            //                            File.WriteAllLines(DataFileName, Datalines);
            //                        }
            //                    }
            //                    catch (Exception ex)
            //                    {

            //                    }

            //                }

            //            }
            //            this.resultscrollView_.AddView(llayout);
            //        }
            //    }

            //    //byte[] imageSize = new byte[5];
            //    //byte[] imageLength = BitConverter.GetBytes(imageArray.Length);
            //    //Array.Copy(imageLength, 0, imageSize, 1, 4);
            //    //imageSize[0] = Convert.ToByte(Convert.ToInt16(this.classID.Text));

            //    //this.stream_.Write(imageSize, 0, imageSize.Length);
            //    //var receivedbyte = this.stream_.ReadByte();
            //    //if (receivedbyte == 1)
            //    //{
            //    //    File.WriteAllBytes(System.IO.Path.Combine(path, "ImageToTrain.jpg"),imageArray);
            //    //    this.stream_.Write(imageArray, 0, imageArray.Length);
            //    //    bool waiting = true;
            //    //    while (waiting)
            //    //    {
            //    //        try
            //    //        {
            //    //            if (this.stream_.DataAvailable)
            //    //            {
            //    //                byte[] receivedDataSize = BitConverter.GetBytes(1);
            //    //                this.stream_.Read(receivedDataSize, 0, receivedDataSize.Length);
            //    //                this.stream_.WriteByte(1);
            //    //                byte[] resultArray = new byte[BitConverter.ToInt32(receivedDataSize, 0)];
            //    //                this.stream_.Read(resultArray, 0, resultArray.Length);

            //    //                int offset = 0;
            //    //                byte[] buffer = new byte[4];
            //    //                Array.Copy(resultArray, 0, buffer, 0, 4);
            //    //                offset += 4;
            //    //                int faces = BitConverter.ToInt32(buffer, 0);

            //    //                for (int i = 0; i < faces; i++)
            //    //                {
            //    //                    byte[] data = new byte[4];
            //    //                    Array.Copy(resultArray, offset, data, 0, 4);
            //    //                    offset = offset + 4;
            //    //                    int resSize = BitConverter.ToInt32(data, 0);
            //    //                    buffer = new byte[resSize];
            //    //                    byte[] idArr = new byte[4];
            //    //                    byte[] imgArr = new byte[resSize - 4];

            //    //                    Array.Copy(resultArray, offset, idArr, 0, 4);
            //    //                    offset = offset + 4;
            //    //                    Array.Copy(resultArray, offset, imgArr, 0, (resSize - 4));
            //    //                    offset = offset + (resSize - 4);
            //    //                    int id = BitConverter.ToInt32(idArr, 0);

            //    //                    string filePath = System.IO.Path.Combine(path , id.ToString());
            //    //                    File.WriteAllBytes(filePath, imgArr);

            //    //                    byte[] detectedimageArray = System.IO.File.ReadAllBytes(filePath);

            //    //                    LinearLayout llayout = new LinearLayout(this);
            //    //                    llayout.Orientation = Orientation.Horizontal;

            //    //                    ImageView imgView = new ImageView(this);
            //    //                    this.bitmap_ = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            //    //                    imgView.SetImageBitmap(this.bitmap_);
            //    //                    llayout.AddView(imgView);
            //    //                    //string name = "";
            //    //                    //switch(id)
            //    //                    //{
            //    //                    //    case 1:
            //    //                    //        name = "Angelina Jolie";
            //    //                    //        break;
            //    //                    //    case 2:
            //    //                    //        name = "Brad Pitt";
            //    //                    //        break;
            //    //                    //    case 3:
            //    //                    //        name = "Hugh Jackman";
            //    //                    //        break;
            //    //                    //    case 4:
            //    //                    //        name = "Johnny Depp";
            //    //                    //        break;
            //    //                    //    case 5:
            //    //                    //        name = "Kate Winslet";
            //    //                    //        break;
            //    //                    //    case 6:
            //    //                    //        name = "Leonard DiCaprio";
            //    //                    //        break;
            //    //                    //    case 7:
            //    //                    //        name = "Robert Downey Jr";
            //    //                    //        break;
            //    //                    //    case 8:
            //    //                    //        name = "Scarlet JohanSSon";
            //    //                    //        break;
            //    //                    //    case 9:
            //    //                    //        name = "Tom Cruise";
            //    //                    //        break;
            //    //                    //    case 10:
            //    //                    //        name = "Will Smith";
            //    //                    //        break;
            //    //                    //}
            //    //                    TextView txtvwID = new TextView(this);
            //    //                    txtvwID.Text = "ID : ";
            //    //                    llayout.AddView(txtvwID);
            //    //                    TextView txtvw = new TextView(this);
            //    //                    txtvw.Text = id.ToString();
            //    //                    llayout.AddView(txtvw);
            //    //                    this.linearLayout.AddView(llayout);

            //    //                    string DataFileName = System.IO.Path.Combine(this.path, "DataBase");
            //    //                    if (File.Exists(DataFileName))
            //    //                    {
            //    //                        var lines = File.ReadAllLines(DataFileName);
            //    //                        lines.Append(DateTime.Now.ToShortDateString() + "--" + id.ToString());
            //    //                        File.AppendAllLines(DataFileName, lines);
            //    //                    }
            //    //                    else
            //    //                    {
            //    //                        var lines = new string[1];
            //    //                        lines.Append(DateTime.Now.ToShortDateString() + "--" + id.ToString());
            //    //                        File.AppendAllLines(DataFileName, lines);
            //    //                    }
            //    //                    //txtvw1.Text = name;
            //    //                }
            //    //            waiting = false;
            //    //        }
            //    //    }
            //    //        catch (Exception ex)
            //    //    {

            //    //        waiting = false;
            //    //    }
            //    //}
            //    //}
            //}

            return result;
        }
    }
}