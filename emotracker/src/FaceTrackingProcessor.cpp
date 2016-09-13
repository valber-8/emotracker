#include "FaceTrackingProcessor.h"
#include <assert.h>
#include <afxwin.h>
#include <string>
#include "pxcfaceconfiguration.h"
#include "PXCPersonTrackingData.h"
#include "pxcpersontrackingconfiguration.h"
#include "pxcfacemodule.h"
#include "pxcsensemanager.h"
#include "pxccapture.h"
#include "FaceTrackingUtilities.h"
#include "FaceTrackingAlertHandler.h"
#include "FaceTrackingRendererManager.h"
#include "resource.h"
#include <time.h>
//#include <vld.h>

extern PXCSession* session;
extern FaceTrackingRendererManager* renderer;

extern volatile bool isStopped;
extern volatile bool isActiveApp;
extern pxcCHAR fileName[1024];
extern pxcCHAR fileEmoName[1024];
extern HANDLE ghMutex;


extern int horizontal,vertical;

std::map<PXCFaceData::ExpressionsData::FaceExpression, std::wstring> FaceTrackingProcessor::InitExpressionsMap()
{
	std::map<PXCFaceData::ExpressionsData::FaceExpression, std::wstring> map;
	map[PXCFaceData::ExpressionsData::EXPRESSION_SMILE] = std::wstring(L"Smile");
	map[PXCFaceData::ExpressionsData::EXPRESSION_MOUTH_OPEN] = std::wstring(L"Mouth Open");
	map[PXCFaceData::ExpressionsData::EXPRESSION_KISS] = std::wstring(L"Kiss");
	map[PXCFaceData::ExpressionsData::EXPRESSION_EYES_TURN_LEFT] = std::wstring(L"Eyes Turn Left");
	map[PXCFaceData::ExpressionsData::EXPRESSION_EYES_TURN_RIGHT] = std::wstring(L"Eyes Turn Right");
	map[PXCFaceData::ExpressionsData::EXPRESSION_EYES_UP] = std::wstring(L"Eyes Up");
	map[PXCFaceData::ExpressionsData::EXPRESSION_EYES_DOWN] = std::wstring(L"Eyes Down");
	map[PXCFaceData::ExpressionsData::EXPRESSION_BROW_RAISER_LEFT] = std::wstring(L"Brow Raised Left");
	map[PXCFaceData::ExpressionsData::EXPRESSION_BROW_RAISER_RIGHT] = std::wstring(L"Brow Raised Right");
	map[PXCFaceData::ExpressionsData::EXPRESSION_BROW_LOWERER_LEFT] = std::wstring(L"Brow Lowered Left");
	map[PXCFaceData::ExpressionsData::EXPRESSION_BROW_LOWERER_RIGHT] = std::wstring(L"Brow Lowered Right");
	map[PXCFaceData::ExpressionsData::EXPRESSION_EYES_CLOSED_LEFT] = std::wstring(L"Closed Eye Left");
	map[PXCFaceData::ExpressionsData::EXPRESSION_EYES_CLOSED_RIGHT] = std::wstring(L"Closed Eye Right");
	map[PXCFaceData::ExpressionsData::EXPRESSION_TONGUE_OUT] = std::wstring(L"Tongue Out");
	map[PXCFaceData::ExpressionsData::EXPRESSION_PUFF_RIGHT] = std::wstring(L"Puff Right Cheek");
	map[PXCFaceData::ExpressionsData::EXPRESSION_PUFF_LEFT] = std::wstring(L"Puff Left Cheek");
	return map;
}

FaceTrackingProcessor::FaceTrackingProcessor(HWND window) : m_window(window), m_registerFlag(false), m_unregisterFlag(false) { 
	m_expressionMap = InitExpressionsMap();
}

void FaceTrackingProcessor::PerformRegistration()
{
	m_registerFlag = false;
	if(m_output->QueryFaceByIndex(0))
		m_output->QueryFaceByIndex(0)->QueryRecognition()->RegisterUser();
}

void FaceTrackingProcessor::PerformUnregistration()
{
	m_unregisterFlag = false;
	if(m_output->QueryFaceByIndex(0))
		m_output->QueryFaceByIndex(0)->QueryRecognition()->UnregisterUser();
}

