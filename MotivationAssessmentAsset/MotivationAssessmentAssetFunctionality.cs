using AssetManagerPackage;
using AssetPackage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MotivationAssessmentAssetNameSpace
{

    /// <summary>
    /// Singelton Class for handling MotivationAssessment
    /// </summary>
    internal class MotivationAssessmentHandler
    {
        #region AlgorithParameters

        /// <summary>
        /// List containing allowed math operation signs for the updating.
        /// </summary>
        private static string[] arrayMathOperationSigns = { "-", "+", "*", "/", "(", ")" };
        private List<String> mathOperationSigns = new List<String>(arrayMathOperationSigns);

        /// <summary>
        /// List containing primary motivation aspects.
        /// </summary>
        private static string[] arrayPrimaryMotivationAspects = { "attention", "satisfaction", "confidence" };
        private List<String> primaryMotivationAspects = new List<String>(arrayPrimaryMotivationAspects);

        /// <summary>
        /// List containing motivation hint ids which end a hint series.
        /// </summary>
        private static string[] arrayHintsEnd = { "success", "new level" };
        private List<String> hintEnds = new List<String>(arrayHintsEnd);

        /// <summary>
        /// List containing valid motivation hint ids.
        /// </summary>
        private static string[] arrayValidHints = { "help", "fail", "success", "new level", "new problem" };
        private List<String> validHints = new List<String>(arrayValidHints);

        /// <summary>
        /// Describes the minimum needed reading-time assumed for the task in seconds. 
        /// </summary>
        private double firstTryMinDuration = 6;

        /// <summary>
        /// Describes the maximum needed solving time assumed for the task in seconds.
        /// </summary>
        private double solutionMaxDuration = 12;

        /// <summary>
        /// Descibes the maximum number of errors accepted when solving the task.
        /// </summary>
        private int maxNoErrors = 3;

        /// <summary>
        /// Descripes the maximum number of help requests accepted when solving this task.
        /// </summary>
        private int maxNoHelpRequests = 3;

        #endregion AlgorithParameters
        #region Fields

        /// <summary>
        /// Instance of the MotivationAssessmentAsset
        /// </summary>
        private MotivationAssessmentAsset motivationAssessmentAsset = null;

        /// <summary>
        /// Instance of the class MotivationAssessmentHandler - Singelton pattern
        /// </summary>
        private static MotivationAssessmentHandler instance;

        /// <summary>
        /// Storage of all motivation hints 
        /// </summary>
        private Dictionary<String, List<MotivationHint>> motivationHints = new Dictionary<String, List<MotivationHint>>();

        //TODO: Playermodel-storage?
        /// <summary>
        /// Local storage for motivation states of all players.
        /// </summary>
        private Dictionary<String, MotivationState> motivationStates = new Dictionary<String, MotivationState>();

        /// <summary>
        /// run-time asset storage of motivation models
        /// </summary>
        private Dictionary<String, MotivationModel> motivationModels = new Dictionary<string, MotivationModel>();

        /// <summary>
        /// If true, logging is done.
        /// </summary>
        private Boolean doLogging = true;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// private MotivationAssessmentHandler-ctor for Singelton-pattern 
        /// </summary>
        private MotivationAssessmentHandler() { }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Getter for Instance of the MotivationAssessmentHandler - Singelton pattern
        /// </summary>
        public static MotivationAssessmentHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MotivationAssessmentHandler();
                }
                return instance;
            }
        }

        #endregion Properties
        //TODO: storage of data via game-engine
        //TODO: WEB-requests via AssetManager interfaces
        //TODO: loading always the exampleMotivationModel
        #region InternalMethods


        /// <summary>
        /// Method returning an instance of the MotivationAssessmentAsset.
        /// </summary>
        /// <returns> Instance of the MotivationAssessmentAsset </returns>
        internal MotivationAssessmentAsset getMAsA()
        {
            if (motivationAssessmentAsset == null)
                motivationAssessmentAsset = (MotivationAssessmentAsset)AssetManager.Instance.findAssetByClass("MotivationAssessmentAsset");
            return (motivationAssessmentAsset);
        }

        /// <summary>
        /// Getter for primary motivation aspects.
        /// </summary>
        /// <returns> Primary motivation aspects List. </returns>
        internal List<String> getPrimaryMotivationAspects()
        {
            return primaryMotivationAspects;
        }

        /// <summary>
        /// Method for deserialization of a XML-String to the coressponding MotivationModel.
        /// </summary>
        /// 
        /// <param name="str"> String containing the XML-MotivationModel for deserialization. </param>
        ///
        /// <returns>
        /// MotivationModel-type coressponding to the parameter "str" after deserialization.
        /// </returns>
        internal MotivationModel getMMFromXmlString(String str)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(MotivationModel));
            using (TextReader reader = new StringReader(str))
            {
                MotivationModel result = (MotivationModel)serializer.Deserialize(reader);
                return (result);
            }
        }

        /// <summary>
        /// Method for reading a file containing the XML-MotivationModel and returning the coressponding MotivationModel.
        /// </summary>
        /// 
        /// <param name="filePath"> String containing the file path. </param>
        ///
        /// <returns>
        /// MotivationModel-type coressponding to the XML-String in the file.
        /// </returns>
        internal MotivationModel getMMFromFile(String fileId)
        {
            IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
            if (ids != null)
            {
                loggingMAs("Loading motivation model from File.");
                return (this.getMMFromXmlString(ids.Load(fileId)));
            }
            else
            {
                loggingMAs("Loading of motivation model from file not possible.",Severity.Warning);
                return null;
            }
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
                loggingMAs("Storing DomainModel to File.");
                ids.Save(fileId, mm.toXmlString());
            }
            else
                loggingMAs("No IDataStorage - Bridge implemented!", Severity.Error);
        }

        /*
        /// <summary>
        /// Method for requesting a XML-MotivationModel from a website and returning the coressponding MotivationModel.
        /// </summary>
        /// 
        /// <param name="url"> Website URL containing the MotivationModel. </param>
        ///
        /// <returns>
        /// MotivationModel-type coressponding to the XML-MotivationModel on the spezified website.
        /// </returns>
        internal MotivationModel getMMFromWeb(String url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream resStream = response.GetResponseStream();

            StreamReader reader = new StreamReader(resStream);
            string dm = reader.ReadToEnd();

            MotivationModel mm = getMMFromXmlString(dm);

            if (!validateMotivationModel(mm))
                loggingMAs("ERROR: Motivation model formulars not valid!");

            return (mm);
        }
        */

        /// <summary>
        /// Method for storing a MotivationModel as XML in a File.
        /// </summary>
        /// 
        /// <param name="dm"> MotivationModel to store. </param>
        /// <param name="pathToFile"> String containing the file path. </param>
        internal void writeMMToFile(MotivationModel mm, String fileId)
        {
            IDataStorage ids = (IDataStorage)AssetManager.Instance.Bridge;
            if (ids != null)
            {
                loggingMAs("Storing motivation model to File.");
                ids.Save(fileId, mm.toXmlString());
            }
            else
                loggingMAs("No IDataStorage - Bridge implemented!", Severity.Warning);

        }

        /// <summary>
        /// This method validates the motivation model - the variables in the formulas are checked (Are they all motivation aspects?).
        /// </summary>
        /// 
        /// <param name="MotivationModel"> The motivation model to validate. </param>
        /// 
        /// <returns> True if no error was found in the formulas, false otherwise. </returns>
        internal Boolean validateMotivationModel(MotivationModel mm)
        {
            List<String> motivationAspects = new List<String>();
            foreach (MotivationAspect ma in mm.motivationAspects.motivationAspectList)
                motivationAspects.Add(ma.name);

            Boolean valid;
            foreach (MotivationAspect ma in mm.motivationAspects.motivationAspectList)
            {
                if (primaryMotivationAspects.Contains(ma.name))
                    valid = ruleValid(ma.down + "+" + ma.up, motivationAspects);
                else
                    valid = ruleValid(ma.rule, motivationAspects);

                if (!valid)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Method validating a single formular within the motivation model.
        /// </summary>
        /// 
        /// <param name="formula"> Formula to evaluate. </param>
        /// <param name="motivationAspects"> List of valid variable names within the formula. </param>
        /// 
        /// <returns> True if the formula is valid, false otherwise. </returns>
        internal Boolean ruleValid(string formula, List<string> motivationAspects)
        {
            string letter = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string[] letterList = new string[letter.Length];
            for (int i = 0; i < letterList.Length; i++)
            {
                letterList[i] = letter[i].ToString();
            }

            string[] formulaList = new string[formula.Length];
            for (int i = 0; i < formula.Length; i++)
            {
                formulaList[i] = formula[i].ToString();
            }

            int pos = 0;

            while (pos <= formulaList.Length)
            {
                if (mathOperationSigns.Contains(formulaList[pos]) || formulaList[pos].Equals(" "))
                    pos++;
                else if (letterList.Contains(formulaList[pos]))
                {
                    String variable = formulaList[pos];
                    int add = 1;
                    while (letterList.Contains(formulaList[pos + add]))
                    {
                        variable += formulaList[pos + add];
                        add++;
                    }

                    if (motivationAspects.Contains(variable))
                        pos = pos + add - 1;
                    else
                    {
                        loggingMAs("ERROR: unknown motivation aspect " + variable + " in formula!");
                        return false;
                    }
                }
                else
                {
                    loggingMAs("ERROR: unknown symbole " + formulaList[pos] + " in formula!");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Methode generating a motivation evidence out of a motivation hint series and processing this evidence further.
        /// </summary>
        /// 
        /// <param name="mhs"> List of Motivation hints forming a series. </param>
        internal void evaluateHintSeries(List<MotivationHint> mhs)
        {
            loggingMAs("Hint series complete - start evaluating series.");

            MotivationEvidence me = new MotivationEvidence();
            me.PlayerId = mhs[0].PlayerId;

            if (mhs[mhs.Count - 1].HintId == "new level")
            {
                me.EvidenceType = EvidenceType.LevelReached;
            }
            else if (mhs[mhs.Count - 1].HintId == "success")
            {
                me.EvidenceType = EvidenceType.ProblemSolved;

                TimeSpan ts = mhs[mhs.Count - 1].OccurTime - mhs[0].OccurTime;
                me.SolvingDuration = ts.TotalSeconds;

                ts = mhs[1].OccurTime - mhs[0].OccurTime;
                me.FirstTryDuration = ts.TotalSeconds;

                me.NoOfErrors = 0;
                me.NoOfHelpRequests = 0;

                foreach (MotivationHint mh in mhs)
                {
                    if (mh.HintId.Equals("fail"))
                        me.NoOfErrors++;
                    else if (mh.HintId.Equals("help"))
                        me.NoOfHelpRequests++;
                }

            }

            updateMotivationState(me);
        }

        /// <summary>
        /// Updates the motivation state of a player with an motivation evidence.
        /// </summary>
        /// 
        /// <param name="me"> Motivation evidence for the update. </param>
        internal void updateMotivationState(MotivationEvidence me)
        {
            loggingMAs("Start updating motivation state.");

            MotivationState currentMs = getMotivationState(me.PlayerId);
            MotivationState newMs = currentMs.getCopy();

            if (me.EvidenceType == EvidenceType.LevelReached)
            {
                updatePrimaryMotivationAspect(newMs, "satisfaction", true, me.PlayerId);
            }
            else if (me.EvidenceType == EvidenceType.ProblemSolved)
            {
                if (me.FirstTryDuration < this.firstTryMinDuration)
                    updatePrimaryMotivationAspect(newMs, "attention", false, me.PlayerId);
                else {
                    if (me.SolvingDuration > this.solutionMaxDuration)
                        updatePrimaryMotivationAspect(newMs, "attention", false, me.PlayerId);
                    else
                        updatePrimaryMotivationAspect(newMs, "attention", true, me.PlayerId);

                    if (me.NoOfErrors > this.maxNoErrors || me.NoOfHelpRequests > this.maxNoHelpRequests)
                        updatePrimaryMotivationAspect(newMs, "confidence", false, me.PlayerId);
                    else
                        updatePrimaryMotivationAspect(newMs, "confidence", true, me.PlayerId);
                }
            }
            else
            {
                loggingMAs("Warning: Evidence Type unknown!", Severity.Warning);
            }
            setMotivationState(me.PlayerId, newMs);
            updateSecondaryMotivationAspects(newMs, me.PlayerId);
            newMs.print();
            setMotivationState(me.PlayerId, newMs);
        }

        /// <summary>
        /// Method for updating primary motivation Aspect (attention/satisfaction/confidence)
        /// </summary>
        /// 
        ///<param name="ms"> Motivation state for storing the new values. </param>
        ///<param name="aspect"> String containing "attention","satisfaction" or "confidence". Describes which component gets updated. </param>
        ///<param name="direction"> Boolean - if true upgrade, else downgrade is done. </param>
        ///<param name="playerId"> Identification of the player. </param>
        internal void updatePrimaryMotivationAspect(MotivationState ms, String aspect, Boolean direction, String playerId)
        {
            MotivationModel mm = ms.getMotivationModel();
            MotivationAspect ma = mm.motivationAspects.getMotivationAspectByName(aspect);
            String expression = direction ? ma.up : ma.down;
            try
            {
                double sol = FormulaInterpreter.eval(expression, playerId);
                ms.updateMotivationAspect(aspect, sol);
            }
            catch (Exception e)
            {
                loggingMAs("Warning: Update for primary motivation aspects not done.", Severity.Warning);
                loggingMAs("Exception caught: " + e.Message);
            }
        }

        /// <summary>
        /// Method for updating secondary motivation Aspect ( != attention/satisfaction/confidence)
        /// </summary>
        /// 
        /// <param name="ms"> Motivation state for storing the new values. </param>
        /// <param name="playerId"> Identification of the player. </param>
        internal void updateSecondaryMotivationAspects(MotivationState ms, String playerId)
        {
            MotivationModel mm = ms.getMotivationModel();

            foreach (MotivationAspect ma in mm.motivationAspects.motivationAspectList)
            {
                if (!primaryMotivationAspects.Contains(ma.name))
                {
                    if (ms.getMotivation().ContainsKey(ma.name))
                    {
                        try
                        {
                            double sol = FormulaInterpreter.eval(ma.rule, playerId);
                            ms.updateMotivationAspect(ma.name, sol);
                        }
                        catch (Exception e)
                        {
                            loggingMAs("Warning: Update for secondary motivation aspects not done.", Severity.Warning);
                            loggingMAs("Exception caught: " + e.Message);
                        }
                    }
                    else
                        loggingMAs("Warning: Motivation aspect not found!",Severity.Warning);
                }
            }
        }

        //TODO: Where to get the MM from?
        /// <summary>
        /// Method for loading default motivation model - specified by asset settings
        /// </summary>
        /// <returns></returns>
        internal MotivationModel loadDefaultMotivationModel()
        {
            return createExampleMM();
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Method for adding a new motivation hint and processing aggregated hints in case a evidence can be calculated.
        /// </summary>
        /// 
        ///<param name="mh"> Motivation hint added to already existing motivation hints. </param>
        internal void addMotivationHint(MotivationHint mh)
        {
            loggingMAs("New Motivation hint added.");

            if (!validHints.Contains(mh.HintId))
            {
                loggingMAs("Warning:  HINT ID NOT VALID - HINT IGNORED",Severity.Warning);
                return;
            }

            if (motivationHints.Keys.Contains(mh.PlayerId))
            {
                motivationHints[mh.PlayerId].Add(mh);
            }
            else
            {
                motivationHints[mh.PlayerId] = new List<MotivationHint>();
                motivationHints[mh.PlayerId].Add(mh);
            }

            if (hintEnds.Contains(mh.HintId))
            {
                List<MotivationHint> mhs = motivationHints[mh.PlayerId];
                evaluateHintSeries(mhs);
                motivationHints.Remove(mh.PlayerId);
            }
        }

        #endregion InternalMethods
        #region PublicMethods

        /// <summary>
        /// Method for loading the motivation model for a certain player.
        /// </summary>
        /// 
        /// <param name="playerId"> Identification of player for which the MM is loaded. </param>
        /// 
        /// <returns> The motivation model for the specified player. </returns>
        public MotivationModel getMotivationModel(String playerId)
        {
            if (motivationModels.ContainsKey(playerId))
                return motivationModels[playerId];
            MotivationModel mm = loadDefaultMotivationModel();
            motivationModels[playerId] = mm;
            return mm;
        }

        /// <summary>
        /// Returns the Motivation State of a player when provided with player-Id.
        /// </summary>
        /// 
        /// <param name="playerId"> Identifier of the player. </param>
        /// 
        /// <returns> Motivation state of the specified player. </returns>
        public MotivationState getMotivationState(String playerId)
        {
            MotivationState ms;
            if (motivationStates.ContainsKey(playerId))
                ms = motivationStates[playerId];
            else
                ms = new MotivationState(getMotivationModel(playerId));

            return ms;
        }

        /// <summary>
        /// Method for saving a updated motivation state.
        /// </summary>
        /// 
        /// <param name="playerId"> Identifier of the player. </param>
        /// <param name="ms"> MotivationState to be stored. </param>
        public void setMotivationState(String playerId, MotivationState ms)
        {
            motivationStates[playerId] = ms;
        }

        /// <summary>
        /// Method for adding a new motivation hint and processing aggregated hints in case a evidence can be calculated.
        /// </summary>
        /// 
        ///<param name="hintId"> String specifying the hint. </param>
        ///<param name="playerId"> String specifying the player. </param>
        public void addMotivationHint(String hintId, String playerId)
        {
            MotivationHint mh = new MotivationHint();
            mh.HintId = hintId;
            mh.PlayerId = playerId;
            addMotivationHint(mh);
        }

        #endregion PublicMethods
        #region TestMethods

        /// <summary>
        /// Method for logging (Diagnostics).
        /// </summary>
        /// 
        /// <param name="msg"> Message to be logged. </param>
        internal void loggingMAs(String msg, Severity severity = Severity.Information)
        {
            if (doLogging)
                getMAsA().Log(severity, "[MAsA]: " + msg);
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

            InterventionInstance ii11 = new InterventionInstance();
            ii11.instance = "Hey, my friend! Are you sleeping?!";
            InterventionInstance ii12 = new InterventionInstance();
            ii12.instance = "Sorry for interrupting you; we need to go on.";
            Intervention i1 = new Intervention();
            List<InterventionInstance> i1List = new List<InterventionInstance>();
            i1List.Add(ii11);
            i1List.Add(ii12);
            i1.interventionInstances = i1List;
            i1.name = "attention catcher";
            i1.rule = "attention < 0.4";

            InterventionInstance ii21 = new InterventionInstance();
            ii21.instance = "Once this mission is over, it's time to celebrate.";
            InterventionInstance ii22 = new InterventionInstance();
            ii22.instance = "By solving this task you will earn another 10000 points.";
            Intervention i2 = new Intervention();
            List<InterventionInstance> i2List = new List<InterventionInstance>();
            i2List.Add(ii21);
            i2List.Add(ii22);
            i2.interventionInstances = i2List;
            i2.name = "incitation intervention";
            i2.rule = "satisfaction < 0.4";

            InterventionInstance ii31 = new InterventionInstance();
            ii31.instance = "Don't give up. Try again.";
            InterventionInstance ii32 = new InterventionInstance();
            ii32.instance = "It is a challenge, I know. Let's give it another trial.";
            InterventionInstance ii33 = new InterventionInstance();
            ii33.instance = "Go on. Practice makes perfect.";
            Intervention i3 = new Intervention();
            List<InterventionInstance> i3List = new List<InterventionInstance>();
            i3List.Add(ii31);
            i3List.Add(ii32);
            i3List.Add(ii33);
            i3.interventionInstances = i3List;
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
        /// Method for calling all tests in this Class.
        /// </summary>
        public void performAllTests()
        {
            loggingMAs("*****************************************************************");
            loggingMAs("Calling all tests (MAsA):");
            performTest1();
            performTest2();
            loggingMAs("Tests - done!");
            loggingMAs("*****************************************************************");
        }

        /// <summary>
        /// Method performing a simple test: creating example MotivationModel and feeding hints for a testplayer. 
        /// </summary>
        internal void performTest1()
        {
            loggingMAs("***************TEST 1********************");

            String testPlayer = "testplayer";
            DateTime now = DateTime.Now;

            //reaching a new level
            MotivationHint mh1 = new MotivationHint();
            mh1.HintId = "new level";
            mh1.OccurTime = now;
            mh1.PlayerId = testPlayer;
            addMotivationHint(mh1);

            //perfect solution - 0 error - 0 help requests - 10 seconds
            MotivationHint mh2 = new MotivationHint();
            mh2.HintId = "new problem";
            mh2.OccurTime = now;
            mh2.PlayerId = testPlayer;
            addMotivationHint(mh2);

            MotivationHint mh3 = new MotivationHint();
            mh3.HintId = "success";
            mh3.OccurTime = now + new TimeSpan(0, 0, 0, 10, 0);
            mh3.PlayerId = testPlayer;
            addMotivationHint(mh3);

            //too early guess - 2 seconds
            MotivationHint mh4 = new MotivationHint();
            mh4.HintId = "new problem";
            mh4.OccurTime = now;
            mh4.PlayerId = testPlayer;
            addMotivationHint(mh4);

            MotivationHint mh5 = new MotivationHint();
            mh5.HintId = "success";
            mh5.OccurTime = now + new TimeSpan(0, 0, 0, 2, 0);
            mh5.PlayerId = testPlayer;
            addMotivationHint(mh5);

            //too many errors - 11/7 seconds - 4 errors - 0 help requests
            MotivationHint mh6 = new MotivationHint();
            mh6.HintId = "new problem";
            mh6.OccurTime = now;
            mh6.PlayerId = testPlayer;
            addMotivationHint(mh6);

            MotivationHint mh7 = new MotivationHint();
            mh7.HintId = "fail";
            mh7.OccurTime = now + new TimeSpan(0, 0, 0, 7, 0);
            mh7.PlayerId = testPlayer;
            addMotivationHint(mh7);

            MotivationHint mh8 = new MotivationHint();
            mh8.HintId = "fail";
            mh8.OccurTime = now + new TimeSpan(0, 0, 0, 8, 0);
            mh8.PlayerId = testPlayer;
            addMotivationHint(mh8);

            MotivationHint mh9 = new MotivationHint();
            mh9.HintId = "fail";
            mh9.OccurTime = now + new TimeSpan(0, 0, 0, 9, 0);
            mh9.PlayerId = testPlayer;
            addMotivationHint(mh9);

            MotivationHint mh10 = new MotivationHint();
            mh10.HintId = "fail";
            mh10.OccurTime = now + new TimeSpan(0, 0, 0, 10, 0);
            mh10.PlayerId = testPlayer;
            addMotivationHint(mh10);

            MotivationHint mh11 = new MotivationHint();
            mh11.HintId = "success";
            mh11.OccurTime = now + new TimeSpan(0, 0, 0, 11, 0);
            mh11.PlayerId = testPlayer;
            addMotivationHint(mh11);

            //too late solution - 15 seconds
            MotivationHint mh12 = new MotivationHint();
            mh12.HintId = "new problem";
            mh12.OccurTime = now;
            mh12.PlayerId = testPlayer;
            addMotivationHint(mh12);

            MotivationHint mh13 = new MotivationHint();
            mh13.HintId = "success";
            mh13.OccurTime = now + new TimeSpan(0, 0, 0, 20, 0);
            mh13.PlayerId = testPlayer;
            addMotivationHint(mh13);

        }

        /// <summary>
        /// Method performing a simple test: creating example MotivationModel - storing it in a file and reading from the file
        /// </summary>
        internal void performTest2()
        {
            loggingMAs("***************TEST 2********************");

            MotivationModel mm = createExampleMM();
            string id = "MotivationAssessmentTestId.xml";
            writeMMToFile(mm,id);
            MotivationModel mm2 = getMMFromFile(id);
            if (mm.toXmlString().Equals(mm2.toXmlString()))
                loggingMAs("MotivationModels before and after the loading are identically!");
            else
                loggingMAs("MotivationModels before and after the loading are NOT identically!",Severity.Error);
            
        }

        #endregion TestMethods

    }

    /// <summary>
    /// Classes for Serialization
    /// </summary>
    #region Serialization

    /// <summary>
    /// Class containing all MotivationModel data.
    /// </summary>
    [XmlRoot("motivationmodel")]
    public class MotivationModel
    {
        #region Fields

        /// <summary>
        /// Class storing all motivation aspects.
        /// </summary>
        [XmlElement("motivationaspects")]
        public MotivationAspectList motivationAspects { get; set; }

        /// <summary>
        /// Class containing all Interventions.
        /// </summary>
        [XmlElement("motivationinterventions")]
        public MotivationInterventionList motivationInterventions { get; set; }

        #endregion Fields
        #region Methods

        /// <summary>
        /// Method for converting a motivation model to a xml string.
        /// </summary>
        /// 
        ///<returns>
        /// A string representing the motivation model.
        /// </returns>
        public String toXmlString()
        {
            try
            {
                var xmlserializer = new XmlSerializer(typeof(MotivationModel));
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, this);
                    String xml = stringWriter.ToString();

                    return (xml);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred", ex);
            }
        }

        #endregion Methods
    }

    public class MotivationAspectList
    {
        #region Properties

        /// <summary>
        /// List of all motivational aspects.
        /// </summary>
        [XmlElement("motivationaspect")]
        public List<MotivationAspect> motivationAspectList { get; set; }

        #endregion Properties
        #region Methods

        internal MotivationAspect getMotivationAspectByName(String name)
        {
            foreach (MotivationAspect ma in this.motivationAspectList)
            {
                if (ma.name.Equals(name))
                    return (ma);
            }

            MotivationAssessmentHandler.Instance.loggingMAs("ERROR: Requested motivation aspect name not found!");
            return null;
        }

        #endregion Methods

    }

    public class MotivationAspect
    {
        #region Properties 

        /// <summary>
        /// Motivational Aspect name
        /// </summary>
        [XmlElement("name")]
        public String name { get; set; }

        /// <summary>
        /// Rule for upgrading Motivational Aspect.
        /// </summary>
        [XmlElement("up")]
        public String up { get; set; }

        /// <summary>
        /// Rule for downgrading Motivational Aspect.
        /// </summary>
        [XmlElement("down")]
        public String down { get; set; }

        /// <summary>
        /// Rule for calculating Motivational Aspect.
        /// </summary>
        [XmlElement("rule")]
        public String rule { get; set; }

        #endregion Properties
    }

    public class MotivationInterventionList
    {
        #region Properties

        /// <summary>
        /// List of all motivation interventions.
        /// </summary>
        [XmlElement("intervention")]
        public List<Intervention> motivationInterventionList { get; set; }

        #endregion Properties
    }

    public class Intervention
    {
        #region Properties

        /// <summary>
        /// Intervention name
        /// </summary>
        [XmlElement("name")]
        public String name { get; set; }

        /// <summary>
        /// Rule for applying Intervention.
        /// </summary>
        [XmlElement("rule")]
        public String rule { get; set; }

        /// <summary>
        /// List of intervention instances.
        /// </summary>
        [XmlElement("instances")]
        public List<InterventionInstance> interventionInstances { get; set; }

        #endregion Properties
    }

    public class InterventionInstance
    {
        #region Properties

        /// <summary>
        /// Intervention instance
        /// </summary>
        [XmlElement("insatnce")]
        public String instance { get; set; }

        #endregion Properties
    }

    #endregion Serialization

    /// <summary>
    /// Class for storing Motivational State
    /// </summary>
    public class MotivationState
    {
        #region Fields

        /// <summary>
        /// Dictionary containing motivation-name and the corresponding value (0<value<1)
        /// </summary>
        private Dictionary<String, double> motivation = new Dictionary<String, double>();

        /// <summary>
        /// MotivationModel used as base for this motivational state
        /// </summary>
        private MotivationModel motivationModel;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// Motivation state c-tor
        /// </summary>
        /// 
        /// <param name="mm"> MotivationModel for which this motivation state is created. </param>
        public MotivationState(MotivationModel mm)
        {
            motivationModel = mm;

            int counter = 0;
            foreach (MotivationAspect ma in mm.motivationAspects.motivationAspectList)
            {
                motivation[ma.name] = 0.5;
                if (MotivationAssessmentHandler.Instance.getPrimaryMotivationAspects().Contains(ma.name))
                    counter++;
            }

            if (counter != MotivationAssessmentHandler.Instance.getPrimaryMotivationAspects().Count)
                MotivationAssessmentHandler.Instance.loggingMAs("ERROR: MotivationalModel corrupted - at least one primary motivation aspect is missing!");

        }

        #endregion Constructors
        #region Methods

        /// <summary>
        /// Methode for setting a motivation aspect value by name.
        /// </summary>
        /// 
        /// <param name="motivationAspectName"> Name of the motivational aspect. </param>
        /// <param name="motivationAspectValue"> New value of the motivational aspect. </param>
        public void updateMotivationAspect(String motivationAspectName, double motivationAspectValue)
        {
            motivation[motivationAspectName] = motivationAspectValue;
        }

        /// <summary>
        /// Methode for getting a motivation aspect value by name.
        /// </summary>
        /// 
        /// <param name="motivationAspectName"> Name of the motivational aspect. </param>
        /// 
        ///<returns>
        /// Value of the motivational aspect.
        /// </returns>
        public double getMotivationAspectValue(String motivationAspectName)
        {
            return motivation[motivationAspectName];
        }

        /// <summary>
        /// Generates a copy of the motivation state.
        /// </summary>
        /// 
        /// <returns> A copy of the motivation state. </returns>
        internal MotivationState getCopy()
        {
            MotivationState ms = new MotivationState(this.motivationModel);
            foreach (KeyValuePair<String, double> entry in this.motivation)
                ms.updateMotivationAspect(entry.Key, entry.Value);

            return ms;
        }

        /// <summary>
        /// Motivation model getter.
        /// </summary>
        /// 
        /// <returns> Motivation model of this motivation state. </returns>
        public MotivationModel getMotivationModel()
        {
            return this.motivationModel;
        }

        /// <summary>
        /// Motivation getter.
        /// </summary>
        /// 
        /// <returns> Motivation names and values. </returns>
        public Dictionary<String, double> getMotivation()
        {
            return motivation;
        }

        /// <summary>
        /// Diagnostic method printing out motivation state.
        /// </summary>
        internal void print()
        {
            String msg = "";
            foreach (KeyValuePair<string, double> entry in motivation)
            {
                msg += "|" + entry.Key + "=" + entry.Value.ToString();
            }
            MotivationAssessmentHandler.Instance.loggingMAs(msg);
        }

        #endregion Methods
    }


    /// <summary>
    /// Defining all possible EvidenceTypes for the evaluation
    /// </summary>
    public enum EvidenceType
    {
        ProblemSolved,
        LevelReached
    }


    /// <summary>
    /// Class containing all information for updating the motivational state
    /// </summary>
    public class MotivationEvidence
    {
        #region Fields

        /// <summary>
        /// Specifies the type of evidence.
        /// </summary>
        private EvidenceType evidenceType;

        /// <summary>
        /// Identification of the player assoziated with the evidence.
        /// </summary>
        private String playerId;

        /// <summary>
        /// Duration needed for solving a posted problem in seconds.
        /// </summary>
        private double solvingDuration;

        /// <summary>
        /// Duration till first problem solve attemp was made in seconds.
        /// </summary>
        private double firstTryDuration;

        /// <summary>
        /// Number of errors during solving this problem.
        /// </summary>
        private int noOfErrors;

        /// <summary>
        /// Number of help requests during solving this problem.
        /// </summary>
        private int noOfHelpRequests;

        #endregion Fields
        #region Constructors
        #endregion Constructors
        #region Properties

        /// <summary>
        /// Access of the playerId Field
        /// </summary>
        public String PlayerId
        {
            get
            {
                return playerId;
            }
            set
            {
                this.playerId = value;
            }
        }

        /// <summary>
        /// Access of the solvingDuration Field
        /// </summary>
        public double SolvingDuration
        {
            get
            {
                return solvingDuration;
            }
            set
            {
                this.solvingDuration = value;
            }
        }


        /// <summary>
        /// Access of the firstTryDuration Field
        /// </summary>
        public double FirstTryDuration
        {
            get
            {
                return firstTryDuration;
            }
            set
            {
                this.firstTryDuration = value;
            }
        }


        /// <summary>
        /// Access of the noOfErrors Field
        /// </summary>
        public int NoOfErrors
        {
            get
            {
                return noOfErrors;
            }
            set
            {
                this.noOfErrors = value;
            }
        }


        /// <summary>
        /// Access of the noOfHelpRequests Field
        /// </summary>
        public int NoOfHelpRequests
        {
            get
            {
                return noOfHelpRequests;
            }
            set
            {
                this.noOfHelpRequests = value;
            }
        }

        /// <summary>
        /// Access of the evidenceType Field
        /// </summary>
        public EvidenceType EvidenceType
        {
            get
            {
                return evidenceType;
            }
            set
            {
                this.evidenceType = value;
            }
        }

        #endregion Properties
        #region Methods
        #endregion Methods
    }

    ///<summary>
    /// Class containing a hint towards a motivation evidence.
    /// </summary>
    public class MotivationHint
    {
        #region Fields

        /// <summary>
        /// Motivation Hint identifier
        /// </summary>
        private String hintId;

        /// <summary>
        /// Creation Time of this MotivationHint - approximation for occur-time of the hint
        /// </summary>
        private DateTime occurTime;

        /// <summary>
        /// Identifier of player producing the motivation hint.
        /// </summary>
        private String playerId;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// MotivationHint c-tor
        /// </summary>
        public MotivationHint()
        {
            occurTime = DateTime.Now;
        }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Access to field playerId.
        /// </summary>
        public String PlayerId
        {
            get
            {
                return playerId;
            }
            set
            {
                this.playerId = value;
            }
        }

        /// <summary>
        /// Access to field hintId.
        /// </summary>
        public String HintId
        {
            get
            {
                return hintId;
            }
            set
            {
                this.hintId = value;
            }
        }

        /// <summary>
        /// Access to field occurTime.
        /// </summary>
        public DateTime OccurTime
        {
            get
            {
                return occurTime;
            }
            set
            {
                this.occurTime = value;
            }
        }

        #endregion Properties
        #region Methods
        #endregion Methods
    }

    /// <summary>
    /// Class for executing formular interpretation.
    /// </summary>
    internal static class FormulaInterpreter
    {
        #region Methods

        /// <summary>
        /// Method for interpreting formulas.
        /// </summary>
        /// 
        /// <param name="expression"> Furmula String to interpret. </param>
        /// <param name="playerId"> Identifier of player for which the evaluation should be done. </param>
        /// 
        /// <returns> Double value - result of the interpreted input-string.</returns>
        internal static double eval(String expression, String playerId)
        {
            if (expression.Equals(""))
                MotivationAssessmentHandler.Instance.loggingMAs("ERROR: Empty expression for evaluation received!");
            MotivationAssessmentHandler.Instance.loggingMAs("FormulaInterpreter: expression to evaluate with variables=" + expression);
            System.Data.DataTable table = new System.Data.DataTable();
            return Convert.ToDouble(table.Compute(replaceVariables(expression, playerId), String.Empty));
        }

        /// <summary>
        /// Method for replacing motivation component variables with the current values.
        /// </summary>
        /// 
        /// <param name="expression"> String for which replacement should happen. </param>
        /// <param name="playerId"> Identifier of player for which the evaluation should be done. </param>
        /// 
        /// <returns> String without any motivation component variables. </returns>
        private static String replaceVariables(String expression, String playerId)
        {
            MotivationState ms = MotivationAssessmentHandler.Instance.getMotivationState(playerId);
            MotivationModel mm = ms.getMotivationModel();
            foreach (MotivationAspect ma in mm.motivationAspects.motivationAspectList)
            {
                expression = expression.Replace(ma.name, ms.getMotivationAspectValue(ma.name).ToString());
            }
            MotivationAssessmentHandler.Instance.loggingMAs("FormulaInterpreter: expression to evaluate without variables=" + expression);
            return expression;
        }

        #endregion Methods
    }
}
