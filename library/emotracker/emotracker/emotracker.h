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

//namespace emotracker
//{
	class EmotionsConfiguration {
	public:
		pxcCHAR *calibFilename;
		pxcCHAR *streamFilename;
		pxcCHAR *emoFilename;

	};
	class EmotionsData {
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
		pxcI32 Yaw;
		pxcI32 Pitch;
		pxcI32 Roll;
		PXCFaceData::LandmarkPoint *LandmarkPoints;

		PXCPoint3DF32 faceDirection;
		PXCFaceData::GazePoint gaze;
		pxcI32 Happiness;
		pxcI32 Newtral;
		pxcI32 Sadness;
		pxcI32 Surprise;
		pxcI32 Fear;
		pxcI32 Anger;
		pxcI32 Disgust;
		pxcI32 Contempt;

		pxcI32 pulse;
	};
	class EmotionsTracker {
	private:
		PXCSenseManager* m_sm;
		PXCFaceData* m_face;
		PXCPersonTrackingData* m_person;
		EmotionsConfiguration* m_conf;
		CStdioFile* efile;
		clock_t starttime;
		double prevtime;
		int framenum;
		BOOL stop_thread;
		LPDWORD lpThreadId;
		std::map<PXCFaceData::ExpressionsData::FaceExpression, std::wstring> m_expressionMap;

		std::map<PXCFaceData::ExpressionsData::FaceExpression, std::wstring> InitExpressionsMap();
		pxcStatus WriteEmo();
	public:
		EmotionsTracker();
		EmotionsTracker(PXCSenseManager *m_sm);
		pxcStatus Init();
		pxcStatus ProcessFrame();
		pxcStatus ProcessSample(PXCCapture::Sample *sample);
		pxcStatus SetOutputURL(pxcCHAR *url);
		EmotionsConfiguration *QueryConfiguration();
		EmotionsData *QueryOutput();
		pxcStatus Start();
		pxcStatus Stop();
		pxcStatus Release();
		void Process();




	};
	class EmotionsHandler : public PXCSenseManager::Handler
	{
		EmotionsTracker *m_etr;
	public:
		EmotionsHandler(EmotionsTracker *etr);
		virtual pxcStatus PXCAPI OnNewSample(pxcUID, PXCCapture::Sample *sample);
	};
//}