using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CisoConverter;
using Microsoft.Win32;
using System.Media;
using System.Reflection;
using System.Net.Security;

namespace CisoConverter
{
    public partial class MainWindow : Window
    {
        String IMGPATH = "";
        String csoPath = "";

        //private CsoCompressionStream csoStream;

        public MainWindow()
        {
           
            InitializeComponent();
        }

        private void UpdateProgressBar(double percentage)
        {
            // Assuming progressBar is your ProgressBar control
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = percentage;
            });
        }


        private async void button_Click(object sender, RoutedEventArgs e)
        {
            // Validate the input ISO path.
            if (!textBox.Text.EndsWith(".iso") || !File.Exists(textBox.Text))
            {
                MessageBox.Show("Please add your ISO path to continue\nPlease make sure your file is in that path.");
                return;
            }
            else
            {
                CsoCompressionStream csoStream = null;  // Declare csoStream outside Task.Run

                // Run the time-consuming operation on a separate thread.
                await Task.Run(() =>
                {
                    // Generate the CSO path by trimming the last 4 characters to remove the ".iso" extension.
                    csoPath = IMGPATH.Substring(0, IMGPATH.Length - 4);

                    // Open the input ISO and output CSO streams.
                    using (FileStream inputStream = new FileStream(IMGPATH, FileMode.Open, FileAccess.Read))
                    using (FileStream baseStream = new FileStream($"{csoPath}.cso", FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        long totalBytes = inputStream.Length;
                        byte[] buffer = new byte[8192];  // Use an 8KB buffer for reading.

                        // Initialize the CsoCompressionStream
                        csoStream = new CsoCompressionStream(baseStream, totalBytes);

                        // Set paths for the CSO file
                        csoStream.myCsoPath(csoPath, IMGPATH.Substring(0, IMGPATH.Length - 4));

                        // Subscribe to the ProgressChanged event for updating the progress bar.
                        csoStream.ProgressChanged += UpdateProgressBar;

                        int bytesRead;

                        // Read the ISO file in chunks and write it to the CSO file.
                        while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            csoStream.Write(buffer, 0, bytesRead);
                        }

                        csoStream.Flush();  // Ensure all data is written and buffered data is flushed.
                    }
                });

                csoStream?.splitCSO();  // Call splitCSO() on csoStream if it is not null.

                string xbeTitle = Path.GetFileName(csoPath);
                xbeTitle titleModifier = new xbeTitle(xbeTitle);

                CancellationTokenSource cts = new CancellationTokenSource();
                Task soundLoopTask = Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        SystemSounds.Exclamation.Play();
                        Task.Delay(1000).Wait();
                    }
                });

                MessageBoxResult result = MessageBox.Show($"Compression finished\nYour {xbeTitle}.cso is in the same directory as your selected {xbeTitle}.iso.\nYour XBE for this title is in the 'xbe' directory");

                if (result == MessageBoxResult.OK)
                {
                    cts.Cancel();
                    soundLoopTask.Wait();
                }
            }
        }



        private async void decompressButton_Click(object sender, RoutedEventArgs e)
        {

            // Print message for debugging purposes
            if (!textBox.Text.EndsWith(".cso") || !File.Exists(textBox.Text))
            {
                MessageBox.Show("Please add your CSO path to coninue\nPlease make sure your file is in that path.");
                return;  
            }
            else
            {
                string isoPath = IMGPATH.Substring(0, IMGPATH.Length - 4);
                string xbeTitle = Path.GetFileName(isoPath);
                await System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        // Initialize the decompression stream
                        using (var compressedStream = new FileStream(IMGPATH, FileMode.Open))
                        using (var decompressionStream = new CsoDecompressionStream(compressedStream))
                        using (var outputFileStream = new FileStream($@"{isoPath}-decompressed.iso", FileMode.Create))
                        {
                            decompressionStream.ProgressChanged += UpdateProgressBar;  // <-- Moved outside of the loop

                            // Buffer for reading the decompressed data
                            byte[] buffer = new byte[4096];

                            // Loop to read from the compressed file and write to the decompressed file
                            int bytesRead;
                            while ((bytesRead = decompressionStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                outputFileStream.Write(buffer, 0, bytesRead);
                            }
                        }

                        MessageBox.Show($"Decompression complete!\nYour {xbeTitle}-decompressed.iso is in the same directory as your selected {xbeTitle}.cso");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred: {ex.Message}");
                    }
                });
            }
        }


        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ISO, CSO files|*.iso;*.cso"; // Filter for .iso and .cso files

            // If the user selects a file and clicks 'Open'
            if (openFileDialog.ShowDialog() == true)
            {
                // Save the full path to IMGPATH and display it in the TextBox
                IMGPATH = openFileDialog.FileName;
                textBox.Text = IMGPATH;
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            textBox.Text = IMGPATH;
        }
    }
}

