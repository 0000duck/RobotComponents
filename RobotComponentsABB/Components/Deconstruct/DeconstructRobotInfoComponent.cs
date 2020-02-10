﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using RobotComponentsGoos.Definitions;
using RobotComponentsABB.Parameters.Definitions;

namespace RobotComponentsABB.Components.Deconstruct
{
    /// <summary>
    /// RobotComponents Deconstruct Robot Info component. An inherent from the GH_Component Class.
    /// </summary>
    public class DeconstructRobotInfoComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructRobotInfoComponent class.
        /// </summary>
        public DeconstructRobotInfoComponent()
          : base("Deconstruct Robot Info", "DeRobInfo",
              "Deconstructs a robot info definition into its constituent parts"
                + System.Environment.NewLine +
                "RobotComponents : v" + RobotComponents.Utils.VersionNumbering.CurrentVersion,
              "RobotComponents", "Deconstruct")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new RobotInfoParameter(), "Robot Info", "RI", "Robot Info as Robot Info", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Robot Name as String", GH_ParamAccess.item);
            pManager.AddMeshParameter("Meshes", "M", "Robot Meshes as Mesh List", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Axis Planes", "AP", "Axis Planes as Plane List", GH_ParamAccess.list);
            pManager.AddIntervalParameter("Axis Limits", "AL", "Axis Limits as Interval List", GH_ParamAccess.list);
            pManager.AddPlaneParameter("Position Plane", "PP", "Position Plane of the Robot as Plane", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Mounting Frame", "MF", "Mounting Frame as Frame", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Tool Plane", "TP", "Tool Plane (TCP) as Frame", GH_ParamAccess.item);
            pManager.RegisterParam(new RobotToolParameter(), "Robot Tool", "RT", "Robot Tool", GH_ParamAccess.item);
            pManager.RegisterParam(new ExternalAxisParameter(), "External Axes", "EA", "External Axes as External Axis Parameter", GH_ParamAccess.list);
        }

        // Meshes
        private List<Mesh> _meshes = new List<Mesh>() { };
        private GH_Document _doc;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get the Grasshopper document
            _doc = this.OnPingDocument();

            // Input variables
            GH_RobotInfo robotInfoGoo = null;

            // Catch the input data
            if (!DA.GetData(0, ref robotInfoGoo)) { return; }

            // Check if the input is valid
            if (!robotInfoGoo.IsValid || !robotInfoGoo.Value.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The RobotInfo is not Valid");
                return;
            }

            // Output variables
            string name;
            List<Mesh> meshes = new List<Mesh>();
            List<GH_ExternalAxis> externalAxisGoos = new List<GH_ExternalAxis>();
            List<Plane> axisPlanes;
            List<Interval> axisLimits;
            Plane basePlane;
            Plane mountingFrame;
            Plane toolPlane;
            GH_RobotTool tool;

            // Clear list with display meshes
            _meshes.Clear();

            // Name
            if (robotInfoGoo.Value.Name != null)
            {
                name = robotInfoGoo.Value.Name;
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The RobotInfo Name is not Valid");
                name = null;
            }

            // Meshes
            if (robotInfoGoo.Value.Meshes != null)
            {
                for (int i = 0; i < 7; i++)
                {
                    meshes.Add(robotInfoGoo.Value.Meshes[i]);
                    _meshes.Add(robotInfoGoo.Value.Meshes[i]);
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The RobotInfo Meshes is not Valid");
            }

            // AxisPlanes
            if (robotInfoGoo.Value.InternalAxisPlanes != null)
            {
                axisPlanes = robotInfoGoo.Value.InternalAxisPlanes;
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The RobotInfo AxisPlanes is not Valid");
                axisPlanes = null;
            }

            // AxisLimits
            if (robotInfoGoo.Value.InternalAxisLimits != null)
            {
                axisLimits = robotInfoGoo.Value.InternalAxisLimits;
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The RobotInfo AxisLimits is not Valid");
                axisLimits = null;
            }

            // BasePlane
            if (robotInfoGoo.Value.BasePlane.IsValid)
            {
                basePlane = robotInfoGoo.Value.BasePlane;
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The RobotInfo BasePlane is not Valid");
                basePlane = Plane.Unset;
            }

            // Mounting Frame / Attachment Plane
            if (robotInfoGoo.Value.MountingFrame.IsValid)
            {
                mountingFrame = robotInfoGoo.Value.MountingFrame;
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The RobotInfo MountingFrame is not Valid");
                mountingFrame = Plane.Unset;
            }

            // Tool Plane
            if (robotInfoGoo.Value.ToolPlane.IsValid)
            {
                toolPlane = robotInfoGoo.Value.ToolPlane;
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The RobotInfo ToolPlane is not Valid");
                toolPlane = Plane.Unset;
            }

            // Robot Tool
            if (robotInfoGoo.Value.Tool.IsValid)
            {
                tool = new GH_RobotTool(robotInfoGoo.Value.Tool);

                // Add display mesh
                _meshes.Add(robotInfoGoo.Value.Tool.Mesh);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The RobotInfo Tool is not Valid");
                tool = null;
            }

            // External Axes
            for (int i = 0; i < robotInfoGoo.Value.ExternalAxis.Count; i++)
            {
                externalAxisGoos.Add(new GH_ExternalAxis(robotInfoGoo.Value.ExternalAxis[i]));

                // Add display meshes
                _meshes.Add(robotInfoGoo.Value.ExternalAxis[i].BaseMesh);
                _meshes.Add(robotInfoGoo.Value.ExternalAxis[i].LinkMesh);
            }
           
            // Output
            DA.SetData(0, name);
            DA.SetDataList(1, meshes);
            DA.SetDataList(2, axisPlanes);
            DA.SetDataList(3, axisLimits);
            DA.SetData(4, basePlane);
            DA.SetData(5, mountingFrame);
            DA.SetData(6, toolPlane);
            DA.SetData(7, tool);
            DA.SetDataList(8, externalAxisGoos);
        }

        /// <summary>
        /// This method displays the meshes
        /// </summary>
        /// <param name="args"> Preview display arguments for IGH_PreviewObjects. </param>
        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            // Initiate material
            Rhino.Display.DisplayMaterial material;

            // Selected document objects
            List<IGH_DocumentObject> selectedObjects = _doc.SelectedObjects();

            // Check if component is selected
            if (selectedObjects.Contains(this))
            {
                material = args.ShadeMaterial_Selected;
            }
            else
            {
                material = args.ShadeMaterial;
            }

            // Display the meshes
            for (int i = 0; i != _meshes.Count; i++)
            {
                args.Display.DrawMeshShaded(_meshes[i], material);
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.DeconstructRobotInfoComponent_Icon; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8452629c-5da8-4e64-82f2-23f00c49ae4b"); }
        }
    }
}