#include "stdafx.h"
#include "emotracker_.h"

EmotionsConfiguration* CreateEmotionsConfiguration() {
	return new EmotionsConfiguration();
}
void DisposeEmotionsConfiguration(EmotionsConfiguration* a_pObject) {
	if (a_pObject != NULL)
	{
		delete a_pObject;
		a_pObject = NULL;
	}
} 
void EmotionsConfiguration_setStreamFilename(EmotionsConfiguration* a_pObject, pxcCHAR *streamFilename) {
	if (a_pObject != NULL)
	{
		a_pObject->setStreamFilename(streamFilename);
	}
}
void EmotionsConfiguration_setCalibrationFilename(EmotionsConfiguration* a_pObject, pxcCHAR *calibFilename) {
	if (a_pObject != NULL)
	{
		a_pObject->setCalibrationFilename(calibFilename);
	}
}
void EmotionsConfiguration_setEmotionsFilename(EmotionsConfiguration* a_pObject, pxcCHAR *emoFilename){
	if (a_pObject != NULL)
	{
		a_pObject->setEmotionsFilename(emoFilename);
	}
}


void EmotionsConfiguration_setPersonTracking(EmotionsConfiguration* a_pObject, pxcBool personTracking) {
	if (a_pObject != NULL)
	{
		a_pObject->setPersonTracking(personTracking);
	}
}
void EmotionsConfiguration_setAddGazePoint(EmotionsConfiguration* a_pObject, pxcBool addGazePoint) {
	if (a_pObject != NULL)
	{
		a_pObject->setAddGazePoint(addGazePoint);
	}
}
void EmotionsConfiguration_setRecordingLandmark(EmotionsConfiguration* a_pObject, pxcBool recordingLandmark) {
	if (a_pObject != NULL)
	{
		a_pObject->setRecordingLandmark(recordingLandmark);
	}
}
void EmotionsConfiguration_setRecordingGaze(EmotionsConfiguration* a_pObject, pxcBool recordingGaze) {
	if (a_pObject != NULL)
	{
		a_pObject->setRecordingGaze(recordingGaze);
	}
}
void EmotionsConfiguration_setUsePersonTrackingModuleEmotions(EmotionsConfiguration* a_pObject, pxcBool usePersonTrackingModuleEmotions) {
	if (a_pObject != NULL)
	{
		a_pObject->setUsePersonTrackingModuleEmotions(usePersonTrackingModuleEmotions);
	}
}
void EmotionsConfiguration_setPlaybackMode(EmotionsConfiguration* a_pObject, pxcBool playbackMode) {
	if (a_pObject != NULL)
	{
		a_pObject->setPlaybackMode(playbackMode);
	}
}



EmotionsData* CreateEmotionsData()
	{
		return new EmotionsData();
	}
void DisposeEmotionsData(EmotionsData* a_pObject)
{
	if (a_pObject != NULL)
	{
		delete a_pObject;
		a_pObject = NULL;
	}
}


EmotionsTracker*  CreateEmotionsTracker()
	{
		return new EmotionsTracker();
	}
EmotionsTracker*  CreateEmotionsTrackerSM(PXCSenseManager *m_sm)
{
	return new EmotionsTracker(m_sm);
}
void DisposeEmotionsTracker(EmotionsTracker* a_pObject)
{
	if (a_pObject != NULL)
	{
		delete a_pObject;
		a_pObject = NULL;
	}
}
pxcStatus EmotionsTracker_Init(EmotionsTracker* a_pObject)
{
	if (a_pObject != NULL)
	{
		return a_pObject->Init();
	}
}
pxcStatus EmotionsTracker_ProcessFrame(EmotionsTracker* a_pObject)
{
	if (a_pObject != NULL)
	{
		return a_pObject->ProcessFrame();
	}
}
pxcStatus EmotionsTracker_ProcessSample(EmotionsTracker* a_pObject, PXCCapture::Sample *sample)
{
	if (a_pObject != NULL)
	{
		return a_pObject->ProcessSample(sample);
	}
}
 pxcStatus EmotionsTracker_SetOutputURL(EmotionsTracker* a_pObject, pxcCHAR *url)
 {
	 if (a_pObject != NULL)
	 {
		 return a_pObject->SetOutputURL(url);
	 }
 }
EmotionsConfiguration* EmotionsTracker_QueryConfiguration(EmotionsTracker* a_pObject)
{
	if (a_pObject != NULL)
	{
		return a_pObject->QueryConfiguration();
	}
}
EmotionsData* EmotionsTracker_QueryOutput(EmotionsTracker* a_pObject)
{
	if (a_pObject != NULL)
	{
		return a_pObject->QueryOutput();
	}
}
pxcStatus EmotionsTracker_Start(EmotionsTracker* a_pObject)
	{
		if (a_pObject != NULL)
		{
			return a_pObject->Start();
		}
	}
pxcStatus EmotionsTracker_Stop(EmotionsTracker* a_pObject)
	{
		if (a_pObject != NULL)
		{
			return a_pObject->Stop();
		}
	}
pxcStatus EmotionsTracker_Release(EmotionsTracker* a_pObject)
	{
		if (a_pObject != NULL)
		{
			return a_pObject->Release();
		}
	}
void EmotionsTracker_Process(EmotionsTracker* a_pObject)
	{
		if (a_pObject != NULL)
		{
			return a_pObject->Process();
		}
	}
pxcStatus EmotionsTracker_getStatus(EmotionsTracker* a_pObject)
{
	if (a_pObject != NULL)
	{
		return a_pObject->getStatus();
	}
}


EmotionsHandler* CreateEmotionsHandler(EmotionsTracker *etr)
{
	return new EmotionsHandler(etr);
}

void DisposeEmotionsHandler(EmotionsHandler* a_pObject)
{
	if (a_pObject != NULL)
	{
		delete a_pObject;
		a_pObject = NULL;
	}
}
pxcStatus PXCAPI EmotionsHandler_OnNewSample(EmotionsHandler* a_pObject, pxcUID uid, PXCCapture::Sample *sample)
{
	if (a_pObject != NULL)
	{
		return a_pObject->OnNewSample(uid, sample);
	}
}


