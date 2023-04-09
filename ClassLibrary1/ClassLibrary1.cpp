#include "pch.h"

#include "ClassLibrary1.h"
void ClassLibrary1::Class1::CreateEigenFaceRecognizer(int numComponents, double threshold)
{
    this->LBPHFaceRec_ = face_createEigenFaceRecognizer(0, 0.0);
}
