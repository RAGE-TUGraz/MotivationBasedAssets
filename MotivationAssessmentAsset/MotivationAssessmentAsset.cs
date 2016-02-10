// <copyright file="MotivationAssessmentAsset.cs" company="RAGE">
// Copyright (c) 2016 RAGE All rights reserved.
// </copyright>
// <author>mmaurer</author>
// <date>08.02.2016 10:38:38</date>
// <summary>Implements the MotivationAssessmentAsset class</summary>
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
        public Dictionary<string, double> getMotivationState(String playerId)
        {
            MotivationState ms = MotivationAssessmentHandler.Instance.getMotivationState(playerId);
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
            MotivationAssessmentHandler.Instance.addMotivationHint(hintId, playerId);
        }

        /// <summary>
        /// Method for loading the motivation model for a certain player.
        /// </summary>
        /// 
        /// <param name="playerId"> Identification of player for which the MM is loaded. </param>
        /// 
        /// <returns> The motivation model for the specified player. </returns>
        public MotivationModel loadMotivationModel(String playerId)
        {
            return MotivationAssessmentHandler.Instance.getMotivationModel(playerId);
        }

        #endregion Methods
    }
}