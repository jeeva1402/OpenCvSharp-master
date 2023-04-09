﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using OpenCvSharp.XImgProc;

namespace OpenCvSharp.Tests.XImgProc
{
    [TestFixture]
    public class XImgProcTest : TestBase
    {
        [Test]
        public void Thinning()
        {
            using (var src = Image("blob/shapes2.png", ImreadModes.GrayScale))
            using (var dst = new Mat())
            {
                CvXImgProc.Thinning(src, dst, ThinningTypes.ZHANGSUEN);
                ShowImagesWhenDebugMode(dst);
            }
        }

        [Test]
        public void Niblack()
        {
            using (var src = Image("lenna.png", ImreadModes.GrayScale))
            using (var dst = new Mat())
            {
                CvXImgProc.NiblackThreshold(src, dst, 255, ThresholdTypes.Binary, 5, 0.5);
                ShowImagesWhenDebugMode(dst);
            }
        }

        [Test]
        public void WeightedMedianFilter()
        {
            using (var src = Image("lenna.png", ImreadModes.GrayScale))
            using (var dst = new Mat())
            {
                CvXImgProc.WeightedMedianFilter(src, src, dst, 7);
                ShowImagesWhenDebugMode(dst);
            }
        }

        [Test]
        public void CovarianceEstimation()
        {
            const int windowSize = 7;
            using (var src = Image("lenna.png", ImreadModes.GrayScale))
            using (var dst = new Mat())
            {
                CvXImgProc.CovarianceEstimation(src, dst, windowSize, windowSize);
                // TODO
                Assert.That(dst.Rows, Is.EqualTo(windowSize * windowSize));
                Assert.That(dst.Cols, Is.EqualTo(windowSize * windowSize));
                Assert.That(dst.Type(), Is.EqualTo(MatType.CV_32FC2));
            }
        }
    }
}

