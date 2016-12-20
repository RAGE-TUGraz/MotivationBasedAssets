/*
  Copyright 2016 TUGraz, http://www.tugraz.at/
  
  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  This project has received funding from the European Union’s Horizon
  2020 research and innovation programme under grant agreement No 644187.
  You may obtain a copy of the License at
  
      http://www.apache.org/licenses/LICENSE-2.0
  
  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
  
  This software has been created in the context of the EU-funded RAGE project.
  Realising and Applied Gaming Eco-System (RAGE), Grant agreement No 644187, 
  http://rageproject.eu/

  Development was done by Cognitive Science Section (CSS) 
  at Knowledge Technologies Institute (KTI)at Graz University of Technology (TUGraz).
  http://kti.tugraz.at/css/

  Created by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
*/

using AssetManagerPackage;
using AssetPackage;
using MotivationBasedAdaptionAssetNameSpace;
using MotivationAssessmentAssetNameSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using System.Threading;
using UnitTestMotivation;

namespace TestMotivation
{
    class Program
    {
        static void Main(string[] args)
        {
            AssetManager am = AssetManager.Instance;
            am.Bridge = new Bridge();

            MotivationAssessmentAsset masa = new MotivationAssessmentAsset();
            MotivationBasedAdaptionAsset mada = new MotivationBasedAdaptionAsset();

            TestMotivationAssessmentAsset tmasa = new TestMotivationAssessmentAsset();
            tmasa.performAllTests();

            TestMotivationBasedAdaptationAsset tmada = new TestMotivationBasedAdaptationAsset();
            tmada.performAllTests();

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
        
    }
    
    class TestMotivationAssessmentAsset
    {
        #region HelperMethods

        /// <summary>
        /// Logging functionality for the Tests
        /// </summary>
        /// <param name="msg"> Message to be logged </param>
        public void log(String msg, Severity severity = Severity.Information)
        {
            AssetManager.Instance.Log(severity, "[MAsA Test]: {0}", msg);
        }

        /// <summary>
        /// Method returning the Asset
        /// </summary>
        /// <returns> The Asset</returns>
        public MotivationAssessmentAsset getMAsA()
        {
            return (MotivationAssessmentAsset)AssetManager.Instance.findAssetByClass("MotivationAssessmentAsset");
        }

        /// <summary>
        /// Method for creating an example Motivation Model.
        /// </summary>
        ///
        /// <returns>
        /// MotivationModel with example values.
        /// </returns>
        internal MotivationModel createExampleMM()
        {
            MotivationAspect ma1 = new MotivationAspect();
            ma1.name = "attention";
            ma1.up = "attention+(1-attention)*0.5";
            ma1.down = "attention*0.5";
            ma1.rule = "";
            MotivationAspect ma2 = new MotivationAspect();
            ma2.name = "satisfaction";
            ma2.up = "satisfaction+(1-satisfaction)*0.5";
            ma2.down = "satisfaction*0.5";
            ma2.rule = "";
            MotivationAspect ma3 = new MotivationAspect();
            ma3.name = "confidence";
            ma3.up = "confidence+(1-confidence)*0.5";
            ma3.down = "confidence*0.5";
            ma3.rule = "";
            MotivationAspect ma4 = new MotivationAspect();
            ma4.name = "motivation";
            ma4.up = "";
            ma4.down = "";
            ma4.rule = "(satisfaction+confidence+attention)/3";
            MotivationAspectList mal = new MotivationAspectList();
            List<MotivationAspect> maList = new List<MotivationAspect>();
            maList.Add(ma1);
            maList.Add(ma2);
            maList.Add(ma3);
            maList.Add(ma4);
            mal.motivationAspectList = maList;

            String ii11 = "Hey, my friend! Are you sleeping?!";
            String ii12 = "Sorry for interrupting you; we need to go on.";
            Intervention i1 = new Intervention();
            List<String> i1List = new List<String>();
            i1List.Add(ii11);
            i1List.Add(ii12);
            i1.interventionInstances = new InterventionInstance();
            i1.interventionInstances.instance = i1List;
            i1.name = "attention catcher";
            i1.rule = "attention < 0.4";

            String ii21 = "Once this mission is over, it's time to celebrate.";
            String ii22 = "By solving this task you will earn another 10000 points.";
            Intervention i2 = new Intervention();
            List<String> i2List = new List<String>();
            i2List.Add(ii21);
            i2List.Add(ii22);
            i2.interventionInstances = new InterventionInstance();
            i2.interventionInstances.instance = i2List;
            i2.name = "incitation intervention";
            i2.rule = "satisfaction < 0.4";

            String ii31 = "Don't give up. Try again.";
            String ii32 = "It is a challenge, I know. Let's give it another trial.";
            String ii33 = "Go on. Practice makes perfect.";
            Intervention i3 = new Intervention();
            List<String> i3List = new List<String>();
            i3List.Add(ii31);
            i3List.Add(ii32);
            i3List.Add(ii33);
            i3.interventionInstances = new InterventionInstance();
            i3.interventionInstances.instance = i3List;
            i3.name = "encouraging intervention";
            i3.rule = "confidence < 0.4";


            MotivationInterventionList mil = new MotivationInterventionList();
            List<Intervention> iList = new List<Intervention>();
            iList.Add(i1);
            iList.Add(i2);
            iList.Add(i3);
            mil.motivationInterventionList = iList;

            MotivationModel mm = new MotivationModel();
            mm.motivationAspects = mal;
            mm.motivationInterventions = mil;

            return mm;
        }

