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
		Console::WriteLine(L"-------------------------------------------------------------------------------\n"
			"This program add collected by emotions tracking library gaze points \n"
			"to video file and encode it with installed codecs.\n"
			"You can specify frame rate, size and encoding of output video.\n"
			"Usage:\n"
			"GazePainter inputTTML inputvideoName outputvideoName [-f fps] [-S width height]\n"
			"[-Y] [-mode mode] [-filter filter] [-sm width height]\n"
			"  -f frame rate in fps\n"
			"  -S output video size \n"
			"  -Y provide choosing of output format \n"
			"  -mode mode = {square|round}  \n"
			"  -sm width and height of gaze mark\n"
			"  -filter filter={maxrgb|gray|cartoon} \n"
			"  -av exponential weighted gaze point average\n"
			"  -h help\n"
			"-------------------------------------------------------------------------------\n");
		return 0;
	}


	msclr::interop::marshal_context ctxt;
	const std::string iNAME = static_cast<std::string>(ctxt.marshal_as<std::string>(args[1]));
	VideoCapture inputVideo = VideoCapture(iNAME);              // Open input
	if (!inputVideo.isOpened())
	{
		Console::WriteLine(L"Could not open the input video");
		return -1;
	}

	Size iS = Size((int)inputVideo.get(CV_CAP_PROP_FRAME_WIDTH),    // Acquire input size
		(int)inputVideo.get(CV_CAP_PROP_FRAME_HEIGHT));

	double ifps=(double)inputVideo.get(CV_CAP_PROP_FPS);


	const std::string oNAME = static_cast<std::string>(ctxt.marshal_as<std::string>(args[2]));

	double fps = ifps;
	Size S =  Size(480, 270);//Size(1920, 1080);
	int ex =  CV_FOURCC('I', 'Y', 'U', 'V');
	//int ex = CV_FOURCC('M', 'J', 'P', 'G');

	int mode = 1;//square
	int filter = 1; //maxrgb
	bool av = false;
																				
	for (int i = 3; i < args->Length; i++) {
		if (args[i] == "-f") {
			fps = System::Convert::ToDouble(args[++i]);
		}
		else if (args[i] == "-S") {
			int w = System::Convert::ToInt32(args[++i]);
			int h = System::Convert::ToInt32(args[++i]);
			S = Size(w, h);
		}
		else if (args[i] == "-Y") {
			ex = -1;
		}
		else if (args[i] == "-sm") {
			width=System::Convert::ToInt32(args[++i]);
			height=System::Convert::ToInt32(args[++i]);
		}
		else if (args[i] == "-mode") {
			if (args[++i] == "round") mode = 2;
		}
		else if (args[i] == "-filter") {
			if (args[++i] == "cartoon") filter = 2;
		}
		else if (args[i] == "-av") {
			av = true;
		}
	}										  
																														  //int ex = CV_FOURCC('H', '2', '6', '4'); //mp4
																														  //int ex = CV_FOURCC('M', 'P', 'E', 'G');
									
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

	float est_x = iS.width / 2;
	float est_y = iS.height / 2;
	float alpha = 0.125;

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

			Mat src;
			inputVideo >> src;
			if (src.empty()) break;
			for (; t < e; t += 1.0 / ifps) 
			{
				
				//Mat src, res;
				//vector<Mat> spl;

				//inputVideo >> src;              // read
				//if (src.empty()) break;         // check if at end

				x = x*iS.width / 100;
				y = y*iS.height / 100;
				//(0 <= roi.x && 0 <= roi.width && roi.x + roi.width <= m.cols && 0 <= roi.y && 0 <= roi.height && roi.y + roi.height <= m.rows)
				
				if (x > 0 && x < src.cols&&y>0 && y < src.rows) {
					est_x = alpha*x + (1 - alpha)*est_x;
					est_y = alpha*y + (1 - alpha)*est_y;
				}

				if (av) {
					x = est_x;
					y = est_y;
				}

				x -= width / 2;
				y -= height / 2;


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

				if (filter == 1) {

					unsigned char *input = (unsigned char*)(rect.data);
					for (int j = 0; j < rect.rows; j++) {
						for (int i = 0; i < rect.cols; i++) {
							unsigned char b = input[rect.step * j + i * 3];
							unsigned char g = input[rect.step * j + i * 3 + 1];
							unsigned char r = input[rect.step * j + i * 3 + 2];
							unsigned char m = b > g ? (b > r ? b : r) : (r > g ? r : g);
							if (((double)(i - height / 2)*(i - height / 2) / height / height + (double)(j - width / 2)*(j - width / 2) / width / width < 0.25) || mode == 1) {
								input[rect.step * j + i * 3] = b < m ? 0 : b;
								input[rect.step * j + i * 3 + 1] = g < m ? 0 : g;
								input[rect.step * j + i * 3 + 2] = r < m ? 0 : r;
							}
						}
					}
				}
				else {
					//import cv2

					int num_down = 2;       // number of downsampling steps
					int	num_bilateral = 7;  // number of bilateral filtering steps

					Mat	img_rgb = src.clone();

					// downsample image using Gaussian pyramid
					Mat img_color = img_rgb.clone();
					for (int i = 1; i <= num_down; i++) {
						Mat dst;
						pyrDown(img_color, dst, Size(img_color.cols / 2, img_color.rows / 2));
						img_color = dst;
					}
					// repeatedly apply small bilateral filter instead of
					// applying one large filter
					for (int i = 1; i <= num_bilateral; i++) {
						Mat dst;
						bilateralFilter(img_color, dst, 9, 9, 7);
						img_color = dst;
					}

					// upsample image to original size
					for (int i = 1; i <= num_down; i++){
						Mat dst;
						pyrUp(img_color, dst, Size(img_color.cols * 2, img_color.rows * 2));
						img_color = dst;
					}

					Mat img_gray, img_blur, img_edge, img_edge_color, img_cartoon;

					// convert to grayscale and apply median blur
					cvtColor(img_rgb, img_gray, COLOR_RGB2GRAY);
					medianBlur(img_gray, img_blur, 7);
					// detect and enhance edges
					adaptiveThreshold(img_blur, img_edge, 255, ADAPTIVE_THRESH_MEAN_C, THRESH_BINARY, 9, 2);

					// convert back to color, bit - AND with color image
					cvtColor(img_edge, img_edge_color, COLOR_GRAY2RGB);
					bitwise_and(img_color, img_edge_color, img_cartoon);

					// display
					img_cartoon.copyTo(src);

				
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
