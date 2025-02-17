using UnityEngine;
using System;
using System.Collections.Generic;

public static class EasingFunctions {
    // Linear
    public static float Linear(float t) => t;

    // Quadratic
    public static float EaseInQuad(float t) => t * t;
    public static float EaseOutQuad(float t) => t * (2 - t);
    public static float EaseInOutQuad(float t) => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;

    // Cubic
    public static float EaseInCubic(float t) => t * t * t;
    public static float EaseOutCubic(float t) => (--t) * t * t + 1;
    public static float EaseInOutCubic(float t) => t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

    // Quartic
    public static float EaseInQuart(float t) => t * t * t * t;
    public static float EaseOutQuart(float t) => 1 - (--t) * t * t * t;
    public static float EaseInOutQuart(float t) => t < 0.5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;

    // Quintic
    public static float EaseInQuint(float t) => t * t * t * t * t;
    public static float EaseOutQuint(float t) => 1 + (--t) * t * t * t * t;
    public static float EaseInOutQuint(float t) => t < 0.5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;

    // Sine
    public static float EaseInSine(float t) => 1 - Mathf.Cos(t * Mathf.PI / 2);
    public static float EaseOutSine(float t) => Mathf.Sin(t * Mathf.PI / 2);
    public static float EaseInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1) / 2;

    // Exponential
    public static float EaseInExpo(float t) => t == 0 ? 0 : Mathf.Pow(2, 10 * (t - 1));
    public static float EaseOutExpo(float t) => t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
    public static float EaseInOutExpo(float t) =>
        t == 0 ? 0 : t == 1 ? 1 : t < 0.5f ? Mathf.Pow(2, 20 * t - 10) / 2 : (2 - Mathf.Pow(2, -20 * t + 10)) / 2;

    // Circular
    public static float EaseInCirc(float t) => 1 - Mathf.Sqrt(1 - t * t);
    public static float EaseOutCirc(float t) => Mathf.Sqrt(1 - (--t) * t);
    public static float EaseInOutCirc(float t) =>
        t < 0.5f ? (1 - Mathf.Sqrt(1 - 4 * t * t)) / 2 : (Mathf.Sqrt(1 - (2 * t - 2) * (2 * t - 2)) + 1) / 2;

    // Elastic
    public static float EaseInElastic(float t) =>
        t == 0 ? 0 : t == 1 ? 1 : -Mathf.Pow(2, 10 * (t - 1)) * Mathf.Sin((t - 1.1f) * (2 * Mathf.PI) / 0.4f);

    public static float EaseOutElastic(float t) =>
        t == 0 ? 0 : t == 1 ? 1 : Mathf.Pow(2, -10 * t) * Mathf.Sin((t - 0.1f) * (2 * Mathf.PI) / 0.4f) + 1;

    public static float EaseInOutElastic(float t) =>
        t == 0 ? 0 : t == 1 ? 1 : t < 0.5f
        ? -(Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((20 * t - 11.125f) * (2 * Mathf.PI) / 4.5f)) / 2
        : (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((20 * t - 11.125f) * (2 * Mathf.PI) / 4.5f)) / 2 + 1;

    // Bounce
    public static float EaseInBounce(float t) => 1 - EaseOutBounce(1 - t);

    public static float EaseOutBounce(float t) {
        if (t < 1 / 2.75f)
            return 7.5625f * t * t;
        if (t < 2 / 2.75f)
            return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
        if (t < 2.5f / 2.75f)
            return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
        return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
    }

    public static float EaseInOutBounce(float t) =>
        t < 0.5f ? EaseInBounce(t * 2) * 0.5f : EaseOutBounce(t * 2 - 1) * 0.5f + 0.5f;

    // Enum for Easing Function Names
    public enum EasingFunctionName {
        Linear,
        EaseInQuad, EaseOutQuad, EaseInOutQuad,
        EaseInCubic, EaseOutCubic, EaseInOutCubic,
        EaseInQuart, EaseOutQuart, EaseInOutQuart,
        EaseInQuint, EaseOutQuint, EaseInOutQuint,
        EaseInSine, EaseOutSine, EaseInOutSine,
        EaseInExpo, EaseOutExpo, EaseInOutExpo,
        EaseInCirc, EaseOutCirc, EaseInOutCirc,
        EaseInElastic, EaseOutElastic, EaseInOutElastic,
        EaseInBounce, EaseOutBounce, EaseInOutBounce
    }

    // Seems to require .NET 10.0?
    // public static Func<float, float> Function(this EasingFunctionName name) => FromName(name);

    // Mapping from Enum to Easing Function
    private static readonly Dictionary<EasingFunctionName, Func<float, float>> FunctionMap =
        new Dictionary<EasingFunctionName, Func<float, float>> {
            { EasingFunctionName.Linear, Linear },
            { EasingFunctionName.EaseInQuad, EaseInQuad },
            { EasingFunctionName.EaseOutQuad, EaseOutQuad },
            { EasingFunctionName.EaseInOutQuad, EaseInOutQuad },
            { EasingFunctionName.EaseInCubic, EaseInCubic },
            { EasingFunctionName.EaseOutCubic, EaseOutCubic },
            { EasingFunctionName.EaseInOutCubic, EaseInOutCubic },
            { EasingFunctionName.EaseInQuart, EaseInQuart },
            { EasingFunctionName.EaseOutQuart, EaseOutQuart },
            { EasingFunctionName.EaseInOutQuart, EaseInOutQuart },
            { EasingFunctionName.EaseInQuint, EaseInQuint },
            { EasingFunctionName.EaseOutQuint, EaseOutQuint },
            { EasingFunctionName.EaseInOutQuint, EaseInOutQuint },
            { EasingFunctionName.EaseInSine, EaseInSine },
            { EasingFunctionName.EaseOutSine, EaseOutSine },
            { EasingFunctionName.EaseInOutSine, EaseInOutSine },
            { EasingFunctionName.EaseInExpo, EaseInExpo },
            { EasingFunctionName.EaseOutExpo, EaseOutExpo },
            { EasingFunctionName.EaseInOutExpo, EaseInOutExpo },
            { EasingFunctionName.EaseInCirc, EaseInCirc },
            { EasingFunctionName.EaseOutCirc, EaseOutCirc },
            { EasingFunctionName.EaseInOutCirc, EaseInOutCirc },
            { EasingFunctionName.EaseInElastic, EaseInElastic },
            { EasingFunctionName.EaseOutElastic, EaseOutElastic },
            { EasingFunctionName.EaseInOutElastic, EaseInOutElastic },
            { EasingFunctionName.EaseInBounce, EaseInBounce },
            { EasingFunctionName.EaseOutBounce, EaseOutBounce },
            { EasingFunctionName.EaseInOutBounce, EaseInOutBounce }
        };

    // Retrieve Easing Function by Name
    public static Func<float, float> FromName(EasingFunctionName name) {
        if (FunctionMap.TryGetValue(name, out var function))
            return function;

        throw new ArgumentException($"Easing function '{name}' not found.");
    }
}
