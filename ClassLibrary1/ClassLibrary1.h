#pragma once
#include <face.h>
# include <include_opencv.h>
#include <opencv2/core/cvstd.hpp>
using namespace System;

namespace ClassLibrary1 {
	static public class Class1
	{
	protected:
		cv::Ptr<BasicFaceRecognizer> *LBPHFaceRec_;
	public:
		void CreateEigenFaceRecognizer(int numComponents, double threshold);
		// TODO: Add your methods for this class here.
	};
}