        /// <summary>
        /// Method for setting the motivation model
        /// </summary>
        /// <param name="mm"> The motivation model to load</param>
        public void setMotivationModel(MotivationModel mm)
        {
            string fileId = "MotivationAssessmentAssetTestId.xml";

            IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
            if (ids != null)
            {
                log("Storing DomainModel to File.");
                ids.Save(fileId, mm.toXmlString());
            }
            else
                log("No IDataStorage - Bridge implemented!", Severity.Warning);

            //change Settings to load local file
            MotivationAssessmentAssetSettings newMAS = new MotivationAssessmentAssetSettings();
            newMAS.XMLLoadingId = fileId;
            ((MotivationAssessmentAsset)AssetManager.Instance.findAssetByClass("MotivationAssessmentAsset")).Settings = newMAS;
        }

        /// <summary>
        /// Method for storing a Motivation Model as XML in a file.
        /// </summary>
        /// <param name="fileId"> File-Id for storing the model. </param>
        /// <param name="mm"> The Motivation model to store. </param>
        internal void writeMMToFile(string fileId, MotivationModel mm)
        {
            IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
            if (ids != null)
            {
                log("Storing DomainModel to File.");
                ids.Save(fileId, mm.toXmlString());
            }
            else
                log("No IDataStorage - Bridge implemented!", Severity.Error);
        }

        #endregion HelperMethods
        #region TestMethods

        /// <summary>
        /// Method for calling all tests in this Class.
        /// </summary>
        public void performAllTests()
        {
            log("*****************************************************************");
            log("Calling all tests (MAsA):");
            performTest1();
            performTest2();
            log("Tests - done!");
            log("*****************************************************************");
        }

        /// <summary>
        /// Method performing a simple test: creating example MotivationModel and feeding hints for a testplayer. 
        /// </summary>
        internal void performTest1()
        {
            log("***************TEST 1********************");

            DateTime now = DateTime.Now;

            //reaching a new level
            getMAsA().addMotivationHint(MotivationHintEnum.NewLevel);

            //perfect solution - 0 error - 0 help requests - 10 seconds
            getMAsA().addMotivationHint(MotivationHintEnum.NewProblem);

            log("Sleeping for 10 seconds...");
            Thread.Sleep(10*1000);
            getMAsA().addMotivationHint(MotivationHintEnum.Success);

            //too early guess - 2 seconds
            getMAsA().addMotivationHint(MotivationHintEnum.NewProblem);

            log("Sleeping for 2 seconds...");
            Thread.Sleep(2 * 1000);
            getMAsA().addMotivationHint(MotivationHintEnum.Success);

            //too many errors - 11/7 seconds - 4 errors - 0 help requests
            getMAsA().addMotivationHint(MotivationHintEnum.NewProblem);

            log("Sleeping for 7 seconds...");
            Thread.Sleep(7 * 1000);
            getMAsA().addMotivationHint(MotivationHintEnum.Fail);

            log("Sleeping for 1 second...");
            Thread.Sleep(1 * 1000);
            getMAsA().addMotivationHint(MotivationHintEnum.Fail);

            log("Sleeping for 1 second...");
            Thread.Sleep(1 * 1000);
            getMAsA().addMotivationHint(MotivationHintEnum.Fail);

            log("Sleeping for 1 second...");
            Thread.Sleep(1 * 1000);
            getMAsA().addMotivationHint(MotivationHintEnum.Fail);

            log("Sleeping for 1 second...");
            Thread.Sleep(1 * 1000);
            getMAsA().addMotivationHint(MotivationHintEnum.Success);

            //too late solution - 15 seconds
            getMAsA().addMotivationHint(MotivationHintEnum.NewProblem);

            log("Sleeping for 20 seconds...");
            Thread.Sleep(20 * 1000);
            getMAsA().addMotivationHint(MotivationHintEnum.Success);

        }

