/*
 *  Created by Dragutin Sredojevic.
 *  https://www.nitugard.com
 */

using System;
using Unity.Mathematics;

namespace ibc.solvers
{

    public struct Interval
    {
        public readonly double Low, High;

        public Interval(double low, double high)
        {
            Low = low;
            High = high;
        }

        public static Interval Zero => new Interval(0, 0);
    }
    
    public struct RootResult
    {
        public double Root1, Root2, Root3, Root4;
        public int Count;

        public RootResult(double root1)
        {
            Root1 = root1;
            Count = 1;
            Root2 = 0;
            Root3 = 0;
            Root4 = 0;
        }
        

        public void AddRootModify(double root)
        {
            if (Count == 0) Root1 = root;
            else if (Count == 1) Root2 = root;
            else if (Count == 2) Root3 = root;
            else if (Count == 3) Root4 = root;
            Count++;
        }
        
        public RootResult AddRoot(double root)
        {
            RootResult rootResult = this;
            rootResult.AddRootModify(root);
            return rootResult;
        }
        
        public RootResult Sort()
        {
            if (Count < 2) return this;

            for (int i = 0; i < Count - 1; i++)
            {
                bool swapped = false;
                if (Count > 1 && Root1 > Root2) { Swap(ref Root1, ref Root2); swapped = true; }
                if (Count > 2 && Root2 > Root3) { Swap(ref Root2, ref Root3); swapped = true; }
                if (Count > 3 && Root3 > Root4) { Swap(ref Root3, ref Root4); swapped = true; }

                if (!swapped) break;
            }

            return this;
        }


        private static void Swap(ref double a, ref double b)
        {
            (a, b) = (b, a);
        }
        
        public RootResult TrimRoots(double lo, double hi)
        {
            RootResult trimmed = new RootResult();
        
            if (Count > 0 && Root1 >= lo && Root1 <= hi) trimmed.AddRootModify(Root1);
            if (Count > 1 && Root2 >= lo && Root2 <= hi) trimmed.AddRootModify(Root2);
            if (Count > 2 && Root3 >= lo && Root3 <= hi) trimmed.AddRootModify(Root3);
            if (Count > 3 && Root4 >= lo && Root4 <= hi) trimmed.AddRootModify(Root4);

            return trimmed;
        }
        
