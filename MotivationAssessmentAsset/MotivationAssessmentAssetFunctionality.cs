﻿/*
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
#if !PORTABLE
using System.Threading;
#endif

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

        #endregion AlgorithParameters
        #region Fields
        
        /// <summary>
        /// Instance of the tracker Asset
        /// </summary>
        private TrackerAsset tracker = null;

        /// <summary>
        /// Storage of all motivation hints 
        /// </summary>
        private List<MotivationHint> motivationHints = new List<MotivationHint>();

        //TODO: Playermodel-storage?
        /// <summary>
        /// Local storage for motivation states of the player.
        /// </summary>
        private  MotivationState motivationState = null;

        /// <summary>
        /// run-time asset storage of motivation models
        /// </summary>
        internal MotivationModel motivationModel = null;

        /// <summary>
        /// If true, logging is done.
        /// </summary>
        private Boolean doLogging = true;

        /// <summary>
        /// Last time an satisfaction update was done.
        /// </summary>
        private  DateTime lastTimeUpdated = DateTime.Now;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// private MotivationAssessmentHandler-ctor for Singelton-pattern 
        /// </summary>
        internal MotivationAssessmentHandler() { }

        #endregion Constructors
        #region Properties
        #endregion Properties
        #region InternalMethods
        
        /// <summary>
        /// Method returning an instance of the MotivationAssessmentAsset.
        /// </summary>
        /// <returns> Instance of the MotivationAssessmentAsset </returns>
        internal MotivationAssessmentAsset getMAsA()
        {
            return MotivationAssessmentAsset.Instance;
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
            IDataStorage ids = MotivationAssessmentAsset.Instance.getInterfaceFromAsset<IDataStorage>();
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

            if (mhs[mhs.Count - 1].HintId == "new level")
            {
                me.EvidenceType = EvidenceType.LevelReached;
                if (mhs.Count != 1)
                {
                    loggingMAs("Hint series corrupted! - no evaluation done.");
                    return;
                }
            }
            else if (mhs[mhs.Count - 1].HintId == "success")
            {
                if(mhs[0].HintId != "new problem")
                {
                    loggingMAs("Hint series corrupted! - no evaluation done.");
                    return;
                }


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
        /// Method downgrading satisfaction, if needed
        /// </summary>
        public void updateSatisfaction()
        {
            MotivationState currentMs = getMotivationState();
            MotivationState newMs = currentMs.getCopy();

            if(checkSatisfactionDowngrade(newMs))         
                storeNewMotivationState(newMs);
        }

        /// <summary>
        /// Updates the motivation state of a player with an motivation evidence.
        /// </summary>
        /// 
        /// <param name="me"> Motivation evidence for the update. </param>
        internal void updateMotivationState(MotivationEvidence me)
        {
            loggingMAs("Start updating motivation state.");

            MotivationState currentMs = getMotivationState();
            MotivationState newMs = currentMs.getCopy();

            MotivationAssessmentAssetSettings maas = (MotivationAssessmentAssetSettings) getMAsA().Settings;

            if (me.EvidenceType == EvidenceType.LevelReached)
            {
                updatePrimaryMotivationAspect(newMs, "satisfaction", true);
                lastTimeUpdated = DateTime.Now;
            }
            else if (me.EvidenceType == EvidenceType.ProblemSolved)
            {
                if (me.FirstTryDuration < maas.FirstTryMinDuration)
                    updatePrimaryMotivationAspect(newMs, "attention", false);
                else {
                    if (me.SolvingDuration > maas.SolutionMaxDuration)
                        updatePrimaryMotivationAspect(newMs, "attention", false);
                    else
                        updatePrimaryMotivationAspect(newMs, "attention", true);

                    if (me.NoOfErrors > maas.MaxNoErrors || me.NoOfHelpRequests > maas.MaxNoHelpRequests)
                        updatePrimaryMotivationAspect(newMs, "confidence", false);
                    else
                        updatePrimaryMotivationAspect(newMs, "confidence", true);
                }
            }
            else
            {
                loggingMAs("Warning: Evidence Type unknown!", Severity.Warning);
            }

            //downgrade satisfaction, if too much time passed by
            checkSatisfactionDowngrade(newMs);

            //Method for storing changes
            storeNewMotivationState(newMs);
        }

        /// <summary>
        /// Method for storing new calculated motivation state
        /// </summary>
        /// <param name="newMs"> state to store</param>
        internal void storeNewMotivationState(MotivationState newMs)
        {
            setMotivationState(newMs);
            updateSecondaryMotivationAspects(newMs);
            newMs.print();
            setMotivationState(newMs);
        }

        /// <summary>
        /// Method for updating the motivation state aspect satisfaction due to 'too much time no new level'
        /// </summary>
        /// <param name="newMs"> the new motivation state</param>
        private bool checkSatisfactionDowngrade(MotivationState newMs)
        {
            MotivationAssessmentAssetSettings maas = (MotivationAssessmentAssetSettings)getMAsA().Settings;

            if (lastTimeUpdated.AddSeconds(maas.SatisfactionDowngradeTime) < DateTime.Now)
            {
                updatePrimaryMotivationAspect(newMs, "satisfaction", false);
                lastTimeUpdated = DateTime.Now;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Method for updating primary motivation Aspect (attention/satisfaction/confidence)
        /// </summary>
        /// 
        ///<param name="ms"> Motivation state for storing the new values. </param>
        ///<param name="aspect"> String containing "attention","satisfaction" or "confidence". Describes which component gets updated. </param>
        ///<param name="direction"> Boolean - if true upgrade, else downgrade is done. </param>
        ///<param name="playerId"> Identification of the player. </param>
        internal void updatePrimaryMotivationAspect(MotivationState ms, String aspect, Boolean direction)
        {
            MotivationModel mm = ms.getMotivationModel();
            MotivationAspect ma = mm.motivationAspects.getMotivationAspectByName(aspect);
            String expression = direction ? ma.up : ma.down;
            try
            {
                double sol = FormulaInterpreter.eval(expression);
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
        internal void updateSecondaryMotivationAspects(MotivationState ms)
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

                            double sol = FormulaInterpreter.eval(ma.rule);
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
        
        /// <summary>
        /// Method for loading default motivation model - specified by asset settings
        /// </summary>
        /// <returns></returns>
        internal MotivationModel loadDefaultMotivationModel()
        {
            
            loggingMAs("Loading default Domain model.");
            MotivationAssessmentAssetSettings maas = (MotivationAssessmentAssetSettings)getMAsA().Settings;
            
            IDataStorage ids = MotivationAssessmentAsset.Instance.getInterfaceFromAsset<IDataStorage>();
            if (ids != null )
            {
                if (!ids.Exists(maas.XMLLoadingId))
                {
                    loggingMAs("File "+ maas.XMLLoadingId + " not found for loading Motivation model.", Severity.Error);
                    //throw new Exception("EXCEPTION: File "+ maas.XMLLoadingId + " not found for loading Motivation model.") ;
                    return null;
                }

                loggingMAs("Loading Motivation model from File.");
                return (this.getMMFromXmlString(ids.Load(maas.XMLLoadingId)));
            }
            else
            {
                loggingMAs("IDataStorage bridge absent for requested local loading method of the Motivation model.", Severity.Error);
                //throw new Exception("EXCEPTION: IDataStorage bridge absent for requested local loading method of the Motivation model.");
                return null;
            }
            
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


            motivationHints.Add(mh);


            if (hintEnds.Contains(mh.HintId))
            {
                List<MotivationHint> mhs = motivationHints;
                evaluateHintSeries(mhs);
                motivationHints = new List<MotivationHint>();

                //sending traces to tracker
                sendMotivationValuesToTracker();
            }
        }
        
        /// <summary>
        /// Method for sending the motivation values to the tracker
        /// </summary>
        internal void sendMotivationValuesToTracker()
        {
            //get the tracker
            if (tracker == null)
            {
                if (AssetManager.Instance.findAssetsByClass("TrackerAsset").Count >= 1)
                {
                    tracker = (TrackerAsset)AssetManager.Instance.findAssetsByClass("TrackerAsset")[0];
                    loggingMAs("Found tracker for tracking motivation values!");
                }
                else
                {
                    //no tracking
                    loggingMAs("No tracker implemented - motivational state is not send to the server");
                    return;
                }
            }

            if (tracker.CheckHealth())
            {
                loggingMAs(tracker.Health);
                MotivationAssessmentAssetSettings maas = (MotivationAssessmentAssetSettings)getMAsA().Settings;
                if (tracker.Login(maas.TrackerName, maas.TrackerPassword))
                {
                    loggingMAs("logged in - tracker");
                }
                else
                {
                    loggingMAs("Maybe you forgot to store name/password for the tracker to the Motivation Assessment Asset Settings.");
                }
            }

            if (tracker.Connected)
            {
                tracker.Start();
                Dictionary<string,double> ms =  getMotivationState().getMotivation();
                foreach (string motivationAspect in ms.Keys)
                    tracker.setVar(motivationAspect, ms[motivationAspect].ToString());
                tracker.Completable.Completed("MotivationAssessmentAsset");

#if !PORTABLE
                new Thread(() =>
                {
                    //next line: thread is killed after all foreground threads are dead
                    Thread.CurrentThread.IsBackground = true;
                    tracker.Flush();
                }).Start();
#else
                tracker.Flush();
#endif
            }
            else
            {
                loggingMAs("Not connected to tracker.");
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
        public MotivationModel getMotivationModel()
        {
            if (motivationModel != null)
                return motivationModel;
            MotivationModel mm = loadDefaultMotivationModel();
            motivationModel = mm;
            return mm;
        }

        /// <summary>
        /// Returns the Motivation State of the player 
        /// </summary>
        /// 
        /// 
        /// <returns> Motivation state of the specified player. </returns>
        public MotivationState getMotivationState()
        {
            MotivationState ms;
            if (motivationState != null)
                return motivationState;
            else
                ms = new MotivationState(getMotivationModel());

            return ms;
        }

        /// <summary>
        /// Method for saving a updated motivation state.
        /// </summary>
        /// 
        /// <param name="ms"> MotivationState to be stored. </param>
        public void setMotivationState( MotivationState ms)
        {
            motivationState = ms;
        }

        /// <summary>
        /// Method for adding a new motivation hint and processing aggregated hints in case a evidence can be calculated.
        /// </summary>
        /// 
        ///<param name="hint"> Enum specifying the hint. </param>
        public void addMotivationHint(MotivationHintEnum hint)
        {
            String hintIdString = "";
            switch (hint)
            {
                case MotivationHintEnum.Fail:
                    hintIdString = "fail";
                    break;
                case MotivationHintEnum.Help:
                    hintIdString = "help";
                    break;
                case MotivationHintEnum.Success:
                    hintIdString = "success";
                    break;
                case MotivationHintEnum.NewLevel:
                    hintIdString = "new level";
                    break;
                case MotivationHintEnum.NewProblem:
                    hintIdString = "new problem";
                    break;
                default:
                    {
                        loggingMAs("Motivation Hint unknown!", Severity.Error);
                        //throw new Exception("Motivation Hint unknown!");
                        return;
                    }
            }

            MotivationHint mh = new MotivationHint();
            mh.HintId = hintIdString;
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
      
        #endregion TestMethods

    }

    public enum MotivationHintEnum
    {
        Help,
        Fail,
        Success,
        NewLevel,
        NewProblem
    };

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
                if (MotivationAssessmentAsset.Handler.getPrimaryMotivationAspects().Contains(ma.name))
                    counter++;
            }

            if (counter != MotivationAssessmentAsset.Handler.getPrimaryMotivationAspects().Count)
                MotivationAssessmentAsset.Handler.loggingMAs("Warning: MotivationalModel corrupted - at least one primary motivation aspect is missing!", Severity.Warning);

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
            MotivationAssessmentAsset.Handler.loggingMAs(msg);
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
        #region Fields

        private static String[] digitsArray = { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private static String[] digitsZeroArray = { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private static List<String> digits = new List<String>(digitsArray);
        private static List<String> digitsZero = new List<String>(digitsZeroArray);

        private static String[] operatorArray = {"-","+","*","/"};
        private static List<String> operators = new List<string>(operatorArray);

        #endregion Fields
        #region Methods

        /// <summary>
        /// Method for interpreting formulas.
        /// </summary>
        /// 
        /// <param name="expression"> Furmula String to interpret. </param>
        /// 
        /// <returns> Double value - result of the interpreted input-string.</returns>
        internal static double eval(String expression)
        {
            if (expression.Equals(""))
                MotivationAssessmentAsset.Handler.loggingMAs("Warning: Empty expression for evaluation received!", Severity.Warning);
            MotivationAssessmentAsset.Handler.loggingMAs("FormulaInterpreter: expression to evaluate with variables=" + expression);
            string mathExpression = replaceVariables(expression).Replace(":","/");
            return evaluate(mathExpression);
        }

        /// <summary>
        /// Method for replacing motivation component variables with the current values.
        /// </summary>
        /// 
        /// <param name="expression"> String for which replacement should happen. </param>
        /// 
        /// <returns> String without any motivation component variables. </returns>
        private static String replaceVariables(String expression)
        {
            MotivationState ms = MotivationAssessmentAsset.Handler.getMotivationState();
            MotivationModel mm = ms.getMotivationModel();
            foreach (MotivationAspect ma in mm.motivationAspects.motivationAspectList)
            {
                expression = expression.Replace(ma.name, ms.getMotivationAspectValue(ma.name).ToString());
            }
            MotivationAssessmentAsset.Handler.loggingMAs("FormulaInterpreter: expression to evaluate without variables=" + expression);
            return expression;
        }

        /// <summary>
        /// Evaluates a given Formula containing the operators +,*,-,/,(,)
        /// </summary>
        /// 
        /// <param name="str"> Formula to interpret </param>
        public static double evaluate(String str)
        {
            if (!checkInput(str))
            {
                MotivationAssessmentAsset.Handler.loggingMAs("Input corrupted!");
                //throw new Exception("Input corrupted!");
                return 0;
            }


            if (isPlainNumber(str))
            {
                Double result;
                if (!Double.TryParse(str, out result))
                {
                    MotivationAssessmentAsset.Handler.loggingMAs("Input corrupted!");
                    //throw new Exception("Input corrupted!");
                    return 0;
                }
                return (result);
            }

            while (str.Contains("("))
            {
                str = resolveBrackets(str);
            }

            return solveOperation(str);
        }

        /// <summary>
        /// Method for resolving one pair of brackets within a formula-string
        /// </summary>
        /// <param name="str"> formula with brackets</param>
        /// <returns> String with expression instead of one pair of brackets</returns>
        public static String resolveBrackets(String str)
        {
            int open = -2;
            int nextOpen = str.IndexOf('(');
            int close = nextOpen + 1;
            while (nextOpen < close && nextOpen != open)
            {
                open = nextOpen;
                close = 1 + open + str.Substring(open + 1).IndexOf(')');
                nextOpen = 1 + open + str.Substring(open + 1).IndexOf('(');
            }
            String inBrackets = str.Substring(open + 1, close - open - 1);
            String returnValue = str.Substring(0, open) + solveOperation(inBrackets) + str.Substring(close + 1);
            return returnValue;
        }

        /// <summary>
        /// Method for checking the input digits/operators
        /// </summary>
        /// <param name="str"> formula to evaluate</param>
        /// <returns> true if the string contains valid characters, false otherwise</returns>
        public static Boolean checkInput(String str)
        {
            String validOperators = "+-*/:().";
            String digits = "0123456789";

            for (int i = 0; i < str.Length; i++)
                if (!validOperators.Contains(str[i].ToString()) && !digits.Contains(str[i].ToString()))
                    return false;

            return true;
        }

        /// <summary>
        /// Method for identifying strings, which can be casted to numbers
        /// </summary>
        /// <param name="str"> formula to evaluate</param>
        /// <returns></returns>
        public static Boolean isPlainNumber(String str)
        {
            String digits = "0123456789";
            if (str.Length == 0 || (str.Length == 1 && str[0] == '-'))
                return false;
            if (str[0] == '-')
                str = str.Substring(1, str.Length - 1);
            if (str.Contains("-"))
                return false;
            if (str.Contains("."))
            {
                String str1 = str.Substring(0, str.IndexOf('.'));
                String str2 = str.Substring(str.IndexOf('.') + 1, str.Length - str.IndexOf('.') - 1);
                str = str1 + str2;
            }
            for (int i = 0; i < str.Length; i++)
                if (!digits.Contains(str[i].ToString()))
                    return false;

            return true;
        }

        /// <summary>
        /// Method for solving an arithmetic formula containing +,-,*,/
        /// </summary>
        /// <param name="str">formula to solve</param>
        /// <returns>result of this formula</returns>
        public static Double solveOperation(String str)
        {
            str = str.Replace("-+", "-");
            //Console.WriteLine("::"+str);
            if (isPlainNumber(str))
            {
                Double result;
                if (!Double.TryParse(str, out result))
                    throw new Exception("Input corrupted!");
                return (result);
            }

            if (str.Contains("+"))
            {
                String str1 = str.Substring(0, str.IndexOf('+'));
                String str2 = str.Substring(str.IndexOf('+') + 1, str.Length - str.IndexOf('+') - 1);
                if (str1[str1.Length - 1] == '*' || str1[str1.Length - 1] == '/')
                    goto ContinuePlus;
                if (str1.Length == 0 || str2.Length == 0)
                    throw new Exception("Input corrupted!");
                return (solveOperation(str1) + solveOperation(str2));
            }

            ContinuePlus:

            if (str.Contains("-"))
            {
                String str1 = str.Substring(0, str.IndexOf('-'));
                String str2 = str.Substring(str.IndexOf('-') + 1, str.Length - str.IndexOf('-') - 1);
                if (str1.Length > 0 && (str1[str1.Length - 1] == '*' || str1[str1.Length - 1] == '/'))
                    goto ContinueMinus;
                if (str2.Length == 0)
                    throw new Exception("Input corrupted!");
                if (str1.Length == 0)
                    return (-solveOperation(str2));
                return (solveOperation(str1) - solveOperation(str2));
            }

            ContinueMinus:

            if (str.Contains("*"))
            {
                String str1 = str.Substring(0, str.IndexOf('*'));
                String str2 = str.Substring(str.IndexOf('*') + 1, str.Length - str.IndexOf('*') - 1);
                if (str1.Length == 0 || str2.Length == 0)
                    throw new Exception("Input corrupted!");
                return (solveOperation(str1) * solveOperation(str2));
            }

            if (str.Contains("/"))
            {
                String str1 = str.Substring(0, str.IndexOf('/'));
                String str2 = str.Substring(str.IndexOf('/') + 1, str.Length - str.IndexOf('/') - 1);
                if (str1.Length == 0 || str2.Length == 0)
                    throw new Exception("Input corrupted!");
                return (solveOperation(str1) / solveOperation(str2));
            }

            return 0.0;
        }

        #endregion Methods
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

            MotivationAssessmentAsset.Handler.loggingMAs("ERROR: Requested motivation aspect name not found!");
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
        public InterventionInstance interventionInstances { get; set; }

        #endregion Properties
    }

    public class InterventionInstance
    {
        #region Properties

        /// <summary>
        /// Intervention instance
        /// </summary>
        [XmlElement("instance")]
        public List<String> instance { get; set; }

        #endregion Properties
    }

    #endregion Serialization
}
