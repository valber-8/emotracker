// GazePainter.cpp: главный файл проекта.

#include "stdafx.h"
#include <vcclr.h>
#include <opencv2/core/core.hpp>        // Basic OpenCV structures (cv::Mat)
#include <opencv2/highgui/highgui.hpp>  // Video write
#include <opencv2/imgproc/imgproc.hpp>  // Image processing
#include <msclr\marshal_cppstd.h>

using namespace System;
using namespace System::IO;
using namespace System::Xml;
//using namespace std;
using namespace cv;

int main(array<System::String ^> ^args)
{
	int width = 200,height = 200;

	if (args->Length < 3) {
		Console::WriteLine(L"------------------------------------------------------------------------------\nThis program add collected by emotions tracking library gaze points \nto video file and encode it with common codecs.\nYou can specify frame rate, size and encoding of output video.\nUsage:\n./GazePainter inputTTML inputvideoName outputvideoName [fps [ width height [Y]]]\n------------------------------------------------------------------------------\n");
		return 0;
	}

	msclr::interop::marshal_context ctxt;
	const string iNAME = static_cast<std::string>(ctxt.marshal_as<std::string>(args[1]));
	VideoCapture inputVideo = VideoCapture(iNAME);              // Open input
	if (!inputVideo.isOpened())
	{
		Console::WriteLine(L"Could not open the input video");
		return -1;
	}

	Size iS = Size((int)inputVideo.get(CV_CAP_PROP_FRAME_WIDTH),    // Acquire input size
		(int)inputVideo.get(CV_CAP_PROP_FRAME_HEIGHT));

	double ifps=(double)inputVideo.get(CV_CAP_PROP_FPS);


	const string oNAME = static_cast<std::string>(ctxt.marshal_as<std::string>(args[2]));
	double fps = args->Length>3 ? System::Convert::ToDouble(args[3]) : 30.0;
	Size S = args->Length>4 ? Size(System::Convert::ToInt32(args[4]), System::Convert::ToInt32(args[5])) : Size(480, 270);//Size(1920, 1080);
																														  //int ex = CV_FOURCC('M', 'J', 'P', 'G');
																														  //int ex = CV_FOURCC('H', '2', '6', '4'); //mp4
																														  //int ex = CV_FOURCC('M', 'P', 'E', 'G');
																														  //int ex = CV_FOURCC('F', 'L', 'V', '1');
	int ex = args->Length>6 ? -1 : CV_FOURCC('I', 'Y', 'U', 'V');
	VideoWriter outputVideo = VideoWriter(oNAME, ex, fps, S, true);

	if (!outputVideo.isOpened())
	{
		Console::WriteLine(L"Could not open the output video for write: ");
		return -1;
	}

	XmlTextReader^ reader = gcnew XmlTextReader(args[0]);
	reader->WhitespaceHandling = WhitespaceHandling::None; // пропускаем пустые узлы 

	// Streaming loop
	double t = 0.0;

	while (reader->Read())
		if (reader->NodeType == XmlNodeType::Element && reader->Name == "p")
		{

			double b, e;
			int h, m, s, ms;
			System::String^ bb = reader->GetAttribute("begin");
			std::string btime = static_cast<std::string>(ctxt.marshal_as<std::string>(bb));

			if (sscanf(btime.c_str(), "%d:%d:%d.%d", &h, &m, &s, &ms) >= 2)
			{
				b = h * 3600 + m * 60 + s + ms / 100.0;
			}

			Mat src;
			inputVideo >> src;
			if (src.empty()) break;

			for (; t < b; t += 1.0 / ifps) {

				//use cv::resize to resize source to a destination image
				Mat im;
				resize(Mat(src), im, S, 0, 0, CV_INTER_LINEAR);

				outputVideo << im;
				cvWaitKey();

				src.release();
				im.release();
				inputVideo >> src;
				if (src.empty()) break;
			}
			
			System::String^ ee = reader->GetAttribute("end");
			std::string etime = static_cast<std::string>(ctxt.marshal_as<std::string>(ee));

			if (sscanf(etime.c_str(), "%d:%d:%d.%d", &h, &m, &s, &ms) >= 2)
			{
				e = h * 3600 + m * 60 + s + ms / 100.0;
			}

			while (reader->Read() && !((reader->NodeType == XmlNodeType::Element && reader->Name == "data")
				|| (reader->NodeType == XmlNodeType::EndElement && reader->Name == "p")));
			if (reader->NodeType == XmlNodeType::EndElement && reader->Name == "p") continue;
			while (reader->Read() && !(reader->NodeType == XmlNodeType::CDATA
				|| (reader->NodeType == XmlNodeType::EndElement && reader->Name == "data")));
			if (reader->NodeType == XmlNodeType::EndElement && reader->Name == "data") continue;

			float x, y;
			XmlTextReader^ creader = gcnew XmlTextReader(gcnew StringReader( reader->Value));
			creader->WhitespaceHandling = WhitespaceHandling::None;
			try {
				if (creader->ReadToFollowing("Gaze")) {
					creader->Read();
					System::String^ vv = creader->Value;
					msclr::interop::marshal_context ctxt;
					std::string val = static_cast<std::string>(ctxt.marshal_as<std::string>(vv));
					if (sscanf(val.c_str(), "%f %f", &x, &y) < 2) continue;
				}
				else continue;
			}
			catch (System::Xml::XmlException const ^ee) { continue; }
			catch (const std::runtime_error const & re) { continue; }
			catch (std::exception const & ex) { continue; }

			while (reader->Read() && !(reader->NodeType == XmlNodeType::EndElement && reader->Name == "data"));
			while (reader->Read() && !(reader->NodeType == XmlNodeType::Text));
			System::String^ emotion= reader->Value;

			inputVideo >> src;
			if (src.empty()) break;
			for (; t < e; t += 1.0 / ifps) {
				
				//Mat src, res;
				//vector<Mat> spl;

				//inputVideo >> src;              // read
				//if (src.empty()) break;         // check if at end

				x = x*iS.width / 100;
				y = y*iS.height / 100;
				//(0 <= roi.x && 0 <= roi.width && roi.x + roi.width <= m.cols && 0 <= roi.y && 0 <= roi.height && roi.y + roi.height <= m.rows)
				
				int xx = x < 0 ? 0 : (x > src.cols ? src.cols : (int)x);
				int yy = y < 0 ? 0 : (y > src.rows ? src.rows : (int)y);
				int ww = x < 0 ? (x + width > 0 ? x + width : 0) : (xx + width > src.cols ? src.cols - xx : width);
				int hh = y < 0 ? (y + height > 0 ? y + height : 0) : (yy + height > src.rows ? src.rows - yy : height);

				cv::Rect r = cv::Rect(xx, yy, ww, hh);
				Mat roi_img(src(r));

				Mat rect1 = roi_img.clone();
				Mat rect;

				rect1.convertTo(rect, CV_8UC4);

				//roi_img.setTo(Scalar(0));

				unsigned char *input = (unsigned char*)(rect.data);
				for (int j = 0; j < rect.rows; j++) {
					for (int i = 0; i < rect.cols; i++) {
						unsigned char b = input[rect.step * j + i];
						unsigned char g = input[rect.step * j + i + 1];
						unsigned char r = input[rect.step * j + i + 2];
						unsigned char m = b > g ? (b > r ? b : r) : (r > g ? r : g);

						input[rect.step * j + i] = b < m ? 0 : b;
						input[rect.step * j + i + 1] = g < m ? 0 : g;
						input[rect.step * j + i + 2] = r < m ? 0 : r;
					}
				}


				rect.convertTo(rect1, roi_img.type());
				rect1.copyTo(roi_img);
				cvWaitKey();

				//outputVideo.write(res); //save or
				//outputVideo << res;

				//IplImage *out = cvCreateImage(cvSize(IS.width,IS.height), IPL_DEPTH_8U, 3);

				//out->imageData = (char*)data.planes[0];

				//use cv::resize to resize source to a destination image
				Mat im;
				resize(Mat(src), im, S, 0, 0, CV_INTER_LINEAR);

				outputVideo << im;
				cvWaitKey();

				// Release access after use.

				src.release();
				im.release();
				inputVideo >> src;
				if (src.empty()) break;

			}
			src.release();
		}


	// Clean up
	outputVideo.release();
	return 0;



}
