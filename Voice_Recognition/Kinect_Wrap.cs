﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.AudioFormat;
using System.IO;

namespace Microsoft.Samples.Kinect.SpeechBasics
{
    class Kinect_Wrap
    {
        KinectSensor myKinect = null;
        EventHandler<SkeletonFrameReadyEventArgs> skeletonFrameEvent = null;
        SpeechRecognitionEngine mySpeechEngine = null;
        EventHandler<SpeechRecognizedEventArgs> recognizedEvent = null;
        EventHandler<SpeechRecognitionRejectedEventArgs> rejectedEvent = null;
        public Kinect_Wrap()
        {}
        public KinectSensor StartKinect(EventHandler<SkeletonFrameReadyEventArgs> skeletonFrame)
        {
            skeletonFrameEvent = skeletonFrame;
            KinectSensor candidate = null;
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)     // Look through all sensors and start the first connected one.
            {                                                               // This requires that a Kinect is connected at the time of app startup.
                if (potentialSensor.Status == KinectStatus.Connected)       // To make your app robust against plug/unplug, 
                {                                                           // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
                    candidate = potentialSensor;
                    break;
                }
            }

            if (null != candidate)                                        //If there is a sensor
            {
                this.myKinect = candidate;
                // Turn on the skeleton stream to receive skeleton frames
                this.myKinect.SkeletonStream.Enable();
                // Add an event handler to be called whenever there is new color frame data
                this.myKinect.SkeletonFrameReady += skeletonFrameEvent;
                //Attempt to set depth range...
                this.myKinect.DepthStream.Range = DepthRange.Near;
                this.myKinect.SkeletonStream.EnableTrackingInNearRange = true;
                this.myKinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                // Start the sensor!
                try
                {
                    this.myKinect.Start();                                    // Start the sensor!
                }
                catch (IOException)
                {
                    this.myKinect = null;                                     //Error!
                }
            }
            return this.myKinect;
        }
        public void StopKinect()
        {
            if (null != myKinect)
            {                
                this.myKinect.AudioSource.Stop();
                this.myKinect.Stop();
                this.myKinect.SkeletonFrameReady -= skeletonFrameEvent;
            }
        }
        public SpeechRecognitionEngine StartSpeech(EventHandler<SpeechRecognizedEventArgs> recognized, EventHandler<SpeechRecognitionRejectedEventArgs> rejected)
        {
            recognizedEvent = recognized;
            rejectedEvent = rejected;
            if (myKinect == null)
            {
                throw new Exception("Illegal State: Kinnect Not Started");
            }
            RecognizerInfo ri = GetKinectRecognizer();
            
            if (null != ri)
            {                
                mySpeechEngine = new SpeechRecognitionEngine(ri.Id);

                // Create a grammar from grammar definition XML file.
                using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Properties.Resources.SpeechGrammar)))
                {
                    var g = new Grammar(memoryStream);
                    mySpeechEngine.LoadGrammar(g);
                }

                mySpeechEngine.SpeechRecognized += recognizedEvent;
                mySpeechEngine.SpeechRecognitionRejected += rejectedEvent;

                // For long recognition sessions (a few hours or more), it may be beneficial to turn off adaptation of the acoustic model. 
                // This will prevent recognition accuracy from degrading over time.
                ////speechEngine.UpdateRecognizerSetting("AdaptationOn", 0);

                mySpeechEngine.SetInputToAudioStream(
                    myKinect.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                mySpeechEngine.RecognizeAsync(RecognizeMode.Multiple);
            }
            return mySpeechEngine;

        }
        public void StopSpeech()
        {
            if (null != mySpeechEngine)
            {
                mySpeechEngine.SpeechRecognized -= recognizedEvent;
                mySpeechEngine.SpeechRecognitionRejected -= rejectedEvent;
                mySpeechEngine.RecognizeAsyncStop();
            }
        }

        // Gets the metadata for the speech recognizer (acoustic model) most suitable to
        // process audio from Kinect device.
        private RecognizerInfo GetKinectRecognizer()
        {
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return recognizer;
                }
            }

            return null;
        }
        #region KinectDrawing
        
        #endregion
    }
}
