// <copyright file="MotivationAdaptionAsset.cs" company="RAGE">
// Copyright (c) 2016 RAGE All rights reserved.
// </copyright>
// <author>mmaurer</author>
// <date>08.02.2016 10:39:02</date>
// <summary>Implements the MotivationAdaptionAsset class</summary>
namespace MotivationAdaptionAssetNameSpace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AssetManagerPackage;
    using AssetPackage;

    /// <summary>
    /// An asset.
    /// </summary>
    public class MotivationAdaptionAsset : BaseAsset
    {
        #region Fields

        /// <summary>
        /// Options for controlling the operation.
        /// </summary>
        private MotivationAdaptionAssetSettings settings = null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the MotivationAdaptionAsset.Asset class.
        /// </summary>
        public MotivationAdaptionAsset()
            : base()
        {
            //! Create Settings and let it's BaseSettings class assign Defaultvalues where it can.
            // 
            settings = new MotivationAdaptionAssetSettings();
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
                settings = (value as MotivationAdaptionAssetSettings);
            }
        }

        #endregion Properties

        #region Methods

        // Your code goes here.

        /*
        public void test()
        {
            Console.WriteLine("MotivationAdaption method called!");
            MotivationAdaptionHandler.Instance.performAllTests();
        }
        */

        /// <summary>
        /// Method returning all interventions appropriate for a player with given id.
        /// </summary>
        /// 
        /// <param name="playerId"> Identification of the player. </param>
        /// 
        /// <returns> List containing all appropriate interventions at the moment. </returns>
        public List<String> getInterventions(String playerId)
        {
            return MotivationAdaptionHandler.Instance.getInterventions(playerId);
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
            return MotivationAdaptionHandler.Instance.getInstance(intervention, playerId);
        }

        #endregion Methods
    }
}