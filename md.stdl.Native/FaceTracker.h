#pragma once

#include "stdafx.h"

#if defined(_WIN64)
#define PLATFORM x64
#else
#define PLATFORM x86
#endif

using namespace System;
using namespace System::IO;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

using namespace System::Numerics;


namespace mp {
	namespace dx {
		namespace Tracking {

			/// <summary>
			/// A single detected and tracked face
			/// </summary>
			public ref class DetectedFace
			{
			public:
				property Vector2 Center { Vector2 get(); };
				property Vector2 Size { Vector2 get(); };
				property int Neighbors { int get(); };
				property int Angle { int get(); };
				property array<Vector2>^ Landmarks { array<Vector2>^ get(); };
				bool HasLandmarks = false;

				DetectedFace();
				~DetectedFace() { }

				/// <summary>
				/// Only for internal use! Updates the face data from a pointer to the result buffer
				/// </summary>
				void Update(int * pResult, int ii);

			private:
				int _x;
				int _y;
				int _width;
				int _height;
				int _neighbors;
				int _angle;
				array<Vector2>^ _landmarks;
			};

			/// <summary>
			/// Face tracker working on a single image and managing detected faces
			/// </summary>
			public ref class FaceTrackerContext
			{
			public:
				FaceTrackerContext();
				~FaceTrackerContext();

				//property array<int>^ ResultBuffer { array<int>^ get(); };
				//property array<unsigned char>^ WorkBuffer { array<unsigned char>^ get(); };
				property array<DetectedFace^>^ Faces { array<DetectedFace^>^ get(); };
				property int FaceCount { int get(); };

				float Scale;
				int MinNeighbors;
				int MinObjectWidth;
				int MaxObjectWidth;
				int DoLandmarks;

				/// <summary>
				/// Call this every frame to track faces with this technique
				/// </summary>
				void DetectFrontal(array<unsigned char>^ image, int width, int height);
				/// <summary>
				/// Call this every frame to track faces with this technique
				/// </summary>
				void DetectFrontalSurveillance(array<unsigned char>^ image, int width, int height);
				/// <summary>
				/// Call this every frame to track faces with this technique
				/// </summary>
				void DetectMultiView(array<unsigned char>^ image, int width, int height);
				/// <summary>
				/// Call this every frame to track faces with this technique
				/// </summary>
				void DetectMultiViewReinforce(array<unsigned char>^ image, int width, int height);

			private:

				//array<int>^ _resultBuffer;
				array<unsigned char>^ _workBuffer;
				array<DetectedFace^>^ _faces;
				int _faceCount;

				void handleResult(int * pResult);

				[DllImport("libfacedetect-PLATFORM.dll", EntryPoint = "?facedetect_frontal@@YAPEAHPEAE0HHHMHHHH@Z")]
				static int * facedetect_frontal(unsigned char * result_buffer, //buffer memory for storing face detection results, !!its size must be 0x20000 Bytes!!
					unsigned char * gray_image_data, int width, int height, int step, //input image, it must be gray (single-channel) image!
					float scale, //scale factor for scan windows
					int min_neighbors, //how many neighbors each candidate rectangle should have to retain it
					int min_object_width, //Minimum possible face size. Faces smaller than that are ignored.
					int max_object_width, //Maximum possible face size. Faces larger than that are ignored. It is the largest posible when max_object_width=0.
					int doLandmark); // landmark detection

				[DllImport("libfacedetect-PLATFORM.dll", EntryPoint = "?facedetect_frontal_surveillance@@YAPEAHPEAE0HHHMHHHH@Z")]
				static int * facedetect_frontal_surveillance(unsigned char * result_buffer, //buffer memory for storing face detection results, !!its size must be 0x20000 Bytes!!
					unsigned char * gray_image_data, int width, int height, int step, //input image, it must be gray (single-channel) image!
					float scale, //scale factor for scan windows
					int min_neighbors, //how many neighbors each candidate rectangle should have to retain it
					int min_object_width, //Minimum possible face size. Faces smaller than that are ignored.
					int max_object_width, //Maximum possible face size. Faces larger than that are ignored. It is the largest posible when max_object_width=0.
					int doLandmark); // landmark detection

				[DllImport("libfacedetect-PLATFORM.dll", EntryPoint = "?facedetect_multiview@@YAPEAHPEAE0HHHMHHHH@Z")]
				static int * facedetect_multiview(unsigned char * result_buffer, //buffer memory for storing face detection results, !!its size must be 0x20000 Bytes!!
					unsigned char * gray_image_data, int width, int height, int step, //input image, it must be gray (single-channel) image!
					float scale, //scale factor for scan windows
					int min_neighbors, //how many neighbors each candidate rectangle should have to retain it
					int min_object_width, //Minimum possible face size. Faces smaller than that are ignored.
					int max_object_width, //Maximum possible face size. Faces larger than that are ignored. It is the largest posible when max_object_width=0.
					int doLandmark); // landmark detection

				[DllImport("libfacedetect-PLATFORM.dll", EntryPoint = "?facedetect_multiview_reinforce@@YAPEAHPEAE0HHHMHHHH@Z")]
				static int * facedetect_multiview_reinforce(unsigned char * result_buffer, //buffer memory for storing face detection results, !!its size must be 0x20000 Bytes!!
					unsigned char * gray_image_data, int width, int height, int step, //input image, it must be gray (single-channel) image!
					float scale, //scale factor for scan windows
					int min_neighbors, //how many neighbors each candidate rectangle should have to retain it
					int min_object_width, //Minimum possible face size. Faces smaller than that are ignored.
					int max_object_width, //Maximum possible face size. Faces larger than that are ignored. It is the largest posible when max_object_width=0.
					int doLandmark); // landmark detection
			};
		}
	}
}