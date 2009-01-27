//-----------------------------------------------------------------------
// <copyright file="SplineInterpolation.cs" company="Math.NET Project">
//    Copyright (c) 2002-2009, Christoph R�egg.
//    All Right Reserved.
// </copyright>
// <author>
//    Christoph R�egg, http://christoph.ruegg.name
// </author>
// <product>
//    Math.NET Iridium, part of the Math.NET Project.
//    http://mathnet.opensourcedotnet.info
// </product>
// <license type="opensource" name="LGPL" version="2 or later">
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU Lesser General Public License as published 
//    by the Free Software Foundation; either version 2 of the License, or
//    any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public 
//    License along with this program; if not, write to the Free Software
//    Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
// </license>
// <contribution>
//    Numerical Recipes in C++, Second Edition [2003]
//    Handbook of Mathematical Functions [1965]
//    ALGLIB, Sergey Bochkanov
// </contribution>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace MathNet.Numerics.Interpolation.Algorithms
{
    /// <summary>
    /// Third-Degree Spline Interpolation Algorithm.
    /// </summary>
    /// <remarks>
    /// This algorithm supports both differentiation and interation.
    /// </remarks>
    public class SplineInterpolation :
        IInterpolationMethod
    {
        IList<double> _t;
        IList<double> _c;
        int _n;

        /// <summary>
        /// Initializes a new instance of the SplineInterpolation class.
        /// </summary>
        public
        SplineInterpolation()
        {
        }

        /// <summary>
        /// True if the alorithm supports differentiation.
        /// </summary>
        /// <seealso cref="Differentiate"/>
        public bool SupportsDifferentiation
        {
            get { return true; }
        }

        /// <summary>
        /// True if the alorithm supports integration.
        /// </summary>
        /// <seealso cref="Integrate"/>
        public bool SupportsIntegration
        {
            get { return true; }
        }

        /// <summary>
        /// Initialize the interpolation method with the given spline coefficients.
        /// </summary>
        /// <param name="t">Points t (length: N)</param>
        /// <param name="c">Spline Coefficients (length: 4*(N-1))</param>
        public
        void
        Init(
            IList<double> t,
            IList<double> c)
        {
            if(null == t)
            {
                throw new ArgumentNullException("t");
            }

            if(null == c)
            {
                throw new ArgumentNullException("c");
            }

            if(t.Count < 1)
            {
                throw new ArgumentOutOfRangeException("t");
            }

            if(c.Count != 4 * (t.Count - 1))
            {
                throw new ArgumentOutOfRangeException("c");
            }

            _t = t;
            _c = c;
            _n = t.Count;
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public
        double
        Interpolate(double t)
        {
            // Binary search in the [ t[0], ..., t[n-2] ] (t[n-1] is not included)
            int low = 0;
            int high = _n - 1;
            while(low != high - 1)
            {
                int middle = (low + high) / 2;
                if(_t[middle] > t)
                {
                    high = middle;
                }
                else
                {
                    low = middle;
                }
            }

            // Interpolation
            t = t - _t[low];
            int k = low << 2;
            return _c[k] + (t * (_c[k + 1] + (t * (_c[k + 2] + (t * _c[k + 3])))));
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <param name="first">Interpolated first derivative at point t.</param>
        /// <param name="second">Interpolated second derivative at point t.</param>
        /// <returns>Interpolated value x(t).</returns>
        public
        double
        Differentiate(
            double t,
            out double first,
            out double second)
        {
            // Binary search in the [ t[0], ..., t[n-2] ] (t[n-1] is not included)
            int low = 0;
            int high = _n - 1;
            while(low != high - 1)
            {
                int middle = (low + high) / 2;
                if(_t[middle] > t)
                {
                    high = middle;
                }
                else
                {
                    low = middle;
                }
            }

            // Differentiation
            t = t - _t[low];
            int k = low << 2;
            first = _c[k + 1] + (2 * t * _c[k + 2]) + (3 * t * t * _c[k + 3]);
            second = (2 * _c[k + 2]) + (6 * t * _c[k + 3]);
            return _c[k] + (t * (_c[k + 1] + (t * (_c[k + 2] + (t * _c[k + 3])))));
        }

        /// <summary>
        /// Definite Integrate up to point t.
        /// </summary>
        /// <param name="t">Right bound of the integration interval [a,t].</param>
        /// <returns>Interpolated definite integeral over the interval [a,t].</returns>
        /// <seealso cref="SupportsIntegration"/>
        public
        double
        Integrate(double t)
        {
            // Binary search in the [ t[0], ..., t[n-2] ] (t[n-1] is not included)
            int low = 0;
            int high = _n - 1;
            while(low != high - 1)
            {
                int middle = (low + high) / 2;
                if(_t[middle] > t)
                {
                    high = middle;
                }
                else
                {
                    low = middle;
                }
            }

            // Integration
            double result = 0;
            for(int i = 0, j = 0; i < low; i++, j += 4)
            {
                double w = _t[i + 1] - _t[i];
                result += w * (_c[j] + ((w * (_c[j + 1] * 0.5)) + (w * ((_c[j + 2] / 3) + (w * _c[j + 3] * 0.25)))));
            }

            t = t - _t[low];
            int k = low << 2;
            return result + (t * (_c[k] + ((t * (_c[k + 1] * 0.5)) + (t * (_c[k + 2] / 3)) + (t * _c[k + 3] * 0.25))));
        }
    }
}
