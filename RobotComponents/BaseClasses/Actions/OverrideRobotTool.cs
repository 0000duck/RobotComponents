﻿// This file is part of RobotComponents. RobotComponents is licensed 
// under the terms of GNU General Public License as published by the 
// Free Software Foundation. For more information and the LICENSE file, 
// see <https://github.com/EDEK-UniKassel/RobotComponents>.

// System Libs
using System;
// RobotComponents Libs
using RobotComponents.BaseClasses.Definitions;

namespace RobotComponents.BaseClasses.Actions
{
    /// <summary>
    /// Override Robot Tool class
    /// </summary>
    public class OverrideRobotTool : Action
    {
        #region fields
        private RobotTool _robotTool; // The robot that should be used
        #endregion

        #region constructors
        /// <summary>
        /// Defines an empty Override Robot Tool object. 
        /// </summary>
        public OverrideRobotTool()
        {
        }

        /// <summary>
        /// Creates and Override Robot Tool object.
        /// </summary>
        /// <param name="robotTool"> The Robot Tool that should be set. </param>
        public OverrideRobotTool(RobotTool robotTool)
        {
            _robotTool = robotTool;
        }

        /// <summary>
        /// Creates a new override robot tool by duplicating an existing override robot tool. 
        /// This creates a deep copy of the existing override robot tool. 
        /// </summary>
        /// <param name="overrideRobotTool"> The override robot tool that should be duplicated. </param>
        public OverrideRobotTool(OverrideRobotTool overrideRobotTool)
        {
            _robotTool = overrideRobotTool.RobotTool.Duplicate();
        }

        /// <summary>
        /// Method to duplicate the Override Robot Tool object.
        /// </summary>
        /// <returns> Returns a deep copy of the Override Robot Tool object.</returns>
        public OverrideRobotTool Duplicate()
        {
            return new OverrideRobotTool(this);
        }

        /// <summary>
        /// A method to duplicate the OverrideRobotTool object to an Action object. 
        /// </summary>
        /// <returns> Returns a deep copy of the OverrideRobotTool object as an Action object. </returns>
        public override Action DuplicateAction()
        {
            return new OverrideRobotTool(this) as Action;
        }
        #endregion

        #region method
        /// <summary>
        /// Used to create variable definition code of this action. 
        /// </summary>
        /// <param name="robotInfo"> Defines the Robot Info were the code is generated for. </param>
        /// <returns> Returns the RAPID code line as a string. </returns>
        public override string InitRAPIDVar(RobotInfo robotInfo)
        {
            return string.Empty;
        }

        /// <summary>
        /// Used to create action instruction code line. 
        /// </summary>
        /// <param name="robotInfo"> Defines the Robot Info were the code is generated for. </param>
        /// <returns> Returns the RAPID code line as a string. </returns>
        public override string ToRAPIDFunction(RobotInfo robotInfo)
        {
            return "! " + "Default Robot Tool changed to " + robotInfo.Tool.Name + ".";
        }

        /// <summary>
        /// Used to create variable definitions in the RAPID Code. It is typically called inside the CreateRAPIDCode() method of the RAPIDGenerator class.
        /// </summary>
        /// <param name="RAPIDGenerator"> Defines the RAPIDGenerator. </param>
        public override void InitRAPIDVar(RAPIDGenerator RAPIDGenerator)
        {
        }

        /// <summary>
        /// Used to create action instructions in the RAPID Code. It is typically called inside the CreateRAPIDCode() method of the RAPIDGenerator class.
        /// </summary>
        /// <param name="RAPIDGenerator"> Defines the RAPIDGenerator. </param>
        public override void ToRAPIDFunction(RAPIDGenerator RAPIDGenerator)
        {
            RAPIDGenerator.StringBuilder.Append(Environment.NewLine + "\t\t" + this.ToRAPIDFunction(RAPIDGenerator.RobotInfo));
        }

        /// <summary>
        /// Get the name of the set Robot Tool.
        /// </summary>
        /// <returns> The name of the set Robot Tool. </returns>
        public string GetToolName()
        {
            return _robotTool.Name;
        }
        #endregion

        #region properties
        /// <summary>
        /// A boolean that indicates if the Override Robot Tool object is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                if (ToolName == null) { return false; }
                if (ToolName == "") { return false; }
                if (RobotTool == null) { return false; }
                if (RobotTool.IsValid == false) { return false; }
                return true;
            }
        }

        /// <summary>
        /// The Robot Tool that is set.
        /// </summary>
        public RobotTool RobotTool
        {
            get { return _robotTool; }
            set { _robotTool = value; }
        }

        /// <summary>
        /// The name of the set Robot Tool.
        /// </summary>
        public string ToolName
        {
            get { return _robotTool.Name; }
        }
        #endregion
    }
}
