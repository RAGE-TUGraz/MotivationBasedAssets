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
    using AssetPackage;
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// An asset settings.
    /// 
    /// BaseSettings contains the (de-)serialization methods.
    /// </summary>
    public class MotivationAssessmentAssetSettings : BaseSettings
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the MotivationAssessmentAsset.AssetSettings class.
        /// </summary>
        public MotivationAssessmentAssetSettings()
            : base()
        {
            // Set Default values here.
            XMLLoadingId = "MotivationModelXML.xml";
        }

        #endregion Constructors

        #region Properties

        /// <value>
        /// Id for loading the underlying motivation data from xml
        /// </value>
        [XmlElement()]
        public String XMLLoadingId
        {
            get;
            set;
        }

        /// <value>
        /// Time needed for reading - solutions before are assumed to be only guesses 
        /// </value>
        [XmlElement()]
        public int FirstTryMinDuration
        {
            get;
            set;
        }


        /// <value>
        /// Time needed for for solving the task at the longest
        /// </value>
        [XmlElement()]
        public int SolutionMaxDuration
        {
            get;
            set;
        }


        /// <value>
        /// Descibes the maximum number of errors accepted when solving the task.
        /// </value>
        [XmlElement()]
        public int MaxNoErrors
        {
            get;
            set;
        }


        /// <value>
        /// Descripes the maximum number of help requests accepted when solving this task.
        /// </value>
        [XmlElement()]
        public int MaxNoHelpRequests
        {
            get;
            set;
        }



        #endregion Properties
    }
}
