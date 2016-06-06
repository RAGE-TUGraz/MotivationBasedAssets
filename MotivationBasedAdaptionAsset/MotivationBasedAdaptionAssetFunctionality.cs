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
  Changed by: Matthias Maurer, TUGraz <mmaurer@tugraz.at>
  Changed on: 2016-02-22
*/

using AssetManagerPackage;
using AssetPackage;
using MotivationAssessmentAssetNameSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MotivationBasedAdaptionAssetNameSpace
{

    /// <summary>
    /// Singelton Class for handling MotivationAdaptionHandler
    /// </summary>
    internal class MotivationBasedAdaptionHandler
    {
        #region Fields

        /// <summary>
        /// Instance of the MotivationAssessmentAsset
        /// </summary>
        private MotivationAssessmentAsset motivationAssessmentAsset = null;

        /// <summary>
        /// Instance of the MotivationAdaptionAsset
        /// </summary>
        private MotivationBasedAdaptionAsset motivationBasedAdaptionAsset = null;

        /// <summary>
        /// Instance of the class MotivationAdaptionHandler - Singelton pattern
        /// </summary>
        private static MotivationBasedAdaptionHandler instance;

        /// <summary>
        /// run-time storage: to each player, intervention and instance the number of times this instance was called is stored here 
        /// </summary>
        private Dictionary<String, Dictionary<String, int>> interventionsHistory = new Dictionary<string, Dictionary<string, int>>();

        /// <summary>
        /// If true, logging is done.
        /// </summary>
        private Boolean doLogging = true;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// private MotivationAdaptionHandler-ctor for Singelton-pattern 
        /// </summary>
        private MotivationBasedAdaptionHandler() { }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Getter for Instance of the MotivationAdaptionHandler - Singelton pattern
        /// </summary>
        public static MotivationBasedAdaptionHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MotivationBasedAdaptionHandler();
                }
                return instance;
            }
        }

        #endregion Properties
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
        /// Method returning an instance of the MotivationAdaptionAsset.
        /// </summary>
        /// <returns> Instance of the MotivationAdaptionAsset </returns>
        internal MotivationBasedAdaptionAsset getMAdA()
        {
            if (motivationBasedAdaptionAsset == null)
                motivationBasedAdaptionAsset = (MotivationBasedAdaptionAsset)AssetManager.Instance.findAssetByClass("MotivationBasedAdaptionAsset");
            return (motivationBasedAdaptionAsset);
        }

        //TODO: Loading motivation state over the motivation assesment asset - loading via player model?
        /// <summary>
        /// Method loads the motivation state of the player.
        /// </summary>
        /// 
        /// 
        /// <returns> Motivation state of the player. </returns>
        internal Dictionary<string, double> loadMotivationState( )
        {
            return (getMAsA().getMotivationState());
        }

        //TODO: Where to get the MM from? currently from MotivationAssessmentHandler
        /// <summary>
        /// Method for loading the motivation model for a certain player.
        /// </summary>
        /// 
        /// 
        /// <returns> The motivation model for the specified player. </returns>
        internal MotivationModel loadMotivationModel()
        {
            return getMAsA().loadMotivationModel();
        }

        /// <summary>
        /// Method returning intervention instances to a list of instances provided.
        /// </summary>
        /// 
        /// <param name="interventions"> List of intervenentions provided. </param>
        /// 
        /// <returns> List of intervention instances. </returns>
        internal List<String> getInterventionInstances(List<String> interventions, String playerId)
        {
            List<String> instances = new List<String>();
            MotivationModel mm = loadMotivationModel();

            Random rnd = new Random();
            int pos;

            foreach (Intervention iv in mm.motivationInterventions.motivationInterventionList)
            {
                if (interventions.Contains(iv.name))
                {
                    pos = rnd.Next(1, iv.interventionInstances.instance.Count + 1) - 1;
                    instances.Add(iv.interventionInstances.instance.ElementAt(pos));
                }
            }

            return (instances);
        }

        #endregion InternalMethods
        #region PublicMethods

        /// <summary>
        /// Method returning all interventions appropriate for a player.
        /// </summary>
        /// 
        /// <returns> List containing all appropriate interventions at the moment. </returns>
        public List<String> getInterventions()
        {
            List<String> interventions = new List<String>();

            MotivationModel mm = loadMotivationModel();
            Boolean val;

            foreach (Intervention iv in mm.motivationInterventions.motivationInterventionList)
            {
                try
                {
                    val = FormulaInterpreterBool.eval(iv.rule);
                    if (val)
                        interventions.Add(iv.name);
                }
                catch (Exception e)
                {
                    loggingMAd("ERROR: Evaluation of Boolean value for intervention trigger failed!");
                    loggingMAd(e.Message);
                }
            }

            return interventions;
        }

        /// <summary>
        /// Method returning a least often used instance of an intervnention.
        /// </summary>
        /// 
        /// <param name="intervention"> Intervention id for which the instance is requested. </param>
        /// 
        /// <returns> Intervention instance for the player. </returns>
        public String getInstance(String intervention )
        {
            MotivationModel mm = loadMotivationModel();
            List<String> instances = null;
            foreach (Intervention iv in mm.motivationInterventions.motivationInterventionList)
                if (intervention.Equals(iv.name))
                    instances = iv.interventionInstances.instance;
            if (instances == null)
            {
                loggingMAd("ERROR: requested instance is not part of the motivation model!");
                return "ERROR";
            }

            //create intervention history if needed
            if (interventionsHistory == null)
                interventionsHistory = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, Dictionary<string, int>> playerHistory = interventionsHistory;

            //create interventionCount if needed
            Dictionary<string, int> interventionCount;
            if (!playerHistory.ContainsKey(intervention))
            {
                interventionCount = new Dictionary<string, int>();
                foreach (String ii in instances)
                    interventionCount[ii] = 0;
            }
            else
                interventionCount = playerHistory[intervention];

            //get the least often used intervention instances
            int minUsedInstanceNr = -1;
            List<string> minUsedInstances = new List<string>();
            foreach (KeyValuePair<String, int> pair in interventionCount)
            {
                if (minUsedInstanceNr == -1 || minUsedInstanceNr > pair.Value)
                {
                    minUsedInstanceNr = pair.Value;
                    minUsedInstances = new List<string>();
                    minUsedInstances.Add(pair.Key);
                }
                else if (minUsedInstanceNr == pair.Value)
                    minUsedInstances.Add(pair.Key);
            }

            //choose an random instance 
            Random rnd = new Random();
            int pos = rnd.Next(1, minUsedInstances.Count + 1) - 1;
            String usedInstance = minUsedInstances.ElementAt(pos);

            interventionCount[usedInstance]++;
            interventionsHistory[intervention] = interventionCount;
            return usedInstance;
        }

        #endregion PublicMethods
        #region TestMethods

        /// <summary>
        /// Method for logging (Diagnostics).
        /// </summary>
        /// 
        /// <param name="msg"> Message to be logged. </param>
        internal void loggingMAd(String msg, Severity severity = Severity.Information)
        {
            if (doLogging)
            {
                getMAdA().Log(severity, "[MAdA]: " + msg);
            }
        }

        /// <summary>
        /// Method calling all Tests of this Class.
        /// </summary>
        internal void performAllTests()
        {
            loggingMAd("*****************************************************************");
            loggingMAd("Calling all tests (MAdA):");
            //performTest1();
            loggingMAd("Tests - done!");
            loggingMAd("*****************************************************************");
            throw new NotImplementedException();
        }

        /*
        /// <summary>
        /// Method for performing a test. Setting Motivation State and getting Interventions.
        /// </summary>
        private void performTest1()
        {
            loggingMAd("***************TEST 1********************");
            String testuser = "usr";
            MotivationState ms = loadMotivationState(testuser);
            ms.updateMotivationAspect("attention", 0.25);
            ms.updateMotivationAspect("satisfaction", 0.25);
            ms.updateMotivationAspect("confidence", 0.25);
            MotivationAssessmentHandler.Instance.setMotivationState(testuser, ms);
            List<String> interventions = getInterventions(testuser);
            loggingMAd("Interventions:");
            foreach (String str in interventions)
            {
                loggingMAd(" - " + str);
            }

            String testuser2 = "usr2";
            MotivationState ms2 = loadMotivationState(testuser2);
            ms2.updateMotivationAspect("attention", 0.5);
            ms2.updateMotivationAspect("satisfaction", 0.5);
            ms2.updateMotivationAspect("confidence", 0.25);
            MotivationAssessmentHandler.Instance.setMotivationState(testuser2, ms2);
            List<String> interventions2 = getInterventions(testuser2);
            List<String> instances = getInterventionInstances(interventions2, testuser2);
            loggingMAd("Intervention Instances:");
            foreach (String str in instances)
            {
                loggingMAd(" - " + str);
            }

            String inst;
            int c = 0;
            String intervention = getInterventions(testuser2)[0];
            while (c < 6)
            {
                c++;
                inst = getInstance(intervention, testuser);
                loggingMAd("NEW INSTANCE: " + inst);
            }

        }

        */

        /// <summary>
        /// Performing successive motivation update
        /// </summary>
        private void performtest2()
        {
            String player = "p1";
            int status = 1;
            String stat1 = "(new level...l, new problem...p, exit...e):";
            String stat2 = "(help...h, fail...f, success...s, exit...e):";

            string line = "";
            while (true)
            {
                //"help","fail","success", "new level", "new problem"
                while ((status == 1 && !line.Equals("s") && !line.Equals("f") && !line.Equals("e")) || !line.Equals("s") && !line.Equals("f") && !line.Equals("e"))
                {
                    if (status == 1)
                        loggingMAd("Please enter game information " + stat1);
                    else
                        loggingMAd("Please enter game information " + stat2);
                    line = Console.ReadLine();
                }
                if (!line.Equals("e"))
                {
                    if (status == 1)
                    {

                    }
                    else
                    {

                    }
                }
                else
                {
                    loggingMAd("Test Ended.");
                    return;
                }
                line = "";
            }


            /*
            String player = "p1";
            registerNewPlayer(player, DomainModelHandler.Instance.createExampleDomainModel());
            GameSituationStructure gss = getGameSituationStructure(player);
            GameSituation gs = getCurrentGameSituation(player);
            CompetenceState cs = CompetenceAssessmentHandler.Instance.getCompetenceState(player); ;
            cs.print();

            string line = "";
            while (true)
            {
                while (!line.Equals("s") && !line.Equals("f") && !line.Equals("e"))
                {
                    loggingPRA("Entering game situation " + gs.Id + ". How did the player performe (s-success,f-failure,e-exit)?");
                    line = Console.ReadLine();
                }
                if (!line.Equals("e"))
                {
                    if (line.Equals("s"))
                        setGameSituationUpdate(player, true);
                    else if (line.Equals("f"))
                        setGameSituationUpdate(player, false);
                    cs = CompetenceAssessmentHandler.Instance.getCompetenceState(player);
                    cs.print();
                }
                else
                {
                    loggingPRA("Test Ended.");
                    return;
                }
                line = "";
                gs = gss.getGameSituationById(getNextGameSituationId(player));
            }

    */

        }

        #endregion TestMethods
    }


    /// <summary>
    /// Class for executing formular interpretation.
    /// </summary>
    internal static class FormulaInterpreterBool
    {
        #region Methods

        /// <summary>
        /// Method for interpreting formulas.
        /// </summary>
        /// 
        /// <param name="expression"> Furmula String to interpret. </param>
        /// 
        /// <returns> Double value - result of the interpreted input-string.</returns>
        internal static Boolean eval(String expression)
        {
            if (expression.Equals(""))
                MotivationBasedAdaptionHandler.Instance.loggingMAd("ERROR: Empty expression for evaluation received!");
            MotivationBasedAdaptionHandler.Instance.loggingMAd("FormulaInterpreter: expression to evaluate with variables=" + expression);
            System.Data.DataTable table = new System.Data.DataTable();
            return table.Compute(replaceVariables(expression), String.Empty).ToString().Equals("True") ? true : false;
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
            //OLD:
            /*
            MotivationState ms = MotivationAdaptionHandler.Instance.loadMotivationState(playerId);
            MotivationModel mm = ms.getMotivationModel();
            foreach (MotivationAspect ma in mm.motivationAspects.motivationAspectList)
            {
                expression = expression.Replace(ma.name, ms.getMotivationAspectValue(ma.name).ToString());
            }
            */
            //NEW;
            Dictionary<String,double> ms = MotivationBasedAdaptionHandler.Instance.loadMotivationState();
            foreach(KeyValuePair<String,double> pair in ms)
            {
                expression = expression.Replace(pair.Key, pair.Value.ToString());
            }
            MotivationBasedAdaptionHandler.Instance.loggingMAd("FormulaInterpreter: expression to evaluate without variables=" + expression);
            return expression;
        }

        #endregion Methods
    }
}
