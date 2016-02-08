// <copyright file="MotivationAssessmentAsset.cs" company="RAGE">
// Copyright (c) 2016 RAGE All rights reserved.
// </copyright>
// <author>mmaurer</author>
// <date>08.02.2016 10:38:38</date>
// <summary>Implements the MotivationAssessmentAsset class</summary>
namespace AssetPackage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AssetManagerPackage;

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

        #endregion Methods
    }
}