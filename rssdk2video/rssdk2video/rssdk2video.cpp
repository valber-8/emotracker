// rssdk2video.cpp: главный файл проекта.

#include "stdafx.h"
#include "pxcsensemanager.h"
#include <vcclr.h>
#include <opencv2/core/core.hpp>        // Basic OpenCV structures (cv::Mat)
#include <opencv2/highgui/highgui.hpp>  // Video write
#include <opencv2/imgproc/imgproc.hpp>  // Image processing
#include <msclr\marshal_cppstd.h>

using namespace System;
using namespace System::IO;
//using namespace std;
using namespace cv;

int main(array<System::String ^> ^args)
{
	
	if (args->Length < 2) {
		Console::WriteLine(L"------------------------------------------------------------------------------\nThis program convert rssdk color captured from realsence camera\nto video files encoded with common codecs.\nYou can specify frame rate, size and encoding of output video.\nUsage:\n./rssdk2video inputvideoName outputvideoName [fps [ width height [Y]]]\n------------------------------------------------------------------------------\n");
		return 0;
	}
	pxcCHAR *filename;
	PXCSenseManager* sm = PXCSenseManager::CreateInstance();
	if (!sm) {
		Console::WriteLine(L"Init Failed");
		return -1;
	}
	System::String^ fname = gcnew System::String(System::String::Concat(Directory::GetCurrentDirectory(), L"\\", args[0]));
	pin_ptr<const wchar_t> wch = PtrToStringChars(fname);
	filename = const_cast<wchar_t*>(wch);


	//fname = gcnew System::String(System::String::Concat(Directory::GetCurrentDirectory(), L"\\", args[1]));
	//cv::String ofilename = cv::String(fname);
	
	//fname = args[1];
	//Console::WriteLine(gcnew System::String(filename));
	//args[1]->CopyTo(0, (wchar_t*)filename, 0, args[1]->Length);
	//filename = std::wstring(args[1]->.begin(), args[1]->end()).c_str();
	msclr::interop::marshal_context ctxt;
	const std::string NAME = static_cast<std::string>(ctxt.marshal_as<std::string>(args[1]));// "out.avi";//msclr::interop::marshal_as<std::string>(fname);
	double fps = args->Length>2?System::Convert::ToDouble(args[2]):30.0;
	Size S = args->Length>4 ? Size(System::Convert::ToInt32(args[3]), System::Convert::ToInt32(args[4])):Size(480, 270);//Size(1920, 1080);
	//int ex = CV_FOURCC('M', 'J', 'P', 'G');
	//int ex = CV_FOURCC('H', '2', '6', '4'); //mp4
	//int ex = CV_FOURCC('M', 'P', 'E', 'G');
    //int ex = CV_FOURCC('F', 'L', 'V', '1');
    int ex = args->Length>5 ? -1 : CV_FOURCC('I', 'Y', 'U', 'V');
	VideoWriter outputVideo = VideoWriter( NAME, ex, fps, S, true);
	//VideoWriter outputVideo = cudacodec::createVideoWriter(NAME, fps, S, );
	//VideoWriter outputVideo=VideoWriter(msclr::interop::marshal_as<std::string>(fname), CV_FOURCC('a','v','c','1'), 25, cv::Size(1920,1080), true);
	//VideoWriter outputVideo = VideoWriter(msclr::interop::marshal_as<std::string>(fname), CV_FOURCC('m', 'p', '4', '2'), 25, cv::Size(1920, 1080), true);
	//VideoWriter outputVideo = VideoWriter(msclr::interop::marshal_as<std::string>(fname), -1, 25, cv::Size(1920, 1080), true);
	//VideoWriter outputVideo = VideoWriter("t1.mp4", CV_FOURCC('M', 'P', '4', '2'), 25, cv::Size(720, 400), true);

	if (!outputVideo.isOpened())
	{
		Console::WriteLine(gcnew System::String(System::String::Concat(L"Could not open the output video for write: ",args[1])));
		return -1;
	}
	
	sm->QueryCaptureManager()->SetFileName(filename, false);
	// Enable stream and Initialize
	sm->EnableStream(PXCCapture::STREAM_TYPE_COLOR, 0, 0);

	sm->Init();



	// Set realtime=true and pause=false

	sm->QueryCaptureManager()->SetRealtime(false);

	sm->QueryCaptureManager()->SetPause(true);



	// Streaming loop
	double t = 0.0;
	pxcI64 ts0,ts;
	int nframes = sm->QueryCaptureManager()->QueryNumberOfFrames();
	int i;
	for ( i = 0; i < nframes; i++) {

		// Set to work on every 3rd frame of data

		sm->QueryCaptureManager()->SetFrameByIndex(i);

		sm->FlushFrame();



		// Ready for the frame to be ready

		pxcStatus sts = sm->AcquireFrame(true);

		if (sts < PXC_STATUS_NO_ERROR) break;

		// Retrieve the sample and work on it. The image is in sample->color.

		PXCCapture::Sample* sample = sm->QuerySample();



		//....

		ts = sample->color->QueryTimeStamp();
		if (!i) ts0 = ts;
		PXCImage::ImageInfo info = sample->color->QueryInfo();

		PXCImage::ImageData data;

		// Access to the image data in the RGB24 format.

		sample->color->AcquireAccess(PXCImage::ACCESS_READ, PXCImage::PIXEL_FORMAT_RGB24, &data);

		// work on the image plane data.planes[0] with pitch data.pitches[0]. 

		IplImage *src = cvCreateImage(cvSize(info.width, info.height), IPL_DEPTH_8U, 3);

		src->imageData = (char*)data.planes[0];

		//use cv::resize to resize source to a destination image
		Mat im;
		resize(Mat(src), im, S, 0, 0, CV_INTER_LINEAR);
		
		double dt = (ts - ts0) / 10000000.0;
		while (t <= dt) {
			t += 1.0 / fps;
			outputVideo<<im;
			cvWaitKey();
		}	
		//...

		// Release access after use.

		sample->color->ReleaseAccess(&data);


		// Resume processing the next frame
		sm->ReleaseFrame();
		cvReleaseImage(&src);
		im.release();
	}



	// Clean up

	sm->Release();
	outputVideo.release();
    return 0;
}