        /// <summary>
        /// Method performing a simple test: creating example MotivationModel - storing it in a file and reading from the file
        /// </summary>
        internal void performTest2()
        {
            log("***************TEST 2********************");

            MotivationModel mm = createExampleMM();
            string id = "MotivationAssessmentTestId.xml";
            writeMMToFile(id,mm);

            MotivationAssessmentAssetSettings maass = new MotivationAssessmentAssetSettings();
            maass.XMLLoadingId = id;
            getMAsA().Settings = maass;
            MotivationModel mm2 = getMAsA().loadMotivationModel();

            if (mm.toXmlString().Equals(mm2.toXmlString()))
                log("MotivationModels before and after the loading are identically!");
            else
                log("MotivationModels before and after the loading are NOT identically!", Severity.Error);

        }

        #endregion TestMethods
    }

    class TestMotivationBasedAdaptationAsset
    {
        #region HelperMethods

        /// <summary>
        /// Logging functionality for the Tests
        /// </summary>
        /// <param name="msg"> Message to be logged </param>
        public void log(String msg, Severity severity = Severity.Information)
        {
            AssetManager.Instance.Log(severity, "[MAdA Test]: {0}", msg);
        }

        /// <summary>
        /// Method returning the Asset
        /// </summary>
        /// <returns> The Asset</returns>
        public MotivationBasedAdaptionAsset getMAdA()
        {
            return (MotivationBasedAdaptionAsset)AssetManager.Instance.findAssetByClass("MotivationBasedAdaptionAsset");
        }

        /// <summary>
        /// Method returning the Asset
        /// </summary>
        /// <returns> The Asset</returns>
        public MotivationAssessmentAsset getMAsA()
        {
            return (MotivationAssessmentAsset)AssetManager.Instance.findAssetByClass("MotivationAssessmentAsset");
        }

        /// <summary>
        /// Method printing out the triggered interventions and instances of them
        /// </summary>
        public void printOutInterventions()
        {
            List<string> interventions = getMAdA().getInterventions();
            if(interventions.Count == 0)
            {
                log("No intervention is triggered!");
            }
            else
            {
                foreach(string interv in interventions)
                {
                    log("Intervention \""+interv+"\" triggered, instance: \""+getMAdA().getInstance(interv)+"\"");
                }
            }
        }

        #endregion HelperMethods
        #region TestMethods

        /// <summary>
        /// Method calling all Tests of this Class.
        /// </summary>
        internal void performAllTests()
        {
            log("*****************************************************************");
            log("Calling all tests (MAdA):");
            performtest1();
            log("Tests - done!");
            log("*****************************************************************");
        }

        /// <summary>
        /// Performing successive motivation update
        /// </summary>
        private void performtest1()
        {
            log("Start Test 1:");

            string[] hintArray = { "p", "h", "f", "h", "s","l" };
            List<string> hints = new List<string>(hintArray);
            int[] workingTimesArray = {0,1,2,1,1,0};
            List<int> workingTimes = new List<int>(workingTimesArray);
            int pos = 0;

            int status = 1;
            //String stat1 = "(new level...l, new problem...p, exit...e):";
            //String stat2 = "(help...h, fail...f, success...s, exit...e):";

            string line = "";
            while (pos < hintArray.Length)
            {
                //"help","fail","success", "new level", "new problem"
                while (status == 1 && pos < hintArray.Length)
                {
                    //log("Please enter game information " + stat1);
                    //line = Console.ReadLine();
                    if (workingTimesArray[pos] > 0)
                    {
                        log("Sleeping for "+workingTimesArray[pos]+" second(s)...");
                        Thread.Sleep(workingTimesArray[pos] * 1000);
                    }
                    line = hintArray[pos];
                    pos++;
                    if (line.Equals("l"))
                    {
                        getMAsA().addMotivationHint(MotivationHintEnum.NewLevel);
                        printOutInterventions();
                    }
                    else if (line.Equals("e"))
                    {
                        log("End Test 1");
                        return;
                    }
                    else if (line.Equals("p"))
                    {
                        getMAsA().addMotivationHint(MotivationHintEnum.NewProblem);
                        status = 2;
                    }
                }
                while(status == 2 && pos < hintArray.Length)
                {
                    //log("Please enter game information " + stat2);
                    //line = Console.ReadLine();
                    if (workingTimesArray[pos] > 0)
                    {
                        log("Sleeping for " + workingTimesArray[pos] + " second(s)...");
                        Thread.Sleep(workingTimesArray[pos] * 1000);
                    }
                    line = hintArray[pos];
                    pos++;
                    if (line.Equals("e"))
                    {
                        log("End Test 1");
                        return;
                    }
                    else if (line.Equals("h"))
                    {
                        getMAsA().addMotivationHint(MotivationHintEnum.Help);
                    }
                    else if (line.Equals("f"))
                    {
                        getMAsA().addMotivationHint(MotivationHintEnum.Fail);
                    }
                    else if (line.Equals("s"))
                    {
                        getMAsA().addMotivationHint(MotivationHintEnum.Success);
                        status = 1;
                    }
                    else
                    {
                        continue;
                    }
                    printOutInterventions();
                }
            }
        }

        #endregion TestMethods
    }

}
