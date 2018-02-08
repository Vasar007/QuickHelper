using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using QuickHelper.Hash;

namespace QuickHelper
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields

        /// <summary>
        /// Timer for animating form movement.
        /// </summary>
        private readonly DispatcherTimer _dispatcherTimer;

        /// <summary>
        /// Default movement size of main window.
        /// </summary>
        private const int DEFAULT_MOVEMENT_SIZE = 30;

        /// <summary>
        /// Movement speed of main windwos
        /// </summary>
        private const int MOVEMENT_SPEED = 5;

        /// <summary>
        /// Dynamic movement size to check every time on mouse enter/leave or at the end of 
        /// timer method.
        /// </summary>
        private int _movementSize;

        /// <summary>
        /// Difference between default movement size and current break point.
        /// </summary>
        private int _difference;

        /// <summary>
        /// Counting move phase for timer.
        /// </summary>
        private int _movementCounter;

        #endregion

        #region Private Properties

        /// <summary>
        /// Column counter for dynamically creating buttons.
        /// </summary>
        private int ColumnNumber { set; get; }

        /// <summary>
        /// Row counter for dynamically creating buttons.
        /// </summary>
        private int RowNumber { set; get; }

        /// <summary>
        /// Button counter for dynamically creating buttons. Using in button names and tags.
        /// </summary>
        private int ButtonCounter { set; get; }

        /// <summary>
        /// Boolean flag for handling clipboard changed event after button click.
        /// </summary>
        private bool ButtonClicked { set; get; }

        /// <summary>
        /// Additional boolean flag for handling clipboard changed event after button click.
        /// </summary>
        private bool ButtonClickedTwice { set; get; }

        /// <summary>
        /// Additional helper for creating new buttons.
        /// </summary>
        private bool[] FilledPositions { set; get; }

        /// <summary>
        /// Temporary buffer for defending from keeping key (Ctr + C).
        /// </summary>
        private string TextBuffer { set; get; }

        private string ImageBuffer { set; get; }

        /// <summary>
        /// Boolean flag to track window movement.
        /// </summary>
        private bool HasStartingPosition { set; get; }

        #endregion

        #region Constructor

        /// <inheritdoc />
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Initialize private fields.
            _dispatcherTimer = new DispatcherTimer();
            _movementSize    = DEFAULT_MOVEMENT_SIZE;
            _difference      = default(int);
            _movementCounter = 0;

            // DispatcherTimer setup.
            _dispatcherTimer.Tick    += DispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            _dispatcherTimer.Start();

            // Initialize private properties.
            ColumnNumber        = 0;
            RowNumber           = 1;
            ButtonCounter       = 0;
            ButtonClicked       = false;
            FilledPositions     = new bool[9];
            TextBuffer          = default(string);
            ImageBuffer         = default(string);
            HasStartingPosition = true;
        }

        #endregion

        #region Initialization & Loading

        /// <summary>
        /// Change form position when window loaded.
        /// </summary>
        /// <param name="sender">Window form.</param>
        /// <param name="e">Basic event.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Dynamically set form in screen.
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width;
            Top  = desktopWorkingArea.Bottom - Height * 1.5;
        }

        /// <inheritdoc />
        /// <summary>
        /// Initialize <see cref="T:QuickHelper.ClipboardManager" />.
        /// </summary>
        /// <param name="e">Initialized event.</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Initialize the clipboard now that we have a window soruce to use.
            var windowClipboardManager = new ClipboardManager(this);
            windowClipboardManager.ClipboardChanged += ClipboardChanged;
        }

        #endregion

        #region Clipboard Changed

        /// <summary>
        /// When clipboard changed, handle it and create new elements on form.
        /// </summary>
        /// <param name="sender"><see cref="ClipboardManager"/> class.</param>
        /// <param name="e">Basic <see cref="System.EventArgs"/> class.</param>
        private void ClipboardChanged(object sender, EventArgs e)
        {
            // Check if we handled changing after button click.
            if (ButtonClicked)
            {
                ButtonClicked = false;
                return;
            }
            if (ButtonClickedTwice)
            {
                ButtonClickedTwice = false;
                return;
            }

            if (ToogleButton.IsChecked == false)
            {
                // Change background color for visability because program isn't tracking clipboard.
                foreach (var gridButton in MainGrid.Children.OfType<Button>())
                {
                    gridButton.Background = Brushes.LightGray;
                }
                return;
            }

            // Handle clipboard update here.
            // If user copied text on clipboard.
            if (Clipboard.ContainsText())
            {
                // Get test from clipboard.
                var newText = Clipboard.GetText();

                // Check if user copied exactly same text like before.
                if (TextBuffer == newText)
                    return;

                // Set buffer to check.
                TextBuffer = Clipboard.GetText();

                var dataText = new DataFormat("UnicodeText", 0);
                CreateNewButton(dataText);
            }

            // If user copied image on clipboard.
            if (Clipboard.ContainsImage())
            {
                // Get image from clipboard and transfer it into bytes.
                var buffer   = ImageHash.SaveImage(Clipboard.GetImage());
                var newImage = ImageHash.GetMD5Hash(buffer);

                // Check if user copied exactly same text like before.
                if (ImageBuffer == newImage)
                    return;

                // Set buffer to check.
                ImageBuffer = newImage;

                var dataImage = new DataFormat("Bitmap", 1);
                CreateNewButton(dataImage);
            }

            //if (Clipboard.ContainsFileDropList())
            //{
            //    var temp = Clipboard.GetFileDropList();
            //    foreach (var i in temp)
            //        Debug.WriteLine(i);
            //}
        }

        #endregion

        #region Button Methods

        /// <summary>
        /// Close application.
        /// </summary>
        /// <param name="sender">Button that user clicked.</param>
        /// <param name="e">Click event.</param>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
           // Close this window and application.
            Close();
        }

        /// <summary>
        /// Common method for created buttons (interact with clipboard).
        /// </summary>
        /// <param name="sender">Button object.</param>
        /// <param name="e">Click event.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
           // Cast object sender to button.
            var button = (Button)sender;

            // Remember that we clicked on button.
            ButtonClicked = true;
            if (!(button.Tag is 0))
                ButtonClickedTwice = true;

            // Send this button context to clipboard.
            try
            {
                switch (button.Tag)
                {
                    // If user copied text in clipboard.
                    case 0:
                        Clipboard.SetDataObject(button.DataContext);
                        break;

                    // If user copied image in clipboard.
                    case 1:
                        Clipboard.SetImage((BitmapSource)button.DataContext);
                        break;
                    
                    default:
                        MessageBox.Show("Not implemented yet!");
                        break;

                }

                // Change background color for visability what program have in Clipboard.
                foreach (var gridButton in MainGrid.Children.OfType<Button>())
                {
                    gridButton.Background = Brushes.LightGray;
                }
                button.Background = Brushes.LightGreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed setting new data object at Clipboard. {ex.Message}");
            }
        }

        /// <summary>
        /// Common method to delete created buttons.
        /// </summary>
        /// <param name="sender">Button object.</param>
        /// <param name="e">Mouse click event.</param>
        private void RightMouse_Click(object sender, RoutedEventArgs e)
        {
            // Cast object sender to button.
            var button = (Button)sender;

            // Toogle flag in helper array.
            var index = Grid.GetColumn(button) + (Grid.GetRow(button) - 1) * 3;
            FilledPositions[index] = false;

            // Delete button from form.
            MainGrid.Children.Remove(button);

            // Clear memory from handled resources.
            //GC.GetTotalMemory(true);
            GC.Collect();
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Create new button with keeping resource from clipboard.
        /// </summary>
        /// <param name="dataFormat">Format of sending data.</param>
        private void CreateNewButton(DataFormat dataFormat)
        {
            // Check if grid have free place after deleting button.
            for (var i = 0; i < FilledPositions.Length; ++i)
            {
                if (FilledPositions[i])
                    continue;

                ColumnNumber = i % 3;
                RowNumber    = i / 3 + 1;
                break;
            }

            // If grid filled, return.
            if (RowNumber > 3)
                return;

            // Create button with handled text.
            var button = new Button
            {
                Name    = "Button" + ButtonCounter,
                Cursor  = Cursors.Hand,
                Tag     = dataFormat.Id,
                Opacity = 0.75
            };

            // Set button content.
            switch (dataFormat.Id)
            {
                // Create button with handled text.
                case 0:
                    button.DataContext = Clipboard.GetText();
                    button.Content     = Clipboard.GetText().Trim(' ');
                    break;
                
                // Create button with handled image.
                case 1:
                    button.DataContext = Clipboard.GetImage();
                    button.Content     = new Image
                    {
                        Source            = Clipboard.GetImage(),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    break;
                
                // Create button with empty stuff.
                default:
                    button.DataContext = string.Empty;
                    break;
            }

            // Add click event.
            button.Click                += Button_Click;
            button.MouseRightButtonDown += RightMouse_Click;

            // Change background color for visability what program have in Clipboard.
            foreach (var gridButton in MainGrid.Children.OfType<Button>())
            {
                gridButton.Background = Brushes.LightGray;
            }
            button.Background = Brushes.LightGreen;

            // Change button column and row.
            Grid.SetColumn(button, ColumnNumber);
            Grid.SetRow(button, RowNumber);

            // Add button to grid.
            MainGrid.Children.Add(button);

            // Toogle flag in helper array.
            var index              = ColumnNumber + (RowNumber - 1) * 3;
            FilledPositions[index] = true;

            // Update grid's variables.
            ++ColumnNumber;
            if (ColumnNumber > 2)
            {
                ColumnNumber = 0;
                ++RowNumber;
            }

            ++ButtonCounter;
        }

        /// <summary>
        /// Process mouse enter/leave.
        /// </summary>
        /// <param name="sender">Main window.</param>
        /// <param name="e">Click event.</param>
        private void Window_OnMouseMovement(object sender, MouseEventArgs e)
        {
            // If window is not moving, start moving timer.
            if (!_dispatcherTimer.IsEnabled)
            {
                _dispatcherTimer.Start();
            }
            else
            {
                // Else stop moving timer.
                _dispatcherTimer.Stop();

                // Update all movement variables.
                _movementSize        = _movementSize == DEFAULT_MOVEMENT_SIZE
                                         ? _movementCounter
                                         : _movementCounter + _difference;
                _difference          = DEFAULT_MOVEMENT_SIZE - _movementSize;
                _movementCounter     = 0;
                HasStartingPosition ^= true;

                // And start moving timer again.
                _dispatcherTimer.Start();
            }
        }

        #endregion

        #region Timer Method

        /// <summary>
        /// Animate form movement and change SwitchButton content.
        /// </summary>
        /// <param name="sender">Animation timer.</param>
        /// <param name="e">Basic <see cref="System.EventArgs"/> class.</param>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Change window position dependent on whether the window in the starting position.
            WindowForm.Left += HasStartingPosition ? MOVEMENT_SPEED : -MOVEMENT_SPEED;
            ++_movementCounter;

            // If window riched the end position.
            if (_movementCounter == _movementSize)
            {
                // Stop moving timer.
                _dispatcherTimer.Stop();

                // Reset all movement variables.
                _movementCounter     = 0;
                _difference          = 0;
                _movementSize        = DEFAULT_MOVEMENT_SIZE;
                HasStartingPosition ^= true;
            }


            // If timer has stopped, change text in upper-left corner.
            if (!_dispatcherTimer.IsEnabled)
                SwitchText.Text = HasStartingPosition ? ">>>" : "<<<";


            // Forcing the CommandManager to raise the RequerySuggested event.
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion
    }
}
