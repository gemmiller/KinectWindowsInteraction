//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    #region "Includes"
        //vjoy addition
        using System; //(necessary for math)
        using System.Collections.Generic;
        using System.ComponentModel;
        using System.Data;
        using System.Linq;
        using System.Text;
        using System.Runtime.InteropServices;
        using System.Deployment;
        using System.Windows.Threading;
        //kinnect original
        using System.IO;
        using System.Windows;
        using System.Windows.Media;
        using Microsoft.Kinect;
    #endregion
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region "Dimensions and Declarations"
            #region "Constants"
                /// <summary>
                /// Width of output drawing
                /// </summary>
                private const float RenderWidth = 640.0f;
                /// <summary>
                /// Height of our output drawing
                /// </summary>
                private const float RenderHeight = 480.0f;
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
            #endregion
            #region "Readonly Reference"
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
            #region "Variables"
                #region "Default"
                    /// <summary>
                /// Active Kinect sensor
                /// </summary> 
                    private KinectSensor sensor;
                    /// <summary>
                    /// Drawing group for skeleton rendering output
                    /// </summary>
                    private DrawingGroup drawingGroup;
                    /// <summary>
                    /// Drawing image that we will display
                    /// </summary>
                    private DrawingImage imageSource;
                #endregion
                #region "Custom"
                    private VJoyDemo.VJoy m_vjoy = null;
                    public float [,]LeftData = new float[10,3];
                    public float [,]RightData = new float[10, 3];
                    public Vector4 [] HandPos = new Vector4[2];
                    public bool OneHandedMode = false;
                    public float []zthreashold = {(float)0.85,(float)0.85};
                    public int index = 0;
                    public int OpMode = 0;                                                                  //Used to delineate between driving and calibration mode.  1 = calibration, 0 = driving
                    public Vector4[,] Calibration = new Vector4[2,4];
                    public Vector4[] CalMidpoints = new Vector4[2];
                    public Vector4[] CalDistance = new Vector4[2];
                    //public Vector4[] CalOffset = new Vector4[2];
                    public Vector4[] CalData = new Vector4[11];
                    public Vector[] Bounds = new Vector[6];
                    public int CalState = -1;
                    public int clock=0;
                    const int hTop = 0;
                    const int hBottom = 1;
                    const int hLeft = 3;
                    const int hRight = 2;
                    const int LHand = 0;
                    const int RHand = 1;
                    bool Calibrated = false;
                    public KinectSensor myKinect;
                    public bool ReCapture = false;
                #endregion
            #endregion
        #endregion

        #region "Form Handlers and Initialization"
            // Initialize a new instance of the MainWindow class.
            public MainWindow()
            {
                InitializeComponent();
            }
            // Execute startup tasks
            private void WindowLoaded(object sender, RoutedEventArgs e)
            {
                m_vjoy = new VJoyDemo.VJoy();
                m_vjoy.Initialize();
                m_vjoy.Update(0);
                checkBoxSeatedMode.IsChecked = true;
                #region "Start Kinect"
                this.drawingGroup = new DrawingGroup();                         // Create the drawing group we'll use for drawing
                this.imageSource = new DrawingImage(this.drawingGroup);         // Create an image source that we can use in our image control
                Image.Source = this.imageSource;                                // Display the drawing using our image control
            
                foreach (var potentialSensor in KinectSensor.KinectSensors)     // Look through all sensors and start the first connected one.
                {                                                               // This requires that a Kinect is connected at the time of app startup.
                    if (potentialSensor.Status == KinectStatus.Connected)       // To make your app robust against plug/unplug, 
                    {                                                           // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
                        this.sensor = potentialSensor;
                        break;
                    }
                }
            
                if (null != this.sensor)                                        //If there is a sensor
                {
                    myKinect = this.sensor;
                    //TransformSmoothParameters Smooth = new TransformSmoothParameters();
                    //    Smooth.Smoothing = 0.7f;
                    //    Smooth.Correction = 0.3f;
                    //    Smooth.Prediction = 0.5f;
                    //    Smooth.JitterRadius = 1.0f;
                    //    Smooth.MaxDeviationRadius = 1.0f;
                    myKinect.SkeletonStream.Enable();                                // Turn on the skeleton stream to receive skeleton frames
                    myKinect.SkeletonFrameReady += this.SensorSkeletonFrameReady;    // Add an event handler to be called whenever there is new color frame data
                    myKinect.DepthStream.Range = DepthRange.Near;                    //Attempt to set depth range...     
                    myKinect.SkeletonStream.EnableTrackingInNearRange = true;
                    myKinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    try
                    {
                        myKinect.Start();                                    // Start the sensor!
                    }
                    catch (IOException)
                    {
                        myKinect = null;                                     //Error!
                    }
                }
                if (null == this.sensor)
                {
                    this.statusBarText.Text = Properties.Resources.NoKinectReady;  //Throw Flag
                }
            
                #endregion
            }
            /// Execute shutdown tasks
            private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
            {
                //ADDED!!!!
                m_vjoy.Shutdown();
                //ADDED!!!
                if (null != this.sensor)
                    {this.sensor.Stop();}
            }
        #endregion             

        #region "Skeleton Data Collection Smoothing"
            /// Event handler for Kinect sensor's SkeletonFrameReady event
            private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
            {
                Skeleton[] skeletons = new Skeleton[0];
                using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (skeletonFrame != null)
                        {skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                         skeletonFrame.CopySkeletonDataTo(skeletons);}
                }
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    // Draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                    #region "Get Skeleton Data and Display"
                    if (skeletons.Length != 0)
                    {
                        if (ReCapture == true)
                        {
                            if (myKinect.SkeletonStream.AppChoosesSkeletons==false)
                            {
                                myKinect.SkeletonStream.AppChoosesSkeletons = true; // Ensure AppChoosesSkeletons is set
                            }

                            float closestDistance = 10000f; // Start with a far enough distance
                            int closestID = 0;

                            foreach (Skeleton skeleton in skeletons.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked))
                            {
                                if (skeleton.Position.Z < closestDistance)
                                {
                                    closestID = skeleton.TrackingId;
                                    closestDistance = skeleton.Position.Z;
                                }
                            }
                            if (closestID > 0)
                            {
                                myKinect.SkeletonStream.ChooseSkeletons(closestID); // Track this skeleton
                            }
                            ReCapture = false;
                        }
                        foreach (Skeleton skel in skeletons)
                        {
                            if (skel.TrackingId != 0)
                            { skelId.Text = skel.TrackingId.ToString(); }
                            //if (skel.TrackingId == 31)
                            //{
                                RenderClippedEdges(skel, dc);
                                if (skel.TrackingState == SkeletonTrackingState.Tracked)
                                    {this.DrawBonesAndJoints(skel, dc);}
                                else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                                {
                                    dc.DrawEllipse(
                                    this.centerPointBrush,
                                    null,
                                    this.SkeletonPointToScreen(skel.Position),
                                    BodyCenterThickness,
                                    BodyCenterThickness);
                                }
                                foreach (Joint tjoint in skel.Joints)
                                {
                                    if (tjoint.JointType == JointType.HandLeft & (tjoint.TrackingState == JointTrackingState.Inferred | tjoint.TrackingState == JointTrackingState.Tracked))
                                    {
                                        Vector4 Average = Averaging(LeftData, JointToVector(tjoint));
                                        DrawPoint(Average,this.centerPointBrush, BodyCenterThickness, dc);
                                        HandPos[0] = Average;
                                        DrawText(Average, "Joy 1\r\n(" + Average.X.ToString() + "," + Average.Y.ToString() + "," + Average.Z.ToString() + ")", 15, dc);
                                        LeftHand.Text = "Left Hand: (" + Average.X.ToString() + ", " + Average.Y.ToString() + ", " + Average.Z.ToString() + ")";
                                    }
                                    if (tjoint.JointType == JointType.HandRight & (tjoint.TrackingState == JointTrackingState.Inferred | tjoint.TrackingState == JointTrackingState.Tracked))
                                    {
                                        Vector4 Average = Averaging(RightData, JointToVector(tjoint));
                                        DrawPoint(Average, this.centerPointBrush, BodyCenterThickness, dc);
                                        HandPos[1] = Average;
                                        DrawText(Average, "Joy 2\r\n(" + Average.X.ToString() + "," + Average.Y.ToString() + "," + Average.Z.ToString() + ")", 15, dc);
                                        RightHand.Text = "Right Hand: (" + Average.X.ToString() + ", " + Average.Y.ToString() + ", " + Average.Z.ToString() + ")";
                                    }
                                }
                            //}
                        }
                    }
                    #endregion
                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                    Execute(dc);
                }
            }       
            private Vector4 Averaging(float[,] Data, Vector4 tVector)
            {
                Data[index, 0] = tVector.X;
                Data[index, 1] = tVector.Y;
                Data[index, 2] = tVector.Z;
                index += 1;
                if (index > 9)
                { index = 0; }
                float[] Average = new float[3];
                for (int j = 0; j < 3; j++)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Average[j] += Data[i, j];
                    }
                    Average[j] /= 10;
                    int[] extremes = { 0, 0, 0 };
                    for (int i = 0; i < 10; i++)
                    {
                        if (Math.Abs(Average[j] - Data[i, j]) > Math.Abs(Average[j] - Data[extremes[0], j]))
                        {
                            extremes[0] = i;
                        }
                    }
                    Average[j] *= 10;
                    Average[j] -= Data[extremes[0], j];
                    Average[j] /= 9;
                    Average[j] = ((float)Math.Round(Average[j], 3));
                }
                Vector4 vAverage = ArraytoVector(Average);
                return vAverage;
            }
        #endregion

        #region "Interpretation Execution (Calibration, and Joystick routines)"
            public void Execute(DrawingContext dc)
            {
                #region "Avoid Timeout"
                    clock += 1;
                    lblClock.Text = "Clock: " + clock.ToString();
                    if (clock > 1000)
                    {
                        m_vjoy = new VJoyDemo.VJoy();
                        m_vjoy.Initialize();
                        m_vjoy.Update(0);
                        clock = 0;
                    }
                #endregion
                Vector4 coord;
                switch (OpMode)
                {
                    //Uses default Skeleton Zero (defined in meters)
                    case 0:
                        SendJoyVals(RawToJoy(HandPos));
                        break;
                    #region "Calibration Points"
                        case 1:
                            coord = ListToVector(0, 0, 0);
                            Calibrate("Left Hand:\r\nTop Threshold", 0, 0, coord, dc);
                            break;
                        case 2:
                            coord = ListToVector(0, 0, 0);
                            Calibrate("Left Hand:\r\nBottom Threshold", 0, 1, coord, dc);
                            break;
                        case 3:
                            coord = ListToVector(0, 0, 0);
                            Calibrate("Left Hand:\r\nLeft Threshold", 0, 3, coord, dc);
                            break;
                        case 4:
                            coord = ListToVector(0, 0, 0);
                            Calibrate("Left Hand:\r\nRight Threshold", 0, 2, coord, dc);
                            break;
                        case 5:
                            coord = ListToVector(0, 0, 0);
                            Calibrate("Right Hand:\r\nTop Threshold", 1, 0, coord, dc);
                            break;
                        case 6:
                            coord = ListToVector(0, 0, 0);
                            Calibrate("Right Hand:\r\nBottom Threshold", 1, 1, coord, dc);
                            break;
                        case 7:
                            coord = ListToVector(0, 0, 0);
                            Calibrate("Right Hand:\r\nLeft Threshold", 1, 3, coord, dc);
                            break;
                        case 8:
                            coord = ListToVector(0, 0, 0);
                            Calibrate("Right Hand:\r\nRight Threshold", 1, 2, coord, dc);
                            break;
                    #endregion
                    #region "Calibration Calculations"
                        case 9:
                            CalMidpoints[0].X = (Calibration[0,hRight].X+Calibration[0,hLeft].X)/2;
                            CalMidpoints[1].X = (Calibration[1,hRight].X+Calibration[1,hLeft].X)/2;

                            CalMidpoints[0].Y = (Calibration[0,hTop].Y+Calibration[0,hBottom].Y)/2;
                            CalMidpoints[1].Y = (Calibration[1,hTop].Y+Calibration[1,hBottom].Y)/2;
                            Midpoint.Text = "<" + CalMidpoints[0].X.ToString() + "," + CalMidpoints[0].Y.ToString() + "> <" + CalMidpoints[0].X.ToString() + "," + CalMidpoints[0].Y.ToString() + ">";

                            CalDistance[0].X = (Calibration[0,hRight].X-Calibration[0,hLeft].X);
                            CalDistance[1].X = (Calibration[1,hRight].X-Calibration[1,hLeft].X);

                            CalDistance[0].Y = (Calibration[0,hTop].Y-Calibration[0,hBottom].Y);
                            CalDistance[1].Y = (Calibration[1,hTop].Y-Calibration[1,hBottom].Y);
                            Distance.Text = "<X=" + CalDistance[0].X.ToString() + " Y=" + CalDistance[0].Y.ToString()+"> <X=" + CalDistance[0].X.ToString() + " Y=" + CalDistance[0].Y.ToString()+">";
                            Conversion.Text = "<X=" + (128 / CalDistance[0].X).ToString() + " Y=" + (128 / CalDistance[0].Y).ToString()+ "> <X=" + (128 / CalDistance[0].X).ToString() + " Y=" + (128 / CalDistance[0].Y).ToString()+ ">";
                            for (int i = 0; i < 4; i++)
                            {
                                zthreashold[0] += Calibration[0, i].Z;
                                zthreashold[1] += Calibration[1, i].Z;
                            }
                            zthreashold[0] /= 4;
                            zthreashold[1] /= 4;
                            Bounds[hTop].Y = Calibration[RHand, hTop].Y;
                            Bounds[hBottom].Y = Calibration[RHand, hRight].Y;
                            Bounds[2].X = CalMidpoints[RHand].X - ((40*CalDistance[1].X)/100); //OuterLeft
                            Bounds[3].X = CalMidpoints[RHand].X - ((20*CalDistance[1].X)/100); //InnerLeft
                            Bounds[4].X = CalMidpoints[RHand].X + ((20*CalDistance[1].X)/100); //InnerRight
                            Bounds[5].X = CalMidpoints[RHand].X + ((40*CalDistance[1].X)/100); //OuterRight
                            Calibrated = true;
                            OpMode = 10;
                            goto case 10;
                    #endregion
                    //Uses points from Calibration
                    case 10:                        
                        SendJoyVals(CalToJoy(HandPos));                        
                        #region "Draw"
                        if (checkBoxShowBounds.IsChecked == true)
                            {
                                foreach (Vector4 tVector in Calibration)
                                    { DrawPoint(tVector, this.CalibrationBrush, 5, dc); }
                                //??? Why are there no dots.... for the Right Hand
                                DrawPoint(ListToVector((float)Bounds[hTop].Y, (float)Bounds[2].X, (float)zthreashold[1]), this.BoundBrush, 3, dc);
                                DrawPoint(ListToVector((float)Bounds[hTop].Y, (float)Bounds[3].X, (float)zthreashold[1]), this.BoundBrush, 3, dc);
                                DrawPoint(ListToVector((float)Bounds[hTop].Y, (float)Bounds[4].X, (float)zthreashold[1]), this.BoundBrush, 3, dc);
                                DrawPoint(ListToVector((float)Bounds[hTop].Y, (float)Bounds[5].X, (float)zthreashold[1]), this.BoundBrush, 3, dc);
                                DrawPoint(ListToVector((float)Bounds[hBottom].Y, (float)Bounds[2].X, (float)zthreashold[1]), this.BoundBrush, 3, dc);
                                DrawPoint(ListToVector((float)Bounds[hBottom].Y, (float)Bounds[3].X, (float)zthreashold[1]), this.BoundBrush, 3, dc);
                                DrawPoint(ListToVector((float)Bounds[hBottom].Y, (float)Bounds[4].X, (float)zthreashold[1]), this.BoundBrush, 3, dc);
                                DrawPoint(ListToVector((float)Bounds[hBottom].Y, (float)Bounds[5].X, (float)zthreashold[1]), this.BoundBrush, 3, dc);
                                DrawLine(ListToVector((float)Bounds[2].X, (float)Bounds[hTop].Y, zthreashold[1]), ListToVector((float)Bounds[2].X, (float)Bounds[hBottom].Y, zthreashold[1]), dc);
                                DrawLine(ListToVector((float)Bounds[3].X, (float)Bounds[hTop].Y, zthreashold[1]), ListToVector((float)Bounds[3].X, (float)Bounds[hBottom].Y, zthreashold[1]), dc);
                                DrawLine(ListToVector((float)Bounds[4].X, (float)Bounds[hTop].Y, zthreashold[1]), ListToVector((float)Bounds[4].X, (float)Bounds[hBottom].Y, zthreashold[1]), dc);
                                DrawLine(ListToVector((float)Bounds[5].X, (float)Bounds[hTop].Y, zthreashold[1]), ListToVector((float)Bounds[5].X, (float)Bounds[hBottom].Y, zthreashold[1]), dc);
                            }
                        #endregion
                        break;
                }
            }
            #region "Joystick Routines"
                public void SendJoyVals(Vector4 []tVector)
                {
                    if (OneHandedMode == true)
                    {
                        #region "One Handed Driving"
                            m_vjoy.SetXAxis(0, (byte)0);
                            m_vjoy.SetYAxis(0, (byte)0);
                            if (tVector[LHand].Z < (zthreashold[LHand]-.15+(SpeedAdjust.Value/10)))
                            {
                                //OBSOLETE
                                    //float tmpRight = (float)Math.Round(((Math.Pow(tVector[LHand].Y,3)/16384)*0.7 + (float)(Math.Pow(tVector[LHand].X,3)/16384) ));
                                    //float tmpLeft  = (float)Math.Round(((Math.Pow(tVector[LHand].Y,3)/16384)*0.7 - (float)(Math.Pow(tVector[LHand].X,3)/16384) ));//tVector[LHand].X)*.75);
                                    //float tmpRight = (float)Math.Round(((Math.Pow(tVector[LHand].Y, 3) / (Math.Pow(128,2) * .7))*.75 - (Math.Pow(tVector[LHand].Y, 7) / (Math.Pow(128,6) * 1.6))) + ((Math.Pow(tVector[LHand].X, 3) / (Math.Pow(128,2) * .7)) - (Math.Pow(tVector[LHand].X, 7) / (Math.Pow(128,6) * 1.6)))*.6);
                                    //float tmpLeft = (float)Math.Round(((Math.Pow(tVector[LHand].Y, 3) / (Math.Pow(128,2) * .7))*.75 - (Math.Pow(tVector[LHand].Y, 7) / (Math.Pow(128,6) * 1.6))) - ((Math.Pow(tVector[LHand].X, 3) / (Math.Pow(128,2) * .7)) - (Math.Pow(tVector[LHand].X, 7) / (Math.Pow(128,6) * 1.6)))*.6);
                                //Speed
                                    float SpeedFactor = 0;    
                                    if (tVector[LHand].Y > 90)
                                        {SpeedFactor = (((tVector[LHand].Y - 90) *90)/ 38) + 38;}
                                    else if (tVector[LHand].Y > 24)
                                        {SpeedFactor = (((tVector[LHand].Y - 24) * 38) / 66);}
                                    else if (tVector[LHand].Y < -90)
                                        {SpeedFactor = (((tVector[LHand].Y + 90) * 90) / 38) - 38; }
                                    else if (tVector[LHand].Y < -24)
                                        {SpeedFactor = (((tVector[LHand].Y + 24) * 38 )/ 66);}
                                //Turn
                                    float TurnFactor = 0;
                                    if (tVector[LHand].X > 90)
                                        {TurnFactor = (((tVector[LHand].X - 90) * 28)/ 38) + 28;}
                                    else if (tVector[LHand].X > 24)
                                        {TurnFactor = (((tVector[LHand].X - 24) * 28) / 66);}
                                    else if (tVector[LHand].X < -90)
                                        {TurnFactor = (((tVector[LHand].X + 90) * 28 )/ 38) - 28;}
                                    else if (tVector[LHand].X < -24)
                                        {TurnFactor = (((tVector[LHand].X + 24) * 28) / 66);}
                                //Calculate Motor Speed
                                    float tmpRight = (float)Math.Round(SpeedFactor+TurnFactor);
                                    float tmpLeft = (float)Math.Round(SpeedFactor-TurnFactor);
                                //Truncate Speed to -127 by 127 range
                                    if (tmpRight > 127)
                                        {tmpRight = 127;}
                                    if (tmpRight < -127)
                                        {tmpRight = -127;}
                                    if (tmpLeft > 127)
                                        {tmpLeft = 127;}
                                    if (tmpLeft < -127)
                                        {tmpLeft = -127;}
                                //Set Joystick ValsVals
                                    m_vjoy.SetYAxis(0, (byte)(256 - tmpRight));
                                    m_vjoy.SetXAxis(0, (byte)(tmpLeft));
                            }
                            //Send Joystick Changes
                            m_vjoy.Update(0);
                        #endregion
                        #region "Arm"
                            if (Calibrated == true)
                            {
                                try
                                { Arm_sl.Value = tVector[RHand].Y; }
                                catch (Exception)
                                { }
                                //Set buttons to zero so they are only pressed if set recently (gets rid of ghosts)
                                m_vjoy.SetButton(0, 0, false);
                                m_vjoy.SetButton(0, 1, false);
                                m_vjoy.SetButton(0, 2, false);
                                m_vjoy.SetButton(0, 3, false);
                                m_vjoy.SetButton(0, 4, false);
                                m_vjoy.SetButton(0, 5, false);
                                if (tVector[RHand].Z <= zthreashold[RHand] - 0.15 + (TurnAdjust.Value / 10))
                                {
                                    //Arm
                                    if ((BoundsToJoy((float)Bounds[2].X) <= tVector[RHand].X) & (tVector[RHand].X <= BoundsToJoy((float)Bounds[5].X)))
                                    {
                                        if (tVector[RHand].Y <= -50)
                                            {m_vjoy.SetButton(0, 0, true);}
                                        else if (tVector[RHand].Y <= -30)
                                            {m_vjoy.SetButton(0, 1, true);}
                                        else if (tVector[RHand].Y >= 50)
                                            {m_vjoy.SetButton(0, 3, true);}
                                        else if (tVector[RHand].Y >= 30)
                                            {m_vjoy.SetButton(0, 2, true);}
                                    }
                                    //Batans
                                    BatonDispense.Foreground = Brushes.Green;
                                    if (tVector[RHand].X >= BoundsToJoy((float)Bounds[4].X))       //Extract
                                    {
                                        m_vjoy.SetButton(0, 4, true);
                                        BatonDispense.Text = "Extract";
                                    }
                                    else if (tVector[RHand].X <= BoundsToJoy((float)Bounds[3].X))  //Dispense
                                    {
                                        m_vjoy.SetButton(0, 5, true);
                                        BatonDispense.Text = "Dispense";
                                    }
                                    else
                                    { BatonDispense.Text = "Still"; }
                                }
                                else
                                { BatonDispense.Foreground = Brushes.Yellow; }
                                //Send Arm Changes
                                m_vjoy.Update(0);
                            }
                        #endregion
                    }
                    else
                    {
                        #region "Two Handed Driving"
                            m_vjoy.SetYAxis(0, (byte)0);
                            m_vjoy.SetXAxis(0, (byte)0);
                            if (tVector[LHand].Z < zthreashold[LHand] - .15 + (SpeedAdjust.Value / 10))
                                {float tmpRight = 0;
                                 if (tVector[LHand].Y > 90)
                                    {tmpRight = (((tVector[LHand].Y - 90) * 90) / 38) + 38; }
                                 else if (tVector[LHand].Y > 24)
                                    {tmpRight = (((tVector[LHand].Y - 24) * 38) / 66); }
                                 else if (tVector[LHand].Y < -90)
                                    {tmpRight = (((tVector[LHand].Y + 90) * 90) / 38) - 38; }
                                 else if (tVector[LHand].Y < -24)
                                    {tmpRight = (((tVector[LHand].Y + 24) * 38) / 66); }
                                 tmpRight = (float)Math.Round(tmpRight);
                                 m_vjoy.SetYAxis(0, (byte)(255 - (byte)tmpRight));}
                            if (tVector[RHand].Z < zthreashold[RHand]-.15+(SpeedAdjust.Value/10))
                                {float tmpLeft = 0;
                                 if (tVector[LHand].Y > 90)
                                    {tmpLeft = (((tVector[LHand].Y - 90) * 90) / 38) + 38; }
                                 else if (tVector[LHand].Y > 24)
                                    {tmpLeft = (((tVector[LHand].Y - 24) * 38) / 66); }
                                 else if (tVector[LHand].Y < -90)
                                    {tmpLeft = (((tVector[LHand].Y + 90) * 90) / 38) - 38; }
                                 else if (tVector[LHand].Y < -24)
                                    {tmpLeft = (((tVector[LHand].Y + 24) * 38) / 66); }
                                 tmpLeft = (float)Math.Round(tmpLeft);
                                 m_vjoy.SetXAxis(0, (byte)tmpLeft);}
                            m_vjoy.Update(0);
                        #endregion
                    }
                    #region "Status Report"
                        #region "Motor"
                            if (m_vjoy.GetYAxis(0)>128)
                                {lMotor.Value = 256+128-(int)m_vjoy.GetYAxis(0);}
                            else
                                {lMotor.Value = 128-(int)m_vjoy.GetYAxis(0); }
                            if (m_vjoy.GetXAxis(0) > 128)
                                {rMotor.Value = (int)m_vjoy.GetXAxis(0) - 128; }
                            else
                                {rMotor.Value = (int)m_vjoy.GetXAxis(0) + 128; }
                        #endregion
                        #region "Arm"
                            if (m_vjoy.GetButton(0,0)==true)
                                {Arm_sl.Background = Brushes.Orange;}
                            else if (m_vjoy.GetButton(0,1)==true)
                                {Arm_sl.Background = Brushes.Yellow;}
                            else if (m_vjoy.GetButton(0,2)==true)
                                {Arm_sl.Background = Brushes.Green;}
                            else if (m_vjoy.GetButton(0,3)==true)
                                {Arm_sl.Background = Brushes.LightGreen;}
                            else
                                {Arm_sl.Background = Brushes.Black;}
                            if (m_vjoy.GetButton(0,4))
                                {}
                            if (m_vjoy.GetButton(0,5))
                                {}
                        #endregion  
                    #endregion
                }
                public Vector4 []RawToJoy(Vector4 []tVector)  //convert from Meters to joystick value from 0 to 255
                {
                    tVector[0].X = (float)Math.Round(((tVector[0].X) * 255), 0);                
                    tVector[1].X = (float)Math.Round(((tVector[1].X) * 255), 0);        
        
                    tVector[0].Y = (float)Math.Round(((tVector[0].Y) * 255), 0);                
                    tVector[1].Y = (float)Math.Round(((tVector[1].Y) * 255), 0);                
                    return tVector;
                    //byte tByte = (byte)Math.Round(((tFloat) * 255), 0);                
                    //return tByte;
                }
                public Vector4 []CalToJoy(Vector4 []tVector)
                {
                    //Left X
                    if (tVector[LHand].X <= Calibration[LHand, hLeft].X)
                        {tVector[LHand].X = -128;}
                    else if (tVector[LHand].X >= Calibration[LHand, hRight].X)
                        { tVector[LHand].X = 128; }
                    else
                        {tVector[LHand].X = (float)Math.Round(((tVector[LHand].X - CalMidpoints[LHand].X) *(256/CalDistance[LHand].X)), 0); }
                    //Right X
                    if (tVector[RHand].X <= Calibration[RHand, hLeft].X)
                        {tVector[RHand].X = -128; }
                    else if (tVector[RHand].X >= Calibration[RHand, hRight].X)
                        {tVector[RHand].X = 128; }
                    else
                        {tVector[RHand].X = (float)Math.Round(((tVector[RHand].X - CalMidpoints[RHand].X)*(256/CalDistance[RHand].X)), 0); }
                    //Left Y
                    if (tVector[LHand].Y <= Calibration[LHand, hBottom].Y)
                        {tVector[LHand].Y = -128; }
                    else if (tVector[LHand].Y >= Calibration[LHand, hTop].Y)
                        {tVector[LHand].Y = 128; }
                    else
                        {tVector[LHand].Y = (float)Math.Round(((tVector[LHand].Y - CalMidpoints[LHand].Y) * (256/CalDistance[LHand].Y)), 0); }
                    //Right Y
                    if (tVector[RHand].Y <= Calibration[RHand, hBottom].Y)
                        {tVector[RHand].Y = -128; }
                    else if (tVector[RHand].Y >= Calibration[RHand, hTop].Y)
                        {tVector[RHand].Y = 128; }
                    else
                        {tVector[RHand].Y = (float)Math.Round(((tVector[RHand].Y - CalMidpoints[RHand].Y)*(256/CalDistance[RHand].Y)), 0); }
                    Out.Text = tVector[LHand].X.ToString() + " : " + tVector[LHand].Y.ToString() + " - " +tVector[LHand].X.ToString() + " : " + tVector[LHand].Y.ToString();
                    return tVector;
                }
                public float BoundsToJoy(float tBounds)
                {
                    float tfloat = (float)Math.Round(((tBounds - CalMidpoints[RHand].X)*(256/CalDistance[RHand].X)), 0);
                    return tfloat;
                }
            #endregion
            #region "Calibration"
                public void Calibrate(string tString, int tHand, int tCorner, Vector4 tcoord, DrawingContext dc)
            {
                DrawText(tcoord, tString,28, dc);
                if (clock/20 > 10)
                {
                    CalState += 1; if (CalState > 9) { CalState = 0; }
                    CalData[CalState] = HandPos[tHand];
                    Vector4 Average = CalAverage(CalData);
                    if (CalTollerance(CalData, Average) == true)
                    {
                        Calibration[tHand, tCorner] = Average;
                        CalData = new Vector4[11];
                        CalState = -1;
                        clock = 0;
                        OpMode += 1;
                    }
                }
            }
                public Vector4 CalAverage(Vector4[] tCal)
            {
                Vector4 Average = new Vector4();
                for (int j = 0; j < 10; j++)
                {
                    Average.X+=tCal[j].X;
                    Average.Y+=tCal[j].Y;
                    Average.Z+=tCal[j].Z;
                }
                Average.X /= 10;
                Average.Y /= 10;
                Average.Z /= 10;
                return Average;
            }
                public bool CalTollerance(Vector4 []tCal, Vector4 Average)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (Math.Abs(tCal[j].X - Average.X) > 0.01)
                    { return false; }
                    if (Math.Abs(tCal[j].Y - Average.Y) > 0.01)
                    { return false; }
                }
                return true;
            }
            #endregion
        #endregion

        #region "Drawing"
            #region "custom"
                public void DrawPoint(Vector4 Coord,Brush tbrush, double thick, DrawingContext dc)                             //Used to draw the position of the averaged values
                {
                    SkeletonPoint tSkel = new SkeletonPoint();
                    tSkel = VectortoSkeleton(Coord);
                    dc.DrawEllipse(
                            this.centerPointBrush,
                            null, this.SkeletonPointToScreen(tSkel),
                            BodyCenterThickness,
                            BodyCenterThickness);
                }
                public void DrawLine(Vector4 Xa, Vector4 Xb, DrawingContext dc)
                {
                    Point tA = this.SkeletonPointToScreen(VectortoSkeleton(Xa));
                    Point tB = this.SkeletonPointToScreen(VectortoSkeleton(Xb));
                    dc.DrawLine(this.BoundPen,
                        tA, tB);
                }
                public void DrawText(Vector4 Coord, string tString, int size, DrawingContext dc)                      //Used to draw Text in the skeleton Frame
                {
                    SkeletonPoint Draw = new SkeletonPoint();
                    Draw = VectortoSkeleton(Coord);
                    Typeface FontStyle = new Typeface("Calibri");
                    FormattedText tText = new FormattedText(tString,
                    System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, FontStyle,
                    emSize: size, foreground: centerPointBrush);
                    dc.DrawText(tText, this.SkeletonPointToScreen(Draw));
                }
            #endregion
            #region "default"
                // Maps a SkeletonPoint to lie within our render space and converts to Point(point to map) Returns:mapped point
                private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
                {
                    // Convert point to depth space.  
                    // We are not using depth directly, but we do want the points in our 640x480 output resolution.
                    DepthImagePoint depthPoint = this.sensor.MapSkeletonPointToDepth(
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
                private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
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
            #endregion
        #endregion

        #region "Vector conversions"
            public Vector4 ListToVector(float tx, float ty, float tz)
            {
                Vector4 tVector = new Vector4();
                tVector.X=tx; tVector.Y=ty; tVector.Z=tz;
                return tVector;
            }
            public Vector4 ArraytoVector(float []tArray)
            {
                Vector4 tVector = new Vector4();
                tVector.X = tArray[0];
                tVector.Y = tArray[1];
                tVector.Z = tArray[2];
                return tVector;
            }
            public float []VectortoArray(Vector4 tVector)
            {
                float []tArray = new float[3]; tArray[0]=tVector.X; tArray[1]=tVector.Y; tArray[2]=tVector.Z;
                return tArray;
            }
            public SkeletonPoint VectortoSkeleton(Vector4 tVector)
            {
                SkeletonPoint tSkeleton = new SkeletonPoint();
                tSkeleton.X = tVector.X; tSkeleton.Y = tVector.Y; tSkeleton.Z = tVector.Z;
                return tSkeleton;
            }
            public Vector4 JointToVector(Joint tjoint)
            {
                Vector4 tVector = new Vector4();
                tVector.X = tjoint.Position.X; tVector.Y = tjoint.Position.Y; tVector.Z = tjoint.Position.Z;
                return tVector;
            }
        #endregion

        #region "Form Control handlers"
            /// <summary>
            /// Handles the checking or unchecking of the seated mode combo box
            /// </summary>
            /// <param name="sender">object sending the event</param>
            /// <param name="e">event arguments</param>
            private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
            {
                if (null != this.sensor)
                {
                    if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                        {myKinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;}
                    else
                        {myKinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;}
                }
            }
            private void checkBoxOneHandedChanged(object sender, RoutedEventArgs e)
            {
                System.Windows.Controls.CheckBox tCheckBox = (System.Windows.Controls.CheckBox)sender;
                if (tCheckBox.IsChecked == true)
                    {OneHandedMode = true;}
                else
                    {OneHandedMode = false;}
            }
            private void CalibrationMode_Click(object sender, RoutedEventArgs e)
            {
                clock = 0;
                OpMode = 1;
            }
            private void Capture_Click(object sender, RoutedEventArgs e)
            {
                ReCapture = true;
            }        
        #endregion
    }
}