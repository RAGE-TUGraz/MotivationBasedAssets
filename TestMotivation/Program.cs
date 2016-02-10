using AssetManagerPackage;
using AssetPackage;
using MotivationAdaptionAssetNameSpace;
using MotivationAssessmentAssetNameSpace;
using System;
using System.Collections.Generic;
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

    class Bridge : IBridge, ILog
    {
        public void Log(Severity severity, string msg)
        {
            Console.WriteLine("Bridge: "+msg);
        }
    }
}
