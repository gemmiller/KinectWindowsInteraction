//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.SpeechBasics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Deployment;
    using System.Windows.Threading;
    //using System.IO;
    using System.Windows;
    using System.Windows.Documents;//
    using System.Windows.Media;
    using Microsoft.Kinect;
    using Microsoft.Speech.AudioFormat;//
    using Microsoft.Speech.Recognition;//
    using Microsoft.Win32;//
    using System.Windows.Forms;//

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable",
        Justification = "In a full-fledged application, the SpeechRecognitionEngine object should be properly disposed. For the sake of simplicity, we're omitting that code in this sample.")]
    public partial class MainWindow : Window
    {
        #region "Dimensions and Declarations"
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;
        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;
        #region "Variables"
        /// <summary>
        /// Resource key for medium-gray-colored brush.
        /// </summary>
        private const string MediumGreyBrushKey = "MediumGreyBrush";
        private Kinect_Wrap kinectWrapper;
        private Kinect_Drawing kinectDrawing;
        private KinectSensor sensor;                    // Active Kinect sensor.
        private SpeechRecognitionEngine speechEngine;   // Speech recognition engine using audio data from Kinect.
        private List<Span> recognitionSpans;            // List of all UI span elements used to select recognized text.
        private DrawingGroup drawingGroup;              // Drawing group for skeleton rendering output
        private DrawingImage imageSource;               // Drawing image that we will display

        private bool typing = false;                    // Signal to allow free typing characters.
        public int heartbeat = 0;                       //Application Heartbeat
        int closestID = 0;                              //Determine Closest person
        #endregion 
        #endregion
        public MainWindow()         // Initializes a new instance of the MainWindow class.
        {
            InitializeComponent();
        }
        private void WindowLoaded(object sender, RoutedEventArgs e) // Execute initialization tasks.
        {            
            this.drawingGroup = new DrawingGroup();                         // Create the drawing group we'll use for drawing
            this.imageSource = new DrawingImage(this.drawingGroup);         // Create an image source that we can use in our image control
            Image.Source = this.imageSource;                                // Display the drawing using our image control
            kinectWrapper = new Kinect_Wrap();                                      //KINECT_WRAPPER  
            sensor = kinectWrapper.StartKinect(SensorSkeletonFrameReady);           //KINECT_WRAPPER
            kinectDrawing = new Kinect_Drawing(sensor, RenderWidth, RenderHeight);  //KINECT_DRAWING
            if (null == this.sensor)
                this.statusBarText.Text = Properties.Resources.NoKinectReady;  //Throw Flag
            else
            {
                recognitionSpans = new List<Span> { };
                speechEngine = kinectWrapper.StartSpeech(SpeechRecognized, SpeechRejected); //KINECT_WRAPPER
                if (null == speechEngine)
                    this.statusBarText.Text = Properties.Resources.NoSpeechRecognizer;
            }
        }
        private void WindowClosing(object sender, CancelEventArgs e)    // Execute uninitialization tasks.
        {
            kinectWrapper.StopKinect(); //KINECT_WRAPPER
            kinectWrapper.StopSpeech();
        }

        #region "Speech Recognition"

        /// <summary>
        /// Remove any highlighting from recognition instructions.
        /// </summary>
        private void ClearRecognitionHighlights()
        {
            foreach (Span span in recognitionSpans)
            {
                span.Foreground = (Brush)this.Resources[MediumGreyBrushKey];
                span.FontWeight = FontWeights.Normal;
            }
        }
        private void SpeechRecognized(object sender, SpeechRecognizedEventArgs e)       // Handler for recognized speech events.
        {
            // Speech utterance confidence below which we treat speech as if it hadn't been heard
            const double ConfidenceThreshold = 0.6;

            //ClearRecognitionHighlights();

            if (!typing)
            {
                if (e.Result.Confidence >= ConfidenceThreshold)
                {

                    switch (e.Result.Semantics.Value.ToString())
                    {
                        case "INTERNET":
                            System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Internet Explorer\iexplore.exe", "www.microsoft.com");
                            break;

                        case "CHROME":
                            System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", "www.microsoft.net");
                            break;

                        case "FACEBOOK":
                            System.Diagnostics.Process.Start(GetSystemDefaultBrowser(), "www.facebook.com");
                            break;

                        case "IASTATE":
                            System.Diagnostics.Process.Start(GetSystemDefaultBrowser(), "www.iastate.edu");
                            break;

                        case "GOOGLE":
                            System.Diagnostics.Process.Start(GetSystemDefaultBrowser(), "www.google.com");
                            break;

                        case "CMD":
                            System.Diagnostics.Process.Start(@"C:\Windows\System32\cmd.exe", "/K cd C:\\");
                            break;

                        case "NOTEPAD":
                            System.Diagnostics.Process.Start(@"C:\Windows\notepad.exe");
                            break;

                        case "DOT":
                            WinAPIWrapper.WinAPI.ManagedSendKeys(".");
                            break;

                        case "LEFTCLICK":
                            WinAPIWrapper.WinAPI.MouseClick("left");
                            break;

                        case "RIGHTCLICK":
                            WinAPIWrapper.WinAPI.MouseClick("right");
                            break;

                        case "DOUBLECLICK":
                            WinAPIWrapper.WinAPI.MouseClick("left");
                            WinAPIWrapper.WinAPI.MouseClick("left");
                            break;

                        case "GRAB":
                            WinAPIWrapper.WinAPI.MouseStartDrag();
                            break;
                        case "TypeMicrosoft":
                            WinAPIWrapper.WinAPI.ManagedSendKeys("Microsoft{ENTER}");
                            break;
                        case "TypeWeather":
                            WinAPIWrapper.WinAPI.ManagedSendKeys("Ames Weather{ENTER}");
                            break;
                        case "TypeEnter":
                            WinAPIWrapper.WinAPI.ManagedSendKeys("{ENTER}");
                            break;
                        case "RELEASE":
                            WinAPIWrapper.WinAPI.MouseStopDrag();
                            break;
                        case "STARTTYPE":
                            typing = true;
                            break;
                    }
                }
            }
            else
            {

                if (e.Result.Confidence >= ConfidenceThreshold)
                {

                    switch (e.Result.Semantics.Value.ToString())
                    {
                        case "PHRASE":
                            System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Internet Explorer\iexplore.exe", "www.microsoft.com");
                            break;

                        case "ENDTYPE":
                            typing = false;
                            break;
                    }
                }
            }
        }
        internal string GetSystemDefaultBrowser()
        {
            string name = string.Empty;
            RegistryKey regKey = null;

            try
            {
                //set the registry key we want to open
                regKey = Registry.ClassesRoot.OpenSubKey("HTTP\\shell\\open\\command", false);

                //get rid of the enclosing quotes
                name = regKey.GetValue(null).ToString().ToLower().Replace("" + (char)34, "");

                //check to see if the value ends with .exe (this way we can remove any command line arguments)
                if (!name.EndsWith("exe"))
                    //get rid of all command line arguments (anything after the .exe must go)
                    name = name.Substring(0, name.LastIndexOf(".exe") + 4);

            }
            catch (Exception ex)
            {
                name = string.Format("ERROR: An exception of type: {0} occurred in method: {1} in the following module: {2}", ex.GetType(), ex.TargetSite, this.GetType());
            }
            finally
            {
                //check and see if the key is still open, if so
                //then close it
                if (regKey != null)
                    regKey.Close();
            }
            //return the value
            return name;

        }
        /// <summary>
        /// Handler for rejected speech events.
        /// </summary>
        /// <param name="sender">object sending the event.</param>
        /// <param name="e">event arguments.</param>
        private void SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            //ClearRecognitionHighlights();
            WinAPIWrapper.WinAPI.ManagedSendKeys(e.Result.Text);
        }
        #endregion
        #region "Skeleton"
        int identified = 0;
        List<Joint> LeftHand = new List<Joint>();
        List<Joint> LeftElbow = new List<Joint>();
        List<Joint> LeftShoulder = new List<Joint>();
        List<Joint> RightHand = new List<Joint>();
        List<Joint> RightElbow = new List<Joint>();
        List<Joint> RightShoulder = new List<Joint>();
        List<Joint> CenterShoulder = new List<Joint>();
        double LReach = 0;
        double LTravel = 0;
        double RReach = 0;
        double RTravel = 0;
        double OffsetX; 
        double OffsetY;
        int RPositionX;
        int RPositionY;
        #region "Skeleton Data Collection Smoothing"
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

                #region "Get Skeleton Data and Display"
                if (skeletons.Length != 0)
                {
                    //Has the skeleton Disapeared
                    identified -= 1;
                    foreach (Skeleton skel in skeletons)
                    {
                        txtTracking.Text = skel.TrackingId.ToString();
                        if (skel.TrackingId == closestID && skel.TrackingId != 0)
                            identified += 1;
                    }
                    //Grab the closest person
                    txtIdentified.Text = identified.ToString();
                    if (identified < -30)
                    {
                        LeftHand.Clear();
                        RightHand.Clear();
                        LeftElbow.Clear();
                        RightElbow.Clear();
                        LeftShoulder.Clear();
                        RightShoulder.Clear();
                        RReach = 0;
                        identified = 0;
                        if (sensor.SkeletonStream.AppChoosesSkeletons == false)                   // Ensure AppChoosesSkeletons is set
                            sensor.SkeletonStream.AppChoosesSkeletons = true;
                        float closestDistance = 10000f;                                             // Start with a far enough distance
                        closestID = 0;
                        foreach (Skeleton skeleton in skeletons.Where(s => s.TrackingState != SkeletonTrackingState.NotTracked))
                        {
                            if (skeleton.Position.Z < closestDistance)
                            {
                                closestID = skeleton.TrackingId;
                                closestDistance = skeleton.Position.Z;
                            }
                        }
                        if (closestID > 0)
                            sensor.SkeletonStream.ChooseSkeletons(closestID);                     // Track this skeleton
                    }
                    foreach (Skeleton skel in skeletons)
                    {
                        if (skel.TrackingId != 0)                                                   //Write Skeleton ID
                            skelId.Text = skel.TrackingId.ToString();
                        kinectDrawing.DrawSkeleton(skel,this.drawingGroup); //KINECT_DRAWING
                        AddJoints(LeftHand, skel.Joints, JointType.HandLeft);
                        AddJoints(LeftElbow, skel.Joints, JointType.ElbowLeft);
                        AddJoints(LeftShoulder, skel.Joints, JointType.ShoulderLeft);
                        AddJoints(RightHand, skel.Joints, JointType.HandRight);
                        AddJoints(RightElbow, skel.Joints, JointType.ElbowRight);
                        AddJoints(RightShoulder, skel.Joints, JointType.ShoulderRight);
                        AddJoints(CenterShoulder, skel.Joints, JointType.ShoulderCenter);
                        if (LeftElbow.Count != 0 && LeftHand.Count != 0)
                        {
                            LReach = Math.Sqrt(Math.Pow(LeftHand.Last().Position.X - LeftElbow.Last().Position.X, 2) + Math.Pow(LeftHand.Last().Position.Y - LeftElbow.Last().Position.Y, 2));
                            LTravel = Math.Sqrt(Math.Pow(LeftHand.Last().Position.X - LeftHand.First().Position.X, 2) + Math.Pow(LeftHand.Last().Position.Y - LeftHand.First().Position.Y, 2));
                            //txtLeftHand.Text = LeftHand.Last().Position.X.ToString() + " , " + LeftHand.Last().Position.Y.ToString();
                        }
                        if (RightElbow.Count != 0 && RightHand.Count != 0 && RightShoulder.Count != 0)
                        {
                            if (RReach == 0)
                            {
                                RReach = Math.Sqrt(Math.Pow(RightHand.Last().Position.X - RightElbow.Last().Position.X, 2) + Math.Pow(RightHand.Last().Position.Y - RightElbow.Last().Position.Y, 2));
                                RTravel = Math.Sqrt(Math.Pow(RightHand.Last().Position.X - RightHand.First().Position.X, 2) + Math.Pow(RightHand.Last().Position.Y - RightHand.First().Position.Y, 2));
                                OffsetX = RightShoulder.Last().Position.X - CenterShoulder.Last().Position.X;
                                OffsetY = RightShoulder.Last().Position.Y - CenterShoulder.Last().Position.Y;
                            }
                            RPositionX = (int)Math.Max(Math.Min((RightHand.Last().Position.X - (CenterShoulder.Last().Position.X + OffsetX)) / ((4.0 / 3.0) * RReach) * System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width), 0);
                            RPositionY = (int)Math.Max(Math.Min(-((RightHand.Last().Position.Y - (CenterShoulder.Last().Position.Y + OffsetY)) - RReach) / ((8.0 / 5.0) * RReach) * System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height), 0);
                            if (RightHand.Last().Position.Z < (RightShoulder.Last().Position.Z - ((1.0 / 2.0) * RReach)))
                                WinAPIWrapper.WinAPI.MouseMove(RPositionX, RPositionY);
                            txtPositionX.Text = RPositionX.ToString();
                            txtPositionY.Text = RPositionY.ToString();
                            //txtRightHand.Text = RightHand.Last().Position.X.ToString() + " , " + RightHand.Last().Position.Y.ToString();
                        }
                        txtLReach.Text = LReach.ToString();
                        txtLTravel.Text = LTravel.ToString();
                        txtRReach.Text = RReach.ToString();
                        txtRTravel.Text = RTravel.ToString();                    
                }
                #endregion
                heartbeat += 1;
                lblClock.Text = "Clock: " + heartbeat.ToString();
                if (heartbeat > 1000)
                {
                    heartbeat = 0;
                }
            }
        }
        private void AddJoints(List<Joint> JointData, JointCollection JointRaw, JointType JointDef)
        {
            IEnumerable<Joint> Results = JointRaw.Where(s => s.JointType == JointDef).Where(s => s.TrackingState == JointTrackingState.Tracked);
            if (Results.Count() > 0)
                JointData.Add(Results.First());
            while (JointData.Count > 10)
            {
                JointData.Remove(JointData.First());
            }
        }
        #endregion
        #endregion
    }
}