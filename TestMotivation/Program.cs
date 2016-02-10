using AssetManagerPackage;
using AssetPackage;
using MotivationAdaptionAssetNameSpace;
using MotivationAssessmentAssetNameSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TestMotivation
{
    class Program
    {
        static void Main(string[] args)
        {
            AssetManager am = AssetManager.Instance;
            am.Bridge = new Bridge();


            MotivationAssessmentAsset masa = new MotivationAssessmentAsset();
            MotivationAdaptionAsset mada = new MotivationAdaptionAsset();

            masa.performTest();


            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }

    class Bridge : IBridge, ILog, IDataStorage
    {

        #region IDataStorage
        public bool Delete(string fileId)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string fileId)
        {
            throw new NotImplementedException();
        }

        public string[] Files()
        {
            throw new NotImplementedException();
        }

        public string Load(string fileId)
        {
            string path = @"C:\Users\mmaurer\Desktop\" + fileId;

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(path))
                {
                    // Read the stream to a string, and write the string to the console.
                    String line = sr.ReadToEnd();
                    return (line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Bridge: Error when reading file!");
            }

            return (null);
        }

        public void Save(string fileId, string fileData)
        {
            string filePath = @"C:\Users\mmaurer\Desktop\" + fileId;
            using (StreamWriter file = new StreamWriter(filePath))
            {
                file.Write(fileData);
            }
        }

        #endregion IDataStorage

        #region ILog

        public void Log(Severity severity, string msg)
        {
            Console.WriteLine("Bridge: "+msg);
        }

        #endregion ILog
    }
}