void FaceTrackingProcessor::CheckForDepthStream(PXCSenseManager* pp, HWND hwndDlg)
{
	PXCFaceModule* faceModule = pp->QueryFace();
	if (faceModule == NULL) 
	{
		assert(faceModule);
		return;
	}
	PXCFaceConfiguration* config = faceModule->CreateActiveConfiguration();
	if (config == NULL)
	{
		assert(config);
		return;
	}

	PXCFaceConfiguration::TrackingModeType trackingMode = config->GetTrackingMode();
	config->Release();
	if (trackingMode == PXCFaceConfiguration::FACE_MODE_COLOR_PLUS_DEPTH)
	{
		PXCCapture::Device::StreamProfileSet profiles={};
		pp->QueryCaptureManager()->QueryDevice()->QueryStreamProfileSet(&profiles);
		if (!profiles.depth.imageInfo.format)
		{            
			std::wstring msg = L"Depth stream is not supported for device: ";
			msg.append(FaceTrackingUtilities::GetCheckedDevice(hwndDlg));           
			msg.append(L". \nUsing 2D tracking");
			MessageBox(hwndDlg, msg.c_str(), L"Face Tracking", MB_OK);            
		}
	}
}

void FaceTrackingProcessor::WriteEmo(PXCFaceData* faceOutput, PXCPersonTrackingData* personOutput, CStdioFile* file)
{
	
	WCHAR tempLine[200];
	double curtime = (clock() - starttime)/ (double)CLOCKS_PER_SEC;
	swprintf_s<sizeof(tempLine) / sizeof(pxcCHAR)>(tempLine, L"			<p begin=\"%.2d:%.2d:%05.2f\" end=\"%.2d:%.2d:%05.2f\">\n",(int)prevtime/3600,(int)prevtime/60-((int)prevtime / 3600)*60, prevtime-((int)prevtime/60)*60.0, (int)curtime/3600, (int)curtime/60 - ((int)curtime/3600)*60, curtime-((int)curtime/60)*60.0);
	file->WriteString(tempLine);
	const int numFaces = faceOutput->QueryNumberOfDetectedFaces();
	for (int i = 0; i < numFaces; ++i)
	{

		PXCFaceData::Face* trackedFace = faceOutput->QueryFaceByIndex(i);
		assert(trackedFace != NULL);

		PXCFaceData::ExpressionsData* expressionsData = trackedFace->QueryExpressions();
		if (!expressionsData)
			continue;
		file->WriteString(L"				<data type=\"text/plain; charset = us-ascii\">\n");
		swprintf_s<sizeof(tempLine) / sizeof(pxcCHAR)>(tempLine, L"					<metadata faceID=\"%d\"></metadata>\n", trackedFace->QueryUserID());
		file->WriteString(tempLine);
		for (auto expressionIter = m_expressionMap.begin(); expressionIter != m_expressionMap.end(); expressionIter++)
		{
			PXCFaceData::ExpressionsData::FaceExpressionResult expressionResult;
			if (expressionsData->QueryExpression(expressionIter->first, &expressionResult))
			{
				int intensity = expressionResult.intensity;
				std::wstring expressionName = expressionIter->second;

				swprintf_s<sizeof(tempLine) / sizeof(WCHAR)>(tempLine, L"					<chunk xml:id=\"%s\">%d</chunk>\n", expressionName.c_str(), intensity);

				file->WriteString(tempLine);
				//TextOut(dc2, xStartingPosition, yPosition, tempLine, (int)std::char_traits<wchar_t>::length(tempLine));
			}
		}

		file->WriteString(L"				</data>\n");
	}


	const int numPersons = personOutput->QueryNumberOfPeople();
	for (int i = 0; i < numPersons; ++i)
	{
		PXCPersonTrackingData::Person* personData = personOutput->QueryPersonData(PXCPersonTrackingData::ACCESS_ORDER_BY_ID, i);

		assert(personData != NULL);

		file->WriteString(L"				<data type=\"text/plain; charset = us-ascii\">\n");
		swprintf_s<sizeof(tempLine) / sizeof(pxcCHAR)>(tempLine, L"					<metadata personID=\"%d\"></metadata>\n", i);
		file->WriteString(tempLine);
		int max = 0;
		std::wstring emo=L"Unavailable";

		PXCPersonTrackingData::PersonExpressions* personExpressions = personData->QueryExpressions();
		PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult neutralResult;
		bool isNeutralExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::NEUTRAL, &neutralResult);
		//if (isNeutralExpressionValid)
		//{
			swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"					<chunk xml:id=\"Neutral\">%d</chunk>\n", neutralResult.confidence);
			file->WriteString(tempLine);
			if (neutralResult.confidence > max) {
				max = neutralResult.confidence;		emo = L"Neutral";
			}
		//}

		PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult happinessResult;
		bool isHappinessExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::HAPPINESS, &happinessResult);
		//if (isHappinessExpressionValid)
		//{
			swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"					<chunk xml:id=\"Happiness\">%d</chunk>\n", happinessResult.confidence);
			file->WriteString(tempLine);
			if (happinessResult.confidence > max) {
				max = happinessResult.confidence;		emo = L"Happy";
			}
		//}

		PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult sadnessResult;
		bool isSadnessExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::SADNESS, &sadnessResult);
		//if (isSadnessExpressionValid)
		//{
			swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"					<chunk xml:id=\"Sadness\">%d</chunk>\n", sadnessResult.confidence);
			file->WriteString(tempLine);
			if (sadnessResult.confidence > max) {
				max = sadnessResult.confidence;		emo = L"Sad";
			}
		//}

		PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult surpriseResult;
		bool isSurpriseExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::SURPRISE, &surpriseResult);
		//if (isSurpriseExpressionValid)
		//{
			swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"					<chunk xml:id=\"Surprised\">%d</chunk>\n", surpriseResult.confidence);
			file->WriteString(tempLine);
			if (surpriseResult.confidence > max) {
				max = surpriseResult.confidence;		emo = L"Surprised";
			}
		//}

		PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult disgustResult;
		bool isDisgustExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::DISGUST, &disgustResult);
		//if (isDisgustExpressionValid)
		//{
			swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"					<chunk xml:id=\"Disgusted\">%d</chunk>\n", disgustResult.confidence);
			file->WriteString(tempLine);
			if (disgustResult.confidence > max) {
				max = disgustResult.confidence;		emo = L"Disgusted";
			}
		//}

		PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult contemptResult;
		bool isContemptExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::CONTEMPT, &contemptResult);
		//if (isContemptExpressionValid)
		//{
			swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"					<chunk xml:id=\"Contemptful\">%d</chunk>\n", contemptResult.confidence);
			file->WriteString(tempLine);
			if (contemptResult.confidence > max) {
				max = contemptResult.confidence;		emo = L"Contemptful";
			}
		//}

		PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult fearResult;
		bool isFearExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::FEAR, &fearResult);
		//if (isFearExpressionValid)
		//{
			swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"					<chunk xml:id=\"Fearful\">%d</chunk>\n", fearResult.confidence);
			file->WriteString(tempLine);
			if (fearResult.confidence > max) {
				max = fearResult.confidence;		emo = L"Fear";
			}
		//}

		PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult angerResult;
		bool isAngerExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::ANGER, &angerResult);
		//if (isAngerExpressionValid)
		//{
			swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"					<chunk xml:id=\"Angry\">%d</chunk>\n", angerResult.confidence);
			file->WriteString(tempLine);
			if (angerResult.confidence > max) {
				max = angerResult.confidence;		emo = L"Angry";
			}
		//}
		file->WriteString(L"				</data>\n");
		swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"				%s\n", emo.c_str());
		file->WriteString(tempLine);
	}
	for (int i = 0; i < numFaces; ++i)
	{

		PXCFaceData::Face* trackedFace = faceOutput->QueryFaceByIndex(i);
		assert(trackedFace != NULL);
		if (trackedFace->QueryGaze()) {

			PXCFaceData::GazePoint new_point = trackedFace->QueryGaze()->QueryGazePoint();
			pxcI32 eye_point_x = new_point.screenPoint.x;
			pxcI32 eye_point_y = new_point.screenPoint.y;

			swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"				<span tts:extent=\"320px 240px\" tts:origin=\"%d%% %d%%\">+</span>\n", (int)(eye_point_x*100.0/horizontal), (int)(eye_point_y*100.0/vertical));
			file->WriteString(tempLine);
		}
	}
	
	file->WriteString(L"				Some emotion\n");
	file->WriteString(L"			</p>\n");
	prevtime = curtime;
}

