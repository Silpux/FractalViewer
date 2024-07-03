using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractalViewer.Classes
{
    public class Complex {
        public double Real { get; set; }
        public double Imaginary { get; set; }

        public Complex(double real, double imaginary) {
            Real = real;
            Imaginary = imaginary;
        }

        public static Complex operator +(Complex a, Complex b) {
            return new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
        }
        public static Complex operator -(Complex a, Complex b) {
            return new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
        }
        public static Complex operator *(Complex a, Complex b) {
            return new Complex(a.Real * b.Real - a.Imaginary * b.Imaginary, a.Real * b.Imaginary + a.Imaginary * b.Real);
        }

        public static Complex Cos(Complex a) {
            return new Complex(Math.Cos(a.Real) * Math.Cosh(a.Imaginary), -Math.Sin(a.Real) * Math.Sinh(a.Imaginary));
        }

        public static Complex Sin(Complex a) {
            return new Complex(Math.Sin(a.Real) * Math.Cosh(a.Imaginary), Math.Cos(a.Real) * Math.Sinh(a.Imaginary));
        }

        public double Magnitude {
            get { return Math.Sqrt(Real * Real + Imaginary * Imaginary); }
        }

    }
}
