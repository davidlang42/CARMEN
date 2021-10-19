using System;

namespace Carmen.CastingEngine.Neural
{
    public enum ActivationFunctionChoice
    {
        Sigmoid,
        Tanh,
        ReLu,
        LeakyReLu,
        ExponentialLu
    }

    public static class ActivationFunctionChoiceExtensions
    {
        public static IVectorActivationFunction Create(this ActivationFunctionChoice choice)
            => choice switch
            {
                ActivationFunctionChoice.Sigmoid => new Sigmoid(),
                ActivationFunctionChoice.Tanh => new Tanh(),
                ActivationFunctionChoice.ReLu => new ReLu(),
                ActivationFunctionChoice.LeakyReLu => new LeakyReLu(),
                ActivationFunctionChoice.ExponentialLu => new ExponentialLu(),
                _ => throw new NotImplementedException($"Enum not implemented: {choice}")
            };
    }
}
