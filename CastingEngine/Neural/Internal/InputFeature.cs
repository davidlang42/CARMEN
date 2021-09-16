using System;

namespace Carmen.CastingEngine.Neural.Internal
{
    //LATER remove this whole file

    /// <summary>
    /// Represents a set of features which are the input to a neural network,
    /// mapped from a domain specific object of type <typeparamref name="T"/>.
    /// </summary>
    public struct InputFeatureMap<T>
    {
        public Func<T, double>[] GetValues;
        public Func<T, string>[] GetNames;
    }

    public struct InputFeature<T>
    {
        public Func<T, double> GetValue;
        public Func<T, string> GetName;
    }

    public struct OutputFeature<T>
    {
        public Action<T, double> SetValue;
        public Func<T, double> GetValue;
        public Func<T, string> GetName;
    }

    //public class InputFeature
    //{
    //    public double Calculate(double input) => input * Weight;
    //    public double Weight { get; set; }
    //}

    //public abstract class InputFeatureSet
    //{
    //    public abstract string[] GetNames();
    //    public abstract double[] GetValues();
    //    public static int GetCount<T>()
    //        where T : InputFeatureSet, new()
    //    {
    //        var blank = new T();
    //        return blank.GetNames().Length;
    //    }
    //}

    public interface IInputFeatureSet
    {
        /// <summary>The number of elements in this feature set.
        /// Must be a fixed value for any instance of this type.</summary>
        public int FixedSize { get; }

        /// <summary>Get a static array containing names for each value in a feature
        /// set of this type. Length must be <see cref="FixedSize"/>.</summary>
        public string[] GetNames();

        /// <summary>Get an array containing the values for this instance.
        /// Length must be <see cref="FixedSize"/>.</summary>
        public double[] GetValues();

        public static int GetSize<T>()
            where T : struct, IInputFeatureSet
        {
            var blank = default(T);
            return blank.FixedSize;
        }
    }

    public interface IOutputFeatureSet : IInputFeatureSet
    {
        /// <summary>Sets this instance's fields to those in an array of length <see cref="FixedSize"/>.</summary>
        public void SetValues(double[] values);
    }

    //public record Blah
    //{

    //}

    //public class Test : InputFeatureSet
    //{
    //    public override string[] GetNames() => throw new System.NotImplementedException();
    //    public override double[] GetValues() => throw new System.NotImplementedException();
    //}

    //public struct TestStruct : IInputFeatureSet
    //{
    //    public int Size { get; init; }

    //    string[] IInputFeatureSet.GetNames() => throw new NotImplementedException();
    //    double[] IInputFeatureSet.GetValues() => throw new NotImplementedException();
    //}
}
