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

        public LoadingSegment(UpdateSegmentDelegate update_segment, UpdateSubTextDelegate update_sub_text, string segment_key)
        {
            this.updateSegment = update_segment;
            this.updateSubText = update_sub_text;
            this.segmentKey = segment_key;
            this.startTime = DateTime.Now;
        }

        public LoadingSegment Segment(string sub_segment_key, string? sub_text = null)
        {
            if (currentSubSegment != null)
                throw new ArgumentException("LoadingSegment already has a sub-segment running.");
            if (sub_text != null)
                updateSubText(sub_text);
            if (!subSegments.Add(sub_segment_key))
                throw new ArgumentException($"Sub-segment '{sub_segment_key}' has already run in this '{segmentKey}' segment.");
            currentSubSegmentPercentage = (100 - previousSubSegmentsPercentage) / 2; //TODO use previous sub-segments to calc this
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
            }
            else
            {
                updateSegment(previousSubSegmentsPercentage + Math.Min(segment_percentage, 99) * currentSubSegmentPercentage / 100, false);
            }
        }

        private void Finish()
        {
            var duration = DateTime.Now - startTime;
            updateSegment(100, true);
            SaveResult(segmentKey, (int)duration.TotalMilliseconds, subSegments);
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
            Properties.Timings.Default.TotalTime[segment_key] = total_milliseconds;
            Properties.Timings.Default.SubSegments[segment_key] = sub_segment_keys.ToArray();
        }
    }
}
