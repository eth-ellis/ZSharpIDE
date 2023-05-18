namespace YourNamespace
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var form = new Form();
            form.Text = "Program";

            var label = new Label();
            label.Text = "Waddup Pimps!";

            form.Controls.Add(label);

            Application.Run(form);
        }
    }
}