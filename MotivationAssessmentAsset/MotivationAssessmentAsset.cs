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

namespace MotivationAssessmentAssetNameSpace
{
    using System.Collections.Generic;
    using AssetPackage;

    /// <summary>
    /// An asset.
    /// </summary>
    public class MotivationAssessmentAsset : BaseAsset
    {
        #region Fields

        /// <summary>
        /// Options for controlling the operation.
        /// </summary>
        private MotivationAssessmentAssetSettings settings = null;

        /// <summary>
        /// Instance of the class MotivationAssessmentAsset - Singelton pattern
        /// </summary>
        static readonly MotivationAssessmentAsset instance = new MotivationAssessmentAsset();

        /// <summary>
        /// Instance of the class MotivationAssessmentHandler - Singelton pattern
        /// </summary>
        static internal MotivationAssessmentHandler motivationAssessmentHandler = new MotivationAssessmentHandler();

        #endregion Fields
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the MotivationAssessmentAsset.Asset class.
        /// </summary>
        private MotivationAssessmentAsset()
            : base()
        {
            //! Create Settings and let it's BaseSettings class assign Defaultvalues where it can.
            settings = new MotivationAssessmentAssetSettings();
        }

        #endregion Constructors
        #region Properties

        /// <summary>
        /// Gets or sets options for controlling the operation.
        /// </summary>
        ///
        /// <remarks>   Besides the toXml() and fromXml() methods, we never use this property but use
        ///                it's correctly typed backing field 'settings' instead. </remarks>
        /// <remarks> This property should go into each asset having Settings of its own. </remarks>
        /// <remarks>   The actual class used should be derived from BaseAsset (and not directly from
        ///             ISetting). </remarks>
        ///
        /// <value>
        /// The settings.
        /// </value>
        public override ISettings Settings
        {
            get
            {
                return settings;
            }
            set
            {
                settings = (value as MotivationAssessmentAssetSettings);
                Handler.motivationModel = null;
            }
        }

        /// <summary>
        /// Getter for Instance of the MotivationAssessmentAsset - Singelton pattern
        /// </summary>
        public static MotivationAssessmentAsset Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Getter for Instance of the MotivationAssessmentHandler
        /// </summary>
        internal static MotivationAssessmentHandler Handler
        {
            get
            {
                return motivationAssessmentHandler;
            }
        }

        #endregion Properties
        #region Methods


        /// <summary>
        /// Returns the Motivation State of a player when provided with player-Id.
        /// </summary>
        /// 
        /// <param name="playerId"> Identifier of the player. </param>
        /// 
        /// <returns> Motivation state of the specified player. </returns>
        public Dictionary<string, double> getMotivationState()
        {
            MotivationState ms = Handler.getMotivationState();
            return ms.getMotivation(); ;
        }

        /// <summary>
        /// Method for adding a new motivation hint and processing aggregated hints in case a evidence can be calculated.
        /// </summary>
        /// 
        ///<param name="hint"> Enum specifying the hint. </param>
        public void addMotivationHint(MotivationHintEnum hint)
        {
            Handler.addMotivationHint(hint);
        }

        /// <summary>
        /// Method for loading the motivation model for the player.
        /// </summary>
        /// 
        /// 
        /// <returns> The motivation model for the player. </returns>
        public MotivationModel loadMotivationModel()
        {
            return Handler.getMotivationModel();
        }

        /// <summary>
        /// Method for satisfaction downgrade check
        /// </summary>
        public void checkForSatisfactionDowngrade()
        {
            Handler.updateSatisfaction();
        }

        #endregion Methods
        #region internal Methods

        /// <summary>
        /// Wrapper method for getting the getInterface method of the base Asset
        /// </summary>
        /// <typeparam name="T">Generic type parameter.</typeparam>
        /// <returns>Corresponding Interface</returns>
        internal T getInterfaceFromAsset<T>()
        {
            return this.getInterface<T>();
        }

        #endregion internal Methods
    }
}