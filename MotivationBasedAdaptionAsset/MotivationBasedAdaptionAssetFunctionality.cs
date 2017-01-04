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
using MotivationAssessmentAssetNameSpace;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MotivationBasedAdaptionAssetNameSpace
{

    /// <summary>
    /// Singelton Class for handling MotivationAdaptionHandler
    /// </summary>
    internal class MotivationBasedAdaptionHandler
    {
        #region Fields
        
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
        internal MotivationBasedAdaptionHandler() { }

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
        /// Method returning an instance of the MotivationAdaptionAsset.
        /// </summary>
        /// <returns> Instance of the MotivationAdaptionAsset </returns>
        internal MotivationBasedAdaptionAsset getMAdA()
        {
            return MotivationBasedAdaptionAsset.Instance;
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
                MotivationBasedAdaptionAsset.Handler.loggingMAd("ERROR: Empty expression for evaluation received!");
            MotivationBasedAdaptionAsset.Handler.loggingMAd("FormulaInterpreter: expression to evaluate with variables=" + expression);
            return evaluateBoolean(replaceVariables(expression));
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
            Dictionary<String,double> ms = MotivationBasedAdaptionAsset.Handler.loadMotivationState();
            foreach(KeyValuePair<String,double> pair in ms)
            {
                expression = expression.Replace(pair.Key, pair.Value.ToString());
            }
            MotivationBasedAdaptionAsset.Handler.loggingMAd("FormulaInterpreter: expression to evaluate without variables=" + expression);
            return expression;
        }
        
        private static Boolean evaluateBoolean(String str)
        {
            if (!checkInputBoolean(str))
                throw new Exception("Input corrupted!");

            while (str.Contains("("))
            {
                str = resolveBracketsBoolean(str);
            }

            return solveOperationBoolean(str).Equals("TRUE") ? true : false;
        }

        /// <summary>
        /// Method for checking the input digits/operators
        /// </summary>
        /// <param name="str"> formula to evaluate</param>
        /// <returns> true if the string contains valid characters, false otherwise</returns>
        private static Boolean checkInputBoolean(String str)
        {
            str = str.Replace("||", "").Replace("&&", "").Replace("==", "").Replace("<=", "").Replace(">=", "").Replace("<", "").Replace(">", "").Replace("!=", "").Replace(" ", "");
            String validOperators = "+-*/:().";
            String digits = "0123456789";

            for (int i = 0; i < str.Length; i++)
                if (!validOperators.Contains(str[i].ToString()) && !digits.Contains(str[i].ToString()))
                    return false;

            return true;
        }

        /// <summary>
        /// Method for resolving one pair of brackets within a formula-string
        /// </summary>
        /// <param name="str"> formula with brackets</param>
        /// <returns> String with expression instead of one pair of brackets</returns>
        private static String resolveBracketsBoolean(String str)
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
            String returnValue = str.Substring(0, open) + solveOperationBoolean(inBrackets) + str.Substring(close + 1);
            return returnValue;
        }

        /// <summary>
        /// Method for solving an arithmetic formula containing +,-,*,/
        /// </summary>
        /// <param name="str">formula to solve</param>
        /// <returns>result of this formula</returns>
        private static String solveOperationBoolean(String str)
        {
            //&& is evaluated first -> check first for || in code
            str = str.Replace(" ", "");

            if (str.Equals("TRUE") || str.Equals("FALSE"))
                return str;

            if (str.Contains("||"))
            {
                String str1 = str.Substring(0, str.IndexOf("||"));
                String str2 = str.Substring(str.IndexOf("||") + 2, str.Length - str.IndexOf("||") - 2);
                if (str1.Length == 0 || str2.Length == 0)
                    throw new Exception("Input corrupted!");
                return (solveOperationBoolean(str1).Equals("TRUE") || solveOperationBoolean(str2).Equals("TRUE")) ? "TRUE" : "FALSE";
            }

            if (str.Contains("&&"))
            {
                String str1 = str.Substring(0, str.IndexOf("&&"));
                String str2 = str.Substring(str.IndexOf("&&") + 2, str.Length - str.IndexOf("&&") - 2);
                if (str1.Length == 0 || str2.Length == 0)
                    throw new Exception("Input corrupted!");
                return (solveOperationBoolean(str1).Equals("TRUE") && solveOperationBoolean(str2).Equals("TRUE")) ? "TRUE" : "FALSE";
            }

            if (str.Contains("=="))
            {
                String str1 = str.Substring(0, str.IndexOf("=="));
                String str2 = str.Substring(str.IndexOf("==") + 2, str.Length - str.IndexOf("==") - 2);
                if (str1.Length == 0 || str2.Length == 0)
                    throw new Exception("Input corrupted!");
                return (solveOperation(str1) == solveOperation(str2)) ? "TRUE" : "FALSE";
            }

            if (str.Contains("<="))
            {
                String str1 = str.Substring(0, str.IndexOf("<="));
                String str2 = str.Substring(str.IndexOf("<=") + 2, str.Length - str.IndexOf("<=") - 2);
                if (str1.Length == 0 || str2.Length == 0)
                    throw new Exception("Input corrupted!");
                return (solveOperation(str1) <= solveOperation(str2)) ? "TRUE" : "FALSE";
            }

            if (str.Contains(">="))
            {
                String str1 = str.Substring(0, str.IndexOf(">="));
                String str2 = str.Substring(str.IndexOf(">=") + 2, str.Length - str.IndexOf(">=") - 2);
                if (str1.Length == 0 || str2.Length == 0)
                    throw new Exception("Input corrupted!");
                return (solveOperation(str1) >= solveOperation(str2)) ? "TRUE" : "FALSE";
            }

            if (str.Contains(">"))
            {
                String str1 = str.Substring(0, str.IndexOf(">"));
                String str2 = str.Substring(str.IndexOf(">") + 1, str.Length - str.IndexOf(">") - 1);
                if (str1.Length == 0 || str2.Length == 0)
                    throw new Exception("Input corrupted!");
                return (solveOperation(str1) > solveOperation(str2)) ? "TRUE" : "FALSE";
            }

            if (str.Contains("<"))
            {
                String str1 = str.Substring(0, str.IndexOf("<"));
                String str2 = str.Substring(str.IndexOf("<") + 1, str.Length - str.IndexOf("<") - 1);
                if (str1.Length == 0 || str2.Length == 0)
                    throw new Exception("Input corrupted!");
                return (solveOperation(str1) < solveOperation(str2)) ? "TRUE" : "FALSE";
            }

            throw new Exception("This line should never be reached! - Error when evaluation logic expression.");
        }

        /// <summary>
        /// Evaluates a given Formula containing the operators +,*,-,/,(,)
        /// </summary>
        /// 
        /// <param name="str"> Formula to interpret </param>
        private static double evaluate(String str)
        {
            if (!checkInput(str))
                throw new Exception("Input corrupted!");


            if (isPlainNumber(str))
            {
                Double result;
                if (!Double.TryParse(str, out result))
                    throw new Exception("Input corrupted!");
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
        private static String resolveBrackets(String str)
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
        private static Boolean checkInput(String str)
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
        private static Boolean isPlainNumber(String str)
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
        private static Double solveOperation(String str)
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
}
