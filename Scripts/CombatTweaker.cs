using System;
using UnityEngine;

using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace DCT
{
    struct LinearParameters
    {
        public const string Name = "LinearParameters";

        public float Scale;
        public float Base;
    }

    public class CombatTweaker : MonoBehaviour
    {
        const string StrengthDamageModifier = "StrengthDamageModifier";
        const string MaxEncumbrance = "MaxEncumbrance";
        const string HandToHandMinDamage = "HandToHandMinDamage";
        const string HandToHandMaxDamage = "HandToHandMaxDamage";


        static Mod mod;

        LinearParameters strengthDamageModifier;
        LinearParameters maxEncumbrance;
        LinearParameters handToHandMinDamage;
        LinearParameters handToHandMaxDamage;

        [Invoke(DaggerfallWorkshop.Game.StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var modObject = new GameObject(mod.Title);
            modObject.AddComponent<CombatTweaker>();
        }

        void Awake()
        {
            var settings = mod.GetSettings();

            if(IsEnabled(settings, StrengthDamageModifier))
            {
                strengthDamageModifier = GetLinearParameters(settings, StrengthDamageModifier);
                FormulaHelper.RegisterOverride<Func<int, int>>(mod, "DamageModifier", strength => ApplyFlooredLinear(strengthDamageModifier, strength));
            }

            if(IsEnabled(settings, MaxEncumbrance))
            {
                maxEncumbrance = GetLinearParameters(settings, MaxEncumbrance);
                FormulaHelper.RegisterOverride<Func<int, int>>(mod, "MaxEncumbrance", strength => ApplyFlooredLinear(maxEncumbrance, strength));
            }

            if (IsEnabled(settings, HandToHandMinDamage))
            {
                handToHandMinDamage = GetLinearParameters(settings, HandToHandMinDamage);
                FormulaHelper.RegisterOverride<Func<int, int>>(mod, "CalculateHandToHandMinDamage", skill => ApplyTruncatedLinear(handToHandMinDamage, skill));
            }

            if (IsEnabled(settings, HandToHandMaxDamage))
            {
                handToHandMaxDamage = GetLinearParameters(settings, HandToHandMaxDamage);
                FormulaHelper.RegisterOverride<Func<int, int>>(mod, "CalculateHandToHandMaxDamage", skill => ApplyTruncatedLinear(handToHandMaxDamage, skill));
            }
        }
        static private bool IsEnabled(ModSettings settings, string section)
        {
            return settings.GetBool(section, "Enabled");
        }

        static private LinearParameters GetLinearParameters(ModSettings settings, string section)
        {
            var tuple = settings.GetTupleFloat(section, LinearParameters.Name);
            return new LinearParameters { Scale = tuple.First, Base = tuple.Second };
        }

        static private int ApplyTruncatedLinear(LinearParameters linearParams, int value)
        {
            return (int)(linearParams.Scale * value + linearParams.Base);
        }

        static private int ApplyFlooredLinear(LinearParameters linearParams, int value)
        {
            return (int)Mathf.Floor(linearParams.Scale * value + linearParams.Base);
        }
    }
}