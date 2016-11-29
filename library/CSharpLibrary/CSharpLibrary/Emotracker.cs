﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CSharpLibrary
{
        
    public class Emotracker
    {
        internal const String DLLNAME = "emotracker";
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public class  EmotionsConfiguration {
		    String calibFilename;
            String streamFilename;
            String emoFilename;
            IntPtr pClassName;

            [DllImport(DLLNAME, CharSet = CharSet.Unicode)]
            internal static extern IntPtr CreateEmotionsConfiguration();
            public EmotionsConfiguration()
            {
                pClassName = CreateEmotionsConfiguration();
            }
            
            internal EmotionsConfiguration(IntPtr pClassName)
            {
                this.pClassName = pClassName;
            }

            [DllImport(DLLNAME, CharSet = CharSet.Unicode)]
            internal static extern void DisposeEmotionsConfiguration(IntPtr pClassName);
            ~EmotionsConfiguration()
            {
                DisposeEmotionsConfiguration(pClassName);
            }

            [DllImport(DLLNAME, CharSet = CharSet.Unicode)]
            internal static extern void EmotionsConfiguration_setStreamFilename(IntPtr pClassName, String streamFilename);
            public void setStreamFilename(String streamFilename) {
                EmotionsConfiguration_setStreamFilename(pClassName, streamFilename);
                this.streamFilename = streamFilename;
            }
            [DllImport(DLLNAME, CharSet = CharSet.Unicode)]
            internal static extern void EmotionsConfiguration_setCalibrationFilename(IntPtr pClassName, String calibFilename);
            public void setCalibrationFilename(String calibFilename) {
                EmotionsConfiguration_setCalibrationFilename(pClassName, calibFilename);
                this.calibFilename = calibFilename;
            }
            [DllImport(DLLNAME, CharSet = CharSet.Unicode)]
            internal static extern void EmotionsConfiguration_setEmotionsFilename(IntPtr pClassName, String emoFilename);
            public void setEmotionsFilename(String emoFilename) {
                EmotionsConfiguration_setEmotionsFilename(pClassName, emoFilename);
                this.emoFilename = emoFilename;
            }

        }


        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public class  EmotionsTracker {

            IntPtr pClassName;
            [DllImport(DLLNAME, CharSet = CharSet.Unicode)]
            internal static extern IntPtr CreateEmotionsTracker();
            public EmotionsTracker()
            {
                pClassName = CreateEmotionsTracker();
            }
            [DllImport(DLLNAME, CharSet = CharSet.Unicode)]
            internal static extern IntPtr CreateEmotionsTrackerSM(PXCMSenseManager m_sm);
            public EmotionsTracker(PXCMSenseManager m_sm)
            {
                pClassName = CreateEmotionsTrackerSM(m_sm);
            }
            
            [DllImport(DLLNAME, CharSet = CharSet.Unicode)]
            internal static extern void DisposeEmotionsTracker(IntPtr pClassName);
            ~EmotionsTracker()
            {
                DisposeEmotionsTracker(pClassName);
            }


            [DllImport(DLLNAME)]
            internal static extern pxcmStatus EmotionsTracker_Init(IntPtr pClassName);
            public pxcmStatus Init() {
                return EmotionsTracker_Init(pClassName);
            }
            [DllImport(DLLNAME)]
            internal static extern pxcmStatus EmotionsTracker_ProcessFrame(IntPtr pClassName);
            public pxcmStatus ProcessFrame() {
                return EmotionsTracker_ProcessFrame(pClassName);
            }

            [DllImport(DLLNAME)]
            internal static extern pxcmStatus EmotionsTracker_ProcessSample(IntPtr pClassName, PXCMCapture.Sample sample);
            public pxcmStatus ProcessSample(PXCMCapture.Sample sample) {
                return EmotionsTracker_ProcessSample(pClassName, sample);
            }

            [DllImport(DLLNAME, CharSet = CharSet.Unicode)]
            internal static extern pxcmStatus EmotionsTracker_SetOutputURL(IntPtr pClassName, String url);
            public pxcmStatus SetOutputURL(String url) {
                return EmotionsTracker_SetOutputURL(pClassName, url);
            }


            [DllImport(DLLNAME)]
            //[DllImport(DLLNAME, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, EntryPoint = "?QueryConfiguration@EmotionsTracker@@QEAAPEAVEmotionsConfiguration@@XZ")]
            internal static extern IntPtr EmotionsTracker_QueryConfiguration(IntPtr pClassName);
            public EmotionsConfiguration QueryConfiguration() {
                IntPtr conf=EmotionsTracker_QueryConfiguration(pClassName);
                if (conf == IntPtr.Zero) return null;
                return new EmotionsConfiguration(conf);
            }

            [DllImport(DLLNAME)]
            internal static extern IntPtr EmotionsTracker_QueryOutput(IntPtr pClassName);
            public EmotionsData QueryOutput() {
                IntPtr output= EmotionsTracker_QueryOutput(pClassName);
                if (output == IntPtr.Zero) return null;
                return new EmotionsData(output);
            }

            [DllImport(DLLNAME)]
            internal static extern pxcmStatus EmotionsTracker_Start(IntPtr pClassName);
            public pxcmStatus Start() {
                return EmotionsTracker_Start(pClassName);
            }

            [DllImport(DLLNAME)]
            internal static extern pxcmStatus EmotionsTracker_Stop(IntPtr pClassName);
            public pxcmStatus Stop() {
                return EmotionsTracker_Stop(pClassName);
            }

            [DllImport(DLLNAME)]
            internal static extern pxcmStatus EmotionsTracker_Release(IntPtr pClassName);
            public pxcmStatus Release() {
                return EmotionsTracker_Release(pClassName);
            }

            [DllImport(DLLNAME)]
            internal static extern void EmotionsTracker_Process(IntPtr pClassName);
            public void Process() {
                EmotionsTracker_Process(pClassName);
            }
        };

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public class EmotionsData {
            Int64 timestamp;
            Int32 ID;

            Int32 Brow_Raised_Left;
            Int32 Brow_Raised_Right;
            Int32 Brow_Lowered_Left;
            Int32 Brow_Lowered_Right;
            Int32 Smile;
            Int32 Kiss;
            Int32 Mouth_Open;
            Int32 Closed_Eye_Left;
            Int32 Closed_Eye_Right;
            Int32 Eyes_Turn_Left;
            Int32 Eyes_Turn_Right;
            Int32 Eyes_Up;
            Int32 Eyes_Down;
            Int32 Tongue_Out;
            Int32 Puff_Right_Cheek;
            Int32 Puff_Left_Cheek;
            Int32 Yaw;
            Int32 Pitch;
            Int32 Roll;
        PXCMFaceData.LandmarkPoint LandmarkPoints;

        PXCMPoint3DF32 faceDirection;
        PXCMFaceData.GazePoint gaze;
            Int32 Happiness;
            Int32 Newtral;
            Int32 Sadness;
            Int32 Surprise;
            Int32 Fear;
            Int32 Anger;
            Int32 Disgust;
            Int32 Contempt;

            Int32 pulse;


            IntPtr pClassName;
            internal EmotionsData(IntPtr pClassName) {
                this.pClassName = pClassName;
            } 
        };

}
}