void FaceTrackingProcessor::Process(HWND dialogWindow)
{
	

	CStdioFile emoFile;
	PXCSenseManager* senseManager = session->CreateSenseManager();
	starttime = clock();
	prevtime = 0.0;
	if (senseManager == NULL) 
	{
		FaceTrackingUtilities::SetStatus(dialogWindow, L"Failed to create an SDK SenseManager", statusPart);
		return;
	}

	/* Set Mode & Source */
	PXCCaptureManager* captureManager = senseManager->QueryCaptureManager();

	if (FaceTrackingUtilities::IsModuleSelected(dialogWindow, IDC_SAVEEMOTIONS))
	{
		
		emoFile.Open(fileEmoName, CFile::modeCreate | CFile::modeWrite);
		emoFile.WriteString(L"<?xml version = \"1.0\" encoding = \"UTF-8\" ?>\n");
		emoFile.WriteString(L"<tt xmlns = \"http://www.w3.org/ns/ttml\" xmlns:tts = \"http://www.w3.org/ns/ttml#styling\" xmlns:ttm = \"http://www.w3.org/ns/ttml#metadata\" >\n");
		emoFile.WriteString(L"	<head>\n");
		emoFile.WriteString(L"		<metadata xmlns:ttm = \"http://www.w3.org/ns/ttml#metadata\">\n");
		emoFile.WriteString(L"			<ttm:title>Timed Text TTML Example</ttm:title>\n");
		emoFile.WriteString(L"			<ttm:copyright>The Authors(c) 2006</ttm:copyright>\n");
		emoFile.WriteString(L"		</metadata>\n");
		emoFile.WriteString(L"	</head>\n");
		emoFile.WriteString(L"	<body>\n");
		emoFile.WriteString(L"		<div>\n");

	}
	if (!FaceTrackingUtilities::GetPlaybackState(dialogWindow))
	{
		captureManager->FilterByDeviceInfo(FaceTrackingUtilities::GetCheckedDeviceInfo(dialogWindow));
	}

	pxcStatus status = PXC_STATUS_NO_ERROR;
	if (FaceTrackingUtilities::GetRecordState(dialogWindow)) 
	{
		status = captureManager->SetFileName(fileName, true);
	} 
	else if (FaceTrackingUtilities::GetPlaybackState(dialogWindow)) 
	{
		status = captureManager->SetFileName(fileName, false);
		senseManager->QueryCaptureManager()->SetRealtime(false);
	} 
	if (status < PXC_STATUS_NO_ERROR) 
	{
		FaceTrackingUtilities::SetStatus(dialogWindow, L"Failed to Set Record/Playback File", statusPart);
		return;
	}

	/* Set Module */
	senseManager->EnableFace();

	senseManager->EnablePersonTracking();

	/* Initialize */
	FaceTrackingUtilities::SetStatus(dialogWindow, L"Init Started", statusPart);

	PXCFaceModule* faceModule = senseManager->QueryFace();
	if (faceModule == NULL)
	{
		assert(faceModule);
		return;
	}
	PXCFaceConfiguration* config = faceModule->CreateActiveConfiguration();
	if (config == NULL)
	{
		assert(config);
		return;
	}

	config->QueryGaze()->isEnabled = true;

	config->SetTrackingMode(FaceTrackingUtilities::GetCheckedProfile(dialogWindow));
	config->ApplyChanges();

	pxcCHAR *calibFileName = L"C:\\Users\\test\\Source\\Repos\\emotracker\\calib.bin";
	FILE* my_file = _wfopen(calibFileName, L"rb");
	short calibBuffersize = config->QueryGaze()->QueryCalibDataSize();
	unsigned char* calibBuffer = new unsigned char[calibBuffersize];
	fread(calibBuffer, calibBuffersize, sizeof(pxcBYTE), my_file);
	fclose(my_file);
	pxcStatus st = config->QueryGaze()->LoadCalibration(calibBuffer, calibBuffersize);
	if (st != PXC_STATUS_NO_ERROR) {
		assert(config);
		return;
	}

	if (FaceTrackingUtilities::IsModuleSelected(dialogWindow, IDC_PULSE) && !FaceTrackingUtilities::GetPlaybackState(dialogWindow))
	{
		PXCCapture::Device::StreamProfileSet set;
		memset(&set, 0, sizeof(set));
		set.color.imageInfo.height = 720;
		set.color.imageInfo.width = 1280;	
		captureManager->FilterByStreamProfiles(&set);
	}
		
	PXCPersonTrackingModule* personModule = senseManager->QueryPersonTracking();
	if (personModule == NULL)
	{
		assert(personModule);
		return;
	}
	PXCPersonTrackingConfiguration* pconfig = personModule->QueryConfiguration();
	if (pconfig == NULL)
	{
		assert(pconfig);
		return;
	}
	if (senseManager->Init() < PXC_STATUS_NO_ERROR)
	{
		captureManager->FilterByStreamProfiles(NULL);
		if (senseManager->Init() < PXC_STATUS_NO_ERROR)
		{
			FaceTrackingUtilities::SetStatus(dialogWindow, L"Init Failed", statusPart);
			config->Release();
			pconfig->Release();
			senseManager->Close();
			senseManager->Release();
			return;
		}
	}

	PXCCapture::DeviceInfo info;
	senseManager->QueryCaptureManager()->QueryDevice()->QueryDeviceInfo(&info);

    CheckForDepthStream(senseManager, dialogWindow);
    FaceTrackingAlertHandler alertHandler(dialogWindow);
    if (FaceTrackingUtilities::GetCheckedModule(dialogWindow))
    {
        config->detection.isEnabled = FaceTrackingUtilities::IsModuleSelected(dialogWindow, IDC_LOCATION);
        config->landmarks.isEnabled = FaceTrackingUtilities::IsModuleSelected(dialogWindow, IDC_LANDMARK);
        config->pose.isEnabled = FaceTrackingUtilities::IsModuleSelected(dialogWindow, IDC_POSE);
			FaceTrackingUtilities::IsModuleSelected(dialogWindow, IDC_PULSE) ? config->QueryPulse()->Enable() : config->QueryPulse()->Disable();
			
        if (FaceTrackingUtilities::IsModuleSelected(dialogWindow, IDC_EXPRESSIONS))
        {
            config->QueryExpressions()->Enable();
            config->QueryExpressions()->EnableAllExpressions();
        }
        else
        {
            config->QueryExpressions()->DisableAllExpressions();
            config->QueryExpressions()->Disable();
        }
        if (FaceTrackingUtilities::IsModuleSelected(dialogWindow, IDC_RECOGNITION))
        {
            config->QueryRecognition()->Enable();
        }
		if (FaceTrackingUtilities::IsModuleSelected(dialogWindow, IDC_SAVEEMOTIONS))
		{
			config->QueryExpressions()->Enable();
			config->QueryExpressions()->EnableAllExpressions();
			config->QueryPulse()->Enable();
			pconfig->QueryExpressions()->Enable();
			pconfig->QueryExpressions()->EnableAllExpressions();
		}
        config->EnableAllAlerts();
        config->SubscribeAlert(&alertHandler);

        config->ApplyChanges();
    }
    FaceTrackingUtilities::SetStatus(dialogWindow, L"Streaming", statusPart);
    m_output = faceModule->CreateOutput();
	m_poutput = personModule->QueryOutput();

    bool isNotFirstFrame = false;
    bool isFinishedPlaying = false;

    ResetEvent(renderer->GetRenderingFinishedSignal());

	renderer->SetSenseManager(senseManager);
    renderer->SetNumberOfLandmarks(config->landmarks.numLandmarks);
    renderer->SetCallback(renderer->SignalProcessor);

    if (!isStopped)
    {
        while (true)
        {
            if (senseManager->AcquireFrame(true) < PXC_STATUS_NO_ERROR)
            {
                isFinishedPlaying = true;
            }

            if (isNotFirstFrame)
            {
                WaitForSingleObject(renderer->GetRenderingFinishedSignal(), INFINITE);
            }

            if (isFinishedPlaying || isStopped)
            {
                if (isStopped)
                    senseManager->ReleaseFrame();

                if (isFinishedPlaying)
                    PostMessage(dialogWindow, WM_COMMAND, ID_STOP, 0);

                break;
            }

            m_output->Update();
            PXCCapture::Sample* sample = senseManager->QueryFaceSample();

            isNotFirstFrame = true;

            if (sample != NULL)
            {
				DWORD dwWaitResult;
				dwWaitResult = WaitForSingleObject(ghMutex,	INFINITE);
				if(dwWaitResult == WAIT_OBJECT_0)
				{
					renderer->DrawBitmap(sample, config->GetTrackingMode() == PXCFaceConfiguration::FACE_MODE_IR);
					renderer->SetOutput(m_output);
					renderer->SignalRenderer();

					WriteEmo(m_output, m_poutput, &emoFile);

					if(!ReleaseMutex(ghMutex))
					{
						throw std::exception("Failed to release mutex");
						return;
					}
				}
            }

            if (config->QueryRecognition()->properties.isEnabled)
            {
                if (m_registerFlag)
                    PerformRegistration();

                if (m_unregisterFlag)
                    PerformUnregistration();
            }

            senseManager->ReleaseFrame();
        }

        m_output->Release();
        FaceTrackingUtilities::SetStatus(dialogWindow, L"Stopped", statusPart);
    }

	if (FaceTrackingUtilities::IsModuleSelected(dialogWindow, IDC_SAVEEMOTIONS))
	{

		emoFile.WriteString(L"		</div>\n");
		emoFile.WriteString(L"	</body>\n");
		emoFile.WriteString(L"</tt>\n");
		emoFile.Close();
	}

	config->Release();
	senseManager->Close(); 
	senseManager->Release();
}

void FaceTrackingProcessor::RegisterUser()
{
	m_registerFlag = true;
}

void FaceTrackingProcessor::UnregisterUser()
{
	m_unregisterFlag = true;
}