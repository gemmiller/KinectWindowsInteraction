﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Samples.Kinect.SpeechBasics
{
    class Kinect_Drawing
    {
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private float RenderWidth = 0;
        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private float RenderHeight = 0;
        private KinectSensor sensor = null;
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;
        private Image image;
        public Kinect_Drawing(KinectSensor sensor, float width, float height, Image image)
        {
            RenderWidth = width;
            RenderHeight = height;
            this.sensor = sensor;
            this.image = image;
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();
            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);
            // Display the drawing using our image control
            this.image.Source = this.imageSource;
        }
        #region Reference Variables
            /// <summary>
            /// Thickness of drawn joint lines
            /// </summary>
            private const double JointThickness = 3;
            /// <summary>
            /// Thickness of body center ellipse
            /// </summary>
            private const double BodyCenterThickness = 10;
            /// <summary>
            /// Thickness of clip edge rectangles
            /// </summary>
            private const double ClipBoundsThickness = 10;
            /// <summary>
            /// Brush used to draw skeleton center point
            /// </summary>
            private readonly Brush centerPointBrush = Brushes.Blue;
            /// <summary>
            /// Brush used for drawing joints that are currently tracked
            /// </summary>
            private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
            /// <summary>
            /// Brush used for drawing joints that are currently inferred
            /// </summary>        
            private readonly Brush inferredJointBrush = Brushes.Yellow;
            /// <summary>
            /// Pen used for drawing bones that are currently tracked
            /// </summary>
            private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);
            /// <summary>
            /// Pen used for drawing bones that are currently inferred
            /// </summary>        
            private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);
            //Custom
            private readonly Brush CalibrationBrush = Brushes.Purple;
            private readonly Brush BoundBrush = Brushes.Red;
            private readonly Pen BoundPen = new Pen(Brushes.Red, 6);
        #endregion
        public void DrawSkeleton(Skeleton[] skeletons)
        {
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                //dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, RenderWidth - 10f, RenderHeight - 10f));
                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);                                               //Render Clipped Edges

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)                    //If skeleton is tracked, show on screen
                            DrawBonesAndJoints(skel, dc);
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)          //If only position of person not full skeleton
                        {
                            dc.DrawEllipse(this.centerPointBrush, null, this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness, BodyCenterThickness);
                        }
                    }
                }
            }
            // prevent drawing outside of our render area
            drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
        }
        // Maps a SkeletonPoint to lie within our render space and converts to Point(point to map) Returns:mapped point
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(
                                                                             skelpoint,
                                                                             DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }
        // Draws a skeleton's bones and joints(skeleton to draw, drawing context to draw to)
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }
        // Draws a bone line between two joints (skeleton to draw bones from, drawing context to draw to, joint to start drawing from, joint to end drawing at)
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }
        // Draws indicators to show which edges are clipping skeleton data (skeleton to draw clipping information for, drawing context to draw to)
        private void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }
    }
}