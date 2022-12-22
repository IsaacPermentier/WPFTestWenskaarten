using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFTestWenskaarten
{
    /// <summary>
    /// Interaction logic for WenskaartWindow.xaml
    /// </summary>
    public partial class WenskaartWindow : Window
    {
        string kaartNaam;
        string kleurNaam;
        public WenskaartWindow()
        {
            InitializeComponent();
            SortDescription sd = new SortDescription("Source", ListSortDirection.Ascending);
            lettertypeComboBox.Items.SortDescriptions.Add(sd);
            lettertypeComboBox.SelectedItem = new FontFamily("Arial");

            foreach (PropertyInfo info in typeof(Colors).GetProperties())
            {
                BrushConverter bc = new BrushConverter();
                SolidColorBrush deKleur = (SolidColorBrush)bc.ConvertFromString(info.Name);
                Kleur kleurtje = new Kleur();
                kleurtje.Borstel = deKleur;
                kleurtje.Naam = info.Name;
                kleurtje.Hex = deKleur.ToString();
                kleurtje.Rood = deKleur.Color.R;
                kleurtje.Groen = deKleur.Color.G;
                kleurtje.Blauw = deKleur.Color.B;
                comboBoxKleuren.Items.Add(kleurtje);
            }
            statusBestand.Content = "Nieuw";
        }

        private void comboBoxKleuren_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Kleur gekozenKleur = (Kleur)comboBoxKleuren.SelectedItem;
            if (gekozenKleur != null)
                bal.Fill = gekozenKleur.Borstel;
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            int grootte = Convert.ToInt32(lettertypeGrootte.Content);
            if (grootte < 40)
            {
                grootte++;
                lettertypeGrootte.Content = grootte;
                TekstBox.FontSize = grootte;
            }
        }

        private void RepeatButton_Click_1(object sender, RoutedEventArgs e)
        {
            int grootte = Convert.ToInt32(lettertypeGrootte.Content);
            if (grootte > 10)
            {
                grootte--;
                lettertypeGrootte.Content = grootte;
                TekstBox.FontSize = grootte;
            }
        }
        private Ellipse sleepBal = new Ellipse();
        private void bal_MouseMove(object sender, MouseEventArgs e)
        {
            sleepBal = (Ellipse)sender;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DataObject sleepKleur = new DataObject("deKleur", sleepBal.Fill);
                DragDrop.DoDragDrop(sleepBal, sleepKleur, DragDropEffects.Move);
            }
        }
        Point droppunt = new Point();
        private void bal_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("deKleur"))
            {
                Brush gesleepteKleur = (Brush)e.Data.GetData("deKleur");
                Ellipse gekozenBal = (Ellipse)e.Data.GetData("gekozenBal");
                Ellipse nieuweBal = new Ellipse();
                droppunt = e.GetPosition(canvas);
                Canvas.SetLeft(nieuweBal, droppunt.X - 20);
                Canvas.SetTop(nieuweBal, droppunt.Y - 20);
                nieuweBal.Fill = gesleepteKleur;
                sleepBal.Fill = Brushes.White;
                canvas.Children.Add(nieuweBal);
                canvas.Children.Remove(gekozenBal);
            }
        }

        private void bal_DragEnter(object sender, DragEventArgs e)
        {
            Ellipse kader = (Ellipse)sender;
            kader.StrokeThickness = 7;
        }

        private void bal_DragLeave(object sender, DragEventArgs e)
        {
            Ellipse kader = (Ellipse)sender;
            kader.StrokeThickness = 5;
        }

        private void Kaarten_Click(object sender, RoutedEventArgs e)
        {
            NieuwVeld();
            MenuItem deKaart = (MenuItem)sender;
            foreach (MenuItem huidig in Kaarten.Items)
            {
                huidig.IsChecked = false;
            }
            deKaart.IsChecked = true;
            kaartNaam = deKaart.Name;
            ImageBrush kaart = new ImageBrush();
            BitmapImage bi = new BitmapImage(new Uri(@$"images/{deKaart.Name}.jpg", UriKind.Relative));
            kaart.ImageSource = bi;
            canvas.Background = (ImageBrush)kaart;
            OpslaanButton.IsEnabled = true;
            AfdrukvoorbeeldButton.IsEnabled = true;
            hoofdscherm.Visibility = Visibility.Visible;
        }
        private void NieuwVeld()
        {
            comboBoxKleuren.SelectedIndex = -1;
            bal.Fill = Brushes.White;
            canvas.Background = Brushes.White;
            canvas.Children.Clear();
            TekstBox.Clear();
            lettertypeComboBox.SelectedItem = new FontFamily("Arial");
            lettertypeGrootte.Content = "20";
            OpslaanButton.IsEnabled = false;
            AfdrukvoorbeeldButton.IsEnabled = false;
            statusBestand.Content = "Nieuw";
            hoofdscherm.Visibility = Visibility.Hidden;
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NieuwVeld();
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int aantalBallen = 0;
            foreach (Ellipse bal in canvas.Children)
            {
                aantalBallen++;
            }
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = "wenskaart";
                dlg.DefaultExt = ".kaart";
                dlg.Filter = "wenskaart |*.kaart";

                if (dlg.ShowDialog() == true)
                {
                    using (StreamWriter bestand = new StreamWriter(dlg.FileName))
                    {
                        bestand.WriteLine(dlg.FileName + " " + kaartNaam);
                        bestand.WriteLine(aantalBallen);
                        foreach (Ellipse bal in canvas.Children)
                        {
                            foreach (Kleur kleur in comboBoxKleuren.Items)
                            {
                                if (bal.Fill.ToString() == kleur.Hex)
                                {
                                    kleurNaam = kleur.Naam;
                                }

                            }
                            bestand.WriteLine(kleurNaam + " X: " + Math.Round(Canvas.GetLeft(bal), 0) + " Y: " + Math.Round(Canvas.GetTop(bal), 0));
                        }
                        bestand.WriteLine(TekstBox.Text);
                        bestand.WriteLine(lettertypeComboBox.SelectedItem.ToString());
                        bestand.WriteLine(lettertypeGrootte.Content);
                    }
                    statusBestand.Content = dlg.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Opslaan mislukt: " + ex.Message);
            }
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int aantalBallen;

            try
            {
                NieuwVeld();
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "wenskaart |*.kaart";

                if (dlg.ShowDialog() == true)
                {
                    using (StreamReader bestand = new StreamReader(dlg.FileName))
                    {
                        string eersteLijn = bestand.ReadLine();
                        int indexKaart = eersteLijn.IndexOf(" ");
                        kaartNaam = eersteLijn.Substring(indexKaart + 1);
                        ImageBrush kaart = new ImageBrush();
                        BitmapImage bi = new BitmapImage(new Uri(@$"images/{kaartNaam}.jpg", UriKind.Relative));
                        kaart.ImageSource = bi;
                        canvas.Background = (ImageBrush)kaart;
                        aantalBallen = Convert.ToInt32(bestand.ReadLine());
                        for (int teller = 0; teller <= aantalBallen - 1; teller++)
                        {
                            Ellipse bal = new Ellipse();
                            string tekst = bestand.ReadLine();
                            int index = tekst.IndexOf(" ");
                            kleurNaam = tekst.Substring(0, index);
                            BrushConverter bc = new BrushConverter();
                            foreach (Kleur kleur in comboBoxKleuren.Items)
                            {
                                if (kleurNaam == kleur.Naam)
                                {
                                    bal.Fill = (SolidColorBrush)bc.ConvertFromString(kleur.Hex);
                                }
                            }
                            int indexX = tekst.IndexOf("X");
                            int startpos = indexX + 2;
                            int indexY = tekst.IndexOf("Y");
                            int lengteX = indexY - 1 - startpos;
                            string stringX = tekst.Substring(indexX + 2, lengteX);
                            string stringY = tekst.Substring(indexY + 2);
                            int x = Convert.ToInt32(stringX);
                            int y = Convert.ToInt32(stringY);
                            Canvas.SetLeft(bal, x);
                            Canvas.SetTop(bal, y);
                            canvas.Children.Add(bal);
                        }
                        TekstBox.Text = bestand.ReadLine();
                        lettertypeComboBox.SelectedItem = new FontFamily(bestand.ReadLine());
                        lettertypeGrootte.Content = bestand.ReadLine();
                    }
                    statusBestand.Content = dlg.FileName;
                    hoofdscherm.Visibility = Visibility.Visible;
                    OpslaanButton.IsEnabled = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Openen mislukt: " + ex.Message);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (MessageBox.Show("Wilt u het programma sluiten ?", "Afsluiten", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
                e.Cancel = true;
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
        private Ellipse VindBal(Object sleepitem)
        {
            DependencyObject keuze = (DependencyObject)sleepitem;
            while (keuze != null)
            {
                if (keuze is Ellipse)
                    return (Ellipse)keuze;
                keuze = VisualTreeHelper.GetParent(keuze);
            }
            return null;
        }
        private void Image_Drop(object sender, DragEventArgs e)
        {
            Ellipse gekozenBal = (Ellipse)e.Data.GetData("gekozenBal");
            Brush gesleepteKleur = (Brush)e.Data.GetData("deKleur");
            canvas.Children.Remove(gekozenBal);

        }
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Ellipse bal = VindBal(e.OriginalSource);
                if (bal != null)
                {
                    DataObject sleepKleur = new DataObject("deKleur", bal.Fill);
                    DragDrop.DoDragDrop(bal, sleepKleur, DragDropEffects.Move);
                    DataObject gesleeptebal = new DataObject("gekozenBal", bal);
                    DragDrop.DoDragDrop(bal, gesleeptebal, DragDropEffects.Move);
                }
            }

        }
    }
}
