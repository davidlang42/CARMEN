using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CarmenUI.Windows
{
    /// <summary>
    /// Interaction logic for LoadingOverlay.xaml
    /// </summary>
    public partial class LoadingOverlay : Window, IDisposable
    {
        public static readonly DependencyProperty MainTextProperty = DependencyProperty.Register(
            nameof(MainText), typeof(string), typeof(LoadingOverlay), new PropertyMetadata("Loading..."));

        private IntPtr ownerHwnd;
        private LoadingSegment? mainSegment;

        public string MainText
        {
            get => (string)GetValue(MainTextProperty);
            set => SetValue(MainTextProperty, value);
        }

        public static readonly DependencyProperty SubTextProperty = DependencyProperty.Register(
            nameof(SubText), typeof(string), typeof(LoadingOverlay), new PropertyMetadata(null));

        public string? SubText
        {
            get => (string?)GetValue(SubTextProperty);
            set
            {
                SetValue(SubTextProperty, value);
                SubTextElement.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
            nameof(Progress), typeof(int?), typeof(LoadingOverlay), new PropertyMetadata(null));

        public int? Progress
        {
            get => (int?)GetValue(ProgressProperty);
            set
            {
                SetValue(ProgressProperty, value);
                ProgressElement.Visibility = value == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public LoadingOverlay(Window owner)
        {
            Owner = owner;
            ownerHwnd = new WindowInteropHelper(owner).Handle;
            if (ownerHwnd == IntPtr.Zero)
                throw new InvalidOperationException("Could not get window handle, wait until window is fully initialised.");
            owner.IsEnabled = false;
            WinUser.EnableWindow(ownerHwnd, false);
            DataContext = this;
            InitializeComponent();
            Show();
        }

        public LoadingOverlay(Page page) : this(GetWindow(page))
        { }

        public void Dispose()
        {
            WinUser.EnableWindow(ownerHwnd, true);
            Owner.IsEnabled = true;
            Hide();
        }

        /// <summary>Creates a main LoadingSegment from which sub-segments can be specified.
        /// Disposing this segment will also dispose the LoadingOverlay.</summary>
        public LoadingSegment AsSegment(string segment_key, string? main_text = null)
        {
            if (mainSegment != null)
                throw new ArgumentException("LoadingOverlay has already registered a main LoadingSegment.");
            if (main_text != null)
                MainText = main_text;
            return mainSegment = new LoadingSegment(SetProgress, s => SubText = s, segment_key);
        }

        private void SetProgress(int percentage, bool complete)
        {
            if (complete)
            {
                Progress = 100;
                Dispose();
            }
            else
            {
                Progress = Math.Min(percentage, 99);
            }
        }
    }

    public class LoadingSegment : IDisposable
    {
        public delegate void UpdateSegmentDelegate(int segment_percentage, bool complete);
        public delegate void UpdateSubTextDelegate(string sub_text);

        bool disposed = false;
        UpdateSegmentDelegate updateSegment;
        UpdateSubTextDelegate updateSubText;
        string segmentKey;
        DateTime startTime;

        HashSet<string> subSegments = new();
        LoadingSegment? currentSubSegment;
        int previousSubSegmentsPercentage = 0;
        int currentSubSegmentPercentage;
        DateTime? lastSubSegmentFinished;

        public LoadingSegment(UpdateSegmentDelegate update_segment, UpdateSubTextDelegate update_sub_text, string segment_key)
        {
            this.updateSegment = update_segment;
            this.updateSubText = update_sub_text;
            this.segmentKey = segment_key;
            this.startTime = DateTime.Now;
            SmoothProgressAsync();
        }

        private async void SmoothProgressAsync()
        {
            const int UPDATE_DELAY = 50;
            if (FindSegmentTime(segmentKey) is not int segment_time)
                return;
            while (true)
            {
                await Task.Delay(UPDATE_DELAY);
                if (disposed || currentSubSegment != null)
                    break;
                var estimated_percentage = (int)((DateTime.Now - startTime).TotalMilliseconds * 100 / segment_time);
                updateSegment(estimated_percentage, false);
                if (estimated_percentage >= 100)
                    break;
            }
        }

        public LoadingSegment Segment(string sub_segment_key, string sub_text)
        {
            if (currentSubSegment != null)
                throw new ArgumentException("LoadingSegment already has a sub-segment running.");
            if (!subSegments.Add(sub_segment_key))
                throw new ArgumentException($"Sub-segment '{sub_segment_key}' has already run in this '{segmentKey}' segment.");
            updateSubText(sub_text);
            currentSubSegmentPercentage = FindSubSegmentPercentage(segmentKey, sub_segment_key)
                ?? (100 - previousSubSegmentsPercentage) / 2; // fallback for the first time this is run
#if DEBUG
            if (lastSubSegmentFinished.HasValue)
            {
                var time_between_segments = (DateTime.Now - lastSubSegmentFinished.Value).TotalMilliseconds;
                if (time_between_segments > 50)
                    MessageBox.Show($"Within {segmentKey}, there was {time_between_segments}ms between the last sub-segment and {sub_segment_key}.");
            }
#endif
            return currentSubSegment = new LoadingSegment(SetSubProgress, updateSubText, sub_segment_key);
        }

        protected virtual void SetSubProgress(int segment_percentage, bool complete)
        {
            if (currentSubSegment == null)
                throw new ArgumentException("UpdateSubSegment was called with no sub-segment running.");
            if (complete)
            {
                currentSubSegment = null;
                previousSubSegmentsPercentage = Math.Min(previousSubSegmentsPercentage + currentSubSegmentPercentage, 99);
                updateSegment(previousSubSegmentsPercentage, false);
                updateSubText("");
                lastSubSegmentFinished = DateTime.Now;
            }
            else
            {
                updateSegment(previousSubSegmentsPercentage + Math.Min(segment_percentage, 99) * currentSubSegmentPercentage / 100, false);
            }
        }

        private void Finish()
        {
            var duration = DateTime.Now - startTime;
            SaveResult(segmentKey, (int)duration.TotalMilliseconds, subSegments);
            updateSegment(100, true);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Finish();
            }
        }

        private static void SaveResult(string segment_key, int total_milliseconds, IEnumerable<string> sub_segment_keys)
        {
#if DEBUG
            if (Properties.Timings.Default.SubSegments.TryGetValue(segment_key, out var previous_subs))
            {
                var previous_in_order = previous_subs.OrderBy(k => k).ToArray();
                var current_in_order = sub_segment_keys.OrderBy(k => k).ToArray();
                if (!previous_in_order.SequenceEqual(current_in_order)
                    && MessageBox.Show($"{segment_key} previously contained sub-segments: {string.Join(", ", previous_in_order)}\n but this time contained: {string.Join(", ", current_in_order)}\nWould you like to clear the timings cache?", "DEBUG", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    Properties.Timings.Default.TotalTime.Clear();
                    Properties.Timings.Default.SubSegments.Clear();
                }
            }
#endif
            Properties.Timings.Default.TotalTime[segment_key] = total_milliseconds;
            Properties.Timings.Default.SubSegments[segment_key] = sub_segment_keys.ToArray();
        }

        private static int? FindSubSegmentPercentage(string segment_key, string this_sub_segment_key)
        {
            if (!Properties.Timings.Default.SubSegments.TryGetValue(segment_key, out var sub_segment_keys))
                return null;
            if (!Properties.Timings.Default.TotalTime.TryGetValue(this_sub_segment_key, out var this_sub_segment_time))
                return null;
            int sub_segments_total = 0;
            foreach (var sub_segment_key in sub_segment_keys)
            {
                if (!Properties.Timings.Default.TotalTime.TryGetValue(sub_segment_key, out var sub_segment_time))
                    return null;
                sub_segments_total += sub_segment_time;
            }
            if (sub_segments_total == 0 || this_sub_segment_time > sub_segments_total)
                return null;
            return this_sub_segment_time * 100 / sub_segments_total;
        }

        private static int? FindSegmentTime(string segment_key)
            => Properties.Timings.Default.TotalTime.TryGetValue(segment_key, out var segment_time) ? segment_time : null;
    }
}
