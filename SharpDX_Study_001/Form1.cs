using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Windows;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;

using Device = SharpDX.Direct3D11.Device;


namespace SharpDX_Study_001
{
    public partial class Form1 : RenderForm
    {
        Device device;
        SwapChain swapChain;
        Color4 color;
        DeviceContext context;
        RenderTargetView renderView;
        public Form1()
        {
            InitializeComponent();
            initDevice();
        }

        private void initDevice()
        {
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(ClientSize.Width, ClientSize.Height,
                                                                new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_11_0 }, desc, out device, out swapChain);
            context = device.ImmediateContext;
            Texture2D backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            renderView = new RenderTargetView(device, backBuffer);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            device = new Device(SharpDX.Direct3D.DriverType.Hardware);
            RenderLoop.Run(this, Draw3D);
        }

        private void Draw3D()
        {
            context.ClearRenderTargetView(renderView, color);
            swapChain.Present(0, 0);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.D1:

                    color = Color.CornflowerBlue;
                    break;
                case System.Windows.Forms.Keys.D2:
                    color = Color.Red;
                    break;
                case System.Windows.Forms.Keys.D3:
                    color = Color.Blue;
                    break;
                case System.Windows.Forms.Keys.D4:
                    color = Color.Orange;
                    break;
                case System.Windows.Forms.Keys.D5:
                    color = Color.Yellow;
                    break;
                case System.Windows.Forms.Keys.D6:
                    color = Color.Olive;
                    break;
                case System.Windows.Forms.Keys.D7:
                    color = Color.Orchid;
                    break;
                case System.Windows.Forms.Keys.D8:
                    color = Color.Black;
                    break;
            }
        }
    }
}
