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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AssetManagerPackage;
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

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the MotivationAssessmentAsset.Asset class.
        /// </summary>
        public MotivationAssessmentAsset()
            : base()
        {
            //! Create Settings and let it's BaseSettings class assign Defaultvalues where it can.
            // 
            settings = new MotivationAssessmentAssetSettings();

            //preventing multiple asset creation
            if (AssetManager.Instance.findAssetsByClass(this.Class).Count > 1)
            {
                this.Log(Severity.Error, "There is only one instance of the MotivationAssessmentAsset permitted!");
                throw new Exception("EXCEPTION: There is only one instance of the MotivationAssessmentAsset permitted!");
            }
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
            }
        }

        #endregion Properties

        #region Methods

        // Your code goes here.

        /*
        public void test()
        {
            Console.WriteLine("MotivationAssessment method called! - calling Tests:");
            MotivationAssessmentHandler.Instance.performAllTests();
        }
        */

        /// <summary>
        /// Returns the Motivation State of a player when provided with player-Id.
        /// </summary>
        /// 
        /// <param name="playerId"> Identifier of the player. </param>
        /// 
        /// <returns> Motivation state of the specified player. </returns>
        public Dictionary<string, double> getMotivationState()
        {
            MotivationState ms = MotivationAssessmentHandler.Instance.getMotivationState();
            return ms.getMotivation(); ;
        }

        /// <summary>
        /// Method for adding a new motivation hint and processing aggregated hints in case a evidence can be calculated.
        /// </summary>
        /// 
        ///<param name="hintId"> String specifying the hint. </param>
        ///<param name="playerId"> String specifying the player. </param>
        public void addMotivationHint(String hintId, String playerId)
        {
            MotivationAssessmentHandler.Instance.addMotivationHint(hintId);
        }

        //TODO: other way of transferring information?!
        /// <summary>
        /// Method for loading the motivation model for the player.
        /// </summary>
        /// 
        /// 
        /// <returns> The motivation model for the player. </returns>
        public MotivationModel loadMotivationModel()
        {
            return MotivationAssessmentHandler.Instance.getMotivationModel();
        }

        /// <summary>
        /// Method performing functionality test for the motivation assessment asset
        /// </summary>
        public void performTest()
        {
            MotivationAssessmentHandler.Instance.performAllTests();
        }

        /// <summary>
        /// Method for receiving Asset settings
        /// </summary>
        /// <returns></returns>
        internal MotivationAssessmentAssetSettings getSettings()
        {
            return this.settings;
        }

        #endregion Methods
    }
}