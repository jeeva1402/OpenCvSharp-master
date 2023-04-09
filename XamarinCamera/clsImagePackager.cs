
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Android.Accounts;
using FireSharp.Config;
using FireSharp.Interfaces;
using Java.Lang;
using Exception = System.Exception;
using Object = System.Object;

namespace XamarinCamera
{
    public class clsJDFirebase
    {
        protected IFirebaseClient client_;
        string firebasePath_ = "JDProjects/AndroidFaceAttendence";
        protected bool isConnected_;
        public bool IsConnected { get { return isConnected_; } }
        public clsJDFirebase()
        {
            this.isConnected_ = this.Connect("8w6GTA4Rt6MPgzTrXIdXY2zqd4Lf57LrUVtdq9iu", "https://jd-workmonitor-default-rtdb.firebaseio.com/");
        }
        public bool Connect(string authSecretorUsername, string basePathorPassword)
        {
            bool serverStatus = false;
            try
            {
                IFirebaseConfig fcon = new FirebaseConfig()
                {
                    AuthSecret = authSecretorUsername,
                    BasePath = basePathorPassword
                };
                this.client_ = new FireSharp.FirebaseClient(fcon);
                serverStatus = true;
            }
            catch (Exception ex)
            {

            }

            return serverStatus;
        }
        public void SendPackage(clsImagePackager package)
        {
            this.client_.Set(firebasePath_ + "/" +"Command", "GiveResult" );
            this.client_.Set(firebasePath_ + "/" + "Input", package);
        }

        public string GetCommandString()
        {
            return this.client_.Get(firebasePath_ + "/" + "Command").ResultAs<string>();
        }
        public clsImagePackager GetPackage()
        {
            return this.client_.Get(firebasePath_ + "/" + "Output").ResultAs<clsImagePackager>();
        }
    }
    

    [Serializable]
    public class clsImagePackager
    {
        int classID_;
        int imageCount_;

        List<int> imageSizeList_;
        List<byte[]> bitmapList_;
        List<int> resultIdList_;

        public int ClassID { get => classID_; set => classID_ = value; }
        public int ImageCount { get => imageCount_; set => imageCount_ = value; }
        public List<byte[]> BitmapList { get => bitmapList_; set => bitmapList_ = value; }
        public List<int> ResultIdList { get => resultIdList_; set => resultIdList_ = value; }
        public List<int> ImageSizeList { get => imageSizeList_; set => imageSizeList_ = value; }
        public clsImagePackager()
        {
            this.bitmapList_ = new List<byte[]>();   
            resultIdList_= new List<int>();
            imageSizeList_ = new List<int>();
        }

        public byte[] GetAsBytes()
        {
            int BufferSize = 0;
            int offset = 0;
            BufferSize = (sizeof(int) *2) + (sizeof(int) * this.imageSizeList_.Count) + (sizeof(int) * this.ResultIdList.Count);
            foreach (byte[] item in this.bitmapList_)
            {
                BufferSize = BufferSize + item.Length;
            }
            byte[] Buffer = new byte[BufferSize];

            Array.Copy(BitConverter.GetBytes(this.classID_),0,Buffer,offset, sizeof(int));
            offset = offset + sizeof(int);

            Array.Copy(BitConverter.GetBytes(this.imageCount_), 0, Buffer, offset, sizeof(int));
            offset = offset + sizeof(int);

            foreach (int item in this.imageSizeList_)
            {
                Array.Copy(BitConverter.GetBytes(item), 0, Buffer, offset, sizeof(int));
                offset = offset + sizeof(int);
            }

            foreach (byte[] item in this.bitmapList_)
            {
                Array.Copy(item, 0, Buffer, offset, item.Length);
                offset = offset + item.Length;
            }
            if (this.resultIdList_.Count > 0)
            {
                foreach (int item in this.resultIdList_)
                {
                    Array.Copy(BitConverter.GetBytes(item), 0, Buffer, offset, sizeof(int));
                    offset = offset + sizeof(int);
                }
            }
            return Buffer;
        }

