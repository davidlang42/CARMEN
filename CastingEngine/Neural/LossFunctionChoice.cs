﻿using Carmen.CastingEngine.Neural.Internal;
using System;

namespace Carmen.CastingEngine.Neural
{
    public enum LossFunctionChoice
    {
        MeanSquaredError,
        Classification0_5,
        Classification0_4,
        Classification0_3,
        Classification0_2,
        Classification0_1
    }

    public static class LossFunctionChoiceExtensions
    {
        public static ILossFunction Create(this LossFunctionChoice choice)
            => choice switch
            {
                LossFunctionChoice.MeanSquaredError => new MeanSquaredError(),
                LossFunctionChoice.Classification0_5 => new ClassificationError { Threshold = 0.5 },
                LossFunctionChoice.Classification0_4 => new ClassificationError { Threshold = 0.4 },
                LossFunctionChoice.Classification0_3 => new ClassificationError { Threshold = 0.3 },
                LossFunctionChoice.Classification0_2 => new ClassificationError { Threshold = 0.2 },
                LossFunctionChoice.Classification0_1 => new ClassificationError { Threshold = 0.1 },
                _ => throw new NotImplementedException($"Enum not implemented: {choice}")
            };
    }
}
