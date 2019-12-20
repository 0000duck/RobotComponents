﻿using System;
using System.Drawing;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;

using RobotComponents.BaseClasses;

using RobotComponentsABB.Goos;
using RobotComponentsABB.Parameters;
using RobotComponentsABB.Utils;

namespace RobotComponentsABB.Components
{
    public class OldMovementComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear, subcategory the panel. 
        /// If you use non-existing tab or panel names new tabs/panels will automatically be created.
        /// </summary>
        public OldMovementComponent()
          : base("Action: Movement", "M",
              "Defines a robot movement instruction for simulation and code generation."
                + System.Environment.NewLine +
                "RobotComponent V : " + RobotComponents.Utils.VersionNumbering.CurrentVersion,
              "RobotComponents", "Code Generation")
        {
        }

        /// <summary>
        /// Override the component exposure (makes the tab subcategory).
        /// Can be set to hidden, primary, secondary, tertiary, quarternary, quinary, senary, septenary and obscure
        /// </summary>
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new TargetParameter(), "Target", "T", "Target as Target", GH_ParamAccess.list);
            pManager.AddParameter(new SpeedDataParam(), "Speed Data", "SD", "Speed Data as Custom Speed Data or as a number (vTCP)", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Movement Type", "MT", "Movement Type as integer. Use 0 for MoveAbsJ, 1 for MoveL and 2 for MoveJ", GH_ParamAccess.list, 0);
            pManager.AddIntegerParameter("Precision", "P", "Precision as int. If value is smaller than 0, precision will be set to fine.", GH_ParamAccess.list, 0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.RegisterParam(new MovementParameter(), "Movement", "M", "Resulting Movement");  //Todo: beef this up to be more informative.
        }

        /// <summary>
        /// Creates the value list for the motion type and connects it the input parameter is other source is connected
        /// </summary>
        private void CreateValueList()
        {
            // Gets the input parameter
            var parameter = this.Params.Input[2];

            // Creates the empty value list
            GH_ValueList obj = new GH_ValueList();
            obj.CreateAttributes();
            obj.ListMode = Grasshopper.Kernel.Special.GH_ValueListMode.DropDown;
            obj.ListItems.Clear();

            // Add the items to the value list
            obj.ListItems.Add(new GH_ValueListItem("MoveAbsJ", "0"));
            obj.ListItems.Add(new GH_ValueListItem("MoveL", "1"));
            obj.ListItems.Add(new GH_ValueListItem("MoveJ", "2"));

            // Make point where the valuelist should be created on the canvas
            obj.Attributes.Pivot = new PointF(parameter.Attributes.InputGrip.X - 120, parameter.Attributes.InputGrip.Y - 11);

            // Add the value list to the active canvas
            Instances.ActiveCanvas.Document.AddObject(obj, false);

            // Connect the value list to the input parameter
            parameter.AddSource(obj);

            // Clear input data
            parameter.ClearData();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Gets Document ID
            Guid documentGUID = this.OnPingDocument().DocumentID;

            // Input variables
            List<TargetGoo> targetGoos = new List<TargetGoo>();
            List<SpeedDataGoo> speedDataGoos = new List<SpeedDataGoo>();
            List<int> movementTypes = new List<int>();
            List<int> precisions = new List<int>();

            // Creates the input value list and attachs it to the input parameter
            if (this.Params.Input[2].SourceCount == 0)
            {
                CreateValueList();
            }

            // Catch the input data
            if (!DA.GetDataList(0, targetGoos)) { return; }
            if (!DA.GetDataList(1, speedDataGoos)) { return; }
            if (!DA.GetDataList(2, movementTypes)) { return; }
            if (!DA.GetDataList(3, precisions)) { return; }

            // Get longest Input List
            int[] sizeValues = new int[4];
            sizeValues[0] = targetGoos.Count;
            sizeValues[1] = speedDataGoos.Count;
            sizeValues[2] = movementTypes.Count;
            sizeValues[3] = precisions.Count;
            int biggestSize = HelperMethods.GetBiggestValue(sizeValues);

            // Keeps track of used indicies
            int targetGooCounter = -1;
            int speedDataGooCounter = -1;
            int movementTypeCounter = -1;
            int precisionCounter = -1;

            // Creates movements
            List<Movement> movements = new List<Movement>();

            for (int i = 0; i < biggestSize; i++)
            {
                TargetGoo targetGoo;
                SpeedDataGoo speedDataGoo;
                int movementType;
                int precision; ;

                if (i < targetGoos.Count)
                {
                    targetGoo = targetGoos[i];
                    targetGooCounter++;
                }
                else
                {
                    targetGoo = targetGoos[targetGooCounter];
                }

                if (i < speedDataGoos.Count)
                {
                    speedDataGoo = speedDataGoos[i];
                    speedDataGooCounter++;
                }
                else
                {
                    speedDataGoo = speedDataGoos[speedDataGooCounter];
                }

                if (i < movementTypes.Count)
                {
                    movementType = movementTypes[i];
                    movementTypeCounter++;
                }
                else
                {
                    movementType = movementTypes[movementTypeCounter];
                }

                if (i < precisions.Count)
                {
                    precision = precisions[i];
                    precisionCounter++;
                }
                else
                {
                    precision = precisions[precisionCounter];
                }

                Movement movement = new Movement(targetGoo.Value, speedDataGoo.Value, movementType, precision, documentGUID);
                movements.Add(movement);
            }

            // Check if a right value is used for the movement type
            for (int i = 0; i < movementTypes.Count; i++)
            {
                if (movementTypes[i] != 0 && movementTypes[i] != 1 && movementTypes[i] != 2)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Movement type value <" + i + "> is invalid. " +
                        "In can only be set to 0, 1 and 2. Use 1 for MoveAbsJ, 2 for MoveL and 3 for MoveJ.");
                    break;
                }
            }

            // Check if a right value is used for the input of the precision
            for (int i = 0; i < precisions.Count; i++)
            {
                if (HelperMethods.PrecisionValueIsValid(precisions[i]) == false)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Precision value <" + i + "> is invalid. " +
                        "In can only be set to -1, 0, 1, 5, 10, 15, 20, 30, 40, 50, 60, 80, 100, 150 or 200. " +
                        "A value of -1 will be interpreted as fine movement in RAPID Code.");
                    break;
                }
            }

            // Check if a right predefined speeddata value is used
            for (int i = 0; i < speedDataGoos.Count; i++)
            {
                if (speedDataGoos[i].Value.PreDefinied == true)
                {
                    if (HelperMethods.PredefinedSpeedValueIsValid(speedDataGoos[i].Value.V_TCP) == false)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Pre-defined speed data <" + i +
                            "> is invalid. Use the speed data component to create custom speed data or use of one of the valid pre-defined speed datas. " +
                            "Pre-defined speed data can be set to 5, 10, 20, 30, 40, 50, 60, 80, 100, 150, 200, 300, " +
                            "400, 500, 600, 800, 1000, 1500, 2000, 2500, 3000, 4000, 5000, 6000 or 7000.");
                        break;
                    }
                }
            }

            // Output
            DA.SetDataList(0, movements);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return RobotComponentsABB.Properties.Resources.Movement_Icon_Old; }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F2BBBB2D-96F7-4D65-9031-A6C08D14A448"); }
        }

    }

}