        public void SetFromBytes(byte[] bufferData)
        {            
            int offset = 0;
            byte[] data = new byte[sizeof(int)];

            Array.Copy(bufferData, offset, data, 0, sizeof(int));
            this.classID_ = BitConverter.ToInt32(data, 0);
            offset = offset + sizeof(int);

            Array.Copy(bufferData, offset, data, 0, sizeof(int));
            this.imageCount_ = BitConverter.ToInt32(data, 0);
            offset = offset + sizeof(int);

            for (int i = 0; i < this.imageCount_; i++)
            {
                Array.Copy(bufferData, offset, data, 0, sizeof(int));
                this.imageSizeList_.Add(BitConverter.ToInt32(data, 0));
                offset = offset + sizeof(int);                
            }
            for (int i = 0; i < this.imageCount_; i++)
            {
                data = new byte[this.imageSizeList_[i]];
                Array.Copy(bufferData, offset, data, 0, data.Length);
                this.bitmapList_.Add(data);
                offset = offset + this.imageSizeList_[i];
            }
            if (bufferData.Length > offset)
            {
                data = new byte[sizeof(int)];
                Array.Copy(bufferData, offset, data, 0, sizeof(int));
                this.resultIdList_.Add(BitConverter.ToInt32(data, 0));
                offset = offset + sizeof(int);
            }

        }
        public byte[] ObjectToByteArray(clsImagePackager packager)
        {
            if (packager == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, packager);
            return ms.ToArray();
        }

        // Convert a byte array to an Object
        public Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            object x = null;
            clsImagePackager obj = (clsImagePackager)binForm.Deserialize(memStream);

            return obj;
        }
    }

    public class clsPackage
    {
        int classID_;
        int imageCount_;

        List<int> imageSizeList_;
        List<byte[]> bitmapList_;
        List<int> resultIdList_;

        public int ClassID { get => classID_; set => classID_ = value; }
        public int ImageCount { get => imageCount_; set => imageCount_ = value; }
        public List<byte[]> BitmapList { get => bitmapList_; set => bitmapList_ = value; }
        public List<int> ResultIdList { get => resultIdList_; set => resultIdList_ = value; }
        public List<int> ImageSizeList { get => imageSizeList_; set => imageSizeList_ = value; }
        public clsPackage()
        {
            this.bitmapList_ = new List<byte[]>();
            resultIdList_ = new List<int>();
            imageSizeList_ = new List<int>();
        }

        public byte[] GetAsBytes()
        {
            int BufferSize = 0;
            int offset = 0;
            BufferSize = sizeof(int) * 2 + (sizeof(int) * this.imageSizeList_.Count) + (sizeof(int) * this.ResultIdList.Count);

            byte[] Buffer = new byte[BufferSize];

            Array.Copy(BitConverter.GetBytes(this.classID_), 0, Buffer, offset, sizeof(int));
            offset = offset + sizeof(int);

            Array.Copy(BitConverter.GetBytes(this.imageCount_), 0, Buffer, offset, sizeof(int));
            offset = offset + sizeof(int);

            foreach (int item in this.imageSizeList_)
            {
                Array.Copy(BitConverter.GetBytes(item), 0, Buffer, offset, sizeof(int));
                offset = offset + sizeof(int);
            }


            if (this.resultIdList_.Count > 0)
            {
                foreach (int item in this.resultIdList_)
                {
                    Array.Copy(BitConverter.GetBytes(item), 0, Buffer, offset, sizeof(int));
                    offset = offset + sizeof(int);
                }
            }
            return Buffer;
        }

        public void SetFromBytes(byte[] bufferData)
        {
            int offset = 0;
            byte[] data = new byte[sizeof(int)];

            Array.Copy(bufferData, offset, data, 0, sizeof(int));
            this.classID_ = BitConverter.ToInt32(data, 0);
            offset = offset + sizeof(int);

            Array.Copy(bufferData, offset, data, 0, sizeof(int));
            this.imageCount_ = BitConverter.ToInt32(data, 0);

            offset = offset + sizeof(int);

            for (int i = 0; i < this.imageCount_; i++)
            {
                Array.Copy(bufferData, offset, data, 0, sizeof(int));
                this.imageSizeList_.Add(BitConverter.ToInt32(data, 0));
                offset = offset + sizeof(int);
            }

            if (bufferData.Length > offset)
            {
                data = new byte[sizeof(int)];
                Array.Copy(bufferData, offset, data, 0, sizeof(int));
                this.resultIdList_.Add(BitConverter.ToInt32(data, 0));
                offset = offset + sizeof(int);
            }

        }
    }

}