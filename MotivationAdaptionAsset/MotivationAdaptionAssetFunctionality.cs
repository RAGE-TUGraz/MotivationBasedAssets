using MotivationAssessmentAssetNameSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MotivationAdaptionAssetNameSpace
{

    /// <summary>
    /// Singelton Class for handling MotivationAdaptionHandler
    /// </summary>
    class MotivationAdaptionHandler
    {
        #region Fields

        /// <summary>
        /// Instance of the class MotivationAdaptionHandler - Singelton pattern
        /// </summary>
        private static MotivationAdaptionHandler instance;

        /// <summary>
        /// run-time storage: to each player, intervention and instance the number of times this instance was called is stored here 
        /// </summary>
        private Dictionary<String, Dictionary<String, Dictionary<String, int>>> interventionsHistory = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();

        /// <summary>
        /// If true, logging is done.
        /// </summary>
        private Boolean doLogging = false;

        #endregion Fields
        #region Constructors

        /// <summary>
        /// private MotivationAdaptionHandler-ctor for Singelton-pattern 
        /// </summary>
        private MotivationAdaptionHandler() { }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Getter for Instance of the MotivationAdaptionHandler - Singelton pattern
        /// </summary>
        public static MotivationAdaptionHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MotivationAdaptionHandler();
                }
                return instance;
            }
        }

        #endregion Properties
        #region InternalMethods

        //TODO: Loading motivation state over the motivation assesment asset - loading via player model?
        /// <summary>
        /// Method loads the motivation state of a player.
        /// </summary>
        /// 
        /// <param name="playerId"> identification of the player fpr which the motivation state is loaded. </param>
        /// 
        /// <returns> Motivation state of a player. </returns>
        internal MotivationState loadMotivationState(String playerId)
        {
            return (MotivationAssessmentHandler.Instance.getMotivationState(playerId));
        }

        //TODO: Where to get the MM from? currently from MotivationAssessmentHandler
        /// <summary>
        /// Method for loading the motivation model for a certain player.
        /// </summary>
        /// 
        /// <param name="playerId"> Identification of player for which the MM is loaded. </param>
        /// 
        /// <returns> The motivation model for the specified player. </returns>
        internal MotivationModel loadMotivationModel(String playerId)
        {
            return MotivationAssessmentHandler.Instance.getMotivationModel(playerId);
        }

        /// <summary>
        /// Method returning intervention instances to a list of instances provided.
        /// </summary>
        /// 
        /// <param name="interventions"> List of intervenentions provided. </param>
        /// <param name="playerId"> Player Identification. </param>
        /// 
        /// <returns> List of intervention instances. </returns>
        internal List<String> getInterventionInstances(List<String> interventions, String playerId)
        {
            List<String> instances = new List<String>();
            MotivationModel mm = loadMotivationModel(playerId);

            Random rnd = new Random();
            int pos;

            foreach (Intervention iv in mm.motivationInterventions.motivationInterventionList)
            {
                if (interventions.Contains(iv.name))
                {
                    pos = rnd.Next(1, iv.interventionInstances.Count + 1) - 1;
                    instances.Add(iv.interventionInstances.ElementAt(pos).instance);
                }
            }

            return (instances);
        }

        #endregion InternalMethods
        #region PublicMethods

        /// <summary>
        /// Method returning all interventions appropriate for a player with given id.
        /// </summary>
        /// 
        /// <param name="playerId"> Identification of the player. </param>
        /// 
        /// <returns> List containing all appropriate interventions at the moment. </returns>
        public List<String> getInterventions(String playerId)
        {
            List<String> interventions = new List<String>();

            MotivationModel mm = loadMotivationModel(playerId);
            MotivationState ms = loadMotivationState(playerId);
            Boolean val;

            foreach (Intervention iv in mm.motivationInterventions.motivationInterventionList)
            {
                try
                {
                    val = FormulaInterpreterBool.eval(iv.rule, playerId);
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
        /// <param name="playerId"> Player Id for which the instance is requested. </param>
        /// 
        /// <returns> Intervention instance for the player. </returns>
        public String getInstance(String intervention, String playerId)
        {
            MotivationModel mm = loadMotivationModel(playerId);
            List<InterventionInstance> instances = null;
            foreach (Intervention iv in mm.motivationInterventions.motivationInterventionList)
                if (intervention.Equals(iv.name))
                    instances = iv.interventionInstances;
            if (instances == null)
            {
                loggingMAd("ERROR: requested instance is not part of the motivation model!");
                return "ERROR";
            }

            //create intervention history if needed
            if (!interventionsHistory.ContainsKey(playerId))
                interventionsHistory[playerId] = new Dictionary<string, Dictionary<string, int>>();
            Dictionary<string, Dictionary<string, int>> playerHistory = interventionsHistory[playerId];

            //create interventionCount if needed
            Dictionary<string, int> interventionCount;
            if (!playerHistory.ContainsKey(intervention))
            {
                interventionCount = new Dictionary<string, int>();
                foreach (InterventionInstance ii in instances)
                    interventionCount[ii.instance] = 0;
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
            interventionsHistory[playerId][intervention] = interventionCount;
            return usedInstance;
        }

        #endregion PublicMethods
        #region TestMethods

        /// <summary>
        /// Method for logging (Diagnostics).
        /// </summary>
        /// 
        /// <param name="msg"> Message to be logged. </param>
        internal void loggingMAd(String msg)
        {
            if (doLogging)
                Console.WriteLine(msg);
        }

        /// <summary>
        /// Method calling all Tests of this Class.
        /// </summary>
        public void performAllTests()
        {
            loggingMAd("*****************************************************************");
            loggingMAd("Calling all tests (MAdA):");
            performTest1();
            loggingMAd("Tests - done!");
            loggingMAd("*****************************************************************");
        }

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
        /// <param name="playerId"> Identifier of player for which the evaluation should be done. </param>
        /// 
        /// <returns> Double value - result of the interpreted input-string.</returns>
        internal static Boolean eval(String expression, String playerId)
        {
            if (expression.Equals(""))
                MotivationAdaptionHandler.Instance.loggingMAd("ERROR: Empty expression for evaluation received!");
            MotivationAdaptionHandler.Instance.loggingMAd("FormulaInterpreter: expression to evaluate with variables=" + expression);
            System.Data.DataTable table = new System.Data.DataTable();
            return table.Compute(replaceVariables(expression, playerId), String.Empty).ToString().Equals("True") ? true : false;
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
            MotivationState ms = MotivationAdaptionHandler.Instance.loadMotivationState(playerId);
            MotivationModel mm = ms.getMotivationModel();
            foreach (MotivationAspect ma in mm.motivationAspects.motivationAspectList)
            {
                expression = expression.Replace(ma.name, ms.getMotivationAspectValue(ma.name).ToString());
            }
            MotivationAdaptionHandler.Instance.loggingMAd("FormulaInterpreter: expression to evaluate without variables=" + expression);
            return expression;
        }

        #endregion Methods
    }
}
