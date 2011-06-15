﻿//-----------------------------------------------------------------------
// <copyright file="NodalDoublesCurve.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Fudge;
using Fudge.Serialization;

namespace OGDotNet.Mappedtypes.math.curve
{
    public class NodalDoublesCurve : Curve
    {
        private readonly double[] _xData;
        private readonly double[] _yData;

        public NodalDoublesCurve(string name, double[] xData, double[] yData)
            : base(name)
        {
            if (xData == null) throw new ArgumentNullException("xData");
            if (!xData.Any()) throw new ArgumentNullException("xData");
            if (yData == null) throw new ArgumentNullException("yData");
            if (!yData.Any()) throw new ArgumentNullException("yData");

            if (xData.Length != yData.Length)
            {
                throw new ArgumentException("Graph is not square");
            }

            _xData = xData;
            _yData = yData;
        }

        public override IList<double> XData
        {
            get { return _xData; }
        }
        public override IList<double> YData
        {
            get { return _yData; }
        }

        public override bool IsVirtual
        {
            get { return false; }
        }

        public int Size
        {
            get
            {
                return _xData.Length;
            }
        }

        public override double GetYValue(double x)
        {
            var index = Array.BinarySearch(_xData, x);
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("x", "Curve does not contain data for x point " + x);
            }

            return YData[index];
        }

        public static NodalDoublesCurve FromFudgeMsg(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            string name = GetName(ffc);
            var xData = ffc.GetValue<double[]>("x data");
            var yData = ffc.GetValue<double[]>("y data");

            return new NodalDoublesCurve(name, xData, yData);
        }

        public void ToFudgeMsg(IAppendingFudgeFieldContainer a, IFudgeSerializer s)
        {
            throw new NotImplementedException();
        }
    }
}