using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArchiveSearcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        bool CancelationEnabled = true;
        CancellationTokenSource cancellationTokenSource;
        public bool Paused = false;

        public delegate bool IsPaused();
        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog Dialog = new FolderBrowserDialog();
            DialogResult result = Dialog.ShowDialog();
            if (result.ToString() == "OK")
            {
                txtPath.Text = Dialog.SelectedPath;

            }
        }

      

     

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (txtPattern.Text != "")
            {
                ComboBoxas.Items.Add(txtPattern.Text);
                ComboBoxas.SelectedIndex = 0;
            }
            else
            {
                System.Windows.MessageBox.Show("Prašome įvesti paieškos elementą", "Klaida!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxas.SelectedIndex != -1)
            {
                ComboBoxas.Items.Remove(ComboBoxas.SelectedItem.ToString());
            }
            else
            {
                System.Windows.MessageBox.Show("Prašome pasirinkti, ką ištrinti", "Klaida!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ProgressBar()
        {
            int FileCount = 0;
            int TotalFilesProcessed = 0;
            bool StillSearching = true;
            
            while (StillSearching == true)
            {
                FileCount = BackEnd.ArffCompiler.getFileCount();
                TotalFilesProcessed = BackEnd.ArffCompiler.getFilesProcessed();
                this.Dispatcher.Invoke(() =>
                {
                    if (FileCount != 0)
                    {
                        progressBaras.Maximum = FileCount;
                        progressBaras.Value = BackEnd.ArffCompiler.getFilesProcessed();
                        if (cancellationTokenSource.IsCancellationRequested == false)
                        {
                            if (FileCount == BackEnd.ArffCompiler.getFilesProcessed())
                            {
                                progressBaras.Maximum = FileCount;
                                CancelationEnabled = true;
                                Console.WriteLine("Cancelation true");
                                buttonStart.IsEnabled = true;
                                StillSearching = false;
                                btnCancel.IsEnabled = false;
                                btnPause.IsEnabled = false;
                                btnSelectFile.IsEnabled = true;
                            }
                        }
                        else
                        {
                            btnCancel.IsEnabled = false;
                            progressBaras.Value = 0;
                            System.Windows.MessageBox.Show("Atšaukta sėkmingai", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
                            StillSearching = false;
                            btnSelectFile.IsEnabled = true;
                        }

                    }

                });
            }
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (CancelationEnabled == true)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
            
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
            
            this.Dispatcher.Invoke(() =>
            {
                progressBaras.Value = 0;
                btnPause.IsEnabled = false;
                buttonStart.IsEnabled = true;
                
            });
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
         
            if (Paused == false)
            {
                Paused = true;
                System.Windows.MessageBox.Show("Sustabdyta");
                btnPause.Content = "Pratesti";
            }
            else if (Paused == true)
            {
                Paused = false;
                System.Windows.MessageBox.Show("Pratesta");
                btnPause.Content = "Sustabdyti";

            }
        }
        public bool IsPausedMRE()
        {
            if (Paused == true)
            {
                return true;
            }
            else
            {
                return false;
            }


        }

        private void buttonStart_Click_1(object sender, RoutedEventArgs e)
        {
           try
            {
                btnPause.IsEnabled = true;
                buttonStart.IsEnabled = false;
                IsPaused DelegatePause = this.IsPausedMRE;
                btnCancel.IsEnabled = true;
                CancelationEnabled = false;
                progressBaras.Value = 0;
                cancellationTokenSource = new CancellationTokenSource();
                btnSelectFile.IsEnabled = false;
                //BackEnd.ArffCompiler.find(txtPath.Text,txtPattern.Text);
                string Path = txtPath.Text;
                  if (ComboBoxas.Items.Count > 0)
                  {
                      string[] AtributesSearchingFor = new string[ComboBoxas.Items.Count];
                      for (int i = 0; i < ComboBoxas.Items.Count; i++)
                      {

                          AtributesSearchingFor[i] = ComboBoxas.Items[i].ToString();

                      }

                Thread Threadas = new Thread(delegate ()
                    {
                        BackEnd.ArffCompiler.ExecuteSearch(AtributesSearchingFor, Path, cancellationTokenSource.Token, DelegatePause);

                    });
                    Threadas.Start();

                    Thread Progresas = new Thread(delegate ()
                    {
                        ProgressBar();

                    });
                    Progresas.Start();

                }
                else
                {
                    System.Windows.MessageBox.Show("Prašome įvesti paieškos elementą", "Klaida!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception exc)
            {
                System.Windows.MessageBox.Show(exc.Message, "Klaida!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
