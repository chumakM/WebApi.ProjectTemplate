using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSIXProjectTemplate
{
    public partial class SetupForm : Form
    {
        private static string useSerilog;
        private static string useSwagger;
        private static string useEntity;
        private static string useHealth;
        private static string useRestSharp;
        public SetupForm()
        {
            InitializeComponent();
        }
        public static string UseSerilog
        {
            get { return useSerilog ?? string.Empty; }
            set { useSerilog = value; }
        }

        public static string UseSwagger
        {
            get { return useSwagger ?? string.Empty; }
            set { useSwagger = value; }
        }
        public static string UseEntity
        {
            get { return useEntity ?? string.Empty; }
            set { useEntity = value; }
        }

        public static string UseHealth
        {
            get { return useHealth ?? string.Empty; }
            set { useHealth = value; }
        }
        public static string UseRestSharp
        {
            get { return useRestSharp ?? string.Empty; }
            set { useRestSharp = value; }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (Serilog.Checked)
                useSerilog = "True";
            else
                useSerilog = "False";
            if (Swagger.Checked)
                useSwagger = "True";
            else
                useSwagger = "False";
            if (EntityFramework.Checked)
                useEntity = "True";
            else
                useEntity = "False";
            if (HealthCheck.Checked)
                useHealth = "True";
            else
                useHealth = "False";
            if (RestSharp.Checked)
                useRestSharp = "True";
            else
                useRestSharp = "False";
            this.Close();
        }
    }
}
