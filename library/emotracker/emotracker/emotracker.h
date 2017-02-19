#pragma once

#ifdef EMOTRACKER_EXPORTS
#define EMOTRACKER_API __declspec(dllexport) 
#else
#define EMOTRACKER_API __declspec(dllimport) 
#endif

#define _CRT_SECURE_NO_WARNINGS

#define _AFXDLL

#include <afxwin.h>

#include "stdafx.h"
#include <assert.h>
#include <string>
#include <map>
#include "pxcfaceconfiguration.h"
#include "PXCPersonTrackingData.h"
#include "pxcpersontrackingconfiguration.h"
#include "pxcfacemodule.h"
#include "pxcsensemanager.h"
#include "pxccapture.h"
#include <time.h>

/**
<summary>
	Class for providing iterface to configure Emotions Tracker behaviors
</summary>
*/

	class EMOTRACKER_API EmotionsConfiguration {
	public:
		pxcCHAR *calibFilename = L"calib.bin";
		pxcCHAR *streamFilename = L"1.rssdk";
		pxcCHAR *emoFilename = L"1.ttml";
		pxcBool personTracking = true;
		pxcBool addGazePoint = true;
		pxcBool recordingLandmark = true;
		pxcBool recordingGaze = true;
		pxcBool playbackMode = false;
		pxcBool usePersonTrackingModuleEmotions = false;
		EmotionsConfiguration();
		/**
		<param name="streamFilename">Provides ability to setup the output/playback stream filename in rssdk format. By default is NULL - output stream is not stored</param>
		*/
		void setStreamFilename(pxcCHAR *streamFilename);
		/**
		<param name="calibFilename">Provides ability to setup the input filename with gaze calibration data in rssdk format. By default used <b>calib.bin</b></param>
		*/
		void setCalibrationFilename(pxcCHAR *calibFilename);
		/**
		<param name="emoFilename">Provides ability to setup the output filename with collected emotions data in ttml format. By default used <b>1.ttml</b></param>
		*/
		void setEmotionsFilename(pxcCHAR *emoFilename);

		/**
		<param name="personTracking">Provides ability to collect person tracking data. By default is <b>true</b></param>
		*/
		void setPersonTracking(pxcBool personTracking);

		/**
		<param name="addGazePoint">Provides ability to write gaze point to the subtitles in output ttml file. By default is <b>true</b></param>
		*/
		void setAddGazePoint(pxcBool addGazePoint);

		/**
		<param name="recordingLandmark">Provides ability to collect landmark data. By default is <b>true</b></param>
		*/
		void setRecordingLandmark(pxcBool recordingLandmark);

		/**
		<param name="recordingGaze">Provides ability to collect gaze data. By default is <b>true</b></param>
		*/
		void setRecordingGaze(pxcBool recordingGaze);

		/**
		<param name="usePersonTrackingModuleEmotions">Provides ability to make decision about emotions based on person tracking data. By default is <b>false</b></param>
		*/
		void setUsePersonTrackingModuleEmotions(pxcBool usePersonTrackingModuleEmotions);

		/**
		<param name="playbackMode">Set playback mode, if true it playback streamFilename. By default is <b>false</b></param>
		*/
		void setPlaybackMode(pxcBool playbackMode);

	};


	/**
	<summary>
	Class for providing iterface to collected emotions data
	</summary>
	*/

	class EMOTRACKER_API EmotionsData {
	public:
		pxcI64 timestamp;
		pxcI32 ID;

		pxcI32 Brow_Raised_Left;
		pxcI32 Brow_Raised_Right;
		pxcI32 Brow_Lowered_Left;
		pxcI32 Brow_Lowered_Right;
		pxcI32 Smile;
		pxcI32 Kiss;
		pxcI32 Mouth_Open;
		pxcI32 Closed_Eye_Left;
		pxcI32 Closed_Eye_Right;
		pxcI32 Eyes_Turn_Left;
		pxcI32 Eyes_Turn_Right;
		pxcI32 Eyes_Up;
		pxcI32 Eyes_Down;
		pxcI32 Tongue_Out;
		pxcI32 Puff_Right_Cheek;
		pxcI32 Puff_Left_Cheek;
		pxcF32 Yaw;
		pxcF32 Pitch;
		pxcF32 Roll;
		PXCFaceData::LandmarkPoint *LandmarkPoints;

		PXCPoint3DF32 faceDirection;
		PXCFaceData::GazePoint gaze;
		pxcI32 Happiness;
		pxcI32 Neutral;
		pxcI32 Sadness;
		pxcI32 Surprise;
		pxcI32 Fear;
		pxcI32 Anger;
		pxcI32 Disgust;
		pxcI32 Contempt;

		pxcI32 pulse;
	}; 



	/**
	<summary>
	Class for providing the driver to process data from camera
	</summary>
	*/
	class EMOTRACKER_API  EmotionsTracker {
	private:
		PXCSenseManager* m_sm;
		PXCSenseManager* m_sm2;
		PXCFaceData* m_face;
		PXCPersonTrackingData* m_person;
		EmotionsConfiguration* m_conf;
		EmotionsData* m_data;
		pxcStatus* m_st;
		CStdioFile* efile;
		clock_t starttime;
		double prevtime;
		int framenum;
		volatile BOOL stop_thread;
		LPDWORD lpThreadId;
		HANDLE thread;
		int horizontal=1366, vertical = 768;
		std::map<PXCFaceData::ExpressionsData::FaceExpression, std::wstring> m_expressionMap;

		std::map<PXCFaceData::ExpressionsData::FaceExpression, std::wstring> InitExpressionsMap();
		pxcStatus WriteEmo();
	public:
		/**
		<summary>
		Constructor
		</summary>
		*/
		EmotionsTracker();

		/**
		<summary>
		Destructor
		</summary>
		*/
		~EmotionsTracker();

		/**
		<summary>
		Constructor with 
		</summary>
		<param name="m_sm">Provides ability to setup initial sense manager</param>		
		*/
		EmotionsTracker(PXCSenseManager *m_sm);

		/**
		<summary>
		Initialize driver
		</summary>
		*/
		pxcStatus Init();

		/**
		<summary>
		Process captured frame
		</summary>
		*/
		pxcStatus ProcessFrame();

		/**
		<summary>
		Process frame sample
		</summary>
		<param name="sample">Captured sample</param>
		*/
		pxcStatus ProcessSample(PXCCapture::Sample *sample);

		/**
		<summary>
		Set URL to write ttml output 
		</summary>
		*/
		pxcStatus SetOutputURL(pxcCHAR *url);

		/**
		<summary>
		Get emotions configuration class instance
		</summary>
		*/
		EmotionsConfiguration* QueryConfiguration();


		/**
		<summary>
		Get curently collected emotions data
		</summary>
		*/
		EmotionsData *QueryOutput();


		/**
		<summary>
		Start processing camera stream
		</summary>
		*/
		pxcStatus Start();


		/**
		<summary>
		Stop processing camera stream
		</summary>
		*/
		pxcStatus Stop();

		/**
		<summary>
		Release used memory
		</summary>
		*/
		pxcStatus Release();

		/**
		<summary>
		Process camera stream
		</summary>
		*/
		void Process();

		/**
		<summary>
		Return current status
		</summary>
		*/
		pxcStatus getStatus();
	};



	/**
	<summary>
	Class for providing the camera event handler
	</summary>
	*/
	class EMOTRACKER_API EmotionsHandler : public PXCSenseManager::Handler
	{
		EmotionsTracker *m_etr;
	public:
		EmotionsHandler(EmotionsTracker *etr);
		virtual pxcStatus PXCAPI OnNewSample(pxcUID, PXCCapture::Sample *sample);
	};