using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

using iText.Kernel.Pdf;
using iText.Forms;
using iText.Kernel.Font;
using Path = System.IO.Path;
using System.Data;

namespace Fill_A_Doc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static String SRC = @"";
        public static String DEST = @"";
        List<FormField> FormFields = new List<FormField>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ReadPdf()
        {
            Overlay.Visibility = Visibility.Visible;

            // initialize dictionary and list to collect and store form fields from pdf
            FormFields.Clear();
            IDictionary<String, iText.Forms.Fields.PdfFormField> FormFieldsMap = new Dictionary<String, iText.Forms.Fields.PdfFormField>();

            // read pdf and it's form fields
            PdfDocument pdfDoc = new PdfDocument(new PdfReader(SRC));
            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, false);
            try
            {
                FormFieldsMap = form.GetFormFields();
            }
            catch
            {
                Form_Panel.Visibility = Visibility.Collapsed;
                Overlay.Visibility = Visibility.Collapsed;
                pdf_src_textblock.Text = "no pdf selected!";
                pdf_src_textblock.Foreground = new SolidColorBrush(Colors.Red);
                MessageBox.Show("This PDF does not have any form fields! \n\nPlease create form fields on the PDF using softwares such as Open Office Writer or Adobe Acrobat.");
            }
            finally
            {
                pdfDoc.Close();
            }

            if (FormFieldsMap.Count() > 0)
            {
                foreach (KeyValuePair<String, iText.Forms.Fields.PdfFormField> Field in FormFieldsMap)
                {
                    FormFields.Add(new FormField()
                    {
                        FieldName = Field.Key.ToString(),
                        FieldDisplayName = Field.Key.ToString() + ": ",
                        IsMultiline = Field.Value.IsMultiline(),
                        DefaultValue = Field.Value.GetValueAsString()
                    });
                }

                lbFormFields.ItemsSource = null;
                lbFormFields.ItemsSource = FormFields;
                Dispatcher.BeginInvoke(new Action(OnFormLoaded), System.Windows.Threading.DispatcherPriority.SystemIdle, null);
            }
        }

        private void OnFormLoaded()
        {
            Form_Panel.Visibility = Visibility.Visible;
            Overlay.Visibility = Visibility.Collapsed;

            lbFormFields.UpdateLayout();
        }

        private void Open_Pdf_Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".pdf";
            dlg.Filter = "PDF Files (*.pdf)|*.pdf";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                SRC = dlg.FileName;
                pdf_src_textblock.Text = SRC;
                pdf_src_textblock.Foreground = new SolidColorBrush(Colors.Green);
                ReadPdf();
            }
        }

        private void Save_Pdf_Button_Click(object sender, RoutedEventArgs e)
        {
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = Path.GetDirectoryName(SRC) + "\\" + Path.GetFileNameWithoutExtension(SRC) + " - Filled.pdf";
                dlg.DefaultExt = ".pdf";
                dlg.Filter = "PDF Files (*.pdf)|*.pdf";

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    // Save document
                    DEST = dlg.FileName;

                    PdfDocument pdfDoc = new PdfDocument(new PdfReader(SRC), new PdfWriter(DEST));
                    PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, false);

                    try
                    {
                        form.SetGenerateAppearance(true);

                        PdfFont font = PdfFontFactory.CreateFont();

                        IEnumerable<TextBox> elements = FindVisualChildren<TextBox>(lbFormFields).Where(x => x.Tag != null && x.Tag.ToString() == "textBox_FieldValue");
                        foreach (TextBox tb in elements)
                        {
                            String FieldName = ((Grid)tb.Parent).Tag.ToString();
                            IEnumerable<TextBox> fontSizeTextBoxElements = FindVisualChildren<TextBox>(tb.Parent).Where(x => x.Tag != null && x.Tag.ToString() == FieldName);
                            float fieldFontSize = 10f;
                            float.TryParse(fontSizeTextBoxElements.First().Text, out fieldFontSize);
                            form.GetField(FieldName).SetValue(tb.Text, font, fieldFontSize);
                        }

                        if (MessageBox.Show("PDF saved! \nPath: " + DEST + "\n\nDo you want to open the file?",
                            "Successful", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(DEST);
                        }
                    }
                    catch {
                        MessageBox.Show("An error occurred!");
                    }
                    finally {
                        pdfDoc.Close();
                    }
                }
            }
        }

        private void Fill_Open_pdf_Button_Click(object sender, RoutedEventArgs e)
        {
            // temp save document and open
            var filePath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            filePath = Path.Combine(filePath, "Fill-A-Doc");
            System.IO.Directory.CreateDirectory(filePath);
            DEST = filePath + "\\temp.pdf"; ;

            PdfDocument pdfDoc = new PdfDocument(new PdfReader(SRC), new PdfWriter(DEST));
            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdfDoc, true);

            try
            {
                //FileStream fs = File.Create(DEST, 1024);

                form.SetGenerateAppearance(true);

                PdfFont font = PdfFontFactory.CreateFont();

                IEnumerable<TextBox> elements = FindVisualChildren<TextBox>(lbFormFields).Where(x => x.Tag != null && x.Tag.ToString() == "textBox_FieldValue");
                foreach (TextBox tb in elements)
                {
                    String FieldName = ((Grid)tb.Parent).Tag.ToString();
                    IEnumerable<TextBox> fontSizeTextBoxElements = FindVisualChildren<TextBox>(tb.Parent).Where(x => x.Tag != null && x.Tag.ToString() == FieldName);
                    float fieldFontSize = 10f;
                    float.TryParse(fontSizeTextBoxElements.First().Text, out fieldFontSize);
                    form.GetField(FieldName).SetValue(tb.Text, font, fieldFontSize);
                }

                if (MessageBox.Show("PDF saved! \nPath: " + DEST + "\n\nDo you want to open the file?",
                    "Successful", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(DEST);
                }
            }
            catch
            {
                MessageBox.Show("An error occurred!");
            }
            finally
            {
                pdfDoc.Close();
            }
        }

        public class FormField
        {
            public string FieldName { get; set; }
            public string FieldDisplayName { get; set; }
            public bool IsMultiline { get; set; }
            public string DefaultValue { get; set; }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void About_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutBox window = new AboutBox();
            window.Show();
        }

        private void Exit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    public class MultiLineToMinHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if ((bool)value == true)
                    return 42.0;
                else
                    return 0.0;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
