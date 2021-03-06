// emotracker.cpp: определяет экспортированные функции для приложения DLL.
//

#include "stdafx.h"
#include "emotracker.h"

//using namespace emotracker;

	EmotionsTracker::EmotionsTracker()
	{
		m_conf = new EmotionsConfiguration();
		m_expressionMap = InitExpressionsMap();
		m_data = new EmotionsData();
		m_st = new pxcStatus();
	}

	EmotionsTracker::~EmotionsTracker()
	{
	}

	EmotionsTracker::EmotionsTracker(PXCSenseManager * m_sm)
	{
		this->m_sm = m_sm;
		m_conf = new EmotionsConfiguration();
		m_expressionMap = InitExpressionsMap();
		m_data = new EmotionsData();
		m_st = new pxcStatus();
	}

	pxcStatus EmotionsTracker::Init()
	{
		RECT desktop;
		// Get a handle to the desktop window
		const HWND hDesktop = GetDesktopWindow();
		// Get the size of screen to the variable desktop
		GetWindowRect(hDesktop, &desktop);
		horizontal = desktop.right;
		vertical = desktop.bottom;

		starttime = clock();
		framenum = 0;
		prevtime = 0.0;
		if (m_sm == NULL) {
			PXCSession* session = PXCSession::CreateInstance();
			m_sm = session->CreateSenseManager();
		}
		PXCCaptureManager* captureManager = m_sm->QueryCaptureManager();
		if (m_conf->streamFilename!=NULL){

			if (!m_conf->playbackMode)
				*m_st = captureManager->SetFileName(m_conf->streamFilename, true);
			else {
				*m_st = captureManager->SetFileName(m_conf->streamFilename, false);
				m_sm->QueryCaptureManager()->SetRealtime(false);
				if (*m_st < PXC_STATUS_NO_ERROR)
				{
					return *m_st;
				}
			}
		}

		/* Set Modules */
		m_sm->EnableFace();
		if (m_conf->personTracking) {
			m_sm->EnablePersonTracking();
		}

		/* Initialize */
		PXCFaceModule* faceModule = m_sm->QueryFace();
		if (faceModule == NULL)
		{
			assert(faceModule);
			return *m_st=pxcStatus(pxcStatus::PXC_STATUS_INIT_FAILED);
		}
		PXCFaceConfiguration* config = faceModule->CreateActiveConfiguration();
		if (config == NULL)
		{
			assert(config);
			return *m_st = pxcStatus(pxcStatus::PXC_STATUS_INIT_FAILED);
		}
		config->QueryPulse()->Enable();
		if (m_conf->recordingGaze) {
			config->QueryGaze()->isEnabled = true;
		}

		config->SetTrackingMode(PXCFaceConfiguration::FACE_MODE_COLOR_PLUS_DEPTH);
		config->ApplyChanges();

		if (m_conf->recordingGaze) {

			FILE* cfile = _wfopen(m_conf->calibFilename, L"rb");

			if (cfile == NULL) {
				assert(config);
				return *m_st = pxcStatus(pxcStatus::PXC_STATUS_FILE_READ_FAILED);
			}
			short calibBuffersize = config->QueryGaze()->QueryCalibDataSize();
			unsigned char* calibBuffer = new unsigned char[calibBuffersize];
			fread(calibBuffer, calibBuffersize, sizeof(pxcBYTE), cfile);
			fclose(cfile);
			pxcStatus st = config->QueryGaze()->LoadCalibration(calibBuffer, calibBuffersize);
			if (st != PXC_STATUS_NO_ERROR) {
				assert(config);
				return *m_st = pxcStatus(pxcStatus::PXC_STATUS_NOT_MATCHING_CALIBRATION);
			}
		}

		PXCPersonTrackingModule* personModule = NULL;
		PXCPersonTrackingConfiguration* pconfig = NULL;
		if (m_conf->personTracking) {

			personModule = m_sm->QueryPersonTracking();
			if (personModule == NULL)
			{
				assert(personModule);
				return *m_st = pxcStatus(pxcStatus::PXC_STATUS_INIT_FAILED);
			}
			pconfig = personModule->QueryConfiguration();
			if (pconfig == NULL)
			{
				assert(pconfig);
				return *m_st = pxcStatus(pxcStatus::PXC_STATUS_INIT_FAILED);
			}
		}

		


		config->detection.isEnabled = true;
		config->landmarks.isEnabled = true;
		config->pose.isEnabled = true;
		config->QueryExpressions()->Enable();
		config->QueryExpressions()->EnableAllExpressions();
		config->QueryPulse()->Enable();
		//config->EnableAllAlerts();
		//config->SubscribeAlert(&alertHandler);

		config->ApplyChanges();

		if (m_conf->personTracking) {
			pconfig->QueryExpressions()->Enable();
			pconfig->QueryExpressions()->EnableAllExpressions();
		}
		if (m_sm->Init() < PXC_STATUS_NO_ERROR)
		{
			captureManager->FilterByStreamProfiles(NULL);
			if (m_sm->Init() < PXC_STATUS_NO_ERROR)
			{
				config->Release();
				m_sm->Close();
				m_sm->Release();
				return *m_st = pxcStatus(pxcStatus::PXC_STATUS_INIT_FAILED);
			}
		}


		m_face = faceModule->CreateOutput();
		if (m_conf->personTracking) {
			m_person = personModule->QueryOutput();
		}

		efile = new CStdioFile(m_conf->emoFilename, CFile::modeCreate | CFile::modeWrite);
		if (efile == NULL) {
			return *m_st = pxcStatus(pxcStatus::PXC_STATUS_INIT_FAILED);
		}
		efile->WriteString(L"<?xml version = \"1.0\" encoding = \"UTF-8\" ?>\n");
		efile->WriteString(L"<tt xmlns = \"http://www.w3.org/ns/ttml\" xmlns:tts = \"http://www.w3.org/ns/ttml#styling\" xmlns:ttm = \"http://www.w3.org/ns/ttml#metadata\" >\n");
		efile->WriteString(L"	<head>\n");
		efile->WriteString(L"		<metadata xmlns:ttm = \"http://www.w3.org/ns/ttml#metadata\">\n");
		efile->WriteString(L"			<ttm:title>Timed Text TTML Example</ttm:title>\n");
		efile->WriteString(L"			<ttm:copyright>The Authors(c) 2006</ttm:copyright>\n");
		efile->WriteString(L"		</metadata>\n");
		efile->WriteString(L"	</head>\n");
		efile->WriteString(L"	<body>\n");
		efile->WriteString(L"		<div>\n");

		/*
		if (m_conf->personTracking) {

			//PXCSession* session = PXCSession::CreateInstance();
			//m_sm2 = PXCSenseManager::CreateInstance();
			m_sm2 = m_sm;
			m_sm2->EnablePersonTracking();
			PXCPersonTrackingModule* personModule = m_sm2->QueryPersonTracking();
			if (personModule == NULL)
			{
				assert(personModule);
				return pxcStatus(pxcStatus::PXC_STATUS_INIT_FAILED);
			}
			PXCPersonTrackingConfiguration* pconfig = personModule->QueryConfiguration();
			if (pconfig == NULL)
			{
				assert(pconfig);
				return pxcStatus(pxcStatus::PXC_STATUS_INIT_FAILED);
			}

			if (m_sm2->Init() < PXC_STATUS_NO_ERROR)
			{
				captureManager->FilterByStreamProfiles(NULL);
				if (m_sm2->Init() < PXC_STATUS_NO_ERROR)
				{
					pconfig->Release();
					m_sm2->Close();
					m_sm2->Release();
					return pxcStatus(pxcStatus::PXC_STATUS_INIT_FAILED);
				}
			}

			pconfig->QueryExpressions()->Enable();
			pconfig->QueryExpressions()->EnableAllExpressions();
			m_person = personModule->QueryOutput();
		}
		*/


		return *m_st = pxcStatus(pxcStatus::PXC_STATUS_NO_ERROR);
	}

	pxcStatus EmotionsTracker::ProcessFrame()
	{
		pxcStatus sts = m_sm->AcquireFrame(true);

		if (sts < PXC_STATUS_NO_ERROR)
			return *m_st = sts;
		PXCCapture::Sample *sample;

		sample = m_sm->QuerySample();

		sts = ProcessSample(sample); // process image

		m_sm->ReleaseFrame();

		return *m_st = sts; // pxcStatus();
	}

	pxcStatus EmotionsTracker::ProcessSample(PXCCapture::Sample * sample)
	{
		WriteEmo();
		return *m_st = pxcStatus();
	}

	pxcStatus EmotionsTracker::SetOutputURL(pxcCHAR * url)
	{
		return *m_st = pxcStatus();
	}
		
	EmotionsConfiguration* EmotionsTracker::QueryConfiguration()
		{
			return m_conf;
		}
	
	EmotionsData *EmotionsTracker::QueryOutput()
	{
		return m_data;
	}

	static DWORD WINAPI ProcessingThread(LPVOID arg)
	{
		EmotionsTracker *etr = (EmotionsTracker*)arg;
		if (etr->Init() < PXC_STATUS_NO_ERROR)
		{
			return PXC_STATUS_INIT_FAILED;
		}
		etr->Process();
		etr->Release();
		return 0;
	}

	pxcStatus EmotionsTracker::Start()
	{
		DWORD ecode;

		stop_thread = false;
		thread=CreateThread(0, 0, ProcessingThread, this, 0, lpThreadId);
		GetExitCodeThread(thread,&ecode);
		return pxcStatus();
	}

	pxcStatus EmotionsTracker::Stop()
	{
		stop_thread = true;	
		return pxcStatus();
	}

	pxcStatus EmotionsTracker::Release()
	{


		if (efile)
		{

			efile->WriteString(L"		</div>\n");
			efile->WriteString(L"	</body>\n");
			efile->WriteString(L"</tt>\n");
			efile->Close();
		}

		//->Release();
		m_sm->Close();
		m_sm->Release();
		/* 
		if (m_conf->personTracking) {
			m_sm2->Close();
			m_sm2->Release();
		}
		*/
		return pxcStatus();
	}

	EmotionsHandler::EmotionsHandler(EmotionsTracker *etr)
	{
		m_etr = etr;
	}

	pxcStatus PXCAPI EmotionsHandler::OnNewSample(pxcUID, PXCCapture::Sample * sample)
	{
		pxcStatus sts = m_etr->ProcessSample(sample);
		// return NO ERROR to continue, or 
		// any ERROR to exit the loop
		if (sts < PXC_STATUS_NO_ERROR)
			return sts;
		return PXC_STATUS_NO_ERROR;
	}

 	pxcStatus EmotionsTracker::WriteEmo()
	{

		//int horizontal = 1366, vertical = 768; //change it

		WCHAR tempLine[200];
		WCHAR emoLine[2000];
		std::wstring cumulemo = L"";
		int smile=0;


		m_face->Update();

		double curtime = (clock() - starttime) / (double)CLOCKS_PER_SEC;
		swprintf_s<sizeof(tempLine) / sizeof(pxcCHAR)>(tempLine, L"			<p begin=\"%.2d:%.2d:%05.2f\" end=\"%.2d:%.2d:%05.2f\">\n", (int)prevtime / 3600, (int)prevtime / 60 - ((int)prevtime / 3600) * 60, floor((prevtime - ((int)prevtime / 60)*60.0) * 100) / 100, (int)curtime / 3600, (int)curtime / 60 - ((int)curtime / 3600) * 60, floor((curtime - ((int)curtime / 60)*60.0) * 100) / 100);
		efile->WriteString(tempLine);

		efile->WriteString(L"				<data type=\"text/plain; charset = us-ascii\">\n");
		swprintf_s<sizeof(tempLine) / sizeof(pxcCHAR)>(tempLine, L"					<metadata id=\"%d\"></metadata>\n", ++framenum);
		efile->WriteString(tempLine);
		efile->WriteString(L"				<![CDATA[\n");

		const int numFaces = m_face->QueryNumberOfDetectedFaces();
		for (int i = 0; i < numFaces; ++i)
		{

			PXCFaceData::Face* trackedFace = m_face->QueryFaceByIndex(i);
			assert(trackedFace != NULL);						

			swprintf_s<sizeof(tempLine) / sizeof(pxcCHAR)>(tempLine, L"					<Face id=\"%d\">\n", trackedFace->QueryUserID());
			efile->WriteString(tempLine);

			if (m_conf->recordingLandmark) {
				PXCFaceData::LandmarksData *ldata = trackedFace->QueryLandmarks();
				if (ldata != nullptr) {
					// allocate the array big enough to hold the landmark points.
					pxcI32 npoints = ldata->QueryNumPoints();
					PXCFaceData::LandmarkPoint *points = new PXCFaceData::LandmarkPoint[npoints];
					// get the landmark data
					ldata->QueryPoints(points);
					efile->WriteString(L"						<Landmark>\n");
					for (int i = 0; i < npoints; ++i)
					{
						swprintf_s<sizeof(tempLine) / sizeof(WCHAR)>(tempLine, L"							<Point>%f %f</Point>\n", points[i].image.x, points[i].image.y);
						efile->WriteString(tempLine);
					}
					efile->WriteString(L"						</Landmark>\n");

					delete[] m_data->LandmarkPoints;
					m_data->LandmarkPoints = new PXCFaceData::LandmarkPoint[npoints];
					ldata->QueryPoints(m_data->LandmarkPoints);

					// Clean up
					delete[] points;
				}
			}
			PXCFaceData::PoseData *podata = trackedFace->QueryPose();
			// retrieve the pose information
			PXCFaceData::PoseEulerAngles angles;
			if (podata != nullptr) {
				podata->QueryPoseAngles(&angles);
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR)>(tempLine, L"						<Yaw>%f</Yaw>\n", angles.yaw);
				efile->WriteString(tempLine);
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR)>(tempLine, L"						<Pitch>%f</Pitch>\n", angles.pitch);
				efile->WriteString(tempLine);
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR)>(tempLine, L"						<Roll>%f</Roll>\n", angles.roll);
				efile->WriteString(tempLine);

				m_data->Yaw = angles.yaw;
				m_data->Pitch = angles.pitch;
				m_data->Roll = angles.roll;

				PXCFaceData::HeadPosition pos;
				podata->QueryHeadPosition(&pos);
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR)>(tempLine, L"						<HeadX>%f</HeadX>\n", pos.headCenter.x);
				efile->WriteString(tempLine);
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR)>(tempLine, L"						<HeadY>%f</HeadY>\n", pos.headCenter.y);
				efile->WriteString(tempLine);
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR)>(tempLine, L"						<HeadZ>%f</HeadZ>\n", pos.headCenter.z);
				efile->WriteString(tempLine);

				m_data->faceDirection.x = pos.headCenter.x;
				m_data->faceDirection.y = pos.headCenter.y;
				m_data->faceDirection.z = pos.headCenter.z;
			}
			PXCFaceData::PulseData *pdata = trackedFace->QueryPulse();
			// retrieve the pulse rate
			if (pdata != nullptr) {
				pxcF32 rate = pdata->QueryHeartRate();
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR)>(tempLine, L"						<Pulse>%f</Pulse>\n", rate);
				efile->WriteString(tempLine);

				m_data->pulse = rate;
			}

			if (m_conf->recordingGaze) {
				if (trackedFace->QueryGaze()) {
					PXCFaceData::GazePoint new_point = trackedFace->QueryGaze()->QueryGazePoint();
					pxcI32 eye_point_x = new_point.screenPoint.x;
					pxcI32 eye_point_y = new_point.screenPoint.y;
					swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"						<Gaze>%f %f</Gaze>\n", (eye_point_x*100.0 / horizontal), (eye_point_y*100.0 / vertical));
					efile->WriteString(tempLine);

					m_data->gaze.screenPoint.x = eye_point_x;
					m_data->gaze.screenPoint.y = eye_point_y;
				}
			}

			PXCFaceData::ExpressionsData* expressionsData = trackedFace->QueryExpressions();
			if (!expressionsData)
				continue;

			for (auto expressionIter = m_expressionMap.begin(); expressionIter != m_expressionMap.end(); expressionIter++)
			{
				PXCFaceData::ExpressionsData::FaceExpressionResult expressionResult;
				if (expressionsData->QueryExpression(expressionIter->first, &expressionResult))
				{
					int intensity = expressionResult.intensity;
					std::wstring expressionName = expressionIter->second;

					size_t start_pos = 0;
					while ((start_pos = expressionName.find(L" ", start_pos)) != std::wstring::npos) {
						expressionName.replace(start_pos, 1, L"_");
						start_pos++;
					}

					swprintf_s<sizeof(tempLine) / sizeof(WCHAR)>(tempLine, L"						<%s>%d</%s>\n", expressionName.c_str(), intensity, expressionName.c_str());

					efile->WriteString(tempLine);
				}
			}



			PXCFaceData::ExpressionsData::FaceExpressionResult expressionResult;
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_SMILE, &expressionResult))
			{
				m_data->Smile = expressionResult.intensity;
				smile += expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_MOUTH_OPEN, &expressionResult))
			{
				m_data->Mouth_Open = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_KISS, &expressionResult))
			{
				m_data->Kiss = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_EYES_TURN_LEFT, &expressionResult))
			{
				m_data->Eyes_Turn_Left = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_EYES_TURN_RIGHT, &expressionResult))
			{
				m_data->Eyes_Turn_Right = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_EYES_UP, &expressionResult))
			{
				m_data->Eyes_Up = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_EYES_DOWN, &expressionResult))
			{
				m_data->Eyes_Down = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_BROW_RAISER_LEFT, &expressionResult))
			{
				m_data->Brow_Raised_Left = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_BROW_RAISER_RIGHT, &expressionResult))
			{
				m_data->Brow_Raised_Right = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_BROW_LOWERER_LEFT, &expressionResult))
			{
				m_data->Brow_Lowered_Left = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_BROW_LOWERER_RIGHT, &expressionResult))
			{
				m_data->Brow_Lowered_Right = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_EYES_CLOSED_LEFT, &expressionResult))
			{
				m_data->Closed_Eye_Left = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_EYES_CLOSED_RIGHT, &expressionResult))
			{
				m_data->Closed_Eye_Right = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_TONGUE_OUT, &expressionResult))
			{
				m_data->Tongue_Out = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_PUFF_RIGHT, &expressionResult))
			{
				m_data->Puff_Right_Cheek = expressionResult.intensity;
			}
			if (expressionsData->QueryExpression(PXCFaceData::ExpressionsData::EXPRESSION_PUFF_LEFT, &expressionResult))
			{
				m_data->Puff_Left_Cheek = expressionResult.intensity;
			}
			efile->WriteString(L"					</Face>\n");
		}

		//if (m_conf->personTracking && m_sm2->AcquireFrame(true) >= PXC_STATUS_NO_ERROR) {
		if (m_conf->personTracking && m_person!=nullptr) {
			const int numPersons = m_person->QueryNumberOfPeople();
			for (int i = 0; i < numPersons; ++i)
			{
				PXCPersonTrackingData::Person* personData = m_person->QueryPersonData(PXCPersonTrackingData::ACCESS_ORDER_BY_ID, i);

				assert(personData != NULL);


				PXCPersonTrackingData::PersonTracking* personTracking;
				personTracking = personData->QueryTracking();

				swprintf_s<sizeof(tempLine) / sizeof(pxcCHAR)>(tempLine, L"					<Person id=\"%d\">\n", i);
				efile->WriteString(tempLine);
				int max = 0;
				std::wstring emo = L"Unavailable";

				PXCPersonTrackingData::PersonExpressions* personExpressions = personData->QueryExpressions();
				PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult neutralResult;
				bool isNeutralExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::NEUTRAL, &neutralResult);
				//if (isNeutralExpressionValid)
				//{
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"						<Neutral>%d</Neutral>\n", isNeutralExpressionValid ? neutralResult.confidence : -abs(neutralResult.confidence));
				efile->WriteString(tempLine);

				m_data->Neutral = isNeutralExpressionValid ? neutralResult.confidence : -abs(neutralResult.confidence);

				if (isNeutralExpressionValid && neutralResult.confidence > max) {
					max = neutralResult.confidence;		emo = L"Neutral";
				}
				//}

				PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult happinessResult;
				bool isHappinessExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::HAPPINESS, &happinessResult);
				//if (isHappinessExpressionValid)
				//{
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"						<Happiness>%d</Happiness>\n", isHappinessExpressionValid ? happinessResult.confidence : -abs(happinessResult.confidence));
				efile->WriteString(tempLine);

				m_data->Happiness = isHappinessExpressionValid ? happinessResult.confidence : -abs(happinessResult.confidence);

				if (isHappinessExpressionValid && happinessResult.confidence > max) {
					max = happinessResult.confidence;		emo = L"Happy";
				}
				//}

				PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult sadnessResult;
				bool isSadnessExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::SADNESS, &sadnessResult);
				//if (isSadnessExpressionValid)
				//{
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"						<Sadness>%d</Sadness>\n", isSadnessExpressionValid ? sadnessResult.confidence : -abs(sadnessResult.confidence));
				efile->WriteString(tempLine);

				m_data->Sadness = isSadnessExpressionValid ? sadnessResult.confidence : -abs(sadnessResult.confidence);

				if (isSadnessExpressionValid && sadnessResult.confidence > max) {
					max = sadnessResult.confidence;		emo = L"Sad";
				}
				//}

				PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult surpriseResult;
				bool isSurpriseExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::SURPRISE, &surpriseResult);
				//if (isSurpriseExpressionValid)
				//{
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"						<Surprised>%d</Surprised>\n", isSurpriseExpressionValid ? surpriseResult.confidence : -abs(surpriseResult.confidence));
				efile->WriteString(tempLine);

				m_data->Surprise = isSurpriseExpressionValid ? surpriseResult.confidence : -abs(surpriseResult.confidence);

				if (isSurpriseExpressionValid && surpriseResult.confidence > max) {
					max = surpriseResult.confidence;		emo = L"Surprised";
				}
				//}

				PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult disgustResult;
				bool isDisgustExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::DISGUST, &disgustResult);
				//if (isDisgustExpressionValid)
				//{
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"						<Disgusted>%d</Disgusted>\n", isDisgustExpressionValid ? disgustResult.confidence : -abs(disgustResult.confidence));
				efile->WriteString(tempLine);

				m_data->Disgust = isDisgustExpressionValid ? disgustResult.confidence : -abs(disgustResult.confidence);

				if (isDisgustExpressionValid && disgustResult.confidence > max) {
					max = disgustResult.confidence;		emo = L"Disgusted";
				}
				//}

				PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult contemptResult;
				bool isContemptExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::CONTEMPT, &contemptResult);
				//if (isContemptExpressionValid)
				//{
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"						<Contemptful>%d</Contemptful>\n", isContemptExpressionValid ? contemptResult.confidence : -abs(contemptResult.confidence));
				efile->WriteString(tempLine);

				m_data->Contempt = isContemptExpressionValid ? contemptResult.confidence : -abs(contemptResult.confidence);

				if (isContemptExpressionValid && contemptResult.confidence > max) {
					max = contemptResult.confidence;		emo = L"Contemptful";
				}
				//}

				PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult fearResult;
				bool isFearExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::FEAR, &fearResult);
				//if (isFearExpressionValid)
				//{
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"						<Fearful>%d</Fearful>\n", isFearExpressionValid ? fearResult.confidence : -abs(fearResult.confidence));
				efile->WriteString(tempLine);

				m_data->Fear= isFearExpressionValid ? fearResult.confidence : -abs(fearResult.confidence);

				if (isFearExpressionValid && fearResult.confidence > max) {
					max = fearResult.confidence;		emo = L"Fear";
				}
				//}

				PXCPersonTrackingData::PersonExpressions::PersonExpressionsResult angerResult;
				bool isAngerExpressionValid = personExpressions->QueryExpression(PXCPersonTrackingData::PersonExpressions::ANGER, &angerResult);
				//if (isAngerExpressionValid)
				//{
				swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"						<Angry>%d</Angry>\n", isAngerExpressionValid ? angerResult.confidence : -abs(angerResult.confidence));
				efile->WriteString(tempLine);

				m_data->Anger= isAngerExpressionValid ? angerResult.confidence : -abs(angerResult.confidence);

				if (isAngerExpressionValid && angerResult.confidence > max) {
					max = angerResult.confidence;		emo = L"Angry";
				}
				//}
				cumulemo += L"				" + emo + L"\n";
				efile->WriteString(L"					</Person>\n");
				//m_sm2->ReleaseFrame();
			}
		}
		efile->WriteString(L"				]]>\n");
		efile->WriteString(L"				</data>\n");
		if (m_conf->addGazePoint) {
			for (int i = 0; i < numFaces; ++i)
			{

				PXCFaceData::Face* trackedFace = m_face->QueryFaceByIndex(i);
				assert(trackedFace != NULL);
				if (trackedFace->QueryGaze()) {

					PXCFaceData::GazePoint new_point = trackedFace->QueryGaze()->QueryGazePoint();
					pxcI32 eye_point_x = new_point.screenPoint.x;
					pxcI32 eye_point_y = new_point.screenPoint.y;

					swprintf_s<sizeof(tempLine) / sizeof(WCHAR) >(tempLine, L"				<span tts:extent=\"320px 240px\" tts:origin=\"%d%% %d%%\">+</span>\n", (int)(eye_point_x*100.0 / horizontal), (int)(eye_point_y*100.0 / vertical));
					efile->WriteString(tempLine);
				}
			}
		}

		if (!m_conf->usePersonTrackingModuleEmotions) {
			if (smile > 20) cumulemo = L"Happy";
			else cumulemo = L"Neutral";
		}


		swprintf_s<sizeof(emoLine) / sizeof(WCHAR) >(emoLine, L"%s", cumulemo.c_str());
		efile->WriteString(emoLine);
		//file->WriteString(L"				Some emotion\n");
		efile->WriteString(L"			</p>\n");
		prevtime = curtime;

		m_data->timestamp = curtime;

		return pxcStatus();

	}

	void EmotionsTracker::Process() 
	{
		while (true)
		{
			if (stop_thread) break;
			try {
				pxcStatus sts = m_sm->AcquireFrame(true);
				if ( sts < PXC_STATUS_NO_ERROR)
				{
					*m_st = sts;
					return;
				}

				PXCCapture::Sample* sample = m_sm->QuerySample();
				if (sample != NULL) {
					ProcessSample(sample);
				}
				m_sm->ReleaseFrame();
			}
			catch (const std::runtime_error const & re) {}
			catch (std::exception const & ex) {}
		}
	}

	pxcStatus EmotionsTracker::getStatus() {
		return m_st!=NULL?*m_st:pxcStatus(PXC_STATUS_DATA_UNAVAILABLE);
	}


	std::map<PXCFaceData::ExpressionsData::FaceExpression, std::wstring> EmotionsTracker::InitExpressionsMap()
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

	EmotionsConfiguration::EmotionsConfiguration()
	{

	}

	void EmotionsConfiguration::setStreamFilename(pxcCHAR * streamFilename)
	{
		if (streamFilename == NULL) { this->streamFilename = NULL; return; }
		this->streamFilename = new wchar_t[wcslen(streamFilename)+1];
		wcsncpy(this->streamFilename, streamFilename, wcslen(streamFilename)+1);
	}

	void EmotionsConfiguration::setCalibrationFilename(pxcCHAR * calibFilename)
	{
		this->calibFilename = new wchar_t[wcslen(calibFilename)+1];
		wcsncpy(this->calibFilename, calibFilename, wcslen(calibFilename)+1);
	}

	void EmotionsConfiguration::setEmotionsFilename(pxcCHAR * emoFilename)
	{
		this->emoFilename = new wchar_t[wcslen(emoFilename)+1];
		wcsncpy(this->emoFilename, emoFilename, wcslen(emoFilename)+1);
	}


	void EmotionsConfiguration::setPersonTracking(pxcBool personTracking) {
		this->personTracking = personTracking;
	}
	void EmotionsConfiguration::setAddGazePoint(pxcBool addGazePoint) {
		this->addGazePoint = addGazePoint;
	}
	void EmotionsConfiguration::setRecordingLandmark(pxcBool recordingLandmark) {
		this->recordingLandmark = recordingLandmark;
	}
	void EmotionsConfiguration::setRecordingGaze(pxcBool recordingGaze) {
		this->recordingGaze = recordingGaze;
	}
	void EmotionsConfiguration::setUsePersonTrackingModuleEmotions(pxcBool usePersonTrackingModuleEmotions) {
		this->usePersonTrackingModuleEmotions = usePersonTrackingModuleEmotions;
	}
	void EmotionsConfiguration::setPlaybackMode(pxcBool playbackMode) {
		this->playbackMode = playbackMode;
	}
