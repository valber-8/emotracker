#pragma once

#include <string>
#include <afxwin.h>
#include <map>
#include <time.h>
#include "pxcfacedata.h"
#include "pxccapture.h"
#include "PXCPersonTrackingData.h"

class PXCSenseManager;
class PXCFaceData;

class FaceTrackingProcessor
{
public:
	FaceTrackingProcessor(HWND window);
	void Process(HWND dialogWindow);
	void RegisterUser();
	void UnregisterUser();

private:
	HWND m_window;
	bool m_registerFlag;
	bool m_unregisterFlag;
	clock_t starttime;
	double prevtime;

	PXCFaceData* m_output;
	PXCPersonTrackingData* m_poutput;
	std::map<PXCFaceData::ExpressionsData::FaceExpression, std::wstring> m_expressionMap;

	std::map<PXCFaceData::ExpressionsData::FaceExpression, std::wstring> InitExpressionsMap();
	void CheckForDepthStream(PXCSenseManager* pp, HWND hwndDlg);
	void PerformRegistration();
	void PerformUnregistration();
	void WriteEmo(PXCFaceData* faceOutput, PXCPersonTrackingData* personOutput, CStdioFile* file);
};