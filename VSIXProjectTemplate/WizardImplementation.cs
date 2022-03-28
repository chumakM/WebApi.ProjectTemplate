using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TemplateWizard;
using System.Windows.Forms;
using EnvDTE;

namespace VSIXProjectTemplate
{
    public class WizardImplementation : IWizard
    {
        private SetupForm inputForm;
        private string UseSerilog;
        private string UseSwagger;
        private string UseEntity;
        private string UseHealth;
        private string UseRestSharp;
        // This method is called before opening any item that
        // has the OpenInEditor attribute.
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        // This method is only called for item templates,
        // not for project templates.
        public void ProjectItemFinishedGenerating(ProjectItem
            projectItem)
        {
        }

        // This method is called after the project is created.
        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {
            try
            {
                // Display a form to the user. The form collects
                // input for the custom message.
                inputForm = new SetupForm();
                inputForm.ShowDialog();
                UseSerilog = SetupForm.UseSerilog;
                UseSwagger = SetupForm.UseSwagger;
                UseEntity = SetupForm.UseEntity;
                UseHealth = SetupForm.UseHealth;
                UseRestSharp = SetupForm.UseRestSharp;
                // Add custom parameters.
                replacementsDictionary.Add("$useSerilog$",
                    UseSerilog);
                replacementsDictionary.Add("$useSwagger$",
                    UseSwagger);
                replacementsDictionary.Add("$useEntity$",
                    UseEntity);
                replacementsDictionary.Add("$useHealth$",
                    UseHealth);
                replacementsDictionary.Add("$useRestSharp$",
                    UseRestSharp);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // This method is only called for item templates,
        // not for project templates.
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
    //public partial class UserInputForm : Form
    //{
    //    private static string useSerilog;
    //    private static string useSwagger;
    //    private Button button1;
    //    private CheckBox checkSerilog;
    //    private CheckBox checkSwagger;

    //    public UserInputForm()
    //    {
    //        this.Size = new System.Drawing.Size(600, 800);

    //        checkSerilog = new CheckBox();
    //        checkSerilog.Location = new System.Drawing.Point(10, 25);
    //        checkSerilog.Name = "Serilog";
    //        checkSerilog.Text = "Use Serilog";
    //        this.Controls.Add(checkSerilog);

    //        checkSwagger = new CheckBox();
    //        checkSwagger.Location = new System.Drawing.Point(10, 50);
    //        checkSwagger.Name = "Swagger";
    //        checkSwagger.Text = "Use Swagger";
    //        this.Controls.Add(checkSwagger);

    //        button1 = new Button();
    //        button1.Location = new System.Drawing.Point(110, 100);
    //        button1.Size = new System.Drawing.Size(100, 25);
    //        button1.Click += button1_Click;
    //        button1.Text = "Confirm";
    //        this.Controls.Add(button1);

    //        //textBox1 = new TextBox();
    //        //textBox1.Location = new System.Drawing.Point(10, 25);
    //        //textBox1.Size = new System.Drawing.Size(70, 20);
    //        //this.Controls.Add(textBox1);
    //    }
    //    public static string UseSerilog
    //    {
    //        get { return useSerilog ?? string.Empty; }
    //        set{ useSerilog = value; }
    //    }

    //    public static string UseSwagger
    //    {
    //        get { return useSwagger ?? string.Empty; }
    //        set { useSwagger = value; }
    //    }
    //    private void button1_Click(object sender, EventArgs e)
    //    {
    //        if (checkSerilog.Checked)
    //            useSerilog = "True";
    //        if(checkSwagger.Checked)
    //            useSwagger = "True";
    //        this.Close();
    //    }
    //}
}