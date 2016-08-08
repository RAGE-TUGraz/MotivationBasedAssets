﻿using AssetManagerPackage;
using AssetPackage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MotivationAssessmentAssetNameSpace;
using MotivationBasedAdaptionAssetNameSpace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace UnitTestMotivation
{
    [TestClass]
    public class TestCaseMotivation
    {
        #region HelperMethods

        /// <summary>
        /// Logging functionality for the Tests
        /// </summary>
        /// <param name="msg"> Message to be logged </param>
        public void log(String msg, Severity severity = Severity.Information)
        {
            ILog logger = (ILog)AssetManager.Instance.Bridge;
            logger.Log(severity, "[Motivation test]" + msg);
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

        /// <summary>
        /// Method returning the Asset
        /// </summary>
        /// <returns> The Asset</returns>
        public MotivationBasedAdaptionAsset getMAdA()
        {
            return (MotivationBasedAdaptionAsset)AssetManager.Instance.findAssetByClass("MotivationBasedAdaptionAsset");
        }

        /// <summary>
        /// Method printing out the triggered interventions and instances of them
        /// </summary>
        public void printOutInterventions()
        {
            List<string> interventions = getMAdA().getInterventions();
            if (interventions.Count == 0)
            {
                log("No intervention is triggered!");
            }
            else
            {
                foreach (string interv in interventions)
                {
                    log("Intervention \"" + interv + "\" triggered, instance: \"" + getMAdA().getInstance(interv) + "\"");
                }
            }
        }

        #endregion HelperMethods
        #region TestMethods

        //called before each testcase
        [TestInitialize]
        public void Initialize()
        {
            if (AssetManager.Instance.findAssetsByClass("MotivationAssessmentAsset").Count == 0)
            {
#warning change bridge implementation (in TestCaseCI.cs) for testing (IDataStoragePath and ILog - logging behaviour)
                //Adding the bridge
                AssetManager.Instance.Bridge = new Bridge();

                //creating the asset
                MotivationAssessmentAsset cia = new MotivationAssessmentAsset();
                MotivationBasedAdaptionAsset mbaa = new MotivationBasedAdaptionAsset();
                
            }
        }

        /// <summary>
        /// Method performing a simple test: creating example MotivationModel and feeding hints for a testplayer. 
        /// </summary>
        [TestMethod]
        public void performTest1()
        {
            log("***************TEST 1********************");

            DateTime now = DateTime.Now;

            //reaching a new level
            getMAsA().addMotivationHint(MotivationHintEnum.NewLevel);

            //perfect solution - 0 error - 0 help requests - 10 seconds
            getMAsA().addMotivationHint(MotivationHintEnum.NewProblem);

            log("Sleeping for 10 seconds...");
            Thread.Sleep(10 * 1000);
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
        [TestMethod]
        public void performTest2()
        {
            log("***************TEST 2********************");

            MotivationModel mm = createExampleMM();
            string id = "MotivationAssessmentTestId.xml";
            writeMMToFile(id, mm);

            MotivationAssessmentAssetSettings maass = new MotivationAssessmentAssetSettings();
            maass.XMLLoadingId = id;
            getMAsA().Settings = maass;
            MotivationModel mm2 = getMAsA().loadMotivationModel();

            if (mm.toXmlString().Equals(mm2.toXmlString()))
                log("MotivationModels before and after the loading are identically!");
            else
                log("MotivationModels before and after the loading are NOT identically!", Severity.Error);

        }

        /// <summary>
        /// Performing successive motivation update
        /// </summary>
        [TestMethod]
        public void performtest3()
        {
            log("Start Test 1:");

            string[] hintArray = { "p", "h", "f", "h", "s", "l" };
            List<string> hints = new List<string>(hintArray);
            int[] workingTimesArray = { 0, 1, 2, 1, 1, 0 };
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
                        log("Sleeping for " + workingTimesArray[pos] + " second(s)...");
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
                while (status == 2 && pos < hintArray.Length)
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

    class Bridge : IBridge, ILog, IDataStorage, IWebServiceRequest/*, ISerializer, IVirtualProperties*/
    {
        string IDataStoragePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); //@"C:\Users\mmaurer\Desktop\rageCsFiles\";

        #region IDataStorage

        public bool Delete(string fileId)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string fileId)
        {
#warning Change DataStorage-path if needed in Class Bridge, Variable IDataStoragePath
            string filePath = IDataStoragePath + fileId;
            return (File.Exists(filePath));
        }

        public string[] Files()
        {
            throw new NotImplementedException();
        }

        public string Load(string fileId)
        {
#warning Change Loading-path if needed in Class Bridge, Variable IDataStoragePath
            string filePath = IDataStoragePath + fileId;
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(filePath))
                {
                    // Read the stream to a string, and write the string to the console.
                    String line = sr.ReadToEnd();
                    return (line);
                }
            }
            catch (Exception e)
            {
                Log(Severity.Error, e.Message);
                Log(Severity.Error, "Error by loading the DM! - Maybe you need to change the path: \"" + IDataStoragePath + "\"");
            }

            return (null);
        }

        public void Save(string fileId, string fileData)
        {
#warning Change Saving-path if needed in Class Bridge, Variable IDataStoragePath
            string filePath = IDataStoragePath + fileId;
            using (StreamWriter file = new StreamWriter(filePath))
            {
                file.Write(fileData);
            }
        }

        #endregion IDataStorage

        #region ILog

        public void Log(Severity severity, string msg)
        {
            Console.WriteLine("BRIDGE:  " + msg);
        }

        #endregion ILog    
        #region IWebServiceRequest

        /*
        public void WebServiceRequest(RequestSetttings requestSettings, out RequestResponse requestResponse)
        {
            string url = requestSettings.uri.AbsoluteUri;

            if (string.Equals(requestSettings.method, "get", StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestSettings.uri);
                    HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse();
                    Stream resStream = webResponse.GetResponseStream();

                    Dictionary<string, string> responseHeader = new Dictionary<string, string>();
                    foreach (string key in webResponse.Headers.AllKeys)
                        responseHeader.Add(key, webResponse.Headers[key]);

                    StreamReader reader = new StreamReader(resStream);
                    string dm = reader.ReadToEnd();

                    requestResponse = new RequestResponse();
                    requestResponse.method = requestSettings.method;
                    requestResponse.requestHeaders = requestSettings.requestHeaders;
                    requestResponse.responseCode = (int)webResponse.StatusCode;
                    requestResponse.responseHeaders = responseHeader;
                    requestResponse.responsMessage = dm;
                    requestResponse.body = dm;
                    requestResponse.uri = requestSettings.uri;
                }
                catch (Exception e)
                {
                    requestResponse = new RequestResponse();
                    requestResponse.method = requestSettings.method;
                    requestResponse.requestHeaders = requestSettings.requestHeaders;
                    requestResponse.responsMessage = e.Message;
                    requestResponse.uri = requestSettings.uri;
                    Log(Severity.Error,"Exception: " + e.Message);
                }
            }
            else if (string.Equals(requestSettings.method, "post", StringComparison.CurrentCultureIgnoreCase))
            { //http://stackoverflow.com/questions/4015324/http-request-with-post
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(requestSettings.uri);

                    var data = Encoding.ASCII.GetBytes(requestSettings.body);

                    request.Method = "POST";
                    //request.ContentType = "text/plain";
                    request.ContentType = "application/json";
                    //request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = data.Length;

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        string json = requestSettings.body;

                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    
                    
                    var responsePost = (HttpWebResponse)request.GetResponse();
                    
                    var responseString = new StreamReader(responsePost.GetResponseStream()).ReadToEnd();
                    

                    Dictionary<string, string> responseHeader = new Dictionary<string, string>();
                    foreach (string key in responsePost.Headers.AllKeys)
                        responseHeader.Add(key, responsePost.Headers[key]);
                    

                    requestResponse = new RequestResponse();
                    requestResponse.method = requestSettings.method;
                    requestResponse.requestHeaders = requestSettings.requestHeaders;
                    requestResponse.responseCode = (int)responsePost.StatusCode;
                    requestResponse.responseHeaders = responseHeader;
                    requestResponse.responsMessage = responsePost.StatusDescription;
                    requestResponse.body = responseString;
                    requestResponse.uri = requestSettings.uri;
                }
                catch (Exception e)
                {
                    requestResponse = new RequestResponse();
                    requestResponse.method = requestSettings.method;
                    requestResponse.requestHeaders = requestSettings.requestHeaders;
                    requestResponse.responsMessage = e.Message;
                    requestResponse.uri = requestSettings.uri;
                    Log(Severity.Error, "Exception: " +e.Message);
                    
                }
            }
            else if (string.Equals(requestSettings.method, "put", StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {

                    var request = (HttpWebRequest)WebRequest.Create(requestSettings.uri);
                    request.ContentType = "text/json";
                    request.Method = "PUT";
                    request.ContentLength = Encoding.ASCII.GetBytes(requestSettings.body).Length;

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        string json = requestSettings.body;

                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    var httpResponse = (HttpWebResponse)request.GetResponse();
                    var responseString = new StreamReader(httpResponse.GetResponseStream()).ReadToEnd();


                    Dictionary<string, string> responseHeader = new Dictionary<string, string>();
                    foreach (string key in httpResponse.Headers.AllKeys)
                        responseHeader.Add(key, httpResponse.Headers[key]);


                    requestResponse = new RequestResponse();
                    requestResponse.method = requestSettings.method;
                    requestResponse.requestHeaders = requestSettings.requestHeaders;
                    requestResponse.responseCode = (int)httpResponse.StatusCode;
                    requestResponse.responseHeaders = responseHeader;
                    requestResponse.responsMessage = httpResponse.StatusDescription;
                    requestResponse.body = responseString;
                    requestResponse.uri = requestSettings.uri;
                }
                catch (Exception e)
                {
                    requestResponse = new RequestResponse();
                    requestResponse.method = requestSettings.method;
                    requestResponse.requestHeaders = requestSettings.requestHeaders;
                    requestResponse.responsMessage = e.Message;
                    requestResponse.uri = requestSettings.uri;
                    Log(Severity.Error, "Exception: " + e.Message);

                }
            }
            else
            {
                requestResponse = new RequestResponse();
                requestResponse.method = requestSettings.method;
                requestResponse.requestHeaders = requestSettings.requestHeaders;
                requestResponse.responsMessage = "FAIL request type unknown";
                requestResponse.uri = requestSettings.uri;
                Log(Severity.Error,"request type unknown!");
            }
        }
        */
        #endregion IWebServiceRequest
        #region IWebServiceRequest Members

        // See http://stackoverflow.com/questions/12224602/a-method-for-making-http-requests-on-unity-ios
        // for persistence.
        // See http://18and5.blogspot.com.es/2014/05/mono-unity3d-c-https-httpwebrequest.html

#if ASYNC
        public void WebServiceRequest(RequestSetttings requestSettings, out RequestResponse requestReponse)
        {
            // Wrap the actual method in a Task. Neccesary because we cannot:
            // 1) Make this method async (out is not allowed) 
            // 2) Return a Task<RequestResponse> as it breaks the interface (only void does not break it).
            //
            Task<RequestResponse> taskName = Task.Factory.StartNew<RequestResponse>(() =>
            {
                return WebServiceRequestAsync(requestSettings).Result;
            });

            requestReponse = taskName.Result;
        }

        /// <summary>
        /// Web service request.
        /// </summary>
        ///
        /// <param name="requestSettings"> Options for controlling the operation. </param>
        ///
        /// <returns>
        /// A RequestResponse.
        /// </returns>
        private async Task<RequestResponse> WebServiceRequestAsync(RequestSetttings requestSettings)
#else
        /// <summary>
        /// Web service request.
        /// </summary>
        ///
        /// <param name="requestSettings">  Options for controlling the operation. </param>
        /// <param name="requestResponse"> The request response. </param>
        public void WebServiceRequest(RequestSetttings requestSettings, out RequestResponse requestResponse)
        {
            requestResponse = WebServiceRequest(requestSettings);
        }

        /// <summary>
        /// Web service request.
        /// </summary>
        ///
        /// <param name="requestSettings">Options for controlling the operation. </param>
        ///
        /// <returns>
        /// A RequestResponse.
        /// </returns>
        private RequestResponse WebServiceRequest(RequestSetttings requestSettings)
#endif
        {
            RequestResponse result = new RequestResponse(requestSettings);

            try
            {
                //! Might throw a silent System.IOException on .NET 3.5 (sync).
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestSettings.uri);

                request.Method = requestSettings.method;

                // Both Accept and Content-Type are not allowed as Headers in a HttpWebRequest.
                // They need to be assigned to a matching property.

                if (requestSettings.requestHeaders.ContainsKey("Accept"))
                {
                    request.Accept = requestSettings.requestHeaders["Accept"];
                }

                if (!String.IsNullOrEmpty(requestSettings.body))
                {
                    byte[] data = Encoding.UTF8.GetBytes(requestSettings.body);

                    if (requestSettings.requestHeaders.ContainsKey("Content-Type"))
                    {
                        request.ContentType = requestSettings.requestHeaders["Content-Type"];
                    }

                    foreach (KeyValuePair<string, string> kvp in requestSettings.requestHeaders)
                    {
                        if (kvp.Key.Equals("Accept") || kvp.Key.Equals("Content-Type"))
                        {
                            continue;
                        }
                        request.Headers.Add(kvp.Key, kvp.Value);
                    }

                    request.ContentLength = data.Length;

                    // See https://msdn.microsoft.com/en-us/library/system.net.servicepoint.expect100continue(v=vs.110).aspx
                    // A2 currently does not support this 100-Continue response for POST requets.
                    request.ServicePoint.Expect100Continue = false;

#if ASYNC
                    Stream stream = await request.GetRequestStreamAsync();
                    await stream.WriteAsync(data, 0, data.Length);
                    stream.Close();
#else
                    Stream stream = request.GetRequestStream();
                    stream.Write(data, 0, data.Length);
                    stream.Close();
#endif
                }
                else
                {
                    foreach (KeyValuePair<string, string> kvp in requestSettings.requestHeaders)
                    {
                        if (kvp.Key.Equals("Accept") || kvp.Key.Equals("Content-Type"))
                        {
                            continue;
                        }
                        request.Headers.Add(kvp.Key, kvp.Value);
                    }
                }

#if ASYNC
                WebResponse response = await request.GetResponseAsync();
#else
                WebResponse response = request.GetResponse();
#endif
                if (response.Headers.HasKeys())
                {
                    foreach (string key in response.Headers.AllKeys)
                    {
                        result.responseHeaders.Add(key, response.Headers.Get(key));
                    }
                }

                result.responseCode = (int)(response as HttpWebResponse).StatusCode;

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
#if ASYNC
                    result.body = await reader.ReadToEndAsync();
#else
                    result.body = reader.ReadToEnd();
#endif
                }
            }
            catch (Exception e)
            {
                result.responsMessage = e.Message;

                Log(Severity.Error, String.Format("{0} - {1}", e.GetType().Name, e.Message));
            }

            return result;
        }

        #endregion IWebServiceRequest Members
        /*
        #region ISerializer

        public bool Supports(SerializingFormat format)
        {
            switch (format)
            {
                //case SerializingFormat.Binary:
                //    return false;
                case SerializingFormat.Xml:
                    return false;
                case SerializingFormat.Json:
                    return true;
            }

            return false;
        }

        public object Deserialize<T>(string text, SerializingFormat format)
        {
            return JsonConvert.DeserializeObject(text, typeof(T));
        }

        public string Serialize(object obj, SerializingFormat format)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        #endregion ISerializer
        #region IVirtualProperties Members

        /// <summary>
        /// Looks up a given key to find its associated value.
        /// </summary>
        ///
        /// <param name="model"> The model. </param>
        /// <param name="key">   The key. </param>
        ///
        /// <returns>
        /// An Object.
        /// </returns>
        public object LookupValue(string model, string key)
        {
            if (key.Equals("Virtual"))
            {
                return DateTime.Now;
            }

            return null;
        }

        #endregion IVirtualProperties Members
        */
    }
}
