using System;

namespace Carmen.CastingEngine.Neural
{

    /// <summary>
    /// Represents a set of features which are the input to a neural network,
    /// mapped from a domain specific object of type <typeparamref name="T"/>.
    /// </summary>
    public struct InputFeatureMap<T>//TODO rename file?
    {
        Func<T, double>[] GetValues;
        Func<T, string>[] GetNames;
    }

    public struct InputFeature<T>//TODO needed?
    {
        Func<T, double> GetValue;
        Func<T, string> GetName;
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

    //public interface IInputFeatureSet
    //{
    //    public int Size { get; init; }
    //    public string[] GetNames();
    //    public double[] GetValues();
    //    public static int GetCount<T>()
    //        where T : struct, IInputFeatureSet
    //    {
    //        var blank = default(T);
    //        return blank.GetNames().Length;
    //    }
    //}

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
