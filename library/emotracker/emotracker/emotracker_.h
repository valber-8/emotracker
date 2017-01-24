#pragma once
#include "emotracker.h"

#ifdef __cplusplus
extern "C" {
#endif

	extern EMOTRACKER_API EmotionsConfiguration* CreateEmotionsConfiguration();
	extern EMOTRACKER_API void DisposeEmotionsConfiguration(EmotionsConfiguration* a_pObject);
	extern EMOTRACKER_API void EmotionsConfiguration_setStreamFilename(EmotionsConfiguration* a_pObject, pxcCHAR *streamFilename);
	extern EMOTRACKER_API void EmotionsConfiguration_setCalibrationFilename(EmotionsConfiguration* a_pObject, pxcCHAR *calibFilename);
	extern EMOTRACKER_API void EmotionsConfiguration_setEmotionsFilename(EmotionsConfiguration* a_pObject, pxcCHAR *emoFilename);

	extern EMOTRACKER_API void EmotionsConfiguration_setPersonTracking(EmotionsConfiguration* a_pObject, pxcBool personTracking);
	extern EMOTRACKER_API void EmotionsConfiguration_setAddGazePoint(EmotionsConfiguration* a_pObject, pxcBool addGazePoint);
	extern EMOTRACKER_API void EmotionsConfiguration_setRecordingLandmark(EmotionsConfiguration* a_pObject, pxcBool recordingLandmark);
	extern EMOTRACKER_API void EmotionsConfiguration_setRecordingGaze(EmotionsConfiguration* a_pObject, pxcBool recordingGaze);
	extern EMOTRACKER_API void EmotionsConfiguration_setUsePersonTrackingModuleEmotions(EmotionsConfiguration* a_pObject, pxcBool usePersonTrackingModuleEmotions);
	
	extern EMOTRACKER_API EmotionsData* CreateEmotionsData();
	extern EMOTRACKER_API void DisposeEmotionsData(EmotionsData* a_pObject);

	extern EMOTRACKER_API EmotionsTracker*  CreateEmotionsTracker();
	extern EMOTRACKER_API EmotionsTracker*  CreateEmotionsTrackerSM(PXCSenseManager *m_sm);
	extern EMOTRACKER_API void DisposeEmotionsTracker(EmotionsTracker* a_pObject);
	extern EMOTRACKER_API pxcStatus EmotionsTracker_Init(EmotionsTracker* a_pObject);
	extern EMOTRACKER_API pxcStatus EmotionsTracker_ProcessFrame(EmotionsTracker* a_pObject);
	extern EMOTRACKER_API pxcStatus EmotionsTracker_ProcessSample(EmotionsTracker* a_pObject, PXCCapture::Sample *sample);
	extern EMOTRACKER_API pxcStatus EmotionsTracker_SetOutputURL(EmotionsTracker* a_pObject, pxcCHAR *url);
	extern EMOTRACKER_API EmotionsConfiguration* EmotionsTracker_QueryConfiguration(EmotionsTracker* a_pObject);
	extern EMOTRACKER_API EmotionsData* EmotionsTracker_QueryOutput(EmotionsTracker* a_pObject);
	extern EMOTRACKER_API pxcStatus EmotionsTracker_Start(EmotionsTracker* a_pObject);
	extern EMOTRACKER_API pxcStatus EmotionsTracker_Stop(EmotionsTracker* a_pObject);
	extern EMOTRACKER_API pxcStatus EmotionsTracker_Release(EmotionsTracker* a_pObject);
	extern EMOTRACKER_API void EmotionsTracker_Process(EmotionsTracker* a_pObject);


	extern EMOTRACKER_API EmotionsHandler* CreateEmotionsHandler(EmotionsTracker *etr);
	extern EMOTRACKER_API void DisposeEmotionsHandler(EmotionsHandler* a_pObject);
	extern EMOTRACKER_API pxcStatus PXCAPI EmotionsHandler_OnNewSample(EmotionsHandler* a_pObject, pxcUID, PXCCapture::Sample *sample);
	

#ifdef __cplusplus
}
#endif