        public double this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) throw new IndexOutOfRangeException("Invalid root index.");
                switch (index)
                {
                    case 0: return Root1;
                    case 1: return Root2;
                    case 2: return Root3;
                    case 3: return Root4;
                    default: throw new IndexOutOfRangeException("Invalid root index.");
                }
            }
        }
    }

    public struct Poly2
    {
        public readonly double c2, c1, c0;

        public Poly2(double c2, double c1, double c0)
        {
            this.c2 = c2;
            this.c1 = c1;
            this.c0 = c0;
        }

        public readonly double Evaluate(double x)
        {
            return (c2 * x + c1) * x + c0;
        }

        public readonly RootResult CalculateRoots(double upperBound)
        {
            var root = new RootResult();

            if (math.abs(c2) <= math.EPSILON_DBL)
            {
                if (math.abs(c1) <= math.EPSILON_DBL) return default;

                root.AddRootModify(-c0 / c1);
                return root;
            }

            double D = c1 * c1 - 4 * c2 * c0;
            if (D < 0) return default;
            double r1 = -(c1 + math.sign(c1) * math.sqrt(D)) / (2 * c2);
            double r2 = c0 / (c2 * r1);
            root.AddRootModify(r1);
            root.AddRootModify(r2);
            return root;
        }
    }

    public struct Poly4
    {
        public readonly double c4, c3, c2, c1, c0;

        public Poly4(double c4, double c3, double c2, double c1, double c0)
        {
            this.c4 = c4;
            this.c3 = c3;
            this.c2 = c2;
            this.c1 = c1;
            this.c0 = c0;
        }

        public readonly double Evaluate(double x)
        {
            return (((c4 * x + c3) * x + c2) * x + c1) * x + c0;
        }

        public RootResult CalculateRoots(double upperBound, double errorBound)
        {
            if (math.abs(c4) <= math.EPSILON_DBL && math.abs(c3) <= math.EPSILON_DBL)
                return new Poly2(c2, c1, c0).CalculateRoots(0);
            return Poly4Solver.CalculateRoots(upperBound, this, errorBound);
        }
    }

    public struct Poly4Solver
    {
        private static int CheckInterval(double a, double b, in Poly4 poly)
        {
            double a2 = a * a;
            double a3 = a * a2;
            double a4 = a * a3;

            double b2 = b * b;
            double b3 = b * b2;
            double b4 = b * b3;

            double s4 = poly.c4 * a4 + poly.c3 * a3 + poly.c2 * a2 + poly.c1 * a + poly.c0;
            double s3 = 4 * poly.c0 + 3 * a * poly.c1 + b * poly.c1 + 2 * a2 * poly.c2 + a3 * poly.c3 +
                        2 * a * b * poly.c2 +
                        3 * a2 * b * poly.c3 +
                        4 * a3 * b * poly.c4;
            double s2 = 6 * poly.c4 * a2 * b2 + 3 * poly.c3 * a2 * b + poly.c2 * a2 + 3 * poly.c3 * a * b2 +
                        4 * poly.c2 * a * b +
                        3 * poly.c1 * a +
                        poly.c2 * b2 + 3 * poly.c1 * b + 6 * poly.c0;
            double s1 = 4 * poly.c0 + a * poly.c1 + 3 * b * poly.c1 + 2 * b2 * poly.c2 + b3 * poly.c3 +
                        2 * a * b * poly.c2 +
                        3 * a * b2 * poly.c3 +
                        4 * a * b3 * poly.c4;
            double s0 = (poly.c4 * b4 + poly.c3 * b3 + poly.c2 * b2 + poly.c1 * b + poly.c0);

            int v = 0;
            if (s0 > 0 != s1 > 0) v++;
            if (s1 > 0 != s2 > 0) v++;
            if (s2 > 0 != s3 > 0) v++;
            if (s3 > 0 != s4 > 0) v++;
            return v;
        }

        private static Interval Subdivide(double hi, double lo, in Poly4 poly, out int v, double errorBound)
        {
            v = CheckInterval(lo, hi, in poly);
            if (v == 1) return new Interval(lo, hi);
            if (v == 0) return Interval.Zero;
            if (math.abs(hi - lo) < 1E-20) 
                return new Interval(lo, hi);

            Interval i1 = Subdivide((hi + lo) / 2.0, lo, in poly, out var v1, errorBound);
            if (v1 == 1) { v = v1; return i1; }
            Interval i2 = Subdivide(hi, (hi + lo) / 2, in poly, out var v2, errorBound);
            if (v2 == 1) { v = v2; return i2; }
            v = 0;
            return Interval.Zero;
        }

        private static double Bisect(double hi, double lo, int signHi, int signLow, in Poly4 poly, double errorBound)
        {
            double mid = (hi + lo) * 0.5;
            if (math.abs(hi - lo) * 0.5 < errorBound) return mid;
            double midValue = poly.Evaluate(mid);
            if (math.abs(midValue) < 1E-20) return mid;
            int midSign = (int)math.sign(midValue);
            if (signHi != midSign) return Bisect(hi, mid, signHi, midSign, poly, errorBound);
            if (signLow != midSign) return Bisect(mid, lo, midSign, signLow, poly, errorBound);
            return mid;
        }

        public static RootResult CalculateRoots(double hi, Poly4 poly, double errorBound = 1E-5)
        {
            RootResult result = new RootResult();
            if (hi <= 0)
                return result;

            double lowerBound = 0;
            for(var j=0; j<4; ++j)
            {
                Interval i = Subdivide(hi, lowerBound, in poly, out var v, errorBound);
                if (v == 0) break; 

                var signHi = (int)math.sign(poly.Evaluate(i.High));
                var signLow = (int)math.sign(poly.Evaluate(i.Low));
                double root = Bisect(i.High, i.Low, signHi, signLow, in poly, errorBound);

                result.AddRootModify(root);
                lowerBound = root + errorBound;
            }

            return result;

        }
    }
}