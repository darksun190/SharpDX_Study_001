using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;


namespace SharpDX_Study_001
{
    public partial class Form1 : RenderForm
    {
        Device device;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            device = new Device(SharpDX.Direct3D.DriverType.Hardware);
            RenderLoop.Run(this, Draw3D);
        }

        private void Draw3D()
        {
            
        }
    }
}
