using Carmen.CastingEngine.Base;
using Carmen.CastingEngine.Neural.Internal;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public class NeuralApplicantEngine : ApplicantEngine, IComparer<Applicant>
    {
        public delegate bool UserConfirmation(string message);

        const int TRAINING_ITERATIONS = 100000; //TODO is this the right number?
        const double MINIMUM_CHANGE = 0.1;

        SingleLayerPerceptron model;
        Criteria[] criterias;
        UserConfirmation confirm;

        int maxOverallAbility;
        public override int MaxOverallAbility => maxOverallAbility;

        int minOverallAbility;
        public override int MinOverallAbility => minOverallAbility;

        /// Calculate the overall ability of an Applicant as a simple weighted sum of their Abilities</summary>
        public override int OverallAbility(Applicant applicant)
            => Convert.ToInt32(applicant.Abilities.Sum(a => (double)a.Mark / a.Criteria.MaxMark * a.Criteria.Weight));

        public NeuralApplicantEngine(Criteria[] criterias, UserConfirmation confirm)
        {
            this.criterias = criterias;
            this.confirm = confirm;
            this.model = new SingleLayerPerceptron(criterias.Length * 2, 1);
            LoadWeights();
            UpdateRange();
        }

        private void UpdateRange()
        {
            var max = criterias.Select(c => c.Weight).Where(w => w > 0).Sum();
            if (max > int.MaxValue)
                throw new ApplicationException($"Sum of positive Criteria weights cannot exceed {int.MaxValue}: {max}");
            maxOverallAbility = Convert.ToInt32(max);
            var min = criterias.Select(c => c.Weight).Where(w => w < 0).Sum();
            if (min < int.MinValue)
                throw new ApplicationException($"Sum of negative Criteria weights cannot go below {int.MinValue}: {min}");
            minOverallAbility = Convert.ToInt32(min);
            if (minOverallAbility == maxOverallAbility) // == 0
                maxOverallAbility = 1; // to avoid division by zero errors
        }

        private void LoadWeights()
        {
            var neuron = model.Layer.Neurons[0];
            neuron.Bias = 0;
            for (var i = 0; i < criterias.Length; i++)
            {
                neuron.Weights[i] = criterias[i].Weight;
                neuron.Weights[i + criterias.Length] = -criterias[i].Weight;
            }
        }

        public int Compare(Applicant? a, Applicant? b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();
            var a_better_than_b = model.Predict(InputValues(a, b))[0];
            if (a_better_than_b > 0.5)
                return 1; // A > B
            else if (a_better_than_b < 0.5)
                return -1; // A < B
            else // a_better_than_b == 0.5
                return 0; // A == B
        }

        private double[] InputValues(Applicant a, Applicant b)
        {
            var values = new double[criterias.Length * 2];
            for (var i = 0; i < criterias.Length; i++)
            {
                double max_mark = criterias[i].MaxMark;
                values[i] = a.MarkFor(criterias[i]) / max_mark;
                values[i + criterias.Length] = b.MarkFor(criterias[i]) / max_mark;
            }
            return values;
        }

        public override void UserSelectedCast(IEnumerable<Applicant> applicants_accepted, IEnumerable<Applicant> applicants_rejected)
        {
            // Generate training data
            //TODO split this up by cast group
            var rejected_array = applicants_rejected.ToArray();
            if (rejected_array.Length == 0)
                return; //LATER log: cannot train without rejected applicants
            var training_pairs = new Dictionary<double[], double[]>();
            foreach (var accepted_applicant in applicants_accepted)
                foreach (var rejected_applicant in rejected_array)
                {
                    training_pairs.Add(InputValues(accepted_applicant, rejected_applicant), new[] { 1.0 });
                    training_pairs.Add(InputValues(rejected_applicant, accepted_applicant), new[] { 0.0 });
                }
            if (training_pairs.Count == 0)
                return; //LATER log: cannot train without accepted applicants
            // Train the model
            var trainer = new ModelTrainer(model)
            {
                LossThreshold = 0.005,
                MaxIterations = TRAINING_ITERATIONS / training_pairs.Count
            };
            var m = trainer.Train(training_pairs.Keys, training_pairs.Values);
            //LATER log the result
            // Update the criteria weights
            //TODO normalise weights to be out of the same amount as they currently are
            var neuron = model.Layer.Neurons[0];
            var changes = new List<(Criteria, double)>();
            for (var i = 0; i < criterias.Length; i++)
            {
                var new_weight = (neuron.Weights[i] + -neuron.Weights[i + criterias.Length]) / 2;
                if (Math.Abs(new_weight - criterias[i].Weight) > MINIMUM_CHANGE)
                    changes.Add((criterias[i], new_weight));
            }
            if (changes.Any())
            {
                var msg = "CARMEN's neural network has detected an improvement to the Criteria weights. Would you like to update them?\n";
                msg += string.Join("\n", changes.Select(c => $"{c.Item1.Name}: {c.Item2:0.0} (previously {c.Item1.Weight:0.0})")); //TODO show all criteria even if no change
                if (confirm(msg))
                    foreach (var change in changes)
                        change.Item1.Weight = change.Item2;
            }
        }

        //TODO use this code or delete
        //private static void PairwiseExtract(ShowContext context, StreamWriter f, bool polarised)
        //{
        //    var criterias = context.Criterias.InOrder().ToArray();
        //    var cast_groups = context.CastGroups.ToArray();
        //    var accepted_applicants = new HashSet<Applicant>[cast_groups.Length];
        //    var rejected_applicants = new HashSet<Applicant>[cast_groups.Length];
        //    var cast_group_lookup = new Dictionary<CastGroup, int>();
        //    for (var i = 0; i < cast_groups.Length; i++)
        //    {
        //        accepted_applicants[i] = new HashSet<Applicant>();
        //        rejected_applicants[i] = new HashSet<Applicant>();
        //        cast_group_lookup.Add(cast_groups[i], i);
        //    }
        //    var extracted_applicants = 0;
        //    foreach (var applicant in context.Applicants)
        //    {
        //        if (applicant.HasAuditioned(criterias))
        //        {
        //            var cast_group = applicant.CastGroup ?? cast_groups.FirstOrDefault(cg => cg.Requirements.All(r => r.IsSatisfiedBy(applicant)));
        //            if (cast_group != null)
        //            {
        //                var valid = true;
        //                for (var i = 0; i < criterias.Length; i++)
        //                {
        //                    if (criterias[i].MaxMark == 100 && applicant.MarkFor(criterias[i]) < 10)
        //                    {
        //                        Console.WriteLine($"Skipped \"{applicant.FirstName} {applicant.LastName}\" because they had a {criterias[i].Name} mark less than 10");
        //                        valid = false;
        //                        break;
        //                    }
        //                }
        //                if (valid)
        //                {
        //                    var cast_group_index = cast_group_lookup[cast_group];
        //                    if (applicant.IsAccepted)
        //                        accepted_applicants[cast_group_index].Add(applicant);
        //                    else
        //                        rejected_applicants[cast_group_index].Add(applicant);
        //                    extracted_applicants++;
        //                }
        //            }
        //            else
        //                Console.WriteLine($"Skipped \"{applicant.FirstName} {applicant.LastName}\" because they weren't eligible for any cast group");
        //        }
        //        else
        //            Console.WriteLine($"Skipped \"{applicant.FirstName} {applicant.LastName}\" because they haven't auditioned");
        //    }
        //    f.WriteLine($"A_BetterThan_B,CastGroup,{string.Join(",", criterias.Select(c => "A_" + c.Name))},{string.Join(",", criterias.Select(c => "B_" + c.Name))}");
        //    var extracted_pairs = 0;
        //    for (var i = 0; i < cast_groups.Length; i++)
        //    {
        //        Console.WriteLine($"Found {accepted_applicants[i].Count} accepted applicants and {rejected_applicants[i].Count} rejected applicants in '{cast_groups[i].Name}' cast group");
        //        foreach (var accepted_applicant in accepted_applicants[i])
        //            foreach (var rejected_applicant in rejected_applicants[i])
        //            {
        //                f.WriteLine(ComparisonRow(1, cast_groups[i], accepted_applicant, rejected_applicant, criterias));
        //                f.WriteLine(ComparisonRow(polarised ? -1 : 0, cast_groups[i], rejected_applicant, accepted_applicant, criterias));
        //                extracted_pairs++;
        //            }
        //    }
        //    Console.WriteLine($"Extracted {extracted_applicants} applicants forming {extracted_pairs} unique pairs ({extracted_pairs * 2} data rows)");
        //}
    }
}
