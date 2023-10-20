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
            if (!textBox.Text.EndsWith(".iso") || !File.Exists(textBox.Text))
            {
                MessageBox.Show("Please add your ISO path to coninue\nPlease make sure your file is in that path.");
            }
            else
            {
                await Task.Run(() =>
                {
                    csoPath = IMGPATH.Substring(0, IMGPATH.Length - 4); // Remove the .iso from the string

                    // Initialize your input FileStream
                    using (FileStream inputStream = new FileStream(IMGPATH, FileMode.Open, FileAccess.Read))
                    using (FileStream baseStream = new FileStream($@"{csoPath}.cso", FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        byte[] buffer = new byte[inputStream.Length]; // Create buffer
                        inputStream.Read(buffer, 0, buffer.Length); // Read file to buffer

                        long totalBytes = inputStream.Length; // Initialize your totalBytes variable

                        // Create an instance of CsoCompressionStream
                        using (CsoCompressionStream csoStream = new CsoCompressionStream(baseStream, totalBytes))
                        {
                            csoStream.ProgressChanged += UpdateProgressBar; // Subscribe to ProgressChanged event
                            csoStream.Write(buffer, 0, buffer.Length); // Write buffer to csoStream
                            csoStream.Flush(); // Flush the stream
                        }
                    }
                });

                string xbeTitle = Path.GetFileName(csoPath); // Get the file name from csoPath

                xbeTitle titleModifier = new xbeTitle(xbeTitle); // Create an instance of xbeTitle class

                // Create and start a new task for looping system sound
                CancellationTokenSource cts = new CancellationTokenSource();
                Task soundLoopTask = Task.Run(() =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        SystemSounds.Exclamation.Play();
                        Task.Delay(1000).Wait();
                    }
                });

                // Show MessageBox
                MessageBoxResult result = MessageBox.Show($"Compression finished\nYour {xbeTitle}.cso is in the same directory as your selected {xbeTitle}.iso.\nYour XBE for this title is in the 'xbe' directory");

                // Stop looping system sound when 'OK' is clicked
                if (result == MessageBoxResult.OK)
                {
                    cts.Cancel();
                    soundLoopTask.Wait(); // Wait for the task to complete its work
